﻿/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/
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
    public class BetaBinomialModel : BaseDataModule
    {
        #region Members
        private readonly string m_ModuleName = "BetaBinomialModel";
        private readonly string m_Description = "";

        /// <summary>
        /// Required parameters to run BetaBinomialModel Module
        /// </summary>
        private enum RequiredParameters
        {
            NewTableName, InputTableName, Theta, Fixed_Effect, FactorTable
        }

        private string m_MergeColumn = "Alias"; // default value of MergeColumn
        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an BetaBinomialModel Module
        /// </summary>
        public BetaBinomialModel()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// BetaBinomialModel module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public BetaBinomialModel(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// BetaBinomialModel module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public BetaBinomialModel(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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
                    successful = BetaBinomialModelFunction();
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
        /// Performs the Beta-Binomial Model
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool BetaBinomialModelFunction()
        {
            bool successful;

            var tmpTable = Model.RCalls.GetTemporaryTableName("tmpBBMTable_");
            var factorComplete = Parameters[RequiredParameters.FactorTable.ToString()] + "[,\"" +
                                      Parameters[RequiredParameters.Fixed_Effect.ToString()] + "\"]";
            var tmpInputTableName = Parameters[RequiredParameters.InputTableName.ToString()];

            try
            {
                var rCmd = "require(BetaBinomial)\n";

                if (Parameters.ContainsKey("removePeptideColumn"))
                {
                    rCmd += string.Format(
                        "{0}_tmpT <- data.matrix({0}[,2:ncol({0})])\n",
                        Parameters[RequiredParameters.InputTableName.ToString()]);
                    tmpInputTableName = tmpInputTableName + "_tmpT";
                }

                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);

                GetOrganizedFactorsVector(
                    tmpInputTableName,
                    Parameters[RequiredParameters.FactorTable.ToString()],
                    Parameters[RequiredParameters.Fixed_Effect.ToString()],
                    StepNumber,
                    m_MergeColumn,
                    "tmp_OrgFactor4BBM_");

                var factorList = Model.RCalls.GetColumnNames(tmpInputTableName, true);
                var factorCount = Model.RCalls.GetLengthOfVector(factorComplete);
                if (factorList.Count == factorCount && successful)
                {
                    rCmd = string.Format(
                        "{1} <- data.matrix({1})\n" +
                        "{1}[is.na({1})] <- 0\n" +
                        "sink('')\n" +
                        // ReSharper disable StringLiteralTypo
                        "{4} <- largescale.bb.test({1}, {2}, " +
                        // ReSharper restore StringLiteralTypo
                        "theta.equal={3})\n" +
                        "sink()\n" +
                        "{0} <- cbind('pValue'={4}, {1})\n" +
                        "colnames({0})[1] <- 'pValue'\n" +
                        "rm({4})\n",
                        Parameters[RequiredParameters.NewTableName.ToString()],
                        tmpInputTableName,
                        factorComplete,
                        Parameters[RequiredParameters.Theta.ToString()],
                        tmpTable);

                    if (Parameters.ContainsKey("removePeptideColumn"))
                        rCmd += string.Format("rm({0})\n", tmpInputTableName);

                    successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
                }
                else
                {
                    Model.LogError(string.Format(
                            "ERROR BBM class: Dimensions of spectral count table ({0}) " +
                            "do not match the dimensions of your factor vector ({1})",
                            factorList.Count,
                            factorCount));
                    SaveCurrentREnvironment();
                    successful = false;
                }
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while performing Beta-Binomial Model:\n" +
                    ex, ModuleName, StepNumber);
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
