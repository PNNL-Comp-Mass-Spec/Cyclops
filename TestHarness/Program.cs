using System;
using System.Collections.Generic;
using System.IO;
using Cyclops;

namespace CyclopsTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TestItraqOperations();

            System.Threading.Thread.Sleep(1000);
        }

        private static void TestItraqOperations()
        {
            Console.WriteLine("Entering TestItraqOperations");

            var workingDirectory = new DirectoryInfo(@"..\..\Docs");

            var baseToolRunner = new AnalysisManagerBase.clsAnalysisToolRunnerBase();

            var rProgLoc = baseToolRunner.GetRPathFromWindowsRegistry();
            Console.WriteLine("Using R at " + rProgLoc);

            var d_Params = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {

                    {"Job", "1299605"},
                    {"RDLL", rProgLoc},
                    {"CyclopsWorkflowName", "ITQ_ExportOperation.xml"},
                    {"workDir", workingDirectory.FullName},
                    {"Consolidation_Factor", ""},
                    {"Fixed_Effect", ""},
                    {"RunProteinProphet", "false"},
                    {"orgdbdir", @"C:\DMS_Temp_Org"}
                };


            try
            {
                Console.WriteLine("Initializing the CyclopsController");
                var cyclops = new CyclopsController(d_Params);

                cyclops.ErrorEvent += cyclops_ErrorEvent;
                cyclops.WarningEvent += cyclops_WarningEvent;
                cyclops.MessageEvent += cyclops_MessageEvent;

                Console.WriteLine("Running the workflow against the Results.db3 file in " + workingDirectory.FullName);
                var success = cyclops.Run();

                Console.WriteLine("Success: " + success);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error running Cyclops: " + ex.Message);
            }

        }

        static void cyclops_ErrorEvent(object sender, MessageEventArgs e)
        {
            Console.WriteLine("Error: " + e.Message + " (step " + e.Step + ", module " + e.Module + ")");
        }

        static void cyclops_WarningEvent(object sender, MessageEventArgs e)
        {
            Console.WriteLine("Warning: " + e.Message + " (step " + e.Step + ", module " + e.Module + ")");
        }

        static void cyclops_MessageEvent(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e.Message + " (step " + e.Step + ", module " + e.Module + ")");
        }

    }
}
