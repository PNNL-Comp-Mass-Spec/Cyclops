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

using log4net;
using RDotNet;

namespace Cyclops.DataModules
{
    /// <summary>
    /// This class counts the number of distinct 'columnNames' in 'inputTableName' within a given SQLite database
    /// and adds that number to 'newTableName' with 'variable'.
    /// Input Parameters:
    /// inputTableName:     Name of the table in the database that you want to query
    /// newTableName:       Name of the table in the database that will be updated
    /// columnName:         Name of the column in the inputTableName that will be counted
    /// variable:           Name of the variable to add to the newTableName
    /// </summary>
    public class clsSQLiteSummaryTableGenerator : clsBaseDataModule
    {
        #region Variables
        private string s_RInstance, s_Current_R_Statement = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Contructors
        /// <summary>
        /// Generates a summary table in the SQLite database summarizing the analysis progression
        /// </summary>
        public clsSQLiteSummaryTableGenerator()
        {
            ModuleName = "SQLite Summary Table Generator Module";
        }
        /// <summary>
        /// Generates a summary table in the SQLite database summarizing the analysis progression
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsSQLiteSummaryTableGenerator(string InstanceOfR)
        {
            ModuleName = "SQLite Summary Table Generator Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Generates a summary table in the SQLite database summarizing the analysis progression
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsSQLiteSummaryTableGenerator(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "SQLite Summary Table Generator Module";
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

                int i_Count = GetCount();
                UpdateSummaryTable(i_Count);

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
                traceLog.Error("ERROR: SQLite Summary Table Generator class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: SQLite Summary Table Generator class: 'inputTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            // Check that the inputTableName contains the specified column
            DataTable dt = clsSQLiteHandler.GetDataTable(
                string.Format("SELECT * FROM {0} LIMIT 1", dsp.InputTableName),
                Path.Combine(dsp.WorkDirectory, dsp.InputFileName));
            if (!dt.Columns.Contains(dsp.ColumnName))
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: SQLite Summary Table Generator class: " +
                    dsp.InputTableName + " table does not contain the column: " + dsp.ColumnName);
                b_2Pass = false;
            }

            return b_2Pass;
        }

        private int GetCount()
        {
            dsp.GetParameters(ModuleName, Parameters);

            int i_Count = 0;
            if (CheckPassedParameters())
            {
                try
                {
                    // TableName
                    string s_Command = string.Format(
                        "SELECT COUNT({0}) As Cnt FROM (SELECT {0} FROM {1} GROUP BY {0});",
                        dsp.ColumnName,
                        dsp.InputTableName);


                    traceLog.Info("Performing Summary Count: " + s_Command);
                    DataTable dt = clsSQLiteHandler.GetDataTable(s_Command,
                        Path.Combine(dsp.WorkDirectory, dsp.InputFileName));

                    if (dt.Rows.Count == 1)
                        i_Count = Int32.Parse(dt.Rows[0][0].ToString());
                }
                catch (Exception exc)
                {
                    traceLog.Error("ERROR Summary Count: " + exc.ToString());
                    Model.SuccessRunningPipeline = false;
                }
                return i_Count;
            }

            return -1;
        }

        private void UpdateSummaryTable(int Count)
        {
            try
            {
                DataTable dt = clsSQLiteHandler.GetDataTable(
                    string.Format("SELECT * FROM {0} LIMIT 256",
                    dsp.NewTableName),
                    Path.Combine(dsp.WorkDirectory, dsp.InputFileName));

                string s_ColumnHeaders = "";
                foreach (DataColumn c in dt.Columns)
                {
                    s_ColumnHeaders += c.ColumnName + ",";
                }
                s_ColumnHeaders = s_ColumnHeaders.Substring(0, s_ColumnHeaders.Length - 1); // remove the last comma

                string s_Command = string.Format(
                    "INSERT INTO {0} ({1}) VALUES (NULL, \"{2}\", {3});",
                    dsp.NewTableName,
                    s_ColumnHeaders,
                    dsp.Variable,
                    Count);


                traceLog.Info("Updating Summary Table in Summary Table Generator: " + s_Command);
                clsSQLiteHandler.RunNonQuery(s_Command, Path.Combine(dsp.WorkDirectory, dsp.InputFileName));
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR Updating table in Summary Table Generator: " + exc.ToString());
                Model.SuccessRunningPipeline = false;
            }
        }
        #endregion
    }
}
