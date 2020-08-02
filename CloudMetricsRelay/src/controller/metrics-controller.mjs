import { MetricsRepository } from '../repository/metrics-repository.js'
import * as WebSocket from 'ws'

export class MetricsController {
    /**
     * @type {WebSocket} ws
     */
    ws

    /**
     * @type {MetricsRepository}
     */
    metricsRepo = null

    /**
     *
     * @param {WebSocket} ws
     * @param {MetricsRepository} metricsRepo
     */
    constructor(ws, metricsRepo) {
        this.ws = ws
        this.metricsRepo = metricsRepo
        this.ws.on('message', data => {
            try{
                this.updateMetrics(data)
            }  catch(error) {
                console.error(error)
            }
        })
    }

    /**
     *
     * @param {WebSocket.Data} data
     */
    updateMetrics(body) {
        let data = {}
        try {
            data = JSON.parse(body)
        } catch(error) {
            console.error('Failed parsing JSON', body)
            console.error('Closing bad connection')
            this.ws.close(1008, 'corrupted json recieved')
            //throw error
        }
        if (!data['serviceName']) {
            console.error('serviceName was not provided', new Date().toISOString(), body)
            this.ws.close(1008, 'serviceName was not provided')
        }
        if (!data['metrics']) {
            console.error('metrics was not provided', new Date().toISOString())
            this.ws.close(1008, 'metrics was not provided')
        }
        this.metricsRepo.update(data['serviceName'], data['metrics'], (data['linie'] || data['linienName']), data['location'])
    }
}
