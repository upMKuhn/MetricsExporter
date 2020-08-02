export class MetricsRepository {
    metricsByProject = {}
    onUpdate = () => {}
    constructor(onUpdate = () => {}) {
        this.onUpdate = onUpdate
    }

    update(serviceName, metrics, linie = null, location = null) {
        this.onUpdate(serviceName, metrics, linie, location)
        this.metricsByProject[serviceName] = {
            linie,
            location,
            lastUpdated: Date.now(),
            metrics,
        }
    }

    hasMetrics(serviceName) {
        return this.metricsByProject[serviceName] !== undefined
    }

    hasRecentMetrics(serviceName, timeoutSec = null) {
        return this.hasMetrics(serviceName)
    }

    /**
     *
     * @param {string} serviceName
     * @returns {Date}
     */
    getUpdatedTime(serviceName) {
        return new Date(this.metricsByProject[serviceName].lastUpdated)
    }

    get(serviceName) {
        return this.metricsByProject[serviceName].metrics
    }
}
