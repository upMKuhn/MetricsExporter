
using IPS7Lnk.Advanced;
using MetricsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace S7ExporterService.metrics
{
    public class PlcReader
    {
        const int RACK = 0;
        const int SLOT = 2;

        public event EventHandler<IEnumerable<VariableDefinition<object>>> VariablesUpdated;

        SiemensDevice Device;
        PlcDeviceConnection connection;

        List<VariableDefinition<object>> VariablesToWatch;

        protected int poolInvertalMs { get; }
        protected bool shouldStop = false;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PlcReader(int poolInvertalMs = 1000, string address = "192.168.20.2")
        {
            PlcDeviceEndPoint endpoint = new IPDeviceEndPoint("192.168.20.2", RACK, SLOT);
            this.Device = new SiemensDevice(endpoint, SiemensDeviceType.S7300_400);
            this.connection = this.Device.CreateConnection();
            this.poolInvertalMs = poolInvertalMs;
        }

        ~PlcReader()
        {
            if (this.connection.IsConnected)
            {
                log.Info("Closing Connection");
                this.connection.Close();
                log.Info("Closed Connection");
            }
            else
            {
                log.Info("Connection not closed. As It was not open...");
            }
        }

        public void Run()
        {
            this.shouldStop = false;
            log.Info("PlcReader Run");
            var numErrors = 0;
            var sleepTime = TimeFn.ExponentialTimeoutFn(numErrors, this.poolInvertalMs);

            while (!this.shouldStop)
            {
                try
                {
                    this.Connect();
                    sleepTime = TimeFn.ExponentialTimeoutFn(numErrors, this.poolInvertalMs);
                    Thread.Sleep(sleepTime);
                    log.Debug("Updating Variables");
                    this.Update();
                    log.Debug("Updated Variables");
                    numErrors = 0;
                }
                catch (Exception ex)
                {
                    numErrors += 1;
                    sleepTime = TimeFn.ExponentialTimeoutFn(numErrors, this.poolInvertalMs);

                    log.Error(ex);
                    log.Info($"Retyring next time in {sleepTime / 1000} seconds");
                }
            }
        }

        public void Stop()
        {
            this.shouldStop = true;
        }

        public void Connect()
        {
            if (!this.connection.IsConnected)
            {
                log.Info("Opening Connection");
                this.connection.Open();
                log.Info("Opened Connection");
            }
        }

        public void SetVariablesToWatch(IEnumerable<VariableDefinition<object>> variables)
        {
            this.VariablesToWatch = variables.Where(obj => obj.IsPlcValueConvertable()).ToList();
        }

        public void Update()
        {
            var updatedVariables = this.VariablesToWatch.Where(item => !item.IsCorrupted()).ToList();
            this.connection.ReadValues(updatedVariables.Select(item => item.AsPlcValue));
            log.Debug($"Sending Variables updated event");

            if(VariablesUpdated != null)
            {
                this.VariablesUpdated.Invoke(this, updatedVariables);
            }
        }

    }
}