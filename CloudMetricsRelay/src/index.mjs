import { default as express } from 'express'
import { default as WSApi } from 'ws'
import { default as http } from 'http'
import { default as url } from 'url'
import { default as Prometheus } from 'prom-client'
import { default as Redis } from 'redis'
import { MetricsRepository, MetricsImporter } from './repository/index.js'
import { HolzgasTransformer, KranTransformer } from './transformer/index.js'
import * as config from './utils/config.js'
import { makeInfluxConnection } from './utils/index.js'

(async function () {
    const app = express();
    const server = http.createServer(app)
    const wss = new WSApi.Server({ noServer: true })
    const metricsRepo = new MetricsRepository(60 * 30)

    const redisClient = Redis.createClient({
        host: process.env.REDIS_HOST || config.DEFAULT_REDIS_HOST,
        port: process.env.REDIS_PORT || config.DEFAULT_REDIS_PORT,
        db: process.env.REDIS_DB || config.DEFAULT_REDIS_DB,
        password: process.env['redis-password'] || undefined,
    })

    const influxMetricsClient = await makeInfluxConnection(process.env.INFLUX_DB || config.DEFAULT_INFLUX_DB)
    const influxKpiClient = await makeInfluxConnection(
        process.env.INFLUX_KPI_DB || config.DEFAULT_INFLUX_KPI_DB
    ).ca

    let metricsImporters = []
    let metricTransformers = {
        BR_Metrics: new HolzgasTransformer(influxMetricsClient),
        Kran_Metrics: new KranTransformer(redisClient, influxMetricsClient, influxKpiClient),
    }

    metricsRepo.onUpdate = (serviceName, metrics, linie = null, location = null) => {
        const timestamp = Date.now()
        if (metricTransformers[serviceName]) {
            location = location || process.env.DEFAULT_LOCATION || config.DEFAULT_LOCATION
            linie = linie || process.env.DEFAULT_LINIE || config.DEFAULT_LINIE
            metricTransformers[serviceName].transform(metrics, serviceName, location, timestamp, linie)
        }
    }

    /**
     * @this WebSocket
     */
    function heartbeat() {
        clearTimeout(this.pingTimeout)
        // Use `WebSocket#terminate()`, which immediately destroys the connection,
        // instead of `WebSocket#close()`, which waits for the close timer.
        // Delay should be equal to the interval at which your server
        // sends out pings plus a conservative assumption of the latency.
        this.isAlive = true
        this.pingInterval = setTimeout(() => {
            this.ping(() => {})
        }, 10000)
        this.pingTimeout = setTimeout(() => {
            clearTimeout(this.pingInterval)
            console.warn('Connection dropped due to missing heartbeat', new Date().toISOString())
            this.terminate()
        }, 30000 + 1000)
    }

    wss.on('connection', (ws) => {
        const importer = new MetricsImporter(ws, metricsRepo)
        ws.on('open', heartbeat)
        ws.on('pong', heartbeat)
        ws.on('close', function clear() {
            console.info(
                'client closes connection',
                ws._socket.remoteAddress,
                'aka',
                ws._socket._remoteAddress,
                new Date().toISOString()
            )
            clearTimeout(this.pingTimeout)
            clearTimeout(this.pingInterval)
        })

        ws.ping('ping')
        console.info(
            'client connected',
            ws._socket.remoteAddress,
            'aka',
            ws._socket._remoteAddress,
            new Date().toISOString()
        )

        metricsImporters.push(importer)
        wss.on('close', () => {
            metricsImporters = metricsImporters.filter((value) => value != importer)
        })
    })

    server.on('upgrade', (req, socket, head) => {
        const pathname = url.parse(req.url).pathname
        if (pathname === '/metrics') {
            socket._remoteAddress = req.headers['x-forwarded-for'] || req.connection.remoteAddress
            wss.handleUpgrade(req, socket, head, function done(ws) {
                wss.emit('connection', ws, req)
            })
        } else {
            socket.destroy()
        }
    })

    app.get('/:serviceName/show-metrics', (req, response) => {
        const serviceName = req.params.serviceName
        if (!metricsRepo.hasRecentMetrics(serviceName)) {
            response.status(502)
            response.json({
                msg: `There is no recent update for service ${serviceName}`,
            })
        } else {
            response.json({
                lastUpdated: metricsRepo.getUpdatedTime(serviceName).toISOString(),
                metrics: metricsRepo.get(serviceName),
            })
        }
    })

    app.get('/health', async (req, response) => {
        response.json({
            numClients: wss.clients.size,
            redis: {
                connected: redisClient.connected,
                ready: redisClient.ready,
            },
        })
        return response
    })

    app.get('/metrics', (req, response) => {
        req.headers
        response.set('Content-Type', Prometheus.register.contentType)
        response.end(Prometheus.register.metrics())

        const userAgent = req.headers['user-agent']
        const isPrometheus = userAgent.indexOf('Mozilla') == -1 && userAgent.indexOf('Chrome') == -1
        if (isPrometheus) {
            Object.values(metricTransformers).forEach((t) => t.onScrapped())
        }
        return response
    })
    server.listen(8001)
})().catch((error) => {
    console.error(error)
    process.exit(1)
})
