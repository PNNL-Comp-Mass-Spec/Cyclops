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
using System.IO;

namespace Cyclops.DataModules
{
    public class LoadRWorkspace : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "LoadRWorkspace";
        private string m_Description = "";
        
        /// <summary>
        /// Required parameters to run LoadRWorkspace Module
        /// </summary>
        private enum RequiredParameters
        {
            InputFileName
        }
        
        #endregion

        #region Properties

        #endregion

        #region Constructors
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

                Model.LogMessage("Running LoadRWorkspace", ModuleName, StepNumber);

                if (CheckParameters())
                    b_Successful = LoadRWorkspaceFunction();
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
                    Model.LogError("Required Field Missing: " + s, ModuleName, StepNumber);
                    b_Successful = false;
                    return b_Successful;
                }
            }

            if (!File.Exists(Parameters[RequiredParameters.InputFileName.ToString()]))
            {
                Model.LogError("The R workspace file that you wish to load, " +
                    Parameters[RequiredParameters.InputFileName.ToString()] +
                    ", does not exist!", ModuleName, StepNumber);
                b_Successful = false;
            }

            return b_Successful;
        }

        /// <summary>
        /// Loads the R workspace into the current environment
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool LoadRWorkspaceFunction()
        {
            bool b_Successful = true;

            string Command = string.Format(
                "load('{0}')\n",
                Parameters[RequiredParameters.InputFileName.ToString()].Replace("\\", "/"));

            try
            {
                b_Successful = Model.RCalls.Run(Command, ModuleName, StepNumber);

                if (b_Successful)
                    Model.RWorkEnvironment = Parameters[
                        RequiredParameters.InputFileName.ToString()].Replace("\\", "/");
            }
            catch (Exception ex)
            {
                Model.LogError("Exception was encountered while loading an R workspace: " +
                    ex.ToString(), ModuleName, StepNumber);
                b_Successful = false;
            }

            return b_Successful;
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
        #endregion
    }
}
