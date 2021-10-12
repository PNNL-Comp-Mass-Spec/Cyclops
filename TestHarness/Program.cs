using System;
using System.Collections.Generic;
using System.IO;
using Cyclops;

namespace CyclopsTest
{
    internal static class Program
    {
        // Ignore Spelling: workflow

        private static void Main(string[] args)
        {
            TestItraqOperations();

            System.Threading.Thread.Sleep(1000);
        }

        private static void TestItraqOperations()
        {
            Console.WriteLine("Entering TestItraqOperations");

            var workingDirectory = new DirectoryInfo(@"..\..\Docs");

            var baseToolRunner = new AnalysisManagerBase.clsAnalysisToolRunnerBase();

            var rProgramLoc = baseToolRunner.GetRPathFromWindowsRegistry();
            Console.WriteLine("Using R at " + rProgramLoc);

            var paramDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    {"Job", "1299605"},
                    {"RDLL", rProgramLoc},
                    {"CyclopsWorkflowName", "ITQ_ExportOperation.xml"},
                    {"workDir", workingDirectory.FullName},
                    {"Consolidation_Factor", ""},
                    {"Fixed_Effect", ""},
                    {"RunProteinProphet", "false"},
                    {"orgDbDir", @"C:\DMS_Temp_Org"}
                };

            try
            {
                Console.WriteLine("Initializing the CyclopsController");
                var cyclops = new CyclopsController(paramDictionary);

                cyclops.ErrorEvent += Cyclops_ErrorEvent;
                cyclops.WarningEvent += Cyclops_WarningEvent;
                cyclops.StatusEvent += Cyclops_StatusEvent;

                Console.WriteLine("Running the workflow against the Results.db3 file in " + workingDirectory.FullName);
                var success = cyclops.Run();

                Console.WriteLine("Success: " + success);
            }
            catch (Exception ex)
            {
                PRISM.ConsoleMsgUtils.ShowError("Error running Cyclops: " + ex.Message, ex);
            }
        }

        private static void Cyclops_ErrorEvent(string message, Exception ex)
        {
            PRISM.ConsoleMsgUtils.ShowError(message, ex);
        }

        private static void Cyclops_WarningEvent(string message)
        {
            PRISM.ConsoleMsgUtils.ShowWarning(message);
        }

        private static void Cyclops_StatusEvent(string message)
        {
            Console.WriteLine(message);
        }
    }
}
