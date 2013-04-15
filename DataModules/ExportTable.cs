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
    /// <summary>
    /// Module to export data from R or SQLite databases
    /// </summary>
    public class ExportTable : BaseDataModule
    {
        #region Members
        /// <summary>
        /// Required parameters to run MissedCleavageSummary Module
        /// </summary>
        private enum RequiredParameters
        { 
            Source, Target, TableName, FileName, SeparatingCharacter
        }

        private enum DatabaseTargetRequiredParameters
        {
            NewTableName
        }

        private string m_ModuleName = "ExportTable",
            m_DatabaseFileName = "Results.db3";

        private bool m_DatabaseFound = false;

        private PNNLOmics.Databases.SQLiteHandler sql = new PNNLOmics.Databases.SQLiteHandler();
        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an ExportTable Module
        /// </summary>
        public ExportTable()
        {
            ModuleName = m_ModuleName;
        }

        /// <summary>
        /// ExportTable module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public ExportTable(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
        }

        /// <summary>
        /// ExportTable module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public ExportTable(CyclopsModel CyclopsModel,
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
                    Model.PipelineCurrentlySuccessful = ExportFunction();

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
            {
                if (File.Exists(Parameters["DatabaseFileName"]))
                {
                    m_DatabaseFileName = Parameters["DatabaseFileName"];
                    sql.DatabaseFileName = Parameters["DatabaseFileName"];
                    m_DatabaseFound = true;
                }
                else if (File.Exists(Path.Combine(Model.WorkDirectory,
                    Parameters["DatabaseFileName"])))
                {
                    m_DatabaseFileName = Parameters["DatabaseFileName"];
                    sql.DatabaseFileName = Path.Combine(Model.WorkDirectory,
                    Parameters["DatabaseFileName"]);
                    m_DatabaseFound = true;
                }
            }
            else
            {
                if (File.Exists(Path.Combine(Model.WorkDirectory,
                    "Results.db3")))
                {
                    sql.DatabaseFileName = Path.Combine(Model.WorkDirectory,
                    "Results.db3");
                    m_DatabaseFound = true;
                }
            }

            if (!m_DatabaseFound)
            {
                Model.LogError("Unable to establish successful database connection!",
                    ModuleName, StepNumber);
                b_Successful = false;
            }

            return b_Successful;
        }

        /// <summary>
        /// Checks that fields specific to database handling are present 
        /// in the passed parameters
        /// </summary>
        /// <returns>True, if the parameters contain database specific params</returns>
        public bool CheckDatabaseTargetParameters()
        {
            bool b_Successful = true;

            foreach (string s in Enum.GetNames(typeof(DatabaseTargetRequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogError("Required Database Field Missing: " + s,
                        ModuleName, StepNumber);
                    b_Successful = false;
                    return b_Successful;
                }
            }

            return b_Successful;
        }

        /// <summary>
        /// Main Export Function
        /// </summary>
        /// <returns>True, if the export is successful</returns>
        public bool ExportFunction()
        {
            bool b_Successful = true;

            switch (Parameters[RequiredParameters.Source.ToString()].ToUpper())
            {
                case "R":
                    if (Model.RCalls.ContainsObject(
                        Parameters[RequiredParameters.TableName.ToString()]))
                        return ExportFromR();
                    else
                    {
                        Model.LogWarning(string.Format("Warning encountered while " +
                            "attempting to export table, {0}, from R workspace. " +
                            "The table does not exist in the R environment!",
                            Parameters[RequiredParameters.TableName.ToString()]),
                            ModuleName, StepNumber);
                    }
                    break;
                case "SQLITE":
                    if (m_DatabaseFound)
                    {
                        if (sql.TableExists(Parameters[RequiredParameters.TableName.ToString()]))
                            return ExportFromSQLite();
                        else
                        {
                            Model.LogWarning(string.Format(
                                "Warning Table, {0}, was not found in the SQLite database, {1}, for " +
                                "export!",
                                Parameters[RequiredParameters.TableName.ToString()],
                                m_DatabaseFileName),
                                ModuleName, StepNumber);
                        }
                    }
                    else
                    {
                        Model.LogError("Unable to find SQLite database: " +
                            Path.Combine(Model.WorkDirectory,
                            m_DatabaseFileName),
                            ModuleName, StepNumber);
                    }
                    break;
            }

            return b_Successful;
        }

        /// <summary>
        /// Generic method to export data from R
        /// </summary>
        /// <returns>True, if the export is successful</returns>
        private bool ExportFromR()
        {
            bool b_Successful = true;

            /// TODO : Export from R
            switch (Parameters[RequiredParameters.Target.ToString()].ToUpper())
            {
                case "SQLITE":
                    if (CheckDatabaseTargetParameters())
                        b_Successful = ExportR_to_SQLite();
                    else
                    {
                        Model.LogError("Not all required fields for handling a " +
                            "database were present. Exiting Export Module.",
                            ModuleName, StepNumber);
                        return false;
                    }
                    break;
                case "CSV":
                    b_Successful = ExportR_to_Text(",");
                    break;
                case "TSV":
                    b_Successful = ExportR_to_Text("\t");
                    break;
                case "ACCESS":
                    /// TODO : Implement R to Access Export :(*
                    break;
                case "SQLSERVER":
                    /// TODO : Implement R to SQL Server Export :(*
                    break;
            }

            return b_Successful;
        }

        /// <summary>
        /// Exports a data.frame or matrix to SQLite database
        /// </summary>
        /// <returns>True, if the export is successful</returns>
        private bool ExportR_to_SQLite()
        {
            bool b_Successful = Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.TableName.ToString()]);

            if (!b_Successful)
            {
                Model.LogError(string.Format("Error attempting to " +
                    "export table {0} from R to SQLite! The table was " +
                    "not found in the R environment. Please check " +
                    "the R environment and try again.",
                    Parameters[RequiredParameters.TableName.ToString()]),
                    ModuleName, StepNumber);
                return false;
            }

            string Command = "";

            if (ConnectToSQLiteDatabaseFromR())
            {
                Command = string.Format(
                    "dbWriteTable(" +
                    "conn=con, name=\"{0}\", value=data.frame({1}))",
                    Parameters[DatabaseTargetRequiredParameters.NewTableName.ToString()],
                    Parameters[RequiredParameters.TableName.ToString()]);

                if (Model.RCalls.Run(Command,
                        ModuleName, StepNumber))
                {
                    return DisconnectFromDatabaseFromR();
                }
                else
                {
                    b_Successful = false;
                }
            }
            else
            {
                Model.LogError("Unable to successfully establish a connection " +
                    "with designated SQLite database!",
                    ModuleName, StepNumber);
                b_Successful = false;
            }
            
            return b_Successful;
        }

        /// <summary>
        /// Exports a data.frame or matrix from R to a Text File
        /// </summary>
        /// <param name="Delimiter">Delimits the data in the data.frame or matrix</param>
        /// <returns>True, if the file is exported successfully</returns>
        private bool ExportR_to_Text(string Delimiter)
        {
            bool b_Successful = Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.TableName.ToString()]),
                b_IncludeRowNames = false;

            string Command = "";
            if (Parameters.ContainsKey("IncludeRowNames"))
            {
                b_IncludeRowNames = Convert.ToBoolean(Parameters["IncludeRowNames"]);                
            }

            if (b_IncludeRowNames)
            {
                Command = string.Format("jnb_Write(df={0}, " +
                    "fileName=\"{1}\", firstColumnHeader=\"{2}\", " +
                    "sepChar=\"{3}\")",
                    Parameters[RequiredParameters.TableName.ToString()],
                    Model.WorkDirectory + "/" + 
                        Parameters[RequiredParameters.FileName.ToString()],
                    Parameters["RownamesColumnHeader"],
                    Delimiter);
            }
            else
            {
                string s_FilePath = Model.WorkDirectory + "/" + 
                        Parameters[RequiredParameters.FileName.ToString()];
                s_FilePath = s_FilePath.Replace("\\", "/");

                Command = string.Format("jnb_Write(df={0}, " +
                    "fileName=\"{1}\", row.names=FALSE, " +
                    "sepChar=\"{2}\")",
                    Parameters[RequiredParameters.TableName.ToString()],
                    s_FilePath,
                    Delimiter);
            }

            b_Successful = Model.RCalls.Run(Command,
                ModuleName, StepNumber);

            return b_Successful;
        }

        /// <summary>
        /// Exports a table from a SQLite Database
        /// </summary>
        /// <returns>True, if the export is successful</returns>
        private bool ExportFromSQLite()
        {
            bool b_Successful = true;

            string s_Type = Parameters[RequiredParameters.Target.ToString()].ToLower();

            if (s_Type.Equals("tsv") ||
                s_Type.Equals("txt") ||
                s_Type.Equals("csv"))
            {
                DataTable dt = sql.GetTable(
                    Parameters[RequiredParameters.TableName.ToString()]);

                switch (s_Type)
                {
                    case "tsv":
                        b_Successful = WriteDataTableToFile(dt,
                            Parameters[RequiredParameters.FileName.ToString()],
                            "\t");
                        break;
                    case "txt":
                        b_Successful = WriteDataTableToFile(dt,
                            Parameters[RequiredParameters.FileName.ToString()],
                            "\t");
                        break;
                    case "csv":
                        b_Successful = WriteDataTableToFile(dt,
                            Parameters[RequiredParameters.FileName.ToString()],
                            ",");
                        break;
                }
            }
            else
            {
                Model.LogError(
                    string.Format("The file type, {0}, to export from SQLite was not " +
                    "recognized. Please select either '*.tsv', '*.txt', or '*.csv'.",
                    Parameters[RequiredParameters.Target.ToString()]),
                    ModuleName, StepNumber);
                b_Successful = false;
            }

            return b_Successful;
        }

        /// <summary>
        /// Writes a DataTable out to a text file
        /// </summary>
        /// <param name="Table">DataTable to export</param>
        /// <param name="FileName">File name and path to save the DataTable to</param>
        /// <param name="Delimiter">Delimits the data in the file</param>
        /// <returns>True, if the DataTable is written out successfully</returns>
        private bool WriteDataTableToFile(DataTable Table,
            string FileName, string Delimiter)
        {
            bool b_Successful = true;

            try
            {
                StreamWriter sw = new StreamWriter(FileName);
                List<string> l_Headers = new List<string>();
                List<string> l_Row = new List<string>();
                foreach (DataColumn dc in Table.Columns)
                    l_Headers.Add(dc.ColumnName);

                sw.WriteLine(string.Join(Delimiter, l_Headers));

                foreach (DataRow dr in Table.Rows)
                {
                    foreach (string s in l_Headers)
                    {
                        l_Row.Add(dr[s].ToString());
                    }
                    sw.WriteLine(string.Join(Delimiter, l_Row));
                    l_Row.Clear();
                }

                sw.Close();
            }
            catch (IOException ioe)
            {
                Model.LogError("IOException encountered while writing a table " +
                    "out to " + FileName + "\nIOException: " + ioe.ToString(),
                    ModuleName, StepNumber);
                b_Successful = false;
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while writing a table " +
                    "out to " + FileName + "\nException: " + exc.ToString(),
                    ModuleName, StepNumber);
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

        /// <summary>
        /// Creates a connection to a sqlite database
        /// </summary>
        /// <returns>True, if Connection is established successfully</returns>
        protected bool ConnectToSQLiteDatabaseFromR()
        {
            if (m_DatabaseFound)
            {
                string s_Database = Path.Combine(Model.WorkDirectory,
                    m_DatabaseFileName).Replace("\\", "/"),
                Command = string.Format(
                                "require(RSQLite)\n" +
                                "m <- dbDriver(\"SQLite\", max.con=25)\n" +
                                "con <- dbConnect(m, dbname = \"{0}\")",
                                    s_Database);

                return Model.RCalls.Run(Command,
                    ModuleName, StepNumber);
            }
            else
            {
                Model.LogError("Error while exporting table in R to SQLite. " +
                    "Unable to establish connection with database: " +
                    Path.Combine(Model.WorkDirectory, m_DatabaseFileName),
                    ModuleName, StepNumber);
                return false;
            }
        }

        /// <summary>
        /// Terminates the connection to the SQLite database, releasing control of the database.
        /// </summary>
        public bool DisconnectFromDatabaseFromR()
        {
            string Command = "terminated <- dbDisconnect(con)";

            bool b_Successful = Model.RCalls.Run(Command,
                ModuleName, StepNumber);

            if (b_Successful)
                b_Successful = Model.RCalls.AssessBoolean("terminated");

            if (b_Successful)
            {
                Command = "rm(con)\nrm(m)\nrm(terminated)\nrm(rt)";
                b_Successful = Model.RCalls.Run(Command,
                    ModuleName, StepNumber);
            }
            else
            {
                Model.LogError("Cyclops was unsuccessful at disconnecting " +
                    "from SQLITE database!",
                    ModuleName, StepNumber);
            }

            return b_Successful;
        }
        #endregion
    }
}
