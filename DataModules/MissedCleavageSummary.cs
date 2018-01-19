/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: http://omics.pnl.gov/software
 * -----------------------------------------------------
 *
 * Licensed under the Apache License, Version 2.0; you may not use this
 * file except in compliance with the License.  You may obtain a copy of the
 * License at https://opensource.org/licenses/Apache-2.0
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Cyclops.DataModules
{
    public class MissedCleavageSummary : BaseDataModule
    {
        #region Members
        /// <summary>
        /// Required parameters to run MissedCleavageSummary Module
        /// </summary>
        private enum RequiredParameters 
        { 
            InputTableName, NewTableName, FactorColumn 
        }

        private string m_ModuleName = "MissedCleavageSummary";
        private string m_Description = "";
        private string m_InputFileName = "Results.db3";

        private readonly SQLiteHandler m_SQLiteReader = new SQLiteHandler();

        private Dictionary<int, int> m_Cleavages = new Dictionary<int, int>();
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
            Description = m_Description;
        }

        /// <summary>
        /// MissedCleavageSummary module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public MissedCleavageSummary(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// MissedCleavageSummary module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public MissedCleavageSummary(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
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
            bool successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running " + ModuleName, ModuleName, StepNumber);

                if (CheckParameters())
                    successful = MissedCleavageSummaryFunction();
            }

            return successful;
        }

        /// <summary>
        /// Retrieves a dictionary of all parameters used by the module,
        /// and the corresponding default values
        /// </summary>
        /// <returns>Parameters used by module</returns>
        public override Dictionary<string, string> GetParametersTemplate()
        {
            Dictionary<string, string> paramDictionary = new Dictionary<string, string>();

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                paramDictionary.Add(s, "");
            }

            return paramDictionary;
        }

        /// <summary>
        /// Checks the parameters to ensure that all required keys are present
        /// </summary>
        /// <returns>True, if all required keys are included in the
        /// Parameters</returns>
        public override bool CheckParameters()
        {
            bool successful = true;

            //if (Parameters.ContainsKey(RequiredParameters.InputTableName.ToString()))
            //    m_InputFileName = Parameters[RequiredParameters.InputTableName.ToString()];

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogWarning("Required Field Missing: " + s, ModuleName, StepNumber);
                    successful = false;
                    return successful;
                }
            }

            if (Parameters.ContainsKey("DatabaseFileName"))
                m_InputFileName = Parameters["DatabaseFileName"];

            m_SQLiteReader.DatabaseFileName = Path.Combine(Model.WorkDirectory, m_InputFileName);
            return successful;
        }

        /// <summary>
        /// Main Function to Summarize Missed Cleavages
        /// </summary>
        /// <returns></returns>
        public bool MissedCleavageSummaryFunction()
        {
            bool successful = true;

            successful = DropExistingTable();

            if (successful)
            {
                string sql;

                if (Parameters[RequiredParameters.FactorColumn.ToString()].Equals("*"))
                {
                    List<string> columnNameList =
                        m_SQLiteReader.GetColumnNames(
                        Parameters[RequiredParameters.InputTableName.ToString()]);
                    string columnNames = Utilities.MiscellaneousFunctions.Concatenate(columnNameList, ",", false);

                    sql = string.Format(
                         "SELECT {0} FROM {1} GROUP BY {0};",
                         columnNames,
                         Parameters[RequiredParameters.InputTableName.ToString()]);
                }
                else // single column
                {
                    sql = string.Format(
                         "SELECT {0} FROM {1} GROUP BY {0} " +
                         "HAVING {0} NOT LIKE '';",
                         Parameters[RequiredParameters.FactorColumn.ToString()],
                         Parameters[RequiredParameters.InputTableName.ToString()]);
                }

                DataTable peptides = m_SQLiteReader.SelectTable(sql);

                foreach (DataRow dr in peptides.Rows)
                {
                    string peptide = "";
                    int missedCleavageCounter = 0;

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
                        peptide = dr[Parameters[
                        RequiredParameters.FactorColumn.ToString()]].ToString();
                        peptide = peptide.Substring(0, peptide.Length - 1); // remove the last residue
                    }

                    // count the number of missed cleavages in the peptide (with last residue removed)
                    foreach (char residue in peptide)
                    {
                        if (residue.ToString().ToUpper().Equals("K") ||
                            residue.ToString().ToUpper().Equals("R"))
                        {
                            missedCleavageCounter++;
                        }
                    }

                    // update the dictionary
                    if (m_Cleavages.ContainsKey(missedCleavageCounter))
                    {
                        m_Cleavages[missedCleavageCounter]++;
                    }
                    else
                    {
                        m_Cleavages.Add(missedCleavageCounter, 1);
                    }
                }

                successful = AddDictionaryResultsToRWorkspace();
            }

            return successful;
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
        /// Retrieves the Type Description for automatically
        /// registering the module assembly
        /// </summary>
        /// <returns>Module's Description</returns>
        protected override string GetTypeDescription()
        {
            return Description;
        }

        /// <summary>
        /// If the table already exists, this function drops the table so the table can be reentered into the database.
        /// </summary>
        /// <returns>True, if the table is dropped successfully</returns>
        private bool DropExistingTable()
        {
            bool successful = true;

            if (m_SQLiteReader.TableExists(Parameters[RequiredParameters.NewTableName.ToString()]))
            {
                successful = m_SQLiteReader.DropTable(Parameters[RequiredParameters.NewTableName.ToString()]);

            }

            return successful;
        }

        private Dictionary<string, string> GetMissedCleavageTableStructure()
        {
            return new Dictionary<string, string>();
        }

        private bool AddDictionaryResultsToRWorkspace()
        {
            bool successful = true;

            try
            {
                DataTable missedCleavages = new DataTable(Parameters[RequiredParameters.NewTableName.ToString()]);
                DataColumn missedEvents = new DataColumn("MissedCleavageEvents");
                missedEvents.DataType = System.Type.GetType("System.Int32");
                missedCleavages.Columns.Add(missedEvents);
                DataColumn frequency = new DataColumn("Frequency");
                frequency.DataType = System.Type.GetType("System.Int32");
                missedCleavages.Columns.Add(frequency);

                // Order by MissedCleavageEvents
                m_Cleavages = m_Cleavages.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

                foreach (int i in m_Cleavages.Keys)
                {
                    DataRow myRow = missedCleavages.NewRow();
                    myRow["MissedCleavageEvents"] = i;
                    myRow["Frequency"] = m_Cleavages[i];
                    missedCleavages.Rows.Add(myRow);
                }

                // save to database
                successful = m_SQLiteReader.WriteDataTableToDatabase(missedCleavages);

                successful = Model.RCalls.WriteDataTableToR(missedCleavages, missedCleavages.TableName);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while adding Missed Cleavage " +
                    "results to SQLite database and to R environment:\n" +
                    ex.ToString());
                successful = false;
            }

            return successful;
        }
        #endregion
    }
}
