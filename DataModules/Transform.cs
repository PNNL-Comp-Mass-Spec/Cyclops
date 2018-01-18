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
    public class Transform : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "Transform",
            m_Description = "Scales, adds, and/or log transforms the data";
        /// <summary>
        /// Required parameters to run Transform Module
        /// </summary>
        private enum RequiredParameters
        {
            InputTableName, NewTableName
        }
        #endregion

        #region Properties

        #endregion

        #region Constructors
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
        public Transform(CyclopsModel CyclopsModel,
            Dictionary<string, string> ExportParameters)
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

                Model.LogMessage("Running " + ModuleName,
                        ModuleName, StepNumber);

                if (CheckParameters())
                    b_Successful = TransformFunction();
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

            d_Parameters.Add("Add", "0");
            d_Parameters.Add("Scale", "1");
            d_Parameters.Add("LogBase", "2");

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
                    Model.LogWarning("Required Field Missing: " + s,
                        ModuleName, StepNumber);
                    b_Successful = false;
                    return b_Successful;
                }
            }

            return b_Successful;
        }

        /// <summary>
        /// Performs the transformation
        /// </summary>
        /// <returns>True, if the transformation completes successfully</returns>
        public bool TransformFunction()
        {
            bool b_Successful = true;

            string Command = "";

            if (Parameters.ContainsKey("logBase"))
            {
                Command = string.Format(
                    "{0} <- log((data.matrix({1})+{2})*{3},{4})",
                        Parameters[RequiredParameters.NewTableName.ToString()],
                        Parameters[RequiredParameters.InputTableName.ToString()],
                        Parameters.ContainsKey("add") ? Parameters["add"] : "0",
                        Parameters.ContainsKey("scale") ? Parameters["scale"] : "1",
                        Parameters["logBase"]);
            }
            else
            {
                Command = string.Format(
                    "{0} <- ({1}+{2})*{3}",
                        Parameters[RequiredParameters.NewTableName.ToString()],
                        Parameters[RequiredParameters.InputTableName.ToString()],
                        Parameters.ContainsKey("add") ? Parameters["add"] : "0",
                        Parameters.ContainsKey("scale") ? Parameters["scale"] : "1");
            }

            try
            {
                b_Successful = Model.RCalls.Run(Command,
                    ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while performing transformation:\n" +
                    ex.ToString());
                SaveCurrentREnvironment();
                b_Successful = false;
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
        #endregion
    }
}
