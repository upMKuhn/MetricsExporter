import { default as influx } from 'influx'
import { MetricTransformer } from './transformer.js'
import * as Client from 'prom-client'

export class KranMetric {
    /** @type {string} */
    Name = null
    Value = null
    /** @type {string} */
    Type = null
    /** @type {string} */
    Unit = null
    /** @type {string} */
    Description = null
    /** @type {string} */
    Namespace = null
}

/**
 * @summary Extracts Metrics from Simatec S7 machine-data in namespace Kran  
 */
export class KranTransformer extends MetricTransformer {
    lastUpdated = null
    promMetrics = {}
    cache = {}
    serviceName = 'Kran_Metrics'
    /** @type {influx.InfluxDB} */
    influxClient = null
    /** @type {import('influx').IPoint[]} */
    metricsToWrite = []
    kpiToWrite = []
    timestamp = Date.now()


    /**
     * 
     * @param {redis.Redis} redisClient 
     * @param {influx.InfluxDB} influxMetricClient 
     * @param {influx.InfluxDB} influxKpiClient 
     */
    constructor(redisClient, influxMetricClient, influxKpiClient) {
        super()
        this.influxMetricClient = influxMetricClient
        this.influxKpiClient = influxKpiClient
        this.redisClient = redisClient
    }

    /**
     *
     * @param {Date} lastUpdated
     * @param {HolzMetric[]} metrics
     */
    transform(metrics, serviceName, location, timestamp, linie = null) {
        this.serviceName = serviceName
        this.location = location || 'Pinzberg'
        this.timestamp = timestamp
        this.emitWoodUsageMetrics(metrics.filter(m => m.Namespace == 'Kran'));
        this.flushToInflux()
    }

    flushToInflux() {
        this.influxMetricClient.writePoints(this.metricsToWrite).then(() => { }).catch(console.error)
        this.influxKpiClient.writePoints(this.kpiToWrite).then(() => { }).catch(console.error)
        this.metricsToWrite = []
        this.kpiToWrite = []
    }

    setInCache(key, value) {
        this.redisClient.set(key, value);
    }

    getFromCache(key) {
        return new Promise(resolver => {
            this.redisClient.get(key, (err, value) => {
                if (err) {
                    throw err;
                }
                resolver(value)
            })
        });
    }

    /**
     * 
     * @param {KranMetric[]} metrics 
     */
    emitWoodUsageMetrics(metrics) {
        const fillDate = metrics.filter(metric => metric.Name == 'DB560_KRAN_STAT_DT_CHARGE_dtDateTime')[0]
        const usage = metrics.filter(metric => metric.Name == 'DB76_GewLadungAbgeladen_kg')[0]
        const targeted_line = metrics.filter(metric => metric.Name == 'DB76_Ziel_HVG')[0]

        this.getFromCache('fill_date').then(cached_last_fill_date => {
            if (!cached_last_fill_date) {
                this.setInCache('fill_date', fillDate.Value)
                return;
            }

            if (fillDate.Value != cached_last_fill_date) {
                this.kpiToWrite.push({
                    measurement: 'Kran_Holz_Lieferung',
                    tags: {
                        serviceName: this.serviceName,
                        linie: targeted_line.Value,
                        location: this.location,
                        unit: 'kg'
                    },
                    fields: {
                        time: this.timestamp,
                        Gewicht_Ladung: usage.Value * 1
                    }
                })
                this.setInCache('fill_date', fillDate.Value)
                this.flushToInflux()
            }
        })
    }


}
