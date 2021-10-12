/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
 * -----------------------------------------------------
 *
 * Licensed under the 2-Clause BSD License; you may not use this
 * file except in compliance with the License.  You may obtain
 * a copy of the License at https://opensource.org/licenses/BSD-2-Clause
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Cyclops.DataModules
{
    public class ImportDataModule : BaseDataModule
    {
        #region Members
        /// <summary>
        /// Required parameters to run MissedCleavageSummary Module
        /// </summary>
        private enum RequiredParameters
        {
            Source, NewTableName, TableType, ImportDatasetType
        }

        private enum DatabaseRequiredParameters
        {
            InputTableName
        };

        private readonly string m_ModuleName = "Import";
        private readonly string m_Description = "";

        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an Import Data Module
        /// </summary>
        public ImportDataModule()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// Import Data module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public ImportDataModule(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// Import Data module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="DataParameters">Data Parameters</param>
        public ImportDataModule(CyclopsModel CyclopsModel, Dictionary<string, string> DataParameters)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
            Parameters = DataParameters;
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
                {
                    successful = ImportData();
                }
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
            if (string.IsNullOrEmpty(Model.WorkDirectory))
            {
                Model.LogError("Required Working Directory is Missing", ModuleName, StepNumber);
                return false;
            }

            foreach (var s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogWarning("Required Field Missing: " + s, ModuleName, StepNumber);
                    return false;
                }
            }

            return true;
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
        /// Primary Method called by PerformOperation to import data
        /// </summary>
        /// <returns>True, if data is imported successfully</returns>
        public bool ImportData()
        {
            switch (Parameters[RequiredParameters.Source.ToString()].ToLower())
            {
                case "sqlite":
                    return ProcessSQLiteImport();
                case "csv":
                    return ProcessCSVImport();
                case "tsv":
                    return ProcessTSVImport();
                case "sqlserver":
                    return ProcessSQLServerImport();
                case "access":
                    return ProcessAccessImport();
                default:
                    return false;
            }
        }

        /// <summary>
        /// Imports data from SQLite database into R environment
        /// </summary>
        /// <returns>True, if import is successful</returns>
        private bool ProcessSQLiteImport()
        {
            var successful = ConnectToSQLiteDatabase();

            if (successful)
            {
                successful = ImportSQLiteTable();
            }

            if (successful)
            {
                successful = DisconnectFromDatabase();
            }

            return successful;
        }

        /// <summary>
        /// Imports data from CSV file into R environment
        /// </summary>
        /// <returns>True, if import is successful</returns>
        private bool ProcessCSVImport()
        {
            var successful = ImportCSVFile();

            return successful;
        }

        /// <summary>
        /// Imports data from TSV file into R environment
        /// </summary>
        /// <returns>True, if import is successful</returns>
        private bool ProcessTSVImport()
        {
            var successful = ImportTSVFile();

            return successful;
        }

        /// <summary>
        /// Imports data from SQLServer database into R environment
        /// </summary>
        /// <returns>True, if import is successful</returns>
        private bool ProcessSQLServerImport()
        {
            // TODO
            return false;
        }

        /// <summary>
        /// Imports data from Access database into R environment
        /// </summary>
        /// <returns>True, if import is successful</returns>
        private bool ProcessAccessImport()
        {
            // TODO
            return false;
        }

        /// <summary>
        /// Append the filename to the directory path, using the system's directory separator character
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="fileName"></param>
        /// <returns>New file path</returns>
        /// <remarks>replaces invalid path separators with teh system's directory separator character</remarks>
        private string BuildAndValidatePath(string directoryPath, string fileName)
        {
            var filePath = Path.Combine(directoryPath, fileName);

            if (Path.DirectorySeparatorChar == '\\' && filePath.Contains("/"))
            {
                return filePath.Replace('/', '\\');
            }

            if (Path.DirectorySeparatorChar == '/' && filePath.Contains(@"\"))
            {
                return filePath.Replace('\\', '/');
            }

            return filePath;
        }

        /// <summary>
        /// Connects R environment to a SQLite database
        /// </summary>
        /// <returns>True, if connection established successfully</returns>
        private bool ConnectToSQLiteDatabase()
        {
            var successful = false;

            if (CheckSQLiteRequiredParameters())
            {
                string inputFilePath;

                if (Parameters.ContainsKey("inputFileName"))
                {
                    inputFilePath = BuildAndValidatePath(Model.WorkDirectory, Parameters["inputFileName"]);
                }
                else
                {
                    inputFilePath = BuildAndValidatePath(Model.WorkDirectory, "Results.db3");
                }

                if (File.Exists(inputFilePath))
                {
                    // R requires forward slashes in paths
                    var filePathForR = GenericRCalls.ConvertToRCompatiblePath(inputFilePath);

                    try
                    {
                        var rCmd = string.Format(
                            "require(RSQLite)\n"
                            + "m <- dbDriver(\"SQLite\", max.con=25)\n"
                            + "con <- dbConnect(m, dbname = \"{0}\")\n"
                            , filePathForR);

                        successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
                    }
                    catch (IOException ioe)
                    {
                        Model.LogError("IOException encountered while " +
                            "connecting to SQLite database:\n" +
                            ioe + "\nInnerException: " +
                            ioe.InnerException, ModuleName, StepNumber);
                        SaveCurrentREnvironment();
                        successful = false;
                    }
                    catch (StackOverflowException soe)
                    {
                        Model.LogError("StackOverflowException encountered while " +
                            "connecting to SQLite database:\n" +
                            soe + "\nInnerException: " +
                            soe.InnerException, ModuleName, StepNumber);
                        SaveCurrentREnvironment();
                        successful = false;
                    }
                    catch (Exception ex)
                    {
                        Model.LogError("Exception encountered while " +
                            "connecting to SQLite database:\n" +
                            ex + "\nInnerException: " +
                            ex.InnerException, ModuleName, StepNumber);
                        SaveCurrentREnvironment();
                        successful = false;
                    }
                }
            }

            return successful;
        }

        /// <summary>
        /// Checks the parameters to ensure that all required keys are present
        /// </summary>
        /// <returns>True, if all required keys are included in the
        /// Parameters</returns>
        public bool CheckSQLiteRequiredParameters()
        {
            foreach (var s in Enum.GetNames(typeof(DatabaseRequiredParameters)))
            {
                if (!Parameters.ContainsKey(s))
                {
                    Model.LogError("Required Field Missing for SQLite data import: " +
                        s, ModuleName, StepNumber);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Import a Table from SQLite database into R environment
        /// </summary>
        /// <returns>True, if import is successful</returns>
        private bool ImportSQLiteTable()
        {
            var tableType = Parameters[RequiredParameters.TableType.ToString()];

            if (string.Equals(tableType, "DataTable", StringComparison.OrdinalIgnoreCase))
            {
                return ImportSQLiteDataTable();
            }

            if (string.Equals(tableType, "ColumnMetadataTable", StringComparison.OrdinalIgnoreCase))
            {
                return ImportSQLiteColumnMetadataTable();
            }

            if (string.Equals(tableType, "RowMetadataTable", StringComparison.OrdinalIgnoreCase))
            {
                return ImportSQLiteRowMetadataTable();
            }

            Model.LogError("Unable to recognize table type to import from " +
                           "SQLite: " + Parameters[RequiredParameters.TableType.ToString()],
                           ModuleName, StepNumber);
            return false;
        }

        /// <summary>
        /// Imports a SQLite table as a 'dataTable' into R environment
        /// </summary>
        /// <returns>True, if import is successful</returns>
        private bool ImportSQLiteDataTable()
        {
            GetTableFromSQLite();

            var successful = SetTableRowNames();

            return successful;
        }

        /// <summary>
        /// Imports a SQLite table as a 'columnMetadataTable' into R environment
        /// </summary>
        /// <returns>True, if import is successful</returns>
        private bool ImportSQLiteColumnMetadataTable()
        {
            GetTableFromSQLite();

            var successful = SetTableRowNames();

            return successful;
        }

        /// <summary>
        /// Imports a SQLite table as a 'rowMetadataTable' into R environment
        /// </summary>
        /// <returns>True, if import is successful</returns>
        private bool ImportSQLiteRowMetadataTable()
        {
            GetTableFromSQLite();

            var successful = SetTableRowNames();

            return successful;
        }

        /// <summary>
        /// Generic method for importing a table from SQLite
        /// </summary>
        /// <returns>True, if the table imports successfully</returns>
        private bool GetTableFromSQLite()
        {
            bool successful;
            var rCmd = string.Format(
                "rt <- dbSendQuery(con, \"SELECT * FROM {0}\")\n" +
                "{1} <- fetch(rt, n = -1)\n" +
                "dbClearResult(rt)\n" +
                "DataCleaning({1})",
                Parameters[DatabaseRequiredParameters.InputTableName.ToString()],
                Parameters[RequiredParameters.NewTableName.ToString()]);

            try
            {
                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while running " +
                    "'GetTableFromSQLite': " + ex, ModuleName, StepNumber);
                SaveCurrentREnvironment();
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Sets the row names for a table in R and removes that column from the table
        /// </summary>
        /// <returns>True, if row names are set successfully</returns>
        private bool SetTableRowNames()
        {
            var successful = true;
            if (Parameters.ContainsKey("rownames"))
            {
                var rCmd = string.Format(
                    "rownames({0}) <- {0}[,{1}]\n" +
                    "{0} <- {0}[,-{1}]",
                    Parameters[RequiredParameters.NewTableName.ToString()],
                    Parameters["rownames"]);

                try
                {
                    successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
                }
                catch (Exception ex)
                {
                    Model.LogError("Exception encountered while running " +
                                   "'SetTableRowNames': " + ex, ModuleName, StepNumber);
                    SaveCurrentREnvironment();
                    successful = false;
                }
            }
            return successful;
        }

        /// <summary>
        /// Terminates the R connection to the SQLite database, releasing control of the database.
        /// </summary>
        /// <returns>True, if disconnection finishes successfully</returns>
        private bool DisconnectFromDatabase()
        {
            bool successful;

            var rCmd = "terminated <- dbDisconnect(con)";

            if (Model.RCalls.Run(rCmd, ModuleName, StepNumber))
            {
                successful = true;
            }

            successful = Model.RCalls.AssessBoolean("terminated");

            if (successful)
            {
                rCmd = "rm(con)\nrm(m)\nrm(terminated)\nrm(rt)";
                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }

            return successful;
        }

        /// <summary>
        /// Imports a CSV file into R environment
        /// </summary>
        /// <returns>True, if CSV file is imported successfully</returns>
        private bool ImportCSVFile()
        {
            if (!Parameters.ContainsKey("inputFileName"))
            {
                Model.LogError("Exception encountered while importing CSV File, " +
                    "File Path was not specified!\n Please enter the file path " +
                    "as a parameter entitled 'inputFileName'.");
                return false;
            }

            var inputFilePath = BuildAndValidatePath(Model.WorkDirectory, Parameters["inputFileName"]);
            var filePathForR = GenericRCalls.ConvertToRCompatiblePath(inputFilePath);

            var rCmd = string.Format(
                "{0} <- read.csv(file=\"{1}\")\n",
                Parameters[RequiredParameters.NewTableName.ToString()], filePathForR);

            return Model.RCalls.Run(rCmd, ModuleName, StepNumber);
        }

        /// <summary>
        /// Imports a TSV file into R environment
        /// </summary>
        /// <returns>True, if TSV file is imported successfully</returns>
        private bool ImportTSVFile()
        {
            if (!Parameters.ContainsKey("inputFileName"))
            {
                Model.LogError("Exception encountered while importing TSV File, " +
                    "File Path was not specified!\n Please enter the file path " +
                    "as a parameter entitled 'inputFileName'.");
                return false;
            }

            var inputFilePath = BuildAndValidatePath(Model.WorkDirectory, Parameters["inputFileName"]);
            var filePathForR = GenericRCalls.ConvertToRCompatiblePath(inputFilePath);

            var rCmd = string.Format(
                "{0} <- read.table(file=\"{1}\", sep=\"\\t\", header=T)\n",
                Parameters[RequiredParameters.NewTableName.ToString()],
                filePathForR);

            return Model.RCalls.Run(rCmd, ModuleName, StepNumber);
        }
        #endregion
    }
}
