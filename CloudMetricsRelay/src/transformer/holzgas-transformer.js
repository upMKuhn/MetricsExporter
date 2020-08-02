import { default as influx } from 'influx'
import { MetricTransformer } from './transformer.js'


export class HolzMetric {
    /** @type {string} */
    Name = null
    Value = null
    /** @type {string} */
    Type = null
    /** @type {string} */
    Unit = null
    /** @type {Number} */
    PvId = null
}

export class HolzgasTransformer extends MetricTransformer {
    lastUpdated = null
    promMetrics = {}
    cache = {}
    serviceName = 'Holzgas_linie_?'
    /** @type {influx.InfluxDB} */
    influxClient = null
    /** @type {import('influx').IPoint[]} */
    dataToWrite = []
    last_value = {}
    timestamp = Date.now()

    /**
     * 
     * @param {influx.InfluxDB} influxClient 
     */
    constructor(influxClient) {
        super()
        this.influxClient = influxClient
    }

    /**
     *
     * @param {HolzMetric[]} metrics
     */
    transform(metrics, serviceName, location, timestamp, linie = null) {

        this.serviceName = serviceName
        this.linieName = linie
        this.location = location
        this.timestamp = timestamp
        const transformers = [
            [/BR_CPU.Analyse\/Gasanalyse.*/i, metric => this.transformGasAnalyse(metric)],
        ]

        for (let metric of metrics) {
            // Metrics that always need to be pushed
            this.transformEnergyProduction(metric)

            //Metrics that are pushed only when changed
            if (this.last_value[metric.Name] == metric.Value) {
                continue;
            }

            for (const transformer of transformers) {
                const key = new RegExp(transformer[0])
                if (key.test(metric.Name)) {
                    transformer[1](metric)
                } else if (metric.Type != 'STRING') {
                    this.transformGenericMetric(metric)
                }
            }
            this.last_value[metric.Name] = metric.Value
        }
        this.influxClient.writePoints(this.dataToWrite).then(() => { }).catch(console.error)
        this.dataToWrite = []
    }

    /**
     *
     * @param {HolzMetric} metric
     */
    transformGenericMetric(metric) {
        if (!metric.Name.startsWith("BR_CPU")) {
            return;
        }

        const names = metric.Name.replace('BR_CPU.', '').replace(/[\[|\]|\.|\_]/g, '_').split('/')

        if (names.length < 2) {
            console.log("Skipped metrics", metric.Name)
            return;
        }
        const measurement = names[0]
        const fields = { time: this.timestamp }
        fields[names[1].replace(/^_*|_*$/g, '')] = metric.Value * 1

        this.dataToWrite.push(
            {
                measurement: 'BR_' + measurement.replace(/^_*|_*$/g, ''),
                tags: {
                    serviceName: this.serviceName,
                    linie: this.linieName,
                    location: this.location,
                    unit: metric.Unit || metric.Type
                },
                fields: fields
            }
        )
    }


    /**
     *
     * @param {HolzMetric} metric
     */
    transformGasAnalyse(metric) {
        const name_segments = metric.Name.split('_')
        const gas_type = name_segments[name_segments.length - 1]
        const fields = { time: this.timestamp }
        fields[gas_type] = metric.Value * 1
        this.dataToWrite.push(
            {
                measurement: 'gas_analyse',
                tags: {
                    serviceName: this.serviceName,
                    linie: this.linieName,
                    location: this.location
                },
                fields: fields
            }
        )
    }

    /** @param {HolzMetric]} metric */
    transformEnergyProduction(metric) {
        const name_mapping = {
            'BR_CPU.Synchronis/AI_Summenscheinleistung_S': ['AI_Summenscheinleistung_S'],
            'BR_CPU.Synchronis/AI_Summenwirkleistung_P': ['AI_Summenwirkleistung_P'],
        }

        if (name_mapping[metric.Name]) {
            const kpi = name_mapping[metric.Name][0]
            const fields = { time: this.timestamp }
            fields[kpi] = metric.Value * 1
            this.dataToWrite.push(
                {
                    measurement: 'energy_production',
                    tags: {
                        serviceName: this.serviceName,
                        linie: this.linieName,
                        location: this.location
                    },
                    fields: fields
                }
            )
        }
    }

}
