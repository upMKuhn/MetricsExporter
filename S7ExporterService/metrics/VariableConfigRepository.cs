using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace S7ExporterService.metrics
{
    public class VariableConfigRepository
    {
        string configDirectory;
        List<VariableDefinition<object>> variableDefinitions = new List<VariableDefinition<object>>();

        public VariableConfigRepository(string configDirectory)
        {
            this.configDirectory = configDirectory;
            this.variableDefinitions = this.LoadVariables();
        }

        public IEnumerable<VariableDefinition<object>> GetVariables()
        {
            return this.variableDefinitions;
        }

        public IEnumerable<VariableDefinition<object>> ReloadVariables()
        {
            this.variableDefinitions = this.LoadVariables();
            return this.GetVariables();
        }

        /// <summary>
        /// Load Variable json file that is exported from TIA Portal.
        /// Then Merge together with metadata.json to patch
        /// </summary>
        /// <returns></returns>
        private List<VariableDefinition<object>> LoadVariables()
        {
            List<VariableDefinition<object>> finalDefinitions = new List<VariableDefinition<object>>();
            var varDefinitions = this.ReadVariableFile(Path.Combine(configDirectory, "s7.variables.json"));
            var patches = this.ReadVariableFile(Path.Combine(configDirectory, "s7.variables.patch.json")).ToDictionary(item => item.Name);

            foreach (var varDefinition in varDefinitions)
            {
                if (!patches.ContainsKey(varDefinition.Name))
                {
                    finalDefinitions.Add(varDefinition);
                    continue;
                }
                else
                {
                    var updatedVar = patches[varDefinition.Name];
                    finalDefinitions.Add(varDefinition.Merge(updatedVar));
                }
            }
            return finalDefinitions;
        }

        private List<VariableDefinition<object>> ReadVariableFile(string FileName)
        {
            using (StreamReader file = File.OpenText(FileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                var reader = new JsonTextReader(file);
                return serializer.Deserialize<List<VariableDefinition<object>>>(reader);
            }
        }


    }
}
