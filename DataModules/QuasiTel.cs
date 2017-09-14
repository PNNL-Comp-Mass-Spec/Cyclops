/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: joseph.brown@pnnl.gov
 * Website: http://omics.pnl.gov/software
 * -----------------------------------------------------
 *
 * Notice: This computer software was prepared by Battelle Memorial Institute,
 * hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the
 * Department of Energy (DOE).  All rights in the computer software are reserved
 * by DOE on behalf of the United States Government and the Contractor as
 * provided in the Contract.
 *
 * NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY WARRANTY, EXPRESS OR
 * IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS SOFTWARE.
 *
 * This notice including this sentence must appear on any copies of this computer
 * software.
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace Cyclops.DataModules
{
    public class QuasiTel : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "QuasiTel",
            m_Description = "";
        /// <summary>
        /// Required parameters to run QuasiTel Module
        /// </summary>
        private enum RequiredParameters
        { NewTableName, InputTableName, Fixed_Effect, FactorTable
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
        public QuasiTel(CyclopsModel CyclopsModel,
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
                    Model.PipelineCurrentlySuccessful = QuasiTelFunction();
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
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool QuasiTelFunction()
        {
            bool b_Successful = true;

            string s_TmpDataTable = GetTemporaryTableName("tmpQuasitelData_"),
                s_TmpFactorTable = GetTemporaryTableName("tmpQuasitelFactor_"),
                s_TmpFactor1 = GetTemporaryTableName("tmpQuasiFactor1_"),
                s_TmpFactor2 = GetTemporaryTableName("tmpQuasiFactor2_"),
                s_FactorComplete =
                    Parameters[RequiredParameters.FactorTable.ToString()] +
                    "[,\"" +
                    Parameters[RequiredParameters.Fixed_Effect.ToString()] + "\"]",
                s_TmpInputTableName = Parameters[RequiredParameters.InputTableName.ToString()];

            try
            {
                string Command = "";

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

                b_Successful = Model.RCalls.Run(Command, ModuleName, StepNumber);

                List<string> l_Factors = Model.RCalls.GetColumnNames(
                    s_TmpInputTableName,
                    true);
                int i_FactorCnt = Model.RCalls.GetLengthOfVector(
                    s_FactorComplete);
                if (l_Factors.Count == i_FactorCnt && b_Successful)
                {
                    // setup the pairwise comparisons
                    for (int i = 0; i < l_Factors.Count - 1; i++)
                    {
                        for (int j = 1; j < l_Factors.Count; j++)
                        {
                            string s_ComparisonTableName = "QuasiTel_" +
                                    l_Factors[i] + "_v_" + l_Factors[j];

                            // grab the variables
                            Command = string.Format(
                                "{0} <- as.vector(unlist(subset({1}, " +
                                "{2} == '{3}' | {2} == '{4}', " +
                                "select=c('Alias'))))\n",
                                s_TmpFactorTable,
                                Parameters[RequiredParameters.FactorTable.ToString()],
                                Parameters[RequiredParameters.Fixed_Effect.ToString()],
                                l_Factors[i],
                                l_Factors[j]);
                            // grab the relevant data
                            Command += string.Format(
                                "{0} <- {1}[,which(colnames({1}) %in% {2})]\n",
                                s_TmpDataTable,
                                s_TmpInputTableName,
                                s_TmpFactorTable);
                            // 0 out the null values
                            Command += string.Format(
                                "{0} <- data.matrix({0})\n" +
                                "{0}[is.na({0})] <- 0\n",
                                s_TmpDataTable);
                            // get the column names to pass in as factors
                            Command += string.Format(
                                "{0} <- as.vector(unlist(subset({1}, " +
                                "{2} == '{3}', select=c('Alias'))))\n",
                                s_TmpFactor1,
                                Parameters[RequiredParameters.FactorTable.ToString()],
                                Parameters[RequiredParameters.Fixed_Effect.ToString()],
                                l_Factors[i]);
                            Command += string.Format(
                                "{0} <- as.vector(unlist(subset({1}, " +
                                "{2} == '{3}', select=c('Alias'))))\n",
                                s_TmpFactor2,
                                Parameters[RequiredParameters.FactorTable.ToString()],
                                Parameters[RequiredParameters.Fixed_Effect.ToString()],
                                l_Factors[j]);
                            // run the analysis
                            Command += string.Format(
                                "{0} <- quasitel({1}, {2}, {3})\n",
                                s_ComparisonTableName,
                                s_TmpDataTable,
                                s_TmpFactor1,
                                s_TmpFactor2);
                            // remove temp tables
                            Command += string.Format(
                                "rm({0})\nrm({1})\n" +
                                "rm({2})\nrm({3})\n",
                                s_TmpDataTable,
                                s_TmpFactorTable,
                                s_TmpFactor1,
                                s_TmpFactor2);

                            b_Successful = Model.RCalls.Run(
                                Command, ModuleName, StepNumber);
                        }
                    }
                }
                else
                {
                    Model.LogError(string.Format(
                            "ERROR QuasiTel class: Dimensions of spectral count table ({0}) " +
                            "do not match the dimensions of your factor vector ({1})",
                            l_Factors.Count,
                            i_FactorCnt));
                    SaveCurrentREnvironment();
                    b_Successful = false;
                }
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while performing QuasiTel:\n" +
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
