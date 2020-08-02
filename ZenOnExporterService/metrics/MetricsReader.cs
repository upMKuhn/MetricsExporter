using ZenOnExporterService.models;
using System;
using System.Collections.Generic;
using System.Threading;
using zenOn;
using MetricsCommon;

namespace ZenOnExporterService.metrics
{
    public class MetricsReader
    {
        protected IProject project;
        protected int poolInterval = 1000;
        protected bool shouldStop = false;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected Dictionary<string, ZenonVariable>  variableState = new Dictionary<string, ZenonVariable>();

        public event EventHandler<VariablesUpdatedEventArgs> VariablesUpdated;


        public MetricsReader(IProject project, int poolInterval = 1000)
        {
            this.project = project;
            this.poolInterval = poolInterval;
        }

        ~MetricsReader() {
        }

        public void Run() {
            this.shouldStop = false;
            log.Info("MetricsReader Run");
            const int MAX_ERRORS = 10;
            var numErrors = 0;
            int sleepTime = TimeFn.ExponentialTimeoutFn(numErrors, this.poolInterval);

            while (!this.shouldStop || numErrors < MAX_ERRORS)
            {
                try
                {
                    sleepTime = TimeFn.ExponentialTimeoutFn(numErrors, this.poolInterval);
                    Thread.Sleep(sleepTime);
                    log.Debug("Updating Variables");
                    this.updateVariableStates();
                    log.Debug("Updated Variables");
                    var eventArgs = new VariablesUpdatedEventArgs { variables = this.variableState.Values };
                    this.VariablesUpdated.Invoke(this, eventArgs);
                    numErrors = 0;
                } catch(Exception ex) {
                    numErrors += 1;
                    sleepTime = TimeFn.ExponentialTimeoutFn(numErrors, this.poolInterval);

                    log.Error(ex);
                    log.Info($"Retyring next time in {sleepTime/1000} seconds");
                }
            }

            if(numErrors >= MAX_ERRORS)
            {
                throw new Exception($"Metrics Reader reached {MAX_ERRORS} consecutive errors .... Giving up");
            }
        }

        public void Stop() {
            log.Info("MetricsReader asked to stop");
            this.shouldStop = true;
        }

        /// <summary>
        /// Fetches variables from zenOn and updates this.variableState
        /// </summary>
        protected void updateVariableStates() {
            var variables = this.project.Variables();
            for (var i = 0; i < variables.Count; i++) {
                var variable = variables.Item(i);
                ZenonVariable knownState = null;

                if (!this.variableState.TryGetValue(variable.Name, out knownState)) {
                    this.variableState.Add(variable.Name, new ZenonVariable
                    {
                        Name = variable.Name,
                        Type = variable.BaseType.Name,
                        Unit = variable.Unit,
                        Value = variable.Value,
                        PvId = variable.PvID
                    });
                }

                knownState = this.variableState[variable.Name];
                knownState.Name  = variable.Name;
                knownState.PvId  = variable.PvID;
                knownState.Unit  = variable.Unit;
                knownState.Value = variable.Value;
                knownState.Type = variable.BaseType.Name;
            }
        }
    }


    public class VariablesUpdatedEventArgs : EventArgs {

        public IEnumerable<ZenonVariable> variables = new List<ZenonVariable>();
    }
}
