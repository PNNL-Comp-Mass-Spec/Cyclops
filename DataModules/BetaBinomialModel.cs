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
    public class BetaBinomialModel : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "BetaBinomialModel",
            m_Description = "";
        /// <summary>
        /// Required parameters to run BetaBinomialModel Module
        /// </summary>
        private enum RequiredParameters
        { NewTableName, InputTableName, Theta, Fixed_Effect, FactorTable
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
        public BetaBinomialModel(CyclopsModel CyclopsModel,
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
                    b_Successful = BetaBinomialModelFunction();
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
                    Model.LogWarning("Required Field Missing: " + s,
                        ModuleName, StepNumber);
                    b_Successful = false;
                    return b_Successful;
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
                b_Successful = false;
            }
            if (!Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.FactorTable.ToString()]))
            {
                Model.LogError("R Environment does not contain the " +
                    "specified factor table: " +
                    Parameters[RequiredParameters.FactorTable.ToString()],
                    ModuleName, StepNumber);
                b_Successful = false;
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
                b_Successful = false;
            }

            return b_Successful;
        }

        /// <summary>
        /// Performs the Beta-Binomial Model
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool BetaBinomialModelFunction()
        {
            bool b_Successful = true;

            string s_TmpTable = Model.RCalls.GetTemporaryTableName("tmpBBMTable_"),
                s_FactorComplete =
                    Parameters[RequiredParameters.FactorTable.ToString()] +
                    "[,\"" +
                    Parameters[RequiredParameters.Fixed_Effect.ToString()] + "\"]",
                s_TmpInputTableName = Parameters[RequiredParameters.InputTableName.ToString()];

            try
            {
                string Command = "require(BetaBinomial)\n";

                if (Parameters.ContainsKey("removePeptideColumn"))
                {
                    Command += string.Format("{0}_tmpT <- data.matrix({0}[,2:ncol({0})])\n",
                    Parameters[RequiredParameters.InputTableName.ToString()]);
                    s_TmpInputTableName =
                        s_TmpInputTableName +
                        "_tmpT";
                }

                b_Successful = Model.RCalls.Run(Command, ModuleName, StepNumber);

                GetOrganizedFactorsVector(
                    s_TmpInputTableName,
                    Parameters[RequiredParameters.FactorTable.ToString()],
                    Parameters[RequiredParameters.Fixed_Effect.ToString()],
                    StepNumber,
                    m_MergeColumn,
                    "tmp_OrgFactor4BBM_");

                List<string> l_Factors = Model.RCalls.GetColumnNames(
                    s_TmpInputTableName,
                    true);
                int i_FactorCnt = Model.RCalls.GetLengthOfVector(
                    s_FactorComplete);
                if (l_Factors.Count == i_FactorCnt && b_Successful)
                {
                    Command = string.Format(
                        "{1} <- data.matrix({1})\n" +
                        "{1}[is.na({1})] <- 0\n" +
                        "sink('')\n" +
                        "{4} <- largescale.bb.test({1}, {2}, " +
                        "theta.equal={3})\n" +
                        "sink()\n" +
                        "{0} <- cbind('pValue'={4}, {1})\n" +
                        "colnames({0})[1] <- 'pValue'\n" +
                        "rm({4})\n",
                        Parameters[RequiredParameters.NewTableName.ToString()],
                        s_TmpInputTableName,
                        s_FactorComplete,
                        Parameters[RequiredParameters.Theta.ToString()],
                        s_TmpTable);

                    if (Parameters.ContainsKey("removePeptideColumn"))
                        Command += string.Format("rm({0})\n",
                            s_TmpInputTableName);

                    b_Successful = Model.RCalls.Run(Command,
                        ModuleName, StepNumber);
                }
                else
                {
                    Model.LogError(string.Format(
                            "ERROR BBM class: Dimensions of spectral count table ({0}) " +
                            "do not match the dimensions of your factor vector ({1})",
                            l_Factors.Count,
                            i_FactorCnt));
                    SaveCurrentREnvironment();
                    b_Successful = false;
                }
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while performing Beta-Binomial Model:\n" +
                    ex.ToString(), ModuleName, StepNumber);
                SaveCurrentREnvironment();
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
