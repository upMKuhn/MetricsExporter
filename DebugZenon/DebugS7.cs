using S7ExporterService.metrics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DebugZenon
{
    class DebugS7
    {
        public void Main()
        {
            string VAR_CONFIG_DIR = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            VariableConfigRepository variableRepo = new VariableConfigRepository(VAR_CONFIG_DIR);

            var variableDefinitions = variableRepo.GetVariables();
            var supported_variables = variableDefinitions.Where(obj => obj.IsPlcValueConvertable() && obj.Path == "Kran").OrderBy(obj => obj.Name).ToArray();
            var plcReader = new PlcReader(address: "192.168.20.2");
            plcReader.Connect();
            plcReader.SetVariablesToWatch(supported_variables.ToList());
            while (true)
            {
                Console.Clear();
                Console.WriteLine("____Reading Values____");
                plcReader.Update();
                foreach (var item in supported_variables)
                {
                    Console.WriteLine($"{item.Name}:      {item.Value} ({item.DataType})");
                }

                Thread.Sleep(1 * 1000);
            }

        }

    }
}
