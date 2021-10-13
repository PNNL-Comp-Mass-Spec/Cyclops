
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

namespace Cyclops.DataModules
{
    public class Transform : BaseDataModule
    {
        private readonly string m_ModuleName = "Transform";
        private readonly string m_Description = "Scales, adds, and/or log transforms the data";

        /// <summary>
        /// Required parameters to run Transform Module
        /// </summary>
        private enum RequiredParameters
        {
            InputTableName, NewTableName
        }




        /// <summary>
        /// Generic constructor creating an Transform Module
        /// </summary>
        public Transform()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// Transform module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public Transform(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// Transform module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public Transform(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
            Parameters = ExportParameters;
        }

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
                    successful = TransformFunction();
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

            paramDictionary.Add("Add", "0");
            paramDictionary.Add("Scale", "1");
            paramDictionary.Add("LogBase", "2");

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

        /// <summary>
        /// Performs the transformation
        /// </summary>
        /// <returns>True, if the transformation completes successfully</returns>
        public bool TransformFunction()
        {
            string rCmd;

            if (Parameters.ContainsKey("logBase"))
            {
                rCmd = string.Format(
                    "{0} <- log((data.matrix({1})+{2})*{3},{4})",
                    Parameters[nameof(RequiredParameters.NewTableName)],
                    Parameters[nameof(RequiredParameters.InputTableName)],
                    Parameters.ContainsKey("add") ? Parameters["add"] : "0",
                    Parameters.ContainsKey("scale") ? Parameters["scale"] : "1",
                    Parameters["logBase"]);
            }
            else
            {
                rCmd = string.Format(
                    "{0} <- ({1}+{2})*{3}",
                    Parameters[nameof(RequiredParameters.NewTableName)],
                    Parameters[nameof(RequiredParameters.InputTableName)],
                    Parameters.ContainsKey("add") ? Parameters["add"] : "0",
                    Parameters.ContainsKey("scale") ? Parameters["scale"] : "1");
            }

            try
            {
                var successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
                return successful;
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while performing transformation:\n" + ex);
                SaveCurrentREnvironment();
                return false;
            }
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
    }
}
