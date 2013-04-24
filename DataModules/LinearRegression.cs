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
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Cyclops.DataModules
{
    public class LinearRegression : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "LinearRegression";
        /// <summary>
        /// Required parameters to run LinearRegression Module
        /// </summary>
        private enum RequiredParameters
        { 
            NewTableName, InputTableName, FactorTable, ConsolidationFactor, Variable
        }
        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an LinearRegression Module
        /// </summary>
        public LinearRegression()
        {
            ModuleName = m_ModuleName;
        }

        /// <summary>
        /// LinearRegression module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public LinearRegression(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
        }

        /// <summary>
        /// LinearRegression module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public LinearRegression(CyclopsModel CyclopsModel,
            Dictionary<string, string> ExportParameters)
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
        public override bool PerformOperation()
        {
            bool b_Successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running " + ModuleName,
                        ModuleName, StepNumber);

                if (CheckParameters())
                    b_Successful = LinearRegressionFunction();
            }

            return b_Successful;
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

            if (!Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.InputTableName.ToString()]))
            {
                Model.LogWarning("WARNING in Linear Regression: The R environment does " +
                    "not contain the input table, " +
                    Parameters[RequiredParameters.InputTableName.ToString()]);
                b_Successful = false;
            }

            /// Check that the factorTable Exists
            if (!Model.RCalls.ContainsObject(Parameters[RequiredParameters.FactorTable.ToString()]))
            {
                Model.LogError(string.Format("Error encountered in LinearRegression, " +
                    "{0} factor table was not found in the R environment.",
                    Parameters[RequiredParameters.FactorTable.ToString()]),
                    ModuleName, StepNumber);
                b_Successful = false;
            }

            /// Check that the factorTable contains the ConsolidationFactor column
            if (!Model.RCalls.TableContainsColumn(Parameters[RequiredParameters.FactorTable.ToString()],
                Parameters[RequiredParameters.ConsolidationFactor.ToString()]))
            {
                Model.LogError(string.Format("Error encountered in LinearRegression, " +
                    "{0} factor table does not contain the column, {1}!",
                    Parameters[RequiredParameters.FactorTable.ToString()],
                    Parameters[RequiredParameters.ConsolidationFactor.ToString()]),
                    ModuleName, StepNumber);
                b_Successful = false;
            }

            return b_Successful;
        }

        /// <summary>
        /// Performs the linear regression
        /// </summary>
        /// <returns>True, if the linear regression completes successfully</returns>
        public bool LinearRegressionFunction()
        {
            bool b_Successful = true;

            string Command = string.Format("{0} <- LinReg_normalize(" +
                    "x={1}, factorTable={2}, factorCol=\"{3}\", " +
                    "reference={4})",
                    Parameters[RequiredParameters.NewTableName.ToString()],
                    Parameters[RequiredParameters.InputTableName.ToString()],
                    Parameters[RequiredParameters.FactorTable.ToString()],
                    Parameters[RequiredParameters.ConsolidationFactor.ToString()],
                    Parameters[RequiredParameters.Variable.ToString()]);

            try
            {
                b_Successful = Model.RCalls.Run(Command, ModuleName, StepNumber);
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while performing linear regression:\n" +
                    exc.ToString());
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
        #endregion
    }
}
