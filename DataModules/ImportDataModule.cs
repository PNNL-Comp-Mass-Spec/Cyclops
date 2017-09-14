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

        private string m_ModuleName = "Import",
            m_Description = "";
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
        public ImportDataModule(CyclopsModel CyclopsModel,
            Dictionary<string, string> DataParameters)
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
            bool b_Successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running " + ModuleName,
                        ModuleName, StepNumber);

                if (CheckParameters())
                    b_Successful = ImportData();
            }

            return b_Successful;
        }

        /// <summary>
        /// Retrieves a dictionary of all parameters used by the module,
        /// and the corresponding default values
        /// </summary>
        /// <returns>Parameters used by module</returns>
        public override Dictionary<string, string> GetParametersTemplate()
        {
            Dictionary<string, string> d_Parameters = new Dictionary<string, string>();

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                d_Parameters.Add(s, "");
            }

            return d_Parameters;
        }

        /// <summary>
        /// Checks the parameters to ensure that all required keys are present
        /// </summary>
        /// <returns>True, if all required keys are included in the
        /// Parameters</returns>
        public override bool CheckParameters()
        {
            bool b_Successful = true;

            if (string.IsNullOrEmpty(Model.WorkDirectory))
            {
                Model.LogError("Required Working Directory is Missing",
                    ModuleName, StepNumber);
                return false;
            }

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
            bool b_Successful = false;

            b_Successful = ConnectToSQLiteDatabase();

            if (b_Successful)
                b_Successful = ImportSQLiteTable();

            if (b_Successful)
                b_Successful = DisconnectFromDatabase();

            return b_Successful;
        }

        /// <summary>
        /// Imports data from CSV file into R environment
        /// </summary>
        /// <returns>True, if import is successful</returns>
        private bool ProcessCSVImport()
        {
            bool b_Successful = false;

            b_Successful = ImportCSVFile();

            return b_Successful;
        }

        /// <summary>
        /// Imports data from TSV file into R environment
        /// </summary>
        /// <returns>True, if import is successful</returns>
        private bool ProcessTSVImport()
        {
            bool b_Successful = false;

            b_Successful = ImportTSVFile();

            return b_Successful;
        }

        /// <summary>
        /// Imports data from SQLServer database into R environment
        /// </summary>
        /// <returns>True, if import is successful</returns>
        private bool ProcessSQLServerImport()
        {
            bool b_Successful = false;

            // TODO

            return b_Successful;
        }

        /// <summary>
        /// Imports data from Access database into R environment
        /// </summary>
        /// <returns>True, if import is successful</returns>
        private bool ProcessAccessImport()
        {
            bool b_Successful = false;

            // TODO

            return b_Successful;
        }

        /// <summary>
        /// Connects R environment to a SQLite database
        /// </summary>
        /// <returns>True, if connection established successfully</returns>
        private bool ConnectToSQLiteDatabase()
        {
            bool b_Successful = false;

            if (CheckSQLiteRequiredParameters())
            {
                string s_InputFileName = "";
                if (Parameters.ContainsKey("inputFileName"))
                    s_InputFileName = Model.WorkDirectory + "/" +
                        Parameters["inputFileName"];
                else
                    s_InputFileName = Model.WorkDirectory + "/Results.db3";

                s_InputFileName = s_InputFileName.Replace('\\', '/');

                if (File.Exists(s_InputFileName))
                {
                    try
                    {
                        string s_Command = string.Format(
                            "require(RSQLite)\n"
                            + "m <- dbDriver(\"SQLite\", max.con=25)\n"
                            + "con <- dbConnect(m, dbname = \"{0}\")\n"
                            , s_InputFileName);

                        b_Successful = Model.RCalls.Run(
                            s_Command, ModuleName, StepNumber);
                    }
                    catch (IOException ioe)
                    {
                        Model.LogError("IOException encountered while " +
                            "connecting to SQLite database:\n" +
                            ioe.ToString() + "\nInnerException: " +
                            ioe.InnerException, ModuleName, StepNumber);
                        SaveCurrentREnvironment();
                        b_Successful = false;
                    }
                    catch (StackOverflowException soe)
                    {
                        Model.LogError("StackOverflowException encountered while " +
                            "connecting to SQLite database:\n" +
                            soe.ToString() + "\nInnerException: " +
                            soe.InnerException, ModuleName, StepNumber);
                        SaveCurrentREnvironment();
                        b_Successful = false;
                    }
                    catch (Exception ex)
                    {
                        Model.LogError("Exception encountered while " +
                            "connecting to SQLite database:\n" +
                            ex.ToString() + "\nInnerException: " +
                            ex.InnerException, ModuleName, StepNumber);
                        SaveCurrentREnvironment();
                        b_Successful = false;
                    }
                }
            }

            return b_Successful;
        }

        /// <summary>
        /// Checks the parameters to ensure that all required keys are present
        /// </summary>
        /// <returns>True, if all required keys are included in the
        /// Parameters</returns>
        public bool CheckSQLiteRequiredParameters()
        {
            bool b_Successful = true;

            foreach (string s in Enum.GetNames(typeof(DatabaseRequiredParameters)))
            {
                if (!Parameters.ContainsKey(s))
                {
                    Model.LogError("Required Field Missing for SQLite data import: " +
                        s, ModuleName, StepNumber);
                    b_Successful = false;
                    return b_Successful;
                }
            }

            return b_Successful;
        }

        /// <summary>
        /// Import a Table from SQLite database into R environment
        /// </summary>
        /// <returns>True, if import is successful</returns>
        private bool ImportSQLiteTable()
        {
            switch (Parameters[RequiredParameters.TableType.ToString()].ToLower())
            {
                case "datatable":
                    return ImportSQLiteDataTable();
                case "columnmetadatatable":
                    return ImportSQLiteColumnMetadataTable();
                case "rowmetadatatable":
                    return ImportSQLiteRowMetadataTable();
                default:
                    Model.LogError("Unable to recognize table type to import from " +
                        "SQLite: " + Parameters[RequiredParameters.TableType.ToString()],
                        ModuleName, StepNumber);
                    return false;
            }
        }

        /// <summary>
        /// Imports a SQLite table as a 'dataTable' into R environment
        /// </summary>
        /// <returns>True, if import is successful</returns>
        private bool ImportSQLiteDataTable()
        {
            bool b_Successful = true;

            b_Successful = GetTableFromSQLite();

            b_Successful = SetTableRowNames();

            return b_Successful;
        }

        /// <summary>
        /// Imports a SQLite table as a 'columnMetadataTable' into R environment
        /// </summary>
        /// <returns>True, if import is successful</returns>
        private bool ImportSQLiteColumnMetadataTable()
        {
            bool b_Successful = true;

            b_Successful = GetTableFromSQLite();

            b_Successful = SetTableRowNames();

            return b_Successful;
        }

        /// <summary>
        /// Imports a SQLite table as a 'rowMetadataTable' into R environment
        /// </summary>
        /// <returns>True, if import is successful</returns>
        private bool ImportSQLiteRowMetadataTable()
        {
            bool b_Successful = true;

            b_Successful = GetTableFromSQLite();

            b_Successful = SetTableRowNames();

            return b_Successful;
        }

        /// <summary>
        /// Generic method for importing a table from SQLite
        /// </summary>
        /// <returns>True, if the table imports successfully</returns>
        private bool GetTableFromSQLite()
        {
            bool b_Successful = true;
            string s_Command = string.Format(
                                    "rt <- dbSendQuery(con, \"SELECT * FROM {0}\")\n" +
                                    "{1} <- fetch(rt, n = -1)\n" +
                                    "dbClearResult(rt)\n" +
                                    "DataCleaning({1})",
                                    Parameters[DatabaseRequiredParameters.InputTableName.ToString()],
                                    Parameters[RequiredParameters.NewTableName.ToString()]);

            try
            {
                b_Successful = Model.RCalls.Run(s_Command,
                    ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while running " +
                    "'GetTableFromSQLite': " + ex.ToString(), ModuleName,
                    StepNumber);
                SaveCurrentREnvironment();
                b_Successful = false;
            }

            return b_Successful;
        }

        /// <summary>
        /// Sets the rownames for a table in R and removes that column from the table
        /// </summary>
        /// <returns>True, if rownames are set successfully</returns>
        private bool SetTableRowNames()
        {
            bool b_Successful = true;
            if (Parameters.ContainsKey("rownames"))
            {
                string s_Command = string.Format(
                                        "rownames({0}) <- {0}[,{1}]\n" +
                                        "{0} <- {0}[,-{1}]",
                                        Parameters[RequiredParameters.NewTableName.ToString()],
                                        Parameters["rownames"]);

                try
                {
                    b_Successful = Model.RCalls.Run(s_Command,
                        ModuleName, StepNumber);
                }
                catch (Exception ex)
                {
                    Model.LogError("Exception encountered while running " +
                    "'SetTableRowNames': " + ex.ToString(), ModuleName,
                    StepNumber);
                    SaveCurrentREnvironment();
                    b_Successful = false;
                }
            }
            return b_Successful;
        }

        /// <summary>
        /// Terminates the R connection to the SQLite database, releasing control of the database.
        /// </summary>
        /// <returns>True, if disconnection finishes successfully</returns>
        private bool DisconnectFromDatabase()
        {
            bool b_Successful = false;

            string s_RStatement = "terminated <- dbDisconnect(con)";

            if (Model.RCalls.Run(s_RStatement, ModuleName, StepNumber))
                b_Successful = true;

            b_Successful = Model.RCalls.AssessBoolean("terminated");

            if (b_Successful)
            {
                s_RStatement = "rm(con)\nrm(m)\nrm(terminated)\nrm(rt)";
                b_Successful = Model.RCalls.Run(s_RStatement,
                    ModuleName, StepNumber);
            }

            return b_Successful;
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

            string s_InputFileName = Model.WorkDirectory + "/" +
                Parameters["inputFileName"];
            s_InputFileName = s_InputFileName.Replace('\\', '/');

            string s_Command = string.Format("{0} <- read.csv(" +
                            "file=\"{1}\")\n",
                            Parameters[RequiredParameters.NewTableName.ToString()],
                            s_InputFileName);

            return Model.RCalls.Run(s_Command,
                ModuleName,
                StepNumber);
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

            string s_InputFileName = Model.WorkDirectory + "/" +
                Parameters["inputFileName"];
            s_InputFileName = s_InputFileName.Replace('\\', '/');

            string s_Command = string.Format("{0} <- read.table(" +
                            "file=\"{1}\", sep=\"\\t\", header=T)\n",
                            Parameters[RequiredParameters.NewTableName.ToString()],
                            s_InputFileName);

            return Model.RCalls.Run(s_Command,
                ModuleName,
                StepNumber);
        }
        #endregion
    }
}
