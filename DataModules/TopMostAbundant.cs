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
    public class TopMostAbundant : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "TopMostAbundant";
        private string m_Description = "";
        private string m_Function = "median";

        private bool m_RemoveNAs = true;
        /// <summary>
        /// Required parameters to run TopMostAbundant Module
        /// </summary>
        private enum RequiredParameters
        {
            InputTableName, NewTableName, NumberOfMostAbundant
        }
        
        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an TopMostAbundant Module
        /// </summary>
        public TopMostAbundant()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// TopMostAbundant module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public TopMostAbundant(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// TopMostAbundant module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public TopMostAbundant(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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
            bool successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running TopMostAbundant", ModuleName, StepNumber);

                if (CheckParameters())
                    successful = TopMostAbundantFunction();
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
            Dictionary<string, string> paramDictionary = new Dictionary<string, string>();

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
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
            bool successful = true;

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogError("Required Field Missing: " + s, ModuleName, StepNumber);
                    successful = false;
                    return successful;
                }
            }

            if (successful &&
                !Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.InputTableName.ToString()]))
            {
                Model.LogError("Error: R environment does not contain the " +
                    "input table: " +
                    Parameters[RequiredParameters.InputTableName.ToString()],
                    ModuleName, StepNumber);
                return false;
            }

            if (successful &&
                Parameters.ContainsKey("Function"))
            {
                if (!string.IsNullOrEmpty(Parameters["Function"]))
                    m_Function = Parameters["Function"];
            }

            if (successful &&
                Parameters.ContainsKey("RemoveNA"))
            {
                if (!string.IsNullOrEmpty(Parameters["RemoveNA"]))
                    m_RemoveNAs = Convert.ToBoolean(Parameters["RemoveNA"]);
            }

            return successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool TopMostAbundantFunction()
        {
            bool successful = true;

            string rCmd = string.Format(
                "{0} <- cbind({1}, Median=apply({1}, MARGIN=1, FUN={2}, na.rm={3}))\n" +
                "{0} <- {0}[order({0}[,'Median'], decreasing=T),]\n" +
                "{0} <- {0}[,-grep('Median', colnames({0}))]\n" +
                "{0} <- {0}[1:{4},]\n",
                Parameters[RequiredParameters.NewTableName.ToString()],
                Parameters[RequiredParameters.InputTableName.ToString()],
                m_Function,
                m_RemoveNAs.ToString().ToUpper(),
                Parameters[RequiredParameters.NumberOfMostAbundant.ToString()]);

            try
            {
                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered: " +
                    ex.ToString(),
                    ModuleName, StepNumber);
                SaveCurrentREnvironment();
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
        #endregion
    }
}
