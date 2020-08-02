using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using MetricsCommon.export;
using S7ExporterService.metrics;

namespace S7ExporterService
{
    static class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Console.WriteLine("Starting");
            log.Info("Starting Main");
            Console.WriteLine("First Log");
            S7ExporterServiceSettings settings = new S7ExporterServiceSettings();
            log.Info("Created Dependencies");

            // Make Threaded Services
            var servicesToRun = new List<S7MetricsExporterService>() {
                makeGenericMonitoringService(
                    cloudServiceName:"Kran_Metrics",
                    LinienName: "Kran hat keine Linie",
                    variablesToWatch: loadVariableDefintions().Where(item => item.Path == "Kran"),
                    settings: settings
                ),
            };

            // Starting Threads, Waiting, Exiting
            log.Info("Created Service");
            servicesToRun.ForEach(service => service.OnStart());
            log.Info("Service Running");
            log.Info($"Waiting for all threads to exit");
            servicesToRun.ForEach(service => service.readerThread.Join());
            log.Info($"All Threads exited");
        }

        static S7MetricsExporterService makeGenericMonitoringService(string cloudServiceName, string LinienName, IEnumerable<VariableDefinition<object>> variablesToWatch, S7ExporterServiceSettings settings)
        {
            var KranMetricsReader = makeMetricsReader(settings);
            var KranMetricsService = new S7MetricsExporterService(KranMetricsReader);
            var KranExporter = makseMetricsExporter(settings, cloudServiceName, LinienName);
            KranMetricsReader.SetVariablesToWatch(variablesToWatch);
            KranMetricsReader.VariablesUpdated += (object sender, IEnumerable<VariableDefinition<object>> updatedVariables) => KranExporter.SendMetric(updatedVariables);
            return KranMetricsService;
        }

        static WsMetricsExporter makseMetricsExporter(S7ExporterServiceSettings settings, string serviceName, string LinienName)
        {
            WsMetricsExporter exporter = new WsMetricsExporter(
                new WebSocketSharp.WebSocket(settings.MetricsWsEndpoint),
                serviceName,
                LinienName,
                settings.Location,
                Convert.ToInt32(settings.MetricsWsPingIntervalSec)
            );
            return exporter;
        }

        static PlcReader makeMetricsReader(S7ExporterServiceSettings settings)
        {
            var reader = new PlcReader(poolInvertalMs: Convert.ToInt32(settings.PoolIntervalMs), address: settings.S7IpAddress);
            return reader;
        }

        static IEnumerable<VariableDefinition<object>> loadVariableDefintions()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repo = new VariableConfigRepository(dir);
            return repo.GetVariables();
        }

    }
}
