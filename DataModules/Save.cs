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
    public class Save : BaseDataModule
    {
        #region Members
        /// <summary>
        /// Required parameters to run MissedCleavageSummary Module
        /// </summary>
        private enum RequiredParameters
        { }

        private readonly string m_ModuleName = "Save";
        private readonly string m_Description = "";

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
            var successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running " + ModuleName, ModuleName, StepNumber);

                if (CheckParameters())
                {
                    successful = SaveFunction();
                }
            }

            return successful;
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
        /// Saves the R environment
        /// </summary>
        /// <returns>True, if the R environment is saved successfully</returns>
        public bool SaveFunction()
        {
            bool successful;
            var defaultOutputFileName = "Results.RData";

            try
            {
                string filePath;
                if (!string.IsNullOrEmpty(Model.WorkDirectory) &&
                    Parameters.ContainsKey("fileName"))
                {
                    filePath = Path.Combine(Model.WorkDirectory, Parameters["fileName"]);
                }
                else if (!string.IsNullOrEmpty(Model.WorkDirectory))
                {
                    filePath = Path.Combine(Model.WorkDirectory, defaultOutputFileName);
                }
                else
                {
                    filePath = defaultOutputFileName;
                }

                var filePathForR = GenericRCalls.ConvertToRCompatiblePath(filePath);

                Model.LogMessage("Saving R environment to: " + filePathForR, ModuleName, StepNumber);

                successful = Model.RCalls.SaveEnvironment(filePathForR);

                if (successful)
                {
                    Model.RWorkEnvironment = filePathForR;
                }
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while Saving R Environment: " + ex);
                successful = false;
            }

            return successful;
        }
        #endregion
    }
}
