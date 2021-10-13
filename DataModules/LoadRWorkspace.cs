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
    public class LoadRWorkspace : BaseDataModule
    {
        private readonly string m_ModuleName = "LoadRWorkspace";
        private readonly string m_Description = "";

        /// <summary>
        /// Required parameters to run LoadRWorkspace Module
        /// </summary>
        private enum RequiredParameters
        {
            InputFileName
        }

        /// <summary>
        /// Generic constructor creating an LoadRWorkspace Module
        /// </summary>
        public LoadRWorkspace()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// LoadRWorkspace module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public LoadRWorkspace(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// LoadRWorkspace module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public LoadRWorkspace(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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

                Model.LogMessage("Running LoadRWorkspace", ModuleName, StepNumber);

                if (CheckParameters())
                {
                    successful = LoadRWorkspaceFunction();
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
            var successful = true;

            foreach (var s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogError("Required Field Missing: " + s, ModuleName, StepNumber);
                    return false;
                }
            }

            if (!File.Exists(Parameters[RequiredParameters.InputFileName.ToString()]))
            {
                Model.LogError("The R workspace file that you wish to load, " +
                    Parameters[RequiredParameters.InputFileName.ToString()] +
                    ", does not exist!", ModuleName, StepNumber);
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Loads the R workspace into the current environment
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool LoadRWorkspaceFunction()
        {
            bool successful;

            var rCmd = string.Format(
                "load('{0}')\n",
                Parameters[RequiredParameters.InputFileName.ToString()].Replace("\\", "/"));

            try
            {
                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);

                if (successful)
                {
                    Model.RWorkEnvironment = Parameters[
                        RequiredParameters.InputFileName.ToString()].Replace("\\", "/");
                }
            }
            catch (Exception ex)
            {
                Model.LogError("Exception was encountered while loading an R workspace: " +
                    ex, ModuleName, StepNumber);
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Retrieves the Default Value
        /// </summary>
        /// <returns>Default Value</returns>
        protected override string GetDefaultValue()
        {
            return "false";
        }

        /// <summary>
        /// Retrieves the Type Name for automatically
        /// registering the module assembly
        /// </summary>
        /// <returns>Module's Name</returns>
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
