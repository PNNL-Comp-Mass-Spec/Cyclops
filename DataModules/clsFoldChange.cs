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

using RDotNet;
using log4net;

namespace Cyclops.DataModules
{
    public class clsFoldChange : clsBaseDataModule
    {
        private string s_RInstance, s_Current_R_Statement = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();

        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        #region Constructors
        /// <summary>
        /// Calculates the fold-change between factors
        /// </summary>
        public clsFoldChange()
        {
            ModuleName = "Fold-Change Model Module";
        }
        /// <summary>
        /// Calculates the fold-change between factors
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsFoldChange(string InstanceOfR)
        {
            ModuleName = "Fold-Change Model Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Calculates the fold-change between factors
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsFoldChange(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Fold-Change Model Module";
            Model = TheCyclopsModel;
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Members

        #endregion

        #region Properties

        #endregion

        #region Methods
        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            if (CheckPassedParameters())
            {
                CalculateFoldChange();
            }
            
            RunChildModules();
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
                traceLog.Error("FoldChange class: 'newTableName': \"" +
                    dsp.FoldChangeTable + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("FoldChange class: 'inputTableName': \"" + 
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        private void CalculateFoldChange()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            
            string s_RStatement = string.Format(
                "{0} <- jnb_FoldChangeSpectralCountAndPackage({1}, {2})",
                dsp.NewTableName,
                dsp.InputTableName,
                "0");   // column(s) to exclude (e.g. p-values)

            try
            {
                traceLog.Info("Calculating Fold Change: " + s_RStatement);
                engine.EagerEvaluate(s_RStatement);
            }
            catch (Exception exc)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR calculating fold-change: " + exc.ToString());
            }
        }

        /// <summary>
        /// Unit Test for Beta-binomial Model data
        /// </summary>
        /// <returns>Information regarding the result of the UnitTest</returns>
        public clsTestResult TestFoldChange()
        {
            dsp.GetParameters(ModuleName, Parameters);
            clsTestResult result = new clsTestResult(true, "");
            result.Module = ModuleName;

            try
            {
                if (!CheckPassedParameters())
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR FOLD-CHANGE: Not all required parameters were passed in!";
                    return result;
                }

                // input table has 1,162 rows, so trim the table down before running the analysis
                REngine engine = REngine.GetInstanceFromID(s_RInstance);
                engine.EagerEvaluate("t <- " + dsp.InputTableName + "[c(1:4),]");

                dsp.InputTableName = "t";

                CalculateFoldChange();

                // Confirm by testing if the new table exists within the environment
                if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.NewTableName))
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR FOLD-CHANGE: After running fold-change " +
                        dsp.InputTableName +
                        ", the new table name could not be found within the R workspace";
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }

                System.Data.DataTable dt = clsGenericRCalls.GetDataTable(s_RInstance, dsp.NewTableName);
                if (dt.Columns.Count != 3)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR FOLD-CHANGE: After running fold-change, " +
                        "the new table was supposed to have 3 columns, and instead has " +
                        dt.Columns.Count;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
                if (dt.Rows.Count != 4)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR FOLD-CHANGE: After running fold-change, " +
                        "the new table was supposed to have 4 columns, and instead has " +
                        dt.Rows.Count;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
            }
            catch (Exception exc)
            {
                result.IsSuccessful = false;
                result.Message = "ERROR FOLD-CHANGE: " + dsp.InputFileName + "\n\n" + exc.ToString();
                result.R_Statement = s_Current_R_Statement;
            }

            return result;
        }
        #endregion
    }
}
