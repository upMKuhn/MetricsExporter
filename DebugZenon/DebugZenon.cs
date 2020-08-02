using System;
using System.Collections.Generic;
using System.Text;
using zenOn;
namespace DebugConsoleApp
{   
    /// <summary>
    /// Simple program to extract the sensor data from a Zenon App
    /// </summary>
    class DebugZenonProgram
    {
        Application app;
        Project project;

        Dictionary<string, IVariable> variables = new Dictionary<string, IVariable>();
        String stdOut = "";

        public void Main(string[] args)
        {
            var program = new DebugZenonProgram();
            Application app = new zenOn.Application();
            Projects projects = app.Projects();

            program.app = app;
            program.project = projects.Item(0);

            Console.WriteLine("Project: {0}", program.project.Name);
            program.extractVars();
            //program.dumpVariables();
            Console.WriteLine("Listening to updates");
            program.listenToVergaser();
            Console.ReadLine();
        }

        public void extractVergaserReading()
        {
            //89
            var vergaserLinks = this.project.Variables().ItemPvID(89);
            var vergaserRechts = this.project.Variables().ItemPvID(90);
            Console.WriteLine("VergaserLinks: {0} {1}   <{2}>", vergaserLinks.Value, vergaserLinks.Unit, vergaserLinks.Name);
            Console.WriteLine("VergaserLinks: {0} {1}   <{2}>", vergaserRechts.Value, vergaserRechts.Unit, vergaserRechts.Name);
        }


        void listenToVergaser()
        {
            while (true)
            {
                this.extractVergaserReading();
                System.Threading.Thread.Sleep(100);
            }
        }

        void extractVars()
        {
            var variables = project.Variables();

            Console.WriteLine("_____ VARS _____");
            this.variables.Clear();
            for (int j = 0; j < variables.Count; j++)
            {
                var variable = variables.Item(j);
                this.variables[variable.Name] = variable;
            }
        }


        void dumpVariables()
        {
            foreach (var item in this.variables.Values)
            {
                var type = item.BaseType.Name;
                var message = String.Format("{0}: {1} <{2}> <PvID={3}>", item.Name, item.Value, type, item.PvID);
                this.WriteLine(message);
            }
        }


        void WriteLine(String msg)
        {
            Console.WriteLine(msg);
            this.stdOut += msg + '\n';
        }

    }
}
