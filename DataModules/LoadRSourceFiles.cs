/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: joseph.brown@pnnl.gov
 * Website: http://omics.pnl.gov/software
 * -----------------------------------------------------
 * 
 * Notice: This computer software was prepared by Battelle Memorial Institute,
 * hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the
 * Department of Energy (DOE).  All rights in the computer software are reserved
 * by DOE on behalf of the United States Government and the Contractor as
 * provided in the Contract.
 * 
 * NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY WARRANTY, EXPRESS OR
 * IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS SOFTWARE.
 * 
 * This notice including this sentence must appear on any copies of this computer
 * software.
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
    /// Loads the R source files, and all libraries needed. This 
    /// </summary>
    public class LoadRSourceFiles : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "LoadRSourceFiles";
        /// <summary>
        /// Required parameters to run MissedCleavageSummary Module
        /// </summary>
        private enum RequiredParameters
        {
        }

        private string[] m_PackagesToLoad = new string[] 
        {
            "Cairo"
            , "gplots"
            , "grDevices"
            , "Hmisc"
            , "lme4"
            , "moments"
            , "outliers"
            , "pcaPP"
            , "reshape"
            , "RSQLite"
        };
        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an R Source File Module
        /// </summary>
        public LoadRSourceFiles()
        {
            ModuleName = m_ModuleName;
        }

        /// <summary>
        /// RSourceFileModule module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public LoadRSourceFiles(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
        }

        /// <summary>
        /// RSourceFileModule module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="DataParameters">Parameters to run module</param>
        public LoadRSourceFiles(CyclopsModel CyclopsModel,
            Dictionary<string, string> DataParameters)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
            Parameters = DataParameters;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override bool PerformOperation()
        {
            bool b_Successful = true;
            
            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running " + ModuleName,
                        ModuleName, StepNumber);

                if (CheckParameters())
                {
                    b_Successful = Run_LoadRSourceFiles(); 
                    
                    if (b_Successful)
                        b_Successful = CheckThatRequiredPackagesAreInstalled();

                    if (b_Successful)
                        b_Successful = LoadLibraries();
                }
            }

            return b_Successful;
        }

        /// <summary>
        /// Checks the parameters to ensure that all required keys are present
        /// </summary>
        /// <returns>True, if all required keys are included in the
        /// Parameters</returns>
        public override bool CheckParameters()
        {
            bool b_Successful = true;

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogWarning("Required Field Missing: " + s,
                        ModuleName, StepNumber);
                    b_Successful = false;
                    return b_Successful;
                }
            }

            return b_Successful;
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
            bool b_Successful = true;

            try
            {
                string s_WorkDir = "";
                if (!Parameters.ContainsKey("source"))
                {
                    s_WorkDir = Path.Combine(
                        Model.WorkDirectory, "R_Scripts");
                }
                else
                    s_WorkDir = Parameters["source"];

                Model.LogMessage(
                    string.Format("Preparing to load " +
                    "{0} R scripts into workspace...",
                    Directory.GetFiles(s_WorkDir, "*.R").Length));

                foreach (string s in Directory.GetFiles(s_WorkDir))
                {
                    if (Path.GetExtension(s).ToUpper().Equals(".R"))
                    {
                        if (Parameters.ContainsKey("removeFirstCharacters"))
                            b_Successful = CleanRSourceFile(s);

                        if (b_Successful)
                        {
                            string Command = string.Format(
                                "source('{0}')\n",
                                s.Replace("\\", "/"));
                            b_Successful = Model.RCalls.Run(Command,
                                ModuleName, StepNumber);
                        }
                        if (!b_Successful)
                        {
                            Model.LogError("Unsuccessful attempt to load R source file: " +
                                s, ModuleName, StepNumber);
                            return b_Successful;
                        }
                    }
                }

                Model.RCalls.Run("objects2delete <- ls()\n",
                    ModuleName, StepNumber);
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while loading R source files: " +
                    exc.ToString(), ModuleName, StepNumber);
                b_Successful = false;
            }

            return b_Successful;
        }

        /// <summary>
        /// Visual Studios adds a 3 character format to the
        /// beginning of each ".R" file, so just need to
        /// clean it up before reading it.
        /// </summary>
        /// <param name="FileName">Name of file to clean</param>
        /// <returns>True, if the file is cleaned successfully</returns>
        private bool CleanRSourceFile(string FileName)
        {
            bool b_Successful = true;

            try
            {
                StreamReader sr = new StreamReader(FileName);
                string s_Content = sr.ReadToEnd();
                sr.Close();
                //s_Content = s_Content.Remove(0, 2);
                s_Content.Replace("ï»¿", "");
                StreamWriter sw = new StreamWriter(FileName);
                sw.Write(s_Content);
                sw.Close();
            }
            catch (IOException ioe)
            {
                b_Successful = false;
            }
            catch (Exception exc)
            {
                b_Successful = false;
            }

            return b_Successful;
        }

        public bool CheckThatRequiredPackagesAreInstalled()
        {
            bool b_Successful = true;

            foreach (string s in m_PackagesToLoad)
            {
                if (!Model.RCalls.IsPackageInstalled(s))
                {
                    try
                    {
                        Model.RCalls.InstallPackage(s);
                    }
                    catch (Exception exc)
                    {
                        Model.LogError("Exception encountered while installing " +
                            "package: " + s + "\nException: " + exc.ToString());
                        return false;
                    }
                }
            }

            return b_Successful;
        }

        /// <summary>
        /// Loads the required packages into R environment
        /// </summary>
        /// <returns>True, if packages are loaded successfully</returns>
        private bool LoadLibraries()
        {
            string Command = "";
            foreach (string s in m_PackagesToLoad)
            {
                Command += string.Format("require({0})\n",
                    s);
            }

            return Model.RCalls.Run(Command, ModuleName, StepNumber);
        }
        #endregion
    }
}
