/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: joseph.brown@pnnl.gov
 * Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
 * -----------------------------------------------------
 *
 * Licensed under the 2-Clause BSD License; you may not use this
 * file except in compliance with the License.  You may obtain
 * a copy of the License at https://opensource.org/licenses/BSD-2-Clause
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Cyclops.DataModules
{
    public class BBM_and_QuasiTel : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "BBM_and_QuasiTel";

        /// <summary>
        /// Required parameters to run BBM_and_QuasiTel Module
        /// </summary>
        private enum RequiredParameters
        {
            NewTableName, InputTableName, FactorTable, Fixed_Effect, Theta
        }

        private string m_MergeColumn = "Alias"; // default value of MergeColumn
        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an BBM_and_QuasiTel Module
        /// </summary>
        public BBM_and_QuasiTel()
        {
            ModuleName = m_ModuleName;
        }

        /// <summary>
        /// BBM_and_QuasiTel module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public BBM_and_QuasiTel(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
        }

        /// <summary>
        /// BBM_and_QuasiTel module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public BBM_and_QuasiTel(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
            Parameters = ExportParameters;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                if (CheckParameters())
                    Model.PipelineCurrentlySuccessful = BBM_and_QuasiTelFunction();
            }


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
                    Parameters[RequiredParameters.InputTableName.ToString()], ModuleName, StepNumber);
                successful = false;
            }
            if (!Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.FactorTable.ToString()]))
            {
                Model.LogError("R Environment does not contain the " +
                    "specified factor table: " +
                    Parameters[RequiredParameters.FactorTable.ToString()], ModuleName, StepNumber);
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
        /// Performs the Beta-Binomial & QuasiTel Model
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool BBM_and_QuasiTelFunction()
        {
            bool successful = true;

            // TODO : Make it work
            string tmpFactorTable = GetTemporaryTableName("T_BBMQuasiFactor_");
            string factorComplete = Parameters[RequiredParameters.FactorTable.ToString()] +
                                      "[,\"" +
                                      Parameters[RequiredParameters.Fixed_Effect.ToString()] + "\"]";
            string tmpInputTableName = Parameters[RequiredParameters.InputTableName.ToString()];

            try
            {
                string rCmdInitialize = "require(BetaBinomial)\n";

                if (Parameters.ContainsKey("removePeptideColumn"))
                {
                    rCmdInitialize += string.Format(
                        "{0}_tmpT <- data.matrix({0}[,2:ncol({0})])\n",
                        Parameters[RequiredParameters.InputTableName.ToString()]);
                    tmpInputTableName = tmpInputTableName + "_tmpT";
                }
                successful = Model.RCalls.Run(rCmdInitialize, ModuleName, StepNumber);

                List<string> factorList = Model.RCalls.GetColumnNames(tmpInputTableName, true);
                int factorCount = Model.RCalls.GetLengthOfVector(factorComplete);
                if (factorList.Count == factorCount && successful)
                {
                    var rCmd = string.Format(
                        "{0} <- jnb_BBM_and_QTel(" +
                        "tData={1}, " +
                        "colMetadata={2}, " +
                        "colFactor='{3}', " +
                        "theta={4}, " +
                        "sinkFileName='')\n" +
                        "rm({2})\n",
                        Parameters[RequiredParameters.NewTableName.ToString()],
                        tmpInputTableName,
                        tmpFactorTable,
                        Parameters[RequiredParameters.Fixed_Effect.ToString()],
                        Parameters[RequiredParameters.Theta.ToString()]);

                    if (Parameters.ContainsKey("removePeptideColumn"))
                    {
                        Command += string.Format("\nrm({0})\n", tmpInputTableName);
                    }

                    successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
                }
                else
                {
                    Model.LogError(string.Format(
                            "ERROR BBM_and_QuasiTel class: Dimensions of spectral count table ({0}) " +
                            "do not match the dimensions of your factor vector ({1})",
                            factorList.Count,
                            factorCount));
                    successful = false;
                }
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while performing BBM and QuasiTel analyses:\n" + exc.ToString(),
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
        #endregion
    }
}
