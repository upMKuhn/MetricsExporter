using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsCommon.export
{
    public struct ExportableMetric
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public string Type { get; set; }
        //optional
        public string Description { get; set; }
        //optional
        public string Unit { get; set; }
        public string Namespace { get; set; }

    }

    public interface IExportableMetric
    {
        bool IsCorrupted();
        ExportableMetric AsSerializeable();
    }
}
