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

using PNNLOmics;

namespace Cyclops.DataModules
{
    public class MissedCleavageSummary : BaseDataModule
    {
        #region Members
        /// <summary>
        /// Required parameters to run MissedCleavageSummary Module
        /// </summary>
        private enum RequiredParameters { InputTableName, 
            NewTableName, FactorColumn }

        private string m_ModuleName = "MissedCleavageSummary",
            m_InputFileName = "Results.db3";

        private PNNLOmics.Databases.SQLiteHandler sql = 
            new PNNLOmics.Databases.SQLiteHandler();

        private Dictionary<int, int> dict_Cleavages = new Dictionary<int, int>();
        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an MissedCleavageSummary Module
        /// </summary>
        public MissedCleavageSummary()
        {
            ModuleName = m_ModuleName;
        }

        /// <summary>
        /// MissedCleavageSummary module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public MissedCleavageSummary(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
        }

        /// <summary>
        /// MissedCleavageSummary module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public MissedCleavageSummary(CyclopsModel CyclopsModel,
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
        public override void PerformOperation()
        {
            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running " + ModuleName,
                        ModuleName, StepNumber);

                if (CheckParameters())
                    Model.PipelineCurrentlySuccessful = MissedCleavageSummaryFunction();

                RunChildModules();
            }
        }

        /// <summary>
        /// Checks the parameters to ensure that all required keys are present
        /// </summary>
        /// <returns>True, if all required keys are included in the
        /// Parameters</returns>
        public override bool CheckParameters()
        {
            bool b_Successful = true;

            //if (Parameters.ContainsKey(RequiredParameters.InputTableName.ToString()))
            //    m_InputFileName = Parameters[RequiredParameters.InputTableName.ToString()];
            
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

            if (Parameters.ContainsKey("DatabaseFileName"))
                m_InputFileName = Parameters["DatabaseFileName"];

            sql.DatabaseFileName = Path.Combine(Model.WorkDirectory, m_InputFileName);
            return b_Successful;
        }

        /// <summary>
        /// Main Function to Summarize Missed Cleavages
        /// </summary>
        /// <returns></returns>
        public bool MissedCleavageSummaryFunction()
        {
            bool b_Successful = true;

            b_Successful = DropExistingTable();

            if (b_Successful)
            {
                string s_Command = "";

                if (Parameters[RequiredParameters.FactorColumn.ToString()].Equals("*"))
                {
                    List<string> l_Columns = 
                        sql.GetColumnNames(
                        Parameters[RequiredParameters.InputTableName.ToString()]);
                    string s_Columns = Utilities.MiscellaneousFunctions.Concatenate(l_Columns, ",", false);
                    s_Command = string.Format(
                         "SELECT {0} FROM {1} GROUP BY {0};",
                         s_Columns,
                         Parameters[RequiredParameters.InputTableName.ToString()]);
                }
                else // single column
                {
                    s_Command = string.Format(
                         "SELECT {0} FROM {1} GROUP BY {0} " +
                         "HAVING {0} NOT LIKE '';",
                         Parameters[RequiredParameters.FactorColumn.ToString()],
                         Parameters[RequiredParameters.InputTableName.ToString()]);
                }

                DataTable dt_Peptides = sql.SelectTable(s_Command);

                foreach (DataRow dr in dt_Peptides.Rows)
                {
                    string s_Peptide = "";
                    int i_MissedCleavageCounter = 0;

                    if (Parameters[
                        RequiredParameters.FactorColumn.ToString()].Equals("*"))
                    {
                        // TODO: Handle the asterisks
                    }
                    else if (Parameters[
                        RequiredParameters.FactorColumn.ToString()].Equals("#"))
                    {
                        // TODO: Handle the pound
                    }
                    else
                    {
                        s_Peptide = dr[Parameters[
                        RequiredParameters.FactorColumn.ToString()]].ToString();
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

                b_Successful = AddDictionaryResultsToRWorkspace();
            }

            return b_Successful;
        }

        protected override string GetDefaultValue()
        {
            return "false";
        }

        protected override string GetTypeName()
        {
            return ModuleName;
        }

        /// <summary>
        /// If the table already exists, this function drops the table so the table can be reentered into the database.
        /// </summary>
        /// <returns>True, if the table is dropped successfully</returns>
        private bool DropExistingTable()
        {
            bool b_Successful = true;
            
            if (sql.TableExists(Parameters[RequiredParameters.NewTableName.ToString()]))
            {
                b_Successful = sql.DropTable(Parameters[RequiredParameters.NewTableName.ToString()]);

            }

            return b_Successful;
        }

        private Dictionary<string, string> GetMissedCleavageTableStructure()
        {
            Dictionary<string, string> d_Str = new Dictionary<string, string>();


            return d_Str;
        }

        private bool AddDictionaryResultsToRWorkspace()
        {
            bool b_Successful = true;

            try
            {
                DataTable dt_MissedCleavages = new DataTable(
                    Parameters[RequiredParameters.NewTableName.ToString()]);
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
                b_Successful = sql.WriteDataTableToDatabase(
                   dt_MissedCleavages);

                b_Successful = Model.RCalls.WriteDataTableToR(dt_MissedCleavages,
                    dt_MissedCleavages.TableName);
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while adding Missed Cleavage " +
                    "results to SQLite database and to R environment:\n" +
                    exc.ToString());
                b_Successful = false;
            }

            return b_Successful;
        }
        #endregion
    }
}
