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
    public class LinearRegression : BaseDataModule
    {
        private readonly string m_ModuleName = "LinearRegression";
        private readonly string m_Description = "";

        /// <summary>
        /// Required parameters to run LinearRegression Module
        /// </summary>
        private enum RequiredParameters
        {
            NewTableName, InputTableName, FactorTable, ConsolidationFactor, Variable
        }

        /// <summary>
        /// Generic constructor creating an LinearRegression Module
        /// </summary>
        public LinearRegression()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// LinearRegression module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public LinearRegression(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// LinearRegression module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public LinearRegression(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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
                    successful = LinearRegressionFunction();
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
                    Model.LogWarning("Required Field Missing: " + s, ModuleName, StepNumber);
                    return false;
                }
            }

            if (!Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.InputTableName.ToString()]))
            {
                Model.LogWarning("WARNING in Linear Regression: The R environment does " +
                    "not contain the input table, " +
                    Parameters[RequiredParameters.InputTableName.ToString()]);
                successful = false;
            }

            // Check that the factorTable Exists
            if (!Model.RCalls.ContainsObject(Parameters[RequiredParameters.FactorTable.ToString()]))
            {
                Model.LogError(string.Format("Error encountered in LinearRegression, " +
                    "{0} factor table was not found in the R environment.",
                    Parameters[RequiredParameters.FactorTable.ToString()]),
                    ModuleName, StepNumber);
                successful = false;
            }

            // Check that the factorTable contains the ConsolidationFactor column
            if (!Model.RCalls.TableContainsColumn(Parameters[RequiredParameters.FactorTable.ToString()],
                Parameters[RequiredParameters.ConsolidationFactor.ToString()]))
            {
                Model.LogError(string.Format("Error encountered in LinearRegression, " +
                    "{0} factor table does not contain the column, {1}!",
                    Parameters[RequiredParameters.FactorTable.ToString()],
                    Parameters[RequiredParameters.ConsolidationFactor.ToString()]),
                    ModuleName, StepNumber);
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Performs the linear regression
        /// </summary>
        /// <returns>True, if the linear regression completes successfully</returns>
        public bool LinearRegressionFunction()
        {
            bool successful;

            var rCmd = string.Format("{0} <- LinReg_normalize(" +
                    "x={1}, factorTable={2}, factorCol=\"{3}\", " +
                    "reference={4})",
                    Parameters[RequiredParameters.NewTableName.ToString()],
                    Parameters[RequiredParameters.InputTableName.ToString()],
                    Parameters[RequiredParameters.FactorTable.ToString()],
                    Parameters[RequiredParameters.ConsolidationFactor.ToString()],
                    Parameters[RequiredParameters.Variable.ToString()]);

            try
            {
                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while performing linear regression:\n" + ex);
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
    }
}
