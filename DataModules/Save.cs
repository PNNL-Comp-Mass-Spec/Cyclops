/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: http://omics.pnl.gov/software
 * -----------------------------------------------------
 *
 * Licensed under the Apache License, Version 2.0; you may not use this
 * file except in compliance with the License.  You may obtain a copy of the
 * License at https://opensource.org/licenses/Apache-2.0
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace Cyclops.DataModules
{
    public class Save : BaseDataModule
    {
        #region Members
        /// <summary>
        /// Required parameters to run MissedCleavageSummary Module
        /// </summary>
        private enum RequiredParameters
        { }

        private string m_ModuleName = "Save";
        private string m_Description = "";
        
        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an SaveEnvironment Module
        /// </summary>
        public Save()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// SaveEnvironment module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public Save(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// SaveEnvironment module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public Save(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
            Parameters = ExportParameters;
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

                Model.LogMessage("Running " + ModuleName, ModuleName, StepNumber);

                if (CheckParameters())
                    b_Successful = SaveFunction();
            }

            return b_Successful;
        }

        /// <summary>
        /// Retrieves a dictionary of all parameters used by the module,
        /// and the corresponding default values
        /// </summary>
        /// <returns>Parameters used by module</returns>
        public override Dictionary<string, string> GetParametersTemplate()
        {
            Dictionary<string, string> d_Parameters = new Dictionary<string, string>();

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                d_Parameters.Add(s, "");
            }

            return d_Parameters;
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
                    Model.LogWarning("Required Field Missing: " + s, ModuleName, StepNumber);
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
        /// Retrieves the Type Description for automatically
        /// registering the module assembly
        /// </summary>
        /// <returns>Module's Description</returns>
        protected override string GetTypeDescription()
        {
            return Description;
        }

        /// <summary>
        /// Saves the R environment
        /// </summary>
        /// <returns>True, if the R environment is saved successfully</returns>
        public bool SaveFunction()
        {
            bool b_Successful = true;
            string s_DefaultOutputFileName = "Results.RData", s_FileName;

            try
            {
                if (!string.IsNullOrEmpty(Model.WorkDirectory) &&
                    Parameters.ContainsKey("fileName"))
                    s_FileName = Model.WorkDirectory + "/" + Parameters["fileName"];
                else if (!string.IsNullOrEmpty(Model.WorkDirectory))
                    s_FileName = Model.WorkDirectory + "/" + s_DefaultOutputFileName;
                else
                    s_FileName = s_DefaultOutputFileName;

                s_FileName = s_FileName.Replace("\\", "/");

                Model.LogMessage("Saving R environment to: " + s_FileName, ModuleName, StepNumber);

                b_Successful = Model.RCalls.SaveEnvironment(s_FileName);

                if (b_Successful)
                    Model.RWorkEnvironment = s_FileName;
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while Saving R Environment: " +
                    ex.ToString());
                b_Successful = false;
            }

            return b_Successful;
        }
        #endregion
    }
}
