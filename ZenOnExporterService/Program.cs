using System;
using System.Linq;
using System.Threading;
using MetricsCommon.export;
using MetricsService;
using zenOn;
using ZenOnExporterService.metrics;

namespace ZenOnExporterService
{
    static class Program
    {
        static IApplication app;
        static IProject project;
        static MetricsReader reader;
        static WsMetricsExporter exporter;
        const int OneMinute = 60 * 1000;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            while (true)
            {
                try
                {
                    log.Info("Starting Main");
                    MetricsServiceSettings settings = new MetricsServiceSettings();
                    Program.project = getProject();
                    Program.reader = makeMetricsReader(project, settings);
                    Program.exporter = makseMetricsExporter(settings);
                    log.Info("Created Dependencies");

                    reader.VariablesUpdated += (object sender, VariablesUpdatedEventArgs args) => exporter.SendMetric(args.variables);

                    var ServiceToRun = new ZenOnMetricsExporterService(reader);
                    log.Info("Created Service");
                    ServiceToRun.OnStart();
                    log.Info("Service Running");
                    log.Info($"Waiting for thread to exit {ServiceToRun.readerThread.ThreadState}");
                    ServiceToRun.readerThread.Join();
                    log.Info($"Thread exited {ServiceToRun.readerThread.ThreadState}");
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    Thread.Sleep(OneMinute);
                }
            }
        }

        static IProject getProject()
        {
            while (true)
            {
                Program.app = new Application();
                log.Info("Getting Project");
                project = app.Projects().Item(0);
                if (project != null)
                {
                    log.Info("Received project " + project.Name);
                    return project;
                }
                Thread.Sleep(OneMinute);
            }
        }

        static WsMetricsExporter makseMetricsExporter(MetricsServiceSettings settings)
        {
            WsMetricsExporter exporter = new WsMetricsExporter(
                new WebSocketSharp.WebSocket(settings.MetricsWsEndpoint),
                settings.ServiceName,
                settings.Linie,
                settings.Location,
                Convert.ToInt32(settings.MetricsWsPingIntervalSec)
            );
            return exporter;
        }

        static MetricsReader makeMetricsReader(IProject project, MetricsServiceSettings settings)
        {
            var reader = new MetricsReader(project, poolInterval: Convert.ToInt32(settings.PoolIntervalMs));
            return reader;
        }

    }
}