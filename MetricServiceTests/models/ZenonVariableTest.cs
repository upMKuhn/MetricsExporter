using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZenOnExporterService.models;

namespace MetricServiceTests
{
    [TestClass]
    public class ZenonVariableTest
    {

        #region Initalizing sets correct value
        [TestMethod]
        public void Initating__pvid__sets_correct_value()
        {
            var variable = this.createZenonVariable(pvId:31);
            Assert.AreEqual(31, variable.PvId);
        }


        [TestMethod]
        public void Initating__name__sets_correct_value()
        {
            var variable = this.createZenonVariable(name: "Name");
            Assert.AreEqual("Name", variable.Name);
        }

        [TestMethod]
        public void Initating__type__sets_correct_value()
        {
            var variable = this.createZenonVariable(type: "LONG");
            Assert.AreEqual("LONG", variable.Type);
        }

        [TestMethod]
        public void Initating__unit__sets_correct_value()
        {
            var variable = this.createZenonVariable(unit: "kg");
            Assert.AreEqual("kg", variable.Unit);
        }

        [TestMethod]
        public void Initating__value__sets_correct_value()
        {
            var variable = this.createZenonVariable(value: "12s");
            Assert.AreEqual("12s", variable.Value);
        }
        #endregion

        #region Updating <Property> Sets correct value

        [TestMethod]
        public void Updating__pvid__sets_correct_value()
        {
            var variable = this.createZenonVariable(pvId: 31);
            variable.PvId = 32;
            Assert.AreEqual(32, variable.PvId);
        }


        [TestMethod]
        public void Updating__name__sets_correct_value()
        {
            var variable = this.createZenonVariable(name: "Name");
            variable.Name = "Name2";
            Assert.AreEqual("Name2", variable.Name);
        }

        [TestMethod]
        public void Updating__type__sets_correct_value()
        {
            var variable = this.createZenonVariable(type: "LONG");
            variable.Type = "INT";
            Assert.AreEqual("INT", variable.Type);
        }

        [TestMethod]
        public void Updating__unit__sets_correct_value()
        {
            var variable = this.createZenonVariable(unit: "kg");
            variable.Unit = "mg";
            Assert.AreEqual("mg", variable.Unit);
        }

        [TestMethod]
        public void Updating__value__sets_correct_value()
        {
            var variable = this.createZenonVariable(value: "12s");
            variable.Value = "13s";
            Assert.AreEqual("13s", variable.Value);
        }

        #endregion

        #region Updating <Property> records change

        [TestMethod]
        public void Updating__pvid__records_change()
        {
            var variable = this.createZenonVariable(pvId: 31);
            variable.PvId = 32;
            Assert.AreEqual(31, variable.getChanges()["PvId"]);
        }

        [TestMethod]
        public void Updating__name__records_change()
        {
            var variable = this.createZenonVariable(name: "Name1");
            variable.Name = "Name2";
            Assert.AreEqual("Name1", variable.getChanges()["Name"]);
        }

        [TestMethod]
        public void Updating__type__records_change()
        {
            var variable = this.createZenonVariable(type: "Type1");
            variable.Type = "Type2";
            Assert.AreEqual("Type1", variable.getChanges()["Type"]);
        }

        [TestMethod]
        public void Updating__unit__records_change()
        {
            var variable = this.createZenonVariable(unit: "Unit1");
            variable.Unit = "Unit2";
            Assert.AreEqual("Unit1", variable.getChanges()["Unit"]);
        }

        [TestMethod]
        public void Updating__value__records_change()
        {
            var variable = this.createZenonVariable(value: "Value1");
            variable.Value = "Value2";
            Assert.AreEqual("Value1", variable.getChanges()["Value"]);
        }
        #endregion

        #region HasChanges
        [TestMethod]
        public void HasChanges__AfterUpdatingAny__returns_true()
        {
            var variable = this.createZenonVariable(value: "Value1");
            variable.Value = "Value2";
            Assert.IsTrue(variable.hasChanges());
        }

        [TestMethod]
        public void HasChanges__after_reset__returns_false()
        {
            var variable = this.createZenonVariable(value: "Value1");
            variable.Value = "Value2";
            variable.resetChangeCapture();
            Assert.IsFalse(variable.hasChanges());
        }

        [TestMethod]
        public void HasChanges__in_inital_state__returns_false()
        {
            var variable = this.createZenonVariable(value: "Value1");
            variable.resetChangeCapture();
            Assert.IsFalse(variable.hasChanges());
        }

        #endregion

        private ZenonVariable createZenonVariable(int pvId = 91, string name="VarName", string type="LONG", string unit = "KG", object value = null) {
            var variable = new ZenonVariable
            {
                PvId = pvId,
                Name = name,
                Type = type,
                Unit = unit,
                Value = value
            };
            return variable;
        }
    }
}
