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
    /// <summary>
    /// Filters tables in the R workspace based on column criteria
    /// </summary>
    public class FilterTable : BaseDataModule
    {
        #region Members
        private readonly string m_ModuleName = "FilterTable";
        private readonly string m_Description = "";

        /// <summary>
        /// Required parameters to run FilterTable Module
        /// </summary>
        private enum RequiredParameters
        {
            InputTableName, NewTableName, ColumnName, Operation, Value
        }

        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an FilterTable Module
        /// </summary>
        public FilterTable()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// FilterTable module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public FilterTable(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// FilterTable module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public FilterTable(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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

                Model.LogMessage("Running FilterTable", ModuleName, StepNumber);

                if (CheckParameters())
                {
                    successful = FilterTableFunction();
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

            if (!Model.RCalls.ContainsObject(
                    Parameters[RequiredParameters.InputTableName.ToString()]))
            {
                Model.LogError("Error R Environment does not contain the " +
                    "input table: " +
                    Parameters[RequiredParameters.InputTableName.ToString()],
                    ModuleName, StepNumber);
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool FilterTableFunction()
        {
            bool successful;

            // ToDo: Validate this code
            var rCmd = string.Format(
                "{0} <- {1}[{1}[,'{2}'] {3} {4},]\n",
                Parameters[RequiredParameters.NewTableName.ToString()],
                Parameters[RequiredParameters.InputTableName.ToString()],
                Parameters[RequiredParameters.ColumnName.ToString()],
                Parameters[RequiredParameters.Operation.ToString()],
                Parameters[RequiredParameters.Value.ToString()]);

            try
            {
                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while filtering " +
                    "table: " + Parameters[RequiredParameters.InputTableName.ToString()] + ": " + ex.Message,
                    ModuleName, StepNumber);
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
