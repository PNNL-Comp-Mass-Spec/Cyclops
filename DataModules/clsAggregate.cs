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
//using RDotNet;

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
            if (Model.SuccessRunningPipeline)
            {
                Model.IncrementStep(ModuleName);

                AggregateData();

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
            if (!string.IsNullOrEmpty(dsp.FactorColumn))
            {
                if (!dsp.FactorComplete.EndsWith(dsp.FactorColumn))
                    dsp.FactorComplete = dsp.FactorColumn;
            }
            else if (!string.IsNullOrEmpty(dsp.FactorTable) &&
                !string.IsNullOrEmpty(dsp.ConsolidationFactor))
            {
                dsp.FactorComplete = dsp.FactorTable + "$" + dsp.ConsolidationFactor;                
            }             
            else
            {
                /// Unable to set FactorComplete -> Fail
                traceLog.Error("ERROR: Aggregation class: both Consolidation Factor " +
                    "and FactorColumn were null or empty!");
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
                        List<int> l_DimData = clsGenericRCalls.GetDimensions(s_RInstance, dsp.InputTableName);
                        int i_LengthOfFactor = clsGenericRCalls.GetLengthOfVector(s_RInstance, dsp.FactorComplete);
                        List<string> l_LevelsOfFactor = clsGenericRCalls.GetFactorLevels(s_RInstance, dsp.FactorComplete);

                        string s_RStatement = "";

                        if (Convert.ToInt16(dsp.Margin) == 1)
                        {
                            if (string.IsNullOrEmpty(dsp.X_Link))
                            {
                                traceLog.Error("ERROR: Aggregate class: " +
                                    "Aggregating by rows, and 'xLink' was not " +
                                    "passed in with parameters");
                                Model.SuccessRunningPipeline = false;
                                return;
                            }

                            if (l_DimData[0] == i_LengthOfFactor)
                            {
                                s_RStatement = string.Format(
                                    "{0} <- jnb_Aggregate(x=data.matrix({1}{2}), " +
                                    "myFactor={3}, MARGIN={4}, FUN={5}, MergeLink='{6}')",
                                    dsp.NewTableName,
                                    dsp.SkipTheFirstColumn.ToLower().Equals("true") ? "[,-1]" : "",
                                    dsp.InputTableName,
                                    dsp.FactorComplete,
                                    dsp.Margin,  // '1' indicates rows, '2' indicates columns
                                    dsp.Function,
                                    dsp.X_Link);
                            }
                        }
                        else if (Convert.ToInt16(dsp.Margin) == 2)
                        {
                            string s_TmpColTable = GetOrganizedFactorsVector(s_RInstance, 
                                dsp.InputTableName, dsp.FactorTable, dsp.FactorColumn, 
                                this.StepNumber, Model.NumberOfModules, "Alias");

                            /// Readdress the length of the factor
                            i_LengthOfFactor = clsGenericRCalls.GetLengthOfVector(s_RInstance,
                                s_TmpColTable + "$" + dsp.FactorColumn);

                            if (l_DimData[1] == i_LengthOfFactor)
                            {
                                s_RStatement = string.Format(
                                    "{0} <- jnb_Aggregate(x=data.matrix({1}{2}), " +
                                    "myFactor={3}, MARGIN={4}, FUN={5})\n" +
                                    "rm({6})",
                                    dsp.NewTableName,
                                    dsp.SkipTheFirstColumn.ToLower().Equals("true") ? "[,-1]" : "",
                                    dsp.InputTableName,
                                    s_TmpColTable + "$" + dsp.FactorColumn,
                                    //dsp.FactorComplete,
                                    dsp.Margin,  // '1' indicates rows, '2' indicates columns
                                    dsp.Function,
                                    s_TmpColTable);
                            }
                            else if (l_DimData[1] == l_LevelsOfFactor.Count)
                            {
                                s_RStatement = string.Format(
                                    "{0} <- jnb_Aggregate(x=data.matrix({1}{2}), " +
                                    "myFactor=levels({3}), MARGIN={4}, FUN={5})\n" +
                                    "rm({6})",
                                    dsp.NewTableName,
                                    dsp.SkipTheFirstColumn.ToLower().Equals("true") ? "[,-1]" : "",
                                    dsp.InputTableName,
                                    s_TmpColTable + "$" + dsp.FactorColumn,
                                    //dsp.FactorComplete,
                                    dsp.Margin,  // '1' indicates rows, '2' indicates columns
                                    dsp.Function,
                                    s_TmpColTable);
                            }
                        }

                        Model.SuccessRunningPipeline = clsGenericRCalls.Run(
                            s_RStatement, s_RInstance,
                            "Aggregating Datasets",
                            this.StepNumber,
                            Model.NumberOfModules);
                        
                        // Now make an aggregate Column_Metadata table
                        s_RStatement = string.Format(
                            "Agg_{0} <- as.data.frame(unique(cbind({1}{2}{3})))",
                            dsp.FactorTable,
                            "Alias=" + dsp.FactorComplete,
                            !string.IsNullOrEmpty(dsp.FixedEffect) ?
                            ", " + dsp.FixedEffect + "=" + dsp.FactorTable +
                            "$" + dsp.FixedEffect : "",
                            !string.IsNullOrEmpty(dsp.RandomEffect) ?
                            ", " + dsp.RandomEffect + "=" + dsp.FactorTable +
                            "$" + dsp.RandomEffect : "");
                        
                        Model.SuccessRunningPipeline = clsGenericRCalls.Run(
                            s_RStatement, s_RInstance,
                            "Constructing Aggregated Column Metadata table",
                            this.StepNumber, Model.NumberOfModules);
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
