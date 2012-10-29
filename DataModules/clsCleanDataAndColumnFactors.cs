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
    /// Class checks to make sure that the dataset names in the data table 
    /// are also in the T_Column_Metadata table factor. If there are extra
    /// factors in the T_Column_Metadata table or extra datasets that do not 
    /// contain factors, these are removed from both tables.
    /// </summary>
    public class clsCleanDataAndColumnFactors : clsBaseDataModule
    {
        #region Members
        private string s_RInstance, s_Current_R_Statement = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");        
        #endregion

        #region Constructors
        /// <summary>
        /// Module Cleans DataTable and Column Metadata Table.
        /// </summary>
        public clsCleanDataAndColumnFactors()
        {
            ModuleName = "Clean Data and Column Factors Module";
        }
        /// <summary>
        /// Module Cleans DataTable and Column Metadata Table.
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        public clsCleanDataAndColumnFactors(string InstanceOfR)
        {
            ModuleName = "Clean Data and Column Factors Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Module Cleans DataTable and Column Metadata Table.
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        public clsCleanDataAndColumnFactors(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Clean Data and Column Factors Module";
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

                CleanTables();

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
            if (string.IsNullOrEmpty(dsp.InputTableName))
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Clean Data and Column Factors class: No 'inputTableName': \"" +
                    "was passed in with the parameters");
                b_2Pass = false;
            }
            else if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.InputTableName))
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Clean Data and Column Factors class: 'inputTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            if (string.IsNullOrEmpty(dsp.FactorTable))
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Clean Data and Column Factors class: No 'factorTable': \"" +
                    "was passed in with the parameters");
                b_2Pass = false;
            }
            else if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.FactorTable))
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Clean Data and Column Factors class: 'factorTable': \"" +
                    dsp.FactorTable + "\", was not found in the passed parameters");
                b_2Pass = false;              
            }

            if (string.IsNullOrEmpty(dsp.FactorColumn))
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Clean Data and Column Factors class: 'factorColumn': \"" +
                    "was passed in with the parameters");
                b_2Pass = false;
            }
            else if (!clsGenericRCalls.TableContainsColumn(s_RInstance, 
                dsp.FactorTable, dsp.FactorColumn))
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Clean Data and Column Factors class: 'factorTable': \"" +
                    dsp.FactorTable + "\" does not contain the factorColumn: \"" +
                    dsp.FactorColumn + "\"");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        /// <summary>
        /// Performs the Clean
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        private void CleanTables()
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
                            dsp.FactorTable, dsp.FactorColumn, Model.StepNumber,
                            Model.NumberOfModules, "Alias");

                        Model.SuccessRunningPipeline = 
                            AreDataColLengthAndColumnMetadataRowsEqual(
                            s_RInstance, dsp.InputTableName,
                            dsp.FactorTable, dsp.FactorColumn);
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if the number of datasets in the DataTable
        /// are the same number as in the T_Column_Metadata table. If
        /// they are the same, it checks that the dataset names are 
        /// represented in the T_Column_Metadata table.
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R Workspace</param>
        /// <param name="DataTableName">Name of the table containing the main data</param>
        /// <param name="ColumnMetadataTableName">Name of the Column Metadata table</param>
        /// <param name="ColumnMetadataFactor">Column in the Column Metadata table that maps to the datasets</param>
        /// <returns></returns>
        public static bool AreDataColLengthAndColumnMetadataRowsEqual(
            string InstanceOfR,
            string DataTableName, string ColumnMetadataTableName,
            string ColumnMetadataFactor)
        {
            bool b_Return = false;

            List<int> l_ColumnMetadataDim =
                clsGenericRCalls.GetDimensions(InstanceOfR,
                DataTableName);

            List<int> l_DataDim =
                clsGenericRCalls.GetDimensions(InstanceOfR,
                ColumnMetadataTableName);

            if (l_ColumnMetadataDim[1] != l_DataDim[0])
            {
                b_Return = ModifyFactorAndDataTables(InstanceOfR,
                        DataTableName, ColumnMetadataTableName,
                        ColumnMetadataFactor);
            }
            else
            {
                string s_Command = string.Format(
                    "dim({0}[-which({0}${1}%in%colnames({2})),])[1] == 0",
                    ColumnMetadataTableName,
                    ColumnMetadataFactor,
                    DataTableName);

                if (clsGenericRCalls.AssessBoolean(
                    InstanceOfR, s_Command))
                {
                    // Equal so good to go
                    return true;
                }
                else
                {
                    b_Return = ModifyFactorAndDataTables(InstanceOfR,
                        DataTableName, ColumnMetadataTableName,
                        ColumnMetadataFactor);
                }
            }

            return b_Return;
        }

        /// <summary>
        /// Removes rows in the Column Metadata table that are not in the data table, and
        /// removes columns in the data table that do not have factor information.
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R Workspace</param>
        /// <param name="DataTableName">Name of the table containing the main data</param>
        /// <param name="ColumnMetadataTableName">Name of the Column Metadata table</param>
        /// <param name="ColumnMetadataFactor">Column in the Column Metadata table that maps to the datasets</param>
        /// <returns></returns>
        public static bool ModifyFactorAndDataTables(string InstanceOfR, 
            string DataTableName, string ColumnMetadataTableName,
            string ColumnMetadataFactor)
        {
            bool b_Return = true;
            string s_Command = string.Format(
                            "{0} <- merge(x=cbind({1}=colnames({2}))," +
                            "y={0}, by.x=\"{1}\", by.y=\"{1}\"," +
                            "all.y=F, all.x=F)",
                            ColumnMetadataTableName,
                            ColumnMetadataFactor,
                            DataTableName);

            if (b_Return)
            {
                b_Return = clsGenericRCalls.Run(s_Command,
                    InstanceOfR, "Consolidating Factors Table",
                    null, null);
            }

            s_Command = string.Format(
                "{0} <- {0}[,which(colnames({0})%in%{1}${2})]",
                DataTableName,
                ColumnMetadataTableName,
                ColumnMetadataFactor);

            if (b_Return)
            {
                b_Return = clsGenericRCalls.Run(s_Command,
                    InstanceOfR, "Consolidating Data to match Factors",
                    null, null);
            }

            return b_Return;
        }
        #endregion
    }
}
