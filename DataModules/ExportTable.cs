﻿/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/
 * -----------------------------------------------------
 *
 * Licensed under the 2-Clause BSD License; you may not use this
 * file except in compliance with the License.  You may obtain
 * a copy of the License at https://opensource.org/licenses/BSD-2-Clause
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

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

        private readonly string m_ModuleName = "ExportTable";
        private readonly string m_Description = "";
        private string m_DatabaseFileName = "Results.db3";

        private bool m_DatabaseFound;

        private readonly SQLiteHandler m_SQLiteReader = new SQLiteHandler();
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
            Description = m_Description;
        }

        /// <summary>
        /// ExportTable module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public ExportTable(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// ExportTable module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public ExportTable(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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
            var successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running " + ModuleName, ModuleName, StepNumber);

                if (CheckParameters())
                    successful = ExportFunction();
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
            var paramDictionary = new Dictionary<string, string>();

            foreach (var s in Enum.GetNames(typeof(RequiredParameters)))
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
            var successful = true;

            foreach (var s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogWarning("Required Field Missing: " + s, ModuleName, StepNumber);
                    return false;
                }
            }

            if (Parameters.ContainsKey("DatabaseFileName"))
            {
                if (File.Exists(Parameters["DatabaseFileName"]))
                {
                    m_DatabaseFileName = Parameters["DatabaseFileName"];
                    m_SQLiteReader.DatabaseFileName = Parameters["DatabaseFileName"];
                    m_DatabaseFound = true;
                }
                else if (File.Exists(Path.Combine(Model.WorkDirectory, Parameters["DatabaseFileName"])))
                {
                    m_DatabaseFileName = Parameters["DatabaseFileName"];
                    m_SQLiteReader.DatabaseFileName = Path.Combine(Model.WorkDirectory, Parameters["DatabaseFileName"]);
                    m_DatabaseFound = true;
                }
            }
            else
            {
                if (File.Exists(Path.Combine(Model.WorkDirectory, "Results.db3")))
                {
                    m_SQLiteReader.DatabaseFileName = Path.Combine(Model.WorkDirectory, "Results.db3");
                    m_DatabaseFound = true;
                }
            }

            if (!m_DatabaseFound)
            {
                Model.LogError("Unable to establish successful database connection!", ModuleName, StepNumber);
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Checks that fields specific to database handling are present
        /// in the passed parameters
        /// </summary>
        /// <returns>True, if the parameters contain database specific params</returns>
        public bool CheckDatabaseTargetParameters()
        {
            var successful = true;

            foreach (var s in Enum.GetNames(typeof(DatabaseTargetRequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogError("Required Database Field Missing: " + s, ModuleName, StepNumber);
                    return false;
                }
            }

            return successful;
        }

        /// <summary>
        /// Main Export Function
        /// </summary>
        /// <returns>True, if the export is successful</returns>
        public bool ExportFunction()
        {
            var successful = true;

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
                        if (m_SQLiteReader.TableExists(Parameters[RequiredParameters.TableName.ToString()]))
                            return ExportFromSQLite();

                        Model.LogWarning(string.Format(
                                             "Warning Table, {0}, was not found in the SQLite database, {1}, for " +
                                             "export! Note that table names are case-sensitive.",
                                             Parameters[RequiredParameters.TableName.ToString()],
                                             m_DatabaseFileName),
                                         ModuleName, StepNumber);
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

            return successful;
        }

        /// <summary>
        /// Generic method to export data from R
        /// </summary>
        /// <returns>True, if the export is successful</returns>
        private bool ExportFromR()
        {
            var successful = true;

            // TODO : Export from R
            switch (Parameters[RequiredParameters.Target.ToString()].ToUpper())
            {
                case "SQLITE":
                    if (CheckDatabaseTargetParameters())
                        successful = ExportR_to_SQLite();
                    else
                    {
                        Model.LogError("Not all required fields for handling a " +
                            "database were present. Exiting Export Module.",
                            ModuleName, StepNumber);
                        return false;
                    }
                    break;
                case "CSV":
                    successful = ExportR_to_Text(",");
                    break;
                case "TSV":
                    successful = ExportR_to_Text("\t");
                    break;
                case "TXT":
                    successful = ExportR_to_Text("\t");
                    break;
                case "ACCESS":
                    // TODO : Implement R to Access Export :(*
                    break;
                case "SQLSERVER":
                    // TODO : Implement R to SQL Server Export :(*
                    break;
            }

            return successful;
        }

        /// <summary>
        /// Exports a data.frame or matrix to SQLite database
        /// </summary>
        /// <returns>True, if the export is successful</returns>
        private bool ExportR_to_SQLite()
        {
            var successful = Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.TableName.ToString()]);

            if (!successful)
            {
                Model.LogError(string.Format("Error attempting to " +
                    "export table {0} from R to SQLite! The table was " +
                    "not found in the R environment. Please check " +
                    "the R environment and try again.",
                    Parameters[RequiredParameters.TableName.ToString()]),
                    ModuleName, StepNumber);
                return false;
            }

            var rCmd = "";

            if (ConnectToSQLiteDatabaseFromR())
            {
                rCmd = string.Format(
                    "dbWriteTable(" +
                    "conn=con, name=\"{0}\", value=data.frame({1}))",
                    Parameters[DatabaseTargetRequiredParameters.NewTableName.ToString()],
                    Parameters[RequiredParameters.TableName.ToString()]);

                if (Model.RCalls.Run(rCmd, ModuleName, StepNumber))
                {
                    return DisconnectFromDatabaseFromR();
                }
                else
                {
                    successful = false;
                }
            }
            else
            {
                Model.LogError("Unable to successfully establish a connection " +
                    "with designated SQLite database!",
                    ModuleName, StepNumber);
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Exports a data.frame or matrix from R to a Text File
        /// </summary>
        /// <param name="Delimiter">Delimits the data in the data.frame or matrix</param>
        /// <returns>True, if the file is exported successfully</returns>
        private bool ExportR_to_Text(string Delimiter)
        {
            var successful = Model.RCalls.ContainsObject(Parameters[RequiredParameters.TableName.ToString()]);
            var includeRowNames = false;

            var rCmd = "";
            if (Parameters.ContainsKey("IncludeRowNames"))
            {
                includeRowNames = Convert.ToBoolean(Parameters["IncludeRowNames"]);
            }

            if (includeRowNames)
            {
                rCmd = string.Format("jnb_Write(df={0}, " +
                    "fileName=\"{1}\", firstColumnHeader=\"{2}\", " +
                    "sepChar=\"{3}\")",
                    Parameters[RequiredParameters.TableName.ToString()],
                    Model.WorkDirectory + "/" + Parameters[RequiredParameters.FileName.ToString()],
                    Parameters["RownamesColumnHeader"],
                    Delimiter);
            }
            else
            {
                var filePath = Path.Combine(Model.WorkDirectory, Parameters[RequiredParameters.FileName.ToString()]);
                var filePathForR = GenericRCalls.ConvertToRCompatiblePath(filePath);

                rCmd = string.Format("jnb_Write(df={0}, " +
                    "fileName=\"{1}\", row.names=FALSE, " +
                    "sepChar=\"{2}\")",
                    Parameters[RequiredParameters.TableName.ToString()],
                    filePathForR,
                    Delimiter);
            }

            successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);

            return successful;
        }

        /// <summary>
        /// Exports a table from a SQLite Database
        /// </summary>
        /// <returns>True, if the export is successful</returns>
        private bool ExportFromSQLite()
        {
            var successful = true;

            var targetType = Parameters[RequiredParameters.Target.ToString()].ToLower();

            if (targetType.Equals("tsv") ||
                targetType.Equals("txt") ||
                targetType.Equals("csv"))
            {
                var dt = m_SQLiteReader.GetTable(
                    Parameters[RequiredParameters.TableName.ToString()]);

                var outputFilePath = Path.Combine(Model.WorkDirectory, Parameters[RequiredParameters.FileName.ToString()]);

                switch (targetType)
                {
                    case "tsv":
                        successful = WriteDataTableToFile(dt, outputFilePath, "\t");
                        break;
                    case "txt":
                        successful = WriteDataTableToFile(dt, outputFilePath, "\t");
                        break;
                    case "csv":
                        successful = WriteDataTableToFile(dt, outputFilePath, ",");
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
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Writes a DataTable out to a text file
        /// </summary>
        /// <param name="table">DataTable to export</param>
        /// <param name="filePath">File name and path to save the DataTable to</param>
        /// <param name="delimiter">Delimits the data in the file</param>
        /// <returns>True, if the DataTable is written out successfully</returns>
        private bool WriteDataTableToFile(DataTable table, string filePath, string delimiter)
        {
            var successful = true;

            try
            {
                var sw = new StreamWriter(filePath);
                var columnNames = new List<string>();

                foreach (DataColumn dc in table.Columns)
                    columnNames.Add(dc.ColumnName);

                sw.WriteLine(string.Join(delimiter, columnNames));

                foreach (DataRow dr in table.Rows)
                {
                    var rowData = new List<string>();

                    foreach (var columnName in columnNames)
                    {
                        rowData.Add(dr[columnName].ToString());
                    }
                    sw.WriteLine(string.Join(delimiter, rowData));
                }

                sw.Close();
            }
            catch (IOException ioe)
            {
                Model.LogError("IOException encountered while writing a table " +
                    "out to " + filePath + "\nIOException: " + ioe.ToString(),
                    ModuleName, StepNumber);
                successful = false;
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while writing a table " +
                    "out to " + filePath + "\nException: " + ex.ToString(),
                    ModuleName, StepNumber);
                successful = false;
            }

            return successful;
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
        /// Retrieves the Type Description for automatically
        /// registering the module assembly
        /// </summary>
        /// <returns>Module's Description</returns>
        protected override string GetTypeDescription()
        {
            return Description;
        }

        /// <summary>
        /// Creates a connection to a sqlite database
        /// </summary>
        /// <returns>True, if Connection is established successfully</returns>
        protected bool ConnectToSQLiteDatabaseFromR()
        {
            var databasePath = Path.Combine(Model.WorkDirectory, m_DatabaseFileName);

            if (m_DatabaseFound)
            {
                var databasePathForR = GenericRCalls.ConvertToRCompatiblePath(databasePath);

                var rCmd = string.Format(
                        "require(RSQLite)\n" +
                        "m <- dbDriver(\"SQLite\", max.con=25)\n" +
                        "con <- dbConnect(m, dbname = \"{0}\")",
                            databasePathForR);

                return Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }
            else
            {
                Model.LogError("Error while exporting table in R to SQLite. " +
                    "Unable to establish connection with database: " +
                    databasePath,
                    ModuleName, StepNumber);
                return false;
            }
        }

        /// <summary>
        /// Terminates the connection to the SQLite database, releasing control of the database.
        /// </summary>
        public bool DisconnectFromDatabaseFromR()
        {
            var rCmdDisconnect = "terminated <- dbDisconnect(con)";

            var successful = Model.RCalls.Run(rCmdDisconnect, ModuleName, StepNumber);

            if (successful)
                successful = Model.RCalls.AssessBoolean("terminated");

            if (successful)
            {
                var rCmdTerminate = "rm(con)\nrm(m)\nrm(terminated)\nrm(rt)";
                successful = Model.RCalls.Run(rCmdTerminate, ModuleName, StepNumber);
            }
            else
            {
                Model.LogError("Cyclops was unsuccessful at disconnecting " +
                    "from SQLITE database!",
                    ModuleName, StepNumber);
            }

            return successful;
        }

        public override string ToString()
        {
            if (Parameters.TryGetValue(RequiredParameters.TableName.ToString(), out var tableName))
            {
                if (!Parameters.TryGetValue(RequiredParameters.FileName.ToString(), out var fileName))
                {
                    fileName = "??";
                }
                return string.Format("ExportTable: {0} -> {1}", tableName, fileName);
            }

            return base.ToString();
        }

        #endregion
    }
}
