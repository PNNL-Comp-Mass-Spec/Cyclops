/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
 * -----------------------------------------------------
 *
 * Licensed under the 2-Clause BSD License; you may not use this
 * file except in compliance with the License.  You may obtain
 * a copy of the License at https://opensource.org/licenses/BSD-2-Clause
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Cyclops.DataModules
{
    /// <summary>
    /// Loads the R source files, and all libraries needed.
    /// </summary>
    public class LoadRSourceFiles : BaseDataModule
    {
        // Ignore Spelling: gplots, Hmisc

        private readonly string m_ModuleName = "LoadRSourceFiles";
        private readonly string m_Description = "";

        /// <summary>
        /// Required parameters to run MissedCleavageSummary Module
        /// </summary>
        private enum RequiredParameters
        { }

        private readonly string[] m_PackagesToLoad = {
            "Cairo",
            "gplots",
            "grDevices",
            "Hmisc",
            "lme4",
            "moments",
            "outliers",
            "pcaPP",
            "reshape",
            "RSQLite"
        };

        /// <summary>
        /// Generic constructor creating an R Source File Module
        /// </summary>
        public LoadRSourceFiles()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// Constructor that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public LoadRSourceFiles(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// Constructor that assigns a Cyclops Model and accepts parameters
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="DataParameters">Parameters to run module</param>
        public LoadRSourceFiles(CyclopsModel CyclopsModel, Dictionary<string, string> DataParameters)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
            Parameters = DataParameters;
        }

        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override bool PerformOperation()
        {
            if (!Model.PipelineCurrentlySuccessful)
            {
                return false;
            }

            Model.CurrentStepNumber = StepNumber;

            Model.LogMessage("Running " + ModuleName, ModuleName, StepNumber);

            if (CheckParameters())
            {
                var successLoading = Run_LoadRSourceFiles();

                if (!successLoading)
                {
                    Model.LogMessage("Run_LoadRSourceFiles returned false");
                    return false;
                }

                var packagesValidated = CheckThatRequiredPackagesAreInstalled();
                if (!packagesValidated)
                {
                    Model.LogMessage("CheckThatRequiredPackagesAreInstalled returned false");
                    return false;
                }

                var packagesLoaded = LoadLibraries();
                if (!packagesLoaded)
                {
                    Model.LogMessage("LoadLibraries returned false");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Retrieves a dictionary of all parameters used by the module,
        /// and the corresponding default values
        /// </summary>
        /// <returns>Parameters used by module</returns>
        public override Dictionary<string, string> GetParametersTemplate()
        {
            var paramDictionary = new Dictionary<string, string>();

            foreach (var s in Enum.GetNames(typeof(RequiredParameters)))
            {
                paramDictionary.Add(s, "");
            }

            return paramDictionary;
        }

        /// <summary>
        /// Checks the parameters to ensure that all required keys are present
        /// </summary>
        /// <returns>True, if all required keys are included in the
        /// Parameters</returns>
        public override bool CheckParameters()
        {
            foreach (var s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogWarning("Required Field Missing: " + s, ModuleName, StepNumber);
                    return false;
                }
            }

            return true;
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
        /// Retrieves the Type Description for automatically
        /// registering the module assembly
        /// </summary>
        /// <returns>Module's Description</returns>
        protected override string GetTypeDescription()
        {
            return Description;
        }

        /// <summary>
        /// Externally accessible function to load the R source files
        /// </summary>
        /// <returns>True, if R source files are loaded successfully</returns>
        public bool Run_LoadRSourceFiles()
        {
            var successful = true;

            try
            {
                string workDir;
                if (!Parameters.ContainsKey("source"))
                {
                    workDir = Path.Combine(
                        Model.WorkDirectory, "R_Scripts");
                }
                else
                    workDir = Parameters["source"];

                Model.LogMessage(
                    string.Format("Preparing to load " +
                    "{0} R scripts into workspace...",
                    Directory.GetFiles(workDir, "*.R").Length));

                foreach (var sourceFilePath in Directory.GetFiles(workDir))
                {
                    if (Path.GetExtension(sourceFilePath).ToUpper().Equals(".R"))
                    {
                        if (Parameters.ContainsKey("removeFirstCharacters"))
                            successful = CleanRSourceFile(sourceFilePath);

                        if (successful)
                        {
                            var rCmd = string.Format(
                                "source('{0}')",
                                sourceFilePath.Replace("\\", "/"));
                            successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
                        }

                        if (!successful)
                        {
                            Model.LogError("Unsuccessful attempt to load R source file: " +
                                           sourceFilePath, ModuleName, StepNumber);
                            return false;
                        }
                    }
                }

                Model.RCalls.Run("objects2delete <- ls()\n", ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while loading R source files: " +
                    ex, ModuleName, StepNumber);
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Visual Studio adds a 3 character format to the
        /// beginning of each ".R" file, so just need to
        /// clean it up before reading it.
        /// </summary>
        /// <param name="filePath">Name of file to clean</param>
        /// <returns>True, if the file is cleaned successfully</returns>
        private bool CleanRSourceFile(string filePath)
        {
            var successful = true;

            try
            {
                var sourceFile = new FileInfo(filePath);
                var tempFile = new FileInfo(Path.GetTempFileName());

                using (var sr = new StreamReader(new FileStream(sourceFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                using (var sw = new StreamWriter(new FileStream(tempFile.FullName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    var content = sr.ReadToEnd();
                    if (content.StartsWith("ï»¿"))
                        sw.Write(content.Substring(3));
                    else
                        sw.Write(content);
                }

                tempFile.Refresh();
                if (tempFile.Length == sourceFile.Length)
                {
                    tempFile.Delete();
                }
                else
                {
                    Console.WriteLine("Removing Unicode byte order mark from file " + sourceFile.FullName);
                    sourceFile.Delete();
                    tempFile.MoveTo(sourceFile.FullName);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("IOException in CleanRSourceFile: " + ex.Message);
                successful = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CleanRSourceFile: " + ex.Message);
                successful = false;
            }

            return successful;
        }

        public bool CheckThatRequiredPackagesAreInstalled()
        {
            Model.LogMessage("Validating that R packages are installed");

            foreach (var packageName in m_PackagesToLoad)
            {
                Model.LogMessage("Checking " + packageName);
                if (Model.RCalls.IsPackageInstalled(packageName))
                {
                    continue;
                }

                try
                {
                    Model.LogMessage("Package is missing; installing " + packageName);
                    Model.RCalls.InstallPackage(packageName);
                }
                catch (Exception ex)
                {
                    Model.LogError("Exception encountered while installing " +
                                   "package: " + packageName + "\nException: " + ex);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Loads the required packages into R environment
        /// </summary>
        /// <returns>True, if packages are loaded successfully</returns>
        private bool LoadLibraries()
        {
            var rCmd = "";
            foreach (var s in m_PackagesToLoad)
            {
                rCmd += string.Format("require({0})\n", s);
            }

            return Model.RCalls.Run(rCmd, ModuleName, StepNumber);
        }
    }
}
