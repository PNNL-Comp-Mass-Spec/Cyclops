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
    public class QuasiTel : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "QuasiTel";
        private string m_Description = "";
        
        /// <summary>
        /// Required parameters to run QuasiTel Module
        /// </summary>
        private enum RequiredParameters
        { 
            NewTableName, InputTableName, Fixed_Effect, FactorTable
        }

        private string m_MergeColumn = "Alias"; // default value of MergeColumn
        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an QuasiTel Module
        /// </summary>
        public QuasiTel()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// QuasiTel module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public QuasiTel(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// QuasiTel module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public QuasiTel(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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

                Model.LogMessage("Running " + ModuleName, ModuleName, StepNumber);

                if (CheckParameters())
                    Model.PipelineCurrentlySuccessful = QuasiTelFunction();
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
                    Model.LogWarning("Required Field Missing: " + s, ModuleName, StepNumber);
                    successful = false;
                    return successful;
                }
            }

            if (Parameters.ContainsKey("MergeColumn"))
                m_MergeColumn = Parameters["MergeColumn"];

            if (!Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.InputTableName.ToString()]))
            {
                Model.LogError("R Environment does not contain the " +
                    "specified input table: " +
                    Parameters[RequiredParameters.InputTableName.ToString()],
                    ModuleName, StepNumber);
                successful = false;
            }
            if (!Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.FactorTable.ToString()]))
            {
                Model.LogError("R Environment does not contain the " +
                    "specified factor table: " +
                    Parameters[RequiredParameters.FactorTable.ToString()],
                    ModuleName, StepNumber);
                successful = false;
            }
            if (!Model.RCalls.TableContainsColumn(
                Parameters[RequiredParameters.FactorTable.ToString()],
                Parameters[RequiredParameters.Fixed_Effect.ToString()]))
            {
                Model.LogError(string.Format(
                    "Factor table ({0}) does not contain the specified " +
                    "column ({1})",
                    Parameters[RequiredParameters.FactorTable.ToString()],
                    Parameters[RequiredParameters.Fixed_Effect.ToString()]),
                    ModuleName, StepNumber);
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool QuasiTelFunction()
        {
            bool successful = true;

            string tDataTable = GetTemporaryTableName("tmpQuasitelData_");
            string tFactorTable = GetTemporaryTableName("tmpQuasitelFactor_");
            string tFactor1 = GetTemporaryTableName("tmpQuasiFactor1_");
            string tFactor2 = GetTemporaryTableName("tmpQuasiFactor2_");
            string factorComplete = Parameters[RequiredParameters.FactorTable.ToString()] + "[,\"" +
                                      Parameters[RequiredParameters.Fixed_Effect.ToString()] + "\"]";
            string tInputTableName = Parameters[RequiredParameters.InputTableName.ToString()];

            try
            {
                string rCmd = "";

                if (Parameters.ContainsKey("removePeptideColumn"))
                {
                    rCmd += string.Format(
                        "{0}_tmpT <- data.matrix({0}[,2:ncol({0})])\n",
                        Parameters[RequiredParameters.InputTableName.ToString()]);
                    tInputTableName = tInputTableName + "_tmpT";
                }

                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);

                GetOrganizedFactorsVector(
                    tInputTableName,
                    Parameters[RequiredParameters.FactorTable.ToString()],
                    Parameters[RequiredParameters.Fixed_Effect.ToString()],
                    StepNumber,
                    m_MergeColumn,
                    "tmp_OrgFactor4BBM_");

                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);

                List<string> factorList = Model.RCalls.GetColumnNames(tInputTableName, true);
                int factorCount = Model.RCalls.GetLengthOfVector(factorComplete);
                if (factorList.Count == factorCount && successful)
                {
                    // setup the pairwise comparisons
                    for (int i = 0; i < factorList.Count - 1; i++)
                    {
                        for (int j = 1; j < factorList.Count; j++)
                        {
                            string comparisonTableName = "QuasiTel_" +
                                    factorList[i] + "_v_" + factorList[j];

                            // grab the variables
                            rCmd = string.Format(
                                "{0} <- as.vector(unlist(subset({1}, " +
                                "{2} == '{3}' | {2} == '{4}', " +
                                "select=c('Alias'))))\n",
                                tFactorTable,
                                Parameters[RequiredParameters.FactorTable.ToString()],
                                Parameters[RequiredParameters.Fixed_Effect.ToString()],
                                factorList[i],
                                factorList[j]);
                            // grab the relevant data
                            rCmd += string.Format(
                                "{0} <- {1}[,which(colnames({1}) %in% {2})]\n",
                                tDataTable,
                                tInputTableName,
                                tFactorTable);
                            // 0 out the null values
                            rCmd += string.Format(
                                "{0} <- data.matrix({0})\n" +
                                "{0}[is.na({0})] <- 0\n",
                                tDataTable);
                            // get the column names to pass in as factors
                            rCmd += string.Format(
                                "{0} <- as.vector(unlist(subset({1}, " +
                                "{2} == '{3}', select=c('Alias'))))\n",
                                tFactor1,
                                Parameters[RequiredParameters.FactorTable.ToString()],
                                Parameters[RequiredParameters.Fixed_Effect.ToString()],
                                factorList[i]);
                            rCmd += string.Format(
                                "{0} <- as.vector(unlist(subset({1}, " +
                                "{2} == '{3}', select=c('Alias'))))\n",
                                tFactor2,
                                Parameters[RequiredParameters.FactorTable.ToString()],
                                Parameters[RequiredParameters.Fixed_Effect.ToString()],
                                factorList[j]);
                            // run the analysis
                            rCmd += string.Format(
                                "{0} <- quasitel({1}, {2}, {3})\n",
                                comparisonTableName,
                                tDataTable,
                                tFactor1,
                                tFactor2);
                            // remove temp tables
                            rCmd += string.Format(
                                "rm({0})\nrm({1})\n" +
                                "rm({2})\nrm({3})\n",
                                tDataTable,
                                tFactorTable,
                                tFactor1,
                                tFactor2);

                            successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
                        }
                    }
                }
                else
                {
                    Model.LogError(string.Format(
                            "ERROR QuasiTel class: Dimensions of spectral count table ({0}) " +
                            "do not match the dimensions of your factor vector ({1})",
                            factorList.Count,
                            factorCount));
                    SaveCurrentREnvironment();
                    successful = false;
                }
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while performing QuasiTel:\n" +
                    ex.ToString(), ModuleName, StepNumber);
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
