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
    /// Module for summarizing missed cleavage events in peptide identifications
    /// </summary>
    public class clsMissedCleavageAssessor : clsBaseDataModule
    {
        private string s_RInstance, s_Current_R_Statement = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        private Dictionary<int, int> dict_Cleavages = new Dictionary<int, int>();

        #region Constructors
        /// <summary>
        /// Module for summarizing missed cleavage events in peptide identifications
        /// </summary>
        public clsMissedCleavageAssessor()
        {
            ModuleName = "MissedCleavage Module";
        }
        /// <summary>
        /// Module for summarizing missed cleavage events in peptide identifications
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        public clsMissedCleavageAssessor(string InstanceOfR)
        {
            ModuleName = "MissedCleavage Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Module for summarizing missed cleavage events in peptide identifications
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        public clsMissedCleavageAssessor(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "MissedCleavage Module";
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
            traceLog.Info("Running Missed Cleavage Assessor module");

            RunMissedCleavageAssessor();

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
                traceLog.Error("ERROR: MissedCleavage class: 'newTableName': \"" + 
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: MissedCleavage class: 'inputTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        /// <summary>
        /// Performs the Missed Cleavage assessment
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        private void RunMissedCleavageAssessor()
        {
            dsp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                DropAnyExistingTables();

                string s_Command = "";

                if (dsp.FactorColumn.Equals("*"))
                {
                    List<string> l_Columns = clsGenericRCalls.GetColumnNames(
                        s_RInstance, dsp.InputTableName);
                    string s_Columns = clsMiscFunctions.Concatenate(l_Columns, ",", false);
                    s_Command = string.Format(
                         "SELECT {0} FROM {1} GROUP BY {0};",
                         s_Columns,
                         dsp.InputTableName);
                }
                else  // single column
                {
                    s_Command = string.Format(
                         "SELECT {0} FROM {1} GROUP BY {0} " +
                         "HAVING {0} NOT LIKE '';",
                         dsp.FactorColumn,
                         dsp.InputTableName);
                }

                DataTable dt_Peptides = clsSQLiteHandler.GetDataTable(s_Command, 
                    Path.Combine(dsp.WorkDirectory, dsp.InputFileName));

                foreach (DataRow dr in dt_Peptides.Rows)
                {
                    string s_Peptide = "";
                    int i_MissedCleavageCounter = 0;

                    if (dsp.FactorColumn.Equals("*"))
                    {
                        // TODO: Handle the asterisks
                    }
                    else if (dsp.FactorColumn.Equals("#"))
                    {
                        // TODO: Handle the pound
                    }
                    else
                    {
                        s_Peptide = dr[dsp.FactorColumn].ToString();
                        s_Peptide = s_Peptide.Substring(0, s_Peptide.Length - 1); // remove the last residue                        
                    }

                    // count the number of missed cleavages in the peptide (with last residue removed)
                    foreach (char c in s_Peptide)
                    {
                        if (c.ToString().ToUpper().Equals("K") ||
                            c.ToString().ToUpper().Equals("R"))
                        {
                            i_MissedCleavageCounter++;
                        }
                    }

                    // update the dictionary
                    if (dict_Cleavages.ContainsKey(i_MissedCleavageCounter))
                    {
                        dict_Cleavages[i_MissedCleavageCounter]++;
                    }
                    else
                    {
                        dict_Cleavages.Add(i_MissedCleavageCounter, 1);
                    }
                }

                AddDictionaryResultsToRWorkspace();
            }
        }

        /// <summary>
        /// If the table already exists, this function drops the table so the table can be reentered into the database.
        /// </summary>
        private void DropAnyExistingTables()
        {
            if (clsSQLiteHandler.TableExists(dsp.NewTableName, Path.Combine(dsp.WorkDirectory, dsp.InputFileName)))
            {
                string s_Command = string.Format("DROP TABLE {0}",
                    dsp.NewTableName);

                clsSQLiteHandler.RunNonQuery(s_Command, Path.Combine(dsp.WorkDirectory, dsp.InputFileName));
            }
        }

        private void AddDictionaryResultsToRWorkspace()
        {
            DataTable dt_MissedCleavages = new DataTable("MissedCleavages");
            DataColumn dc_MissedEvents = new DataColumn("MissedCleavageEvents");
            dc_MissedEvents.DataType = System.Type.GetType("System.Int32");
            dt_MissedCleavages.Columns.Add(dc_MissedEvents);
            DataColumn dc_Frequency = new DataColumn("Frequency");
            dc_Frequency.DataType = System.Type.GetType("System.Int32");
            dt_MissedCleavages.Columns.Add(dc_Frequency);

            // Order by MissedCleavageEvents
            dict_Cleavages = dict_Cleavages.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            foreach (int i in dict_Cleavages.Keys)
            {
                DataRow myRow = dt_MissedCleavages.NewRow();
                myRow["MissedCleavageEvents"] = i;
                myRow["Frequency"] = dict_Cleavages[i];
                dt_MissedCleavages.Rows.Add(myRow);
            }

            // save to database
            clsSQLiteHandler.WriteDataTableToSQLiteTable(
                Path.Combine(dsp.WorkDirectory, dsp.InputFileName), dt_MissedCleavages, 
                dsp.NewTableName);
            clsGenericRCalls.SetDataFrame(s_RInstance, dt_MissedCleavages, dsp.NewTableName);
        }
        #endregion
    }
}
