/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: joseph.brown@pnnl.gov
 * Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
 * -----------------------------------------------------
 *
 * Licensed under the 2-Clause BSD License; you may not use this
 * file except in compliance with the License.  You may obtain
 * a copy of the License at https://opensource.org/licenses/BSD-2-Clause
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Cyclops.DataModules
{
    /// <summary>
    /// Loads all the R source files in the directory and subdirectories
    /// </summary>
    public class RSourceFileModule : BaseDataModule
    {
        private string m_ModuleName = "RSourceFileModule";
        /// <summary>
        /// Required parameters to run MissedCleavageSummary Module
        /// </summary>
        private enum RequiredParameters
        { }




        /// <summary>
        /// Generic constructor creating an R Source File Module
        /// </summary>
        public RSourceFileModule()
        {
            ModuleName = m_ModuleName;
        }

        /// <summary>
        /// RSourceFileModule module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public RSourceFileModule(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
        }

        /// <summary>
        /// RSourceFileModule module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="DataParameters">Parameters to run module</param>
        public RSourceFileModule(CyclopsModel CyclopsModel, Dictionary<string, string> DataParameters)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
            Parameters = DataParameters;
        }

        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                if (CheckParameters())
                    Model.PipelineCurrentlySuccessful = Run_LoadRSourceFiles();

                RunChildModules();
            }
        }

        /// <summary>
        /// Checks the parameters to ensure that all required keys are present
        /// </summary>
        /// <returns>True, if all required keys are included in the
        /// Parameters</returns>
        public override bool CheckParameters()
        {
            bool successful = true;

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogError("Required Field Missing: " + s, ModuleName, StepNumber);
                    return false;
                }
            }

            return successful;
        }

        protected override string GetDefaultValue()
        {
            return "false";
        }

        protected override string GetTypeName()
        {
            return ModuleName;
        }

        /// <summary>
        /// Externally accessible function to load the R source files
        /// </summary>
        /// <returns>True, if R source files are loaded successfully</returns>
        public bool Run_LoadRSourceFiles()
        {
            bool successful = true;

            try
            {
                string workDir = "";
                if (!Parameters.ContainsKey("source"))
                {
                    workDir = Path.Combine(Model.WorkDirectory, "R_Scripts");
                }
                else
                    workDir = Parameters["source"];

                Model.LogMessage(
                    string.Format("Preparing to load " +
                    "{0} R scripts into workspace...",
                    Directory.GetFiles(workDir, "*.R").Length));

                foreach (string s in Directory.GetFiles(workDir))
                {
                    if (Path.GetExtension(s).ToUpper().Equals(".R"))
                    {
                        if (Parameters.ContainsKey("removeFirstCharacters"))
                            successful = CleanRSourceFile(s);

                        if (successful)
                        {
                            string Command = string.Format(
                                "source(\"{0}\")\n",
                                s.Replace("\\", "/"));
                            successful = Model.RCalls.Run(Command, ModuleName, StepNumber);
                        }
                        if (!successful)
                        {
                            Model.LogError("Unsuccessful attempt to load R source file: " +
                                s, ModuleName, StepNumber);
                            return successful;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while loading R source files: " +
                    exc.ToString(), ModuleName, StepNumber);
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Visual Studios adds a 3 character format to the
        /// beginning of each ".R" file, so just need to
        /// clean it up before reading it.
        /// </summary>
        /// <param name="FileName">Name of file to clean</param>
        /// <returns>True, if the file is cleaned successfully</returns>
        private bool CleanRSourceFile(string filePath)
        {
            bool successful = true;

            try
            {
                StreamReader sr = new StreamReader(filePath);
                string content = sr.ReadToEnd();
                sr.Close();

                content.Replace("ï»¿", "");
                StreamWriter sw = new StreamWriter(filePath);
                sw.Write(content);
                sw.Close();
            }
            catch (IOException ioe)
            {
                successful = false;
            }
            catch (Exception exc)
            {
                successful = false;
            }

            return successful;
        }
    }
}
