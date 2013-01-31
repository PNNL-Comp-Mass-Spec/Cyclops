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
using System.Linq;
using System.Text;

using RDotNet;
using log4net;

namespace Cyclops.DataModules
{
    /// <summary>
    /// Parameters:
    /// inputTableName: table to perform linear regression on
    /// newTableName: new table name
    /// factorTable: name of the column factor table
    /// variable: '1', '2', '3' representing 1) first dataset, 2) median, and 3) dataset with least missing data
    /// 
    /// uses the consolidation factor for dataset replicates
    /// </summary>
    public class clsLinearRegression : clsBaseDataModule
    {
        #region Members
        private string s_RInstance;
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Contructors
        /// <summary>
        /// Module to perform linear regression on datasets
        /// </summary>
        public clsLinearRegression()
        {
            ModuleName = "Linear Regression Module";
        }
        /// <summary>
        /// Module to perform linear regression on datasets
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsLinearRegression(string InstanceOfR)
        {
            ModuleName = "Linear Regression Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Module to perform linear regression on datasets
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsLinearRegression(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Linear Regression Module";
            Model = TheCyclopsModel;            
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        public override void PerformOperation()
        {
            if (Model.SuccessRunningPipeline)
            {
                Model.IncrementStep(ModuleName);

                RegressData();

                RunChildModules();
            }
        }

        /// <summary>
        /// Checks the dictionary to ensure all the necessary parameters are present
        /// </summary>
        /// <returns>True if all necessary parameters are present</returns>
        protected bool CheckPassedParameters()
        {
            bool b_2Pass = true;

            // NECESSARY PARAMETERS
            if (!dsp.HasNewTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR Linear Regression class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR Linear Regression class: 'inputTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.FactorTable))
            {

            }            
            if (string.IsNullOrEmpty(dsp.FactorTable))
            {
                b_2Pass = false;
                traceLog.Error("ERROR Linear Regression class: 'factorTable' is null or empty. " +
                    "Linear Regression will not be performed on the datasets.");
                return b_2Pass;
            }
            if (string.IsNullOrEmpty(dsp.ConsolidationFactor))
            {
                b_2Pass = false;
                traceLog.Error("ERROR Linear Regression class: 'factorColumn'/'Consolidation_Factor' " +
                    " was not passed in. Linear Regression will not be performed on the datasets.");
                return b_2Pass;
            }
            if (!clsGenericRCalls.TableContainsColumn(s_RInstance, dsp.FactorTable, dsp.ConsolidationFactor))
            {
                b_2Pass = false;
                traceLog.Error("ERROR Linear Regression class: 'factorTable' (" +
                    dsp.FactorTable + ") was not found in R workspace! " +
                    "Linear Regression will not be performed on the datasets.");
                return b_2Pass;
            }

            return b_2Pass;
        }

        private void RegressData()
        {
            dsp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                string s_RStatement = "";

                s_RStatement += string.Format("{0} <- LinReg_normalize(" +
                    "x={1}, factorTable={2}, factorCol=\"{3}\", " +
                    "reference={4})",
                    dsp.NewTableName,
                    dsp.InputTableName,
                    dsp.FactorTable,
                    dsp.ConsolidationFactor,
                    dsp.Variable);

                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "Linear Regression on Datasets",
                    this.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;
            }
        }

        #endregion
    }
}
