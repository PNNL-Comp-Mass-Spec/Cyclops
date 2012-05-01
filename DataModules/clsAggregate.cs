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

using log4net;
using RDotNet;

namespace Cyclops.DataModules
{
    /// <summary>
    /// Aggregates Tables based on Columns, ColumnMetadata, Rows, or RowMetadata
    /// </summary>
    public class clsAggregate : clsBaseDataModule
    {
        #region Members
        private string s_RInstance, s_Current_R_Statement = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Constructors
        /// <summary>
        /// Modules Aggregates data by columns or rows.
        /// </summary>
        public clsAggregate()
        {
            ModuleName = "Aggregate Module";
        }
        /// <summary>
        /// Modules Aggregates data by columns or rows.
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        public clsAggregate(string InstanceOfR)
        {
            ModuleName = "Aggregate Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Modules Aggregates data by columns or rows.
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        public clsAggregate(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Aggregate Module";
            Model = TheCyclopsModel;
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            traceLog.Info("Aggregating Datasets...");

            AggregateData();

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
                traceLog.Error("ERROR: Aggregation class: 'newTableName': \"" + 
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Aggregation class: 'inputTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasFactorComplete)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Aggregation class: 'factor': \"" +
                    dsp.FactorComplete + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasMargin)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Aggregation class: 'margin': \"" +
                    dsp.Margin + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasFunction)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Aggregation class: 'function': \"" +
                    dsp.Function + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        /// <summary>
        /// Performs the Aggregation
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        private void AggregateData()
        {
            dsp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                // check that the table exists
                if (clsGenericRCalls.ContainsObject(s_RInstance, dsp.FactorTable))
                {
                    // check that the table has the expected column
                    if (clsGenericRCalls.TableContainsColumn(s_RInstance, dsp.FactorTable,
                        dsp.FactorColumn))
                    {
                        GetOrganizedFactorsVector(s_RInstance, dsp.InputTableName,
                            dsp.FactorTable, dsp.FactorColumn);


                        REngine engine = REngine.GetInstanceFromID(s_RInstance);

                        string s_RStatement = string.Format(
                                "{0} <- jnb_Aggregate(x=data.matrix({1}), " +
                                "myFactor={2}, MARGIN={3}, FUN={4})",
                                dsp.NewTableName,
                                dsp.InputTableName,
                                dsp.FactorComplete,
                                dsp.Margin,
                                dsp.Function);
                        try
                        {
                            traceLog.Info("Aggregating datasets: " + s_RStatement);
                            s_Current_R_Statement = s_RStatement;
                            engine.EagerEvaluate(s_RStatement);
                        }
                        catch (Exception exc)
                        {
                            Model.SuccessRunningPipeline = false;
                            traceLog.Error("ERROR: Aggregating data: " + exc.ToString());
                        }

                    }
                    else
                    {
                        Model.SuccessRunningPipeline = false;
                        traceLog.Error("ERROR: Aggregation class. The factors table does not contain " +
                            "the necessary column: " + dsp.FactorColumn);
                    }
                }
                else
                {
                    Model.SuccessRunningPipeline = false;
                    traceLog.Error("ERROR: Aggregation class. The factors table does not exist: " +
                        dsp.FactorTable);
                }
            }
        }

        /// <summary>
        /// Unit Test for Aggregating data
        /// </summary>
        /// <returns>Information regarding the result of the UnitTest</returns>
        public clsTestResult TestAggregation()
        {
            dsp.GetParameters(ModuleName, Parameters);
            clsTestResult result = new clsTestResult(true, "");
            result.Module = ModuleName;

            try
            {
                if (!CheckPassedParameters())
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR AGGREGATING DATA: Not all required parameters were passed in!";
                    return result;
                }

                AggregateData();

                // Confirm by testing if the new table exists within the environment
                if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.NewTableName))
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR AGGREGATING: After Aggregating " +
                        dsp.InputTableName +
                        ", the new table name could not be found within the R workspace";
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }

                System.Data.DataTable dt = clsGenericRCalls.GetDataTable(s_RInstance, dsp.NewTableName);
                if (dt.Columns.Count != 2)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR AGGREGATING DATA: After aggregating the table, " +
                        "the new table was supposed to have 2 columns, and instead has " +
                        dt.Columns.Count;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
                if (dt.Rows.Count != 94)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR AGGREGATING DATA: After aggregating the table, " +
                        "the new table was supposed to have 94 rows, and instead has " +
                        dt.Rows.Count;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
            }
            catch (Exception exc)
            {
                result.IsSuccessful = false;
                result.Message = "ERROR AGGREGATING: " + dsp.InputFileName + "\n\n" + exc.ToString();
                result.R_Statement = s_Current_R_Statement;
            }

            return result;
        }
        #endregion
    }
}
