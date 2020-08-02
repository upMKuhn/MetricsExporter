using MetricsCommon.export;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ZenOnExporterService.models
{
    public class ZenonVariable : IExportableMetric
    {

        private Dictionary<string, object> values = new Dictionary<string, object>();
        private Dictionary<string, object> changes = new Dictionary<string, object>();

        public string Name {
            get { return this.values["Name"] as string; }
            set {
                this.detectChanges("Name", value);
                this.values["Name"] = value;
            }
        }

        public object Value {
            get { return values["Value"] as object; }
            set
            {
                detectChanges("Value", value);
                values["Value"] = value;
            }
        }

        public string Type {
            get { return values["Type"] as string; }
            set
            {
                detectChanges("Type", value);
                values["Type"] = value;
            }
        }

        public string Unit {
            get { return values["Unit"] as string; }
            set
            {
                detectChanges("Unit", value);
                values["Unit"] = value;
            }
        }

        public int PvId {
            get { return (values["PvId"] != null ? (int) values["PvId"] : Int32.MinValue ); }
            set
            {
                this.detectChanges("PvId", value);
                this.values["PvId"] = value;
            }
        }


        public void resetChangeCapture() {
            this.changes = new Dictionary<string, object>();
        }

        /// <summary>
        /// Returns all changes made to this object. <PropertyName, oldValue>
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> getChanges() {
            return this.changes;
        }

        public bool IsCorrupted()
        {
            return this.Value.Equals(-2147483638);
        }

        public ExportableMetric AsSerializeable()
        {
            return new ExportableMetric()
            {
                Name = Name,
                Value = Value,
                Type = Type,
                Unit = Unit,
            };
        }

        public bool hasChanges() {
            return this.changes.Count > 0;
        }

        /// <summary>
        /// Detect changes and note them down
        /// </summary>
        /// <param name="property"></param>
        /// <param name="newValue"></param>
        private void detectChanges(string property, object newValue) {
            object oldValue = null;
            if (!this.values.TryGetValue(property, out oldValue)) {
                return;
            }

            if (oldValue != newValue) {
                this.changes[property] = oldValue;
            }
        }

    }
}
