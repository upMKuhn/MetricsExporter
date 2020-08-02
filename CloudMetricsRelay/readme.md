# What is it
The function of the MetricsRelay has the responsibillity of renaming internal variables from the variouse controllers in a readable fashion and pushing it to influxdb.  So that changes can be done without having to re-deploy any exporters.   

# How to Run it locally  

Please install websocat, docker-compose and docker!  

| CMD              | detail                                          |
| ---------------- | ----------------------------------------------- |
| `make up`        | set up docker-compose env                       |
| `make tail`      | follow docker logs                              |
| `make down`      | stop env                                        |
| `make send-tick` | send example message from sample_br_metric.json |
| `make show-br`   | Show current known version of B&R variables     |

# API

-   Getting the latest metrics: `http://localhost:8001/<serviceName>/show-metrics`
-   pushing metrics `ws://localhost:8001/metrics` with the body

```
{ "serviceName": "<serviceName>", "metrics":[], "location": "<Standort/Location of Metric Origin>" }
```
