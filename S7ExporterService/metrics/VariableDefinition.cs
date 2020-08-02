using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPS7Lnk.Advanced;
using MetricsCommon.export;

namespace S7ExporterService.metrics
{
    public class VariableDefinition<T> : IExportableMetric
    {
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Path")]
        public string Path { get; set; }
        [JsonProperty("Connection")]
        public string Connection { get; set; }
        [JsonProperty("DataType")]
        public string DataType { get; set; }
        [JsonProperty("Length")]
        public int Length { get; set; }
        [JsonProperty("Coding")]
        public string Coding { get; set; }
        [JsonProperty("Address")]
        public string Address { get; set; }
        [JsonProperty("Access_Method")]
        public string Access_Method { get; set; }
        [JsonProperty("Indirect_addressing")]
        public bool Indirect_addressing { get; set; }
        [JsonProperty("Start_value")]
        public string Start_value { get; set; }
        [JsonProperty("Display_name")]
        public string Display_name { get; set; }
        [JsonProperty("Comment")]
        public string Comment { get; set; }
        [JsonProperty("Acquisition_mode")]
        public string Acquisition_mode { get; set; }
        [JsonProperty("Acquisition_cycle")]
        public string Acquisition_cycle { get; set; }

        /// <summary>
        /// Manually set in .patch.json
        /// </summary>
        [JsonProperty("Unit")]
        public string Unit { get; set; }

        IPlcValue _AsPlcValue;
        public IPlcValue AsPlcValue
        {
            get
            {
                if (!this.IsPlcValueConvertable()) return null;
                if (this._AsPlcValue != null) return this._AsPlcValue;
                this._AsPlcValue = this.ToIPlcValue();
                return this._AsPlcValue;
            }
        }

        public T Value { get { return (T)this.AsPlcValue.Value; } }


        /// <summary>
        /// Is Variable defintion supported to be read from PLC 
        /// </summary>
        /// <returns></returns>
        public bool IsPlcValueConvertable()
        {
            var TypeMap = this.GetPlcValueConversionMap();
            return this.DataType != null && this.Address!= null && TypeMap.Keys.Contains(this.DataType)
                && this.Address.StartsWith("%");
        }

        public IPlcValue ToIPlcValue()
        {
            var TypeMap = this.GetPlcValueConversionMap();

            if (this.IsPlcValueConvertable())
            {
                return TypeMap[this.DataType]();
            }
            throw new Exception($"Variable of type {this.DataType} is not convertable to IPlcValue");
        }

        /// <summary>
        /// Merge two VariableDefinition instances
        /// </summary>
        /// <param name="updatedVar">Takes precedence</param>
        /// <returns></returns>
        public VariableDefinition<T> Merge(VariableDefinition<T> updatedVar)
        {
            var newObj = new VariableDefinition<T>();
            Type t = typeof(VariableDefinition<object>);
            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite).ToList();
            foreach (var prop in properties)
            {
                var patchedValue = prop.GetValue(updatedVar, null);
                var originalValue = prop.GetValue(this, null);
                var newValue = patchedValue != null? patchedValue : originalValue;
                newValue = newValue != null && newValue.Equals("<No Value>") ? null : newValue;

                prop.SetValue(newObj,  newValue, null);

            }
            return newObj;
        }

        /// <summary>
        /// Is Dummy value, readable or deprecated.
        /// Called:
        /// E.g. Should skip during export?
        /// </summary>
        /// <returns></returns>
        public bool IsCorrupted()
        {
            return !this.IsPlcValueConvertable();
        }

        public ExportableMetric AsSerializeable()
        {
            return new ExportableMetric
            {
                Name = this.Name,
                Description = this.Display_name,
                Type = this.DataType,
                Unit = this.Unit,
                Value = this.Value,
                Namespace = this.Path,
            };
        }


        protected Dictionary<string, Func<IPlcValue>> GetPlcValueConversionMap()
        {
            var Address = this.Address != null ? this.Address.Replace("%", "") : "";
            var Length = this.Length;
            return new Dictionary<string, Func<IPlcValue>>()
            {
                {  "WString" , () => new PlcString(Address, Length) },
                {  "Int" ,  () => new PlcInt16(Address) },
                {  "Bool" , () => new PlcBoolean(Address) },
                {  "DInt" ,  () => new PlcInt32(Address) },
                {  "Date_And_Time" ,  () => new PlcDateTime(Address) },
                {  "Date" ,  () => new PlcDate(Address) },
                {  "Time_Of_Day" , () => new PlcTimeOfDay(Address) },
                {  "Byte" , () => new PlcByte(Address) },
                {  "Real" , () => new PlcReal(Address) },
                {  "DWord" , () => new PlcUInt32(Address) },
                {  "Time" , () => new PlcTime(Address)},
            };
        }
    }
}
