
import { default as influx } from 'influx'
import * as config from './config.js'

/**
 * 
 * @param {string} dbName
 * @returns {influx.InfluxDB} 
 */
export async function makeInfluxConnection(dbName) {
    const client = new influx.InfluxDB({
        host: process.env.INFLUX_HOST || config.DEFAULT_INFLUX_HOST,
        database: dbName,
        port: process.env.INFLUX_PORT || config.DEFAULT_INFLUX_PORT,
        username: process.env.INFLUX_USER || config.DEFAULT_INFLUX_USER,
        password: process.env.INFLUX_PASSWORD,
        schema: config.INFLUX_SCHEMA
    });

    const db_names = await client.getDatabaseNames()
    if (!db_names.includes(dbName)) {
        await client.createDatabase(dbName)
    }
    return client;
}