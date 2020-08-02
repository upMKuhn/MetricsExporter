using System.ServiceProcess;
using System.Threading;
using System.Diagnostics;
using System;
using S7ExporterService.metrics;

namespace S7ExporterService
{
    public class S7MetricsExporterService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected PlcReader metricsReader;
        public Thread readerThread;

        public S7MetricsExporterService(PlcReader reader)
        {
            this.metricsReader = reader;
        }

        ~S7MetricsExporterService() {
            this.OnStop();
        }

        public void OnStart()
        {
            log.Info("On Start");
            this.StartReaderThread();
            log.Info("Started thread");
        }

        public void OnStop()
        {
            log.Info("On Stop");
            this.StopReaderThread();
            log.Info("Stopped");
        }




        protected void StartReaderThread()
        {
            if (this.readerThread != null && this.readerThread.IsAlive)
            {
                log.Warn("ZenOnMetricsExporterService: MetricsReaderThread still alive, aborting it now");
                this.readerThread.Abort();
            }

            this.readerThread = new Thread(new ThreadStart(() => this.metricsReader.Run()));
            this.readerThread.Start();
            log.Info($"Started MetricsReader Thread {readerThread.ThreadState}");
        }

        protected void StopReaderThread()
        {
            log.Info("Stopping MetricsReader Thread");
            this.metricsReader.Stop();
            this.readerThread.Join(2000);

            if (this.readerThread.IsAlive)
            {
                log.Warn("Killing MetricsReader Thread, as it did not stop");
                this.readerThread.Abort();
            }
            log.Info("Stopped MetricsReader Thread");
        }
    }
}
