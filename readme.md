# What is this Metrics exporter good for ?

ZenonMetricsExporter ELT is a proof of concept MVP to extract live internal data from an SIMATIC S7 PLC and a B&R system without it's source code.

## So what, how does it do?

I think a diagram speaks a 1000 words to explain the whole set up. Its basically a normal ETL pipline (Extract, Transform, Load) that loads as much internal data from a system to do some easy explorative analysis.

![Architecture diagram](./Assets/zenon-metrics.png)

| Component          | Responsibillity                                         | Detail                                                    |
| ------------------ | ------------------------------------------------------- | --------------------------------------------------------- |
| S7 PLC             | Controls an Kran suppliying a Wood gasifier with fuel.  | Contains live KPI'se e.g. Wood usage                      |
| B&R PLC            | Controls Wood gasifier plant / produce electricity      | KPI data maybe useful for maintence planning              |
| CloudMetrics Relay | Transform types, varibale names, and store it           | Received Websocket data ticks from export "service"       |
| Kapacitor          | enforce monitoring rules                                | Alert via telegram if expected data is missing            |
| Chronograf UI      | Query data, Display graphs and create dashboards        | Current interface to Influxdb                             |
| InfluxDb           | Store , Query, intervals aggregation, trash old metrics | InfluxDB an time series database for lage volumes of data |

### Current state

This project is currently on hold due hold due to difficulty of remote collaboration with stakeholders, time constraints.
