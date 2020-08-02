import * as config from '../utils/config.js'
import { makeInfluxConnection } from '../utils/index.js'
import * as fs from 'fs'
import { default as influx } from 'influx'
import {default as moment} from 'moment'
import {default as Handlebars} from 'handlebars'

async function main() {
    const databaseName = process.argv[2]
    const pathToScript = process.argv[3]
    const influxClient = await makeInfluxConnection(
        databaseName || process.env.INFLUX_DB || config.DEFAULT_INFLUX_DB
    )

    const scripts = (fs.readFileSync(pathToScript, { encoding: 'utf8' }) || '')
        .split('---')
        .map(query => applyQueryTemplate(query.trim()))

    console.debug('running queries:', '\n', scripts.join('\n'))
    await influxClient.queryRaw(scripts).then(
        rawData => console.log('Executing scripts succeded', rawData),
        error => {
            console.info('failed to run scripts', pathToScript)
            console.error(error)
        }
    )
    console.log('all done')
}

function applyQueryTemplate(queryString) {
    const context = {
        today: moment().format('YYYY-MM-DD'),
        startOfDay: moment().startOf('day').format('YYYY-MM-DD HH:mm:ss'),
        endOfDay: moment().endOf('day').format('YYYY-MM-DD HH:mm:ss'),
        startOfThisMonth: moment().startOf('month').format('YYYY-MM-DD HH:mm:ss'),
        endOfThisMonth: moment().endOf('month').format('YYYY-MM-DD HH:mm:ss'),
        startOfLastMonth: moment().subtract(1, 'months').startOf('month').format('YYYY-MM-DD HH:mm:ss'),
        endOfLastMonth: moment().subtract(1, 'months').endOf('month').format('YYYY-MM-DD HH:mm:ss'),
    }
    return Handlebars.compile(queryString)(context)
}

process.env.TZ = 'UTC' 
main()
