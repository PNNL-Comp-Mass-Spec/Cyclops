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
using System.IO;
using System.Threading;

using RDotNet;
using log4net;

namespace Cyclops.DataModules
{
    /// <summary>
    /// Class pulls tables from SQLite, CSV, TSV, MSAccess, or SQLServer and 
    /// adds them as DataFrames in R. Designed to created ExpressionSets in 
    /// R for further data analysis.
    /// </summary>
    public class clsImportDataModule : clsBaseDataModule
    {
        #region Members
        private string s_RInstance, s_Current_R_Statement = "";
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        private DataModules.clsDataModuleParameterHandler dsp = 
            new DataModules.clsDataModuleParameterHandler();
        #endregion

        #region Constructors
        /// <summary>
        /// Module capable of reading in data from multiple sources
        /// </summary>
        public clsImportDataModule()
        {            
            ModuleName = "Import Module";
        }
        /// <summary>
        /// Module capable of reading in data from multiple sources
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsImportDataModule(string InstanceOfR)
        {
            ModuleName = "Import Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Module capable of reading in data from multiple sources
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsImportDataModule(clsCyclopsModel TheCyclopsModel,
            string InstanceOfR)
        {
            ModuleName = "Import Module";
            Model = TheCyclopsModel;
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties
        
        #endregion

        #region Methods

        public override string GetDescription()
        {
            return ModuleName + GetModuleNameExtension();             
        }
                      
        /// <summary>
        ///  Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            if (Model.SuccessRunningPipeline)
            {
                Model.IncrementStep(ModuleName);
                dsp.GetParameters(ModuleName, Parameters);

                if (CheckPassedParameters())
                {
                    switch (dsp.Source)
                    {
                        case "sqlite":
                            string s_RStatement = "";
                            ConnectToSQLiteDatabase();

                            /// To create an ExpressionSet, the following data must be supplied:
                            /// ASSAY_DATA: F X S Matrix of expression values. F rows (e.g. peptides), S columns (e.g. MS runs)
                            /// PHENO_DATA: S X V Dataframe describing what the MS runs correspond to
                            /// PHENO_DATA_LINK: Name of the column in PHENO_DATA that links to the AssayData
                            /// ROW_DATA: F X Z Dataframe describing the rows (e.g. peptide to proteins linkage)                        
                            if (dsp.ImportDatasetType.Equals("expressionSet"))
                            {
                                /// Pull in the AssayData
                                GetAssayDataFromSQLiteDB();

                                /// Pull in the Phenotype Data
                                GetPhenotypeDataFromSQLiteDB();

                                /// Pull in the Features Data
                                GetFeatureDataFromSQLiteDB();

                                /// Build the ExpressionSet
                                s_RStatement = string.Format("eData <- new(\"ExpressionSet\", exprs=tmpData, " +
                                        "phenoData=pData, featureData=fData)\n" +
                                        "rm(tmpData)\n" +
                                        "rm(pData)\n" +
                                        "rm(fData)");
                            }
                            else if (dsp.TableType.Equals("dataTable"))
                            {
                                GetDataTableFromSQLiteDB();
                                if (dsp.RowNames.Length > 0)
                                    SetTableRowNames(dsp.NewTableName);
                                SetDataMatrix(dsp.NewTableName);
                            }
                            else if (dsp.TableType.Equals("columnMetaDataTable"))
                            {
                                GetColumnMetadataFromSQLiteDB();
                                if (dsp.RowNames.Length > 0)
                                    SetTableRowNames(dsp.NewTableName);
                            }
                            else if (dsp.TableType.Equals("rowMetaDataTable"))
                            {
                                GetRowMetadataFromSQLiteDB();
                                if (dsp.RowNames.Length > 0)
                                    SetTableRowNames(dsp.NewTableName);
                            }

                            if (dsp.Set_0_to_NA)
                            {
                                s_RStatement += "\n" + dsp.NewTableName + "[" +
                                    dsp.NewTableName + "==0] <- NA";
                            }

                            if (!string.IsNullOrEmpty(s_RStatement))
                            {
                                if (!clsGenericRCalls.Run(s_RStatement,
                                    s_RInstance, "Importing Table",
                                    this.StepNumber, Model.NumberOfModules))
                                    Model.SuccessRunningPipeline = false;
                            }

                            DisconnectFromDatabase();
                            break;
                        case "csv":
                            ReadCSV_File();
                            break;
                        case "tsv":
                            ReadTSV_File();
                            break;
                        case "sqlserver":

                            break;
                        case "access":

                            break;
                    }
                }
            }

            RunChildModules();
        }

        /// <summary>
        /// Determine is all the necessary parameters are being passed to the object
        /// </summary>
        /// <returns>Returns true import module can proceed</returns>
        public bool CheckPassedParameters()
        {
            bool b_2Pass = true;

            // NECESSARY PARAMETERS
            if (!dsp.HasSource)
            {
                traceLog.Error("ERROR ImportDataModule: 'source': \"" +
                    dsp.Source + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasTarget)
            {
                traceLog.Error("ERROR ImportDataModule: 'target': \"" +
                    dsp.Target + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasWorkDir)
            {
                traceLog.Error("ERROR ImportDataModule: 'workDir': \"" + 
                    dsp.WorkDirectory + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasImportDatasetType)
            {
                traceLog.Error("ERROR ImportDataModule: 'importDatasetType': \"" +
                    dsp.ImportDatasetType + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasNewTableName)
            {
                traceLog.Error("ERROR ImportDataModule: 'newTableName': \"" + 
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasTableType)
            {
                traceLog.Error("ERROR ImportDataModule: 'tableType': \"" +
                    dsp.TableType + "\", was not found in the passed parameters");
                b_2Pass = false;
            }


            return b_2Pass;
        }

        /// <summary>
        /// Creates a connection to a sqlite database
        /// </summary>
        /// <param name="RInstance">Instance of your R workspace</param>
        protected void ConnectToSQLiteDatabase()
        {
            string s_InputFileName = "";
            if (dsp.HasWorkDir & dsp.HasInputFileName)
                s_InputFileName = dsp.WorkDirectory + "/" + dsp.InputFileName;
            else if (dsp.HasInputFileName)
                s_InputFileName = dsp.InputFileName;
            else
                s_InputFileName = dsp.WorkDirectory + "/Results.db3";

            if (File.Exists(s_InputFileName))
            {
                string s_RStatement = string.Format(
                                "require(RSQLite)\n" +
                                "m <- dbDriver(\"SQLite\", max.con=25)\n" +
                                "con <- dbConnect(m, dbname = \"{0}\")",
                                    s_InputFileName);

                if (!clsGenericRCalls.Run(s_RStatement,
                    s_RInstance, "Connecting to SQLite Database",
                    this.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;
            }
        }

        /// <summary>
        /// Terminates the connection to the SQLite database, releasing control of the database.
        /// </summary>
        /// <param name="RInstance">Instance of the R Workspace</param>
        public void DisconnectFromDatabase()
        {
            string s_RStatement = "terminated <- dbDisconnect(con)";

            traceLog.Info("Disconnecting from Database: " + s_RStatement);
            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Disconnecting from Database", this.StepNumber,
                Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;

            //bool b_Disconnected = true;
            bool b_Disconnected = clsGenericRCalls.AssessBoolean(s_RInstance, "terminated");
            
            if (b_Disconnected)
            {
                s_RStatement = "rm(con)\nrm(m)\nrm(terminated)\nrm(rt)";
                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "Cleaning Database Connection",
                    this.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;
            }
        }


        /// <summary>
        /// Retrieves a table from the SQLite database and converts it to
        /// an AssayData table that can be easily entered into an
        /// ExpressionSet
        /// </summary>
        /// <param name="RInstance">Instance of the R Workspace</param>
        protected void GetAssayDataFromSQLiteDB()
        {
            // Pull in the AssayData
            string s_RStatement = string.Format("library(\"Biobase\")\n" +
                "rt <- dbSendQuery(con, \"SELECT * FROM {0}\")\n" +
                "tmpData <- fetch(rt, n=-1)\n" +
                "tmpData <- as.matrix(tmpData)\n" +
                "rownames(tmpData) <- as.numeric(tmpData[,1])\n" +
                "tmpData <- tmpData[,-1]\n" +
                "DataCleaning(tmpData)",                
                Parameters["assayData"]);

            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Retrieving Assay Data from SQLite",
                this.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;

            SetDataMatrix("tmpData");
        }

        /// <summary>
        /// Gets the Phenotypic metadata (column metadata) for the 
        /// Expression set
        /// </summary>
        /// <param name="RInstance">Instance of the R Workspace</param>
        protected void GetPhenotypeDataFromSQLiteDB()
        {
            // Pull in the Phenotype Data
            string s_RStatement = string.Format("rt <- dbSendQuery(con, \"SELECT * FROM {0}\")\n" +
                "tmpPheno <- fetch(rt, n=-1)\n" +
                "tmpPheno <- tmpPheno[which(tmpPheno${1} " +
                    "%in% colnames(tmpData)),]\n" +
                "t <- cbind(\"Order\"=c(1:ncol(tmpData))," +
                    "\"{1}\"=colnames(tmpData))\n" +
                "t <- cbind(\"Order\"=c(1:ncol(tmpData))," +
                    "\"{1}\"=colnames(tmpData))\n" +
                "t <- merge(x=t, y=tmpPheno, " +
                    "by.x=\"{1}\", by.y=\"{1}\")\n" +
                "t$Order <- as.numeric(as.character(t$Order))\n" +
                "t <- t[order(t$Order),]\n" +
                "rownames(t) <- t${1}\n" +
                "t <- t[,-1]\n" +
                "pData <- new(\"AnnotatedDataFrame\", data=t)\n" +
                "rm(t)\n" +
                "rm(tmpPheno)\n",
                Parameters["phenoData"],
                Parameters["phenoDataLink"]);

            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Retrieving Phenotype Data from SQLite",
                this.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;
        }

        /// <summary>
        /// Gets the Feature metadata (row metadata) for the 
        /// Expression set
        /// </summary>
        /// <param name="s_RInstance">Instance of the R Workspace</param>
        public void GetFeatureDataFromSQLiteDB()
        {
            // Pull in the Features Data
            string s_RStatement = string.Format("rt <- dbSendQuery(con, \"SELECT * FROM {0}\")\n" +
                "tmpFeatures <- fetch(rt, n=-1)\n" +
                "dbClearResult(rt)\n" +
                "rownames(tmpFeatures) <- as.numeric(tmpFeatures${1})\n" +
                "tmpFeature <- tmpFeatures[,-1]\n" +
                "t <- cbind(\"Order\"=c(1:nrow(tmpData))," +
                    "\"Mass_Tag_ID\"=as.numeric(rownames(tmpData)))\n" +
                "t <- merge(x=t, y=tmpFeatures," +
                    "by.x=\"Mass_Tag_ID\", by.y=\"row.names\")\n" +
                "t <- t[order(t$Order),]\n" +
                "rownames(t) <- t$Mass_Tag_ID\n" +
                "t <- t[,-1]\n" +
                "fData <- new(\"AnnotatedDataFrame\", data=t)\n" +
                "rm(t)\n" +
                "rm(tmpFeatures)",
                Parameters["rowMetaData"],
                Parameters["rowMetaDataLink"]);

            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Retrieving Feature Data from SQLite",
                this.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;
        }

        /// <summary>
        /// Pulls a data table from a SQLite database, and cleans it up
        /// so there are no row duplicates
        /// </summary>
        /// <param name="RInstance">Instance of the R Workspace</param>
        public void GetDataTableFromSQLiteDB()
        {
            string s_RStatement = string.Format(
                 "rt <- dbSendQuery(con, \"SELECT * FROM {0}\")\n" +
                 "{1} <- fetch(rt, n = -1)\n" +
                 "dbClearResult(rt)\n" +
                 "DataCleaning({1})",
                 dsp.InputTableName,
                 dsp.NewTableName);

            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Retrieving Table from SQLite",
                this.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;
        }

        /// <summary>
        /// Pulls a column metadata table from a SQLite database, and
        /// cleans it up so there are no row duplicates
        /// </summary>
        /// <param name="RInstance">Instance of the R Workspace</param>
        public void GetColumnMetadataFromSQLiteDB()
        {            
            string s_RStatement = string.Format(
                 "rt <- dbSendQuery(con, \"SELECT * FROM {0}\")\n" +
                 "{1} <- fetch(rt, n = -1)\n" +
                 "dbClearResult(rt)\n" +
                 "DataCleaning({1})",
                 dsp.InputTableName,
                 dsp.NewTableName);

            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Retrieving Column Metadata from SQLite",
                this.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;
        }

        /// <summary>
        /// Pulls the row metadata table from a SQLite database, and
        /// cleans it up so there are no row duplicates
        /// </summary>
        /// <param name="RInstance">Instance of the R Workspace</param>
        public void GetRowMetadataFromSQLiteDB()
        {
            string s_RStatement = string.Format(
                                    "rt <- dbSendQuery(con, \"SELECT * FROM {0}\")\n" +
                                    "{1} <- fetch(rt, n = -1)\n" +
                                    "dbClearResult(rt)\n" +
                                    "DataCleaning({1})",
                                    dsp.InputTableName,
                                    dsp.NewTableName);

            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Retrieving Row Metadata from SQLite",
                this.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;
        }

        /// <summary>
        /// Sets the rownames for a table in R and removes that column from the table
        /// </summary>
        /// <param name="TableName">Name of the table</param>
        public void SetTableRowNames(string TableName)
        {
            if (Parameters.ContainsKey("rowNames"))
            {
                string s_RStatement = string.Format(
                    "rownames({0}) <- {0}[,{1}]\n" +
                    "{0} <- {0}[,-{1}]",
                    TableName,
                    dsp.RowNames);

                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "Setting Rownames on Table",
                    this.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;
            }
        }

        public void SetDataMatrix(string TableName)
        {
            if (dsp.AsDataMatrix.Equals("true"))
            {
                string s_RStatement = string.Format(
                    "{0} <- data.matrix({0})",
                    TableName);

                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "Setting Table to data.matrix",
                    this.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;
            }
        }

        private string GetModuleNameExtension()
        {
            string s_Return = "";
            if (dsp.TableType.Equals("dataTableName"))
            {
                s_Return = ". Importing DataTable: " + dsp.InputTableName + 
                    " -> " + dsp.NewTableName;
            }
            else if (dsp.TableType.Equals("rowMetaDataTableName"))
            {
                s_Return = ". Importing Row Metadata Table: " + dsp.InputTableName +
                    " -> " + dsp.NewTableName;
            }
            else if (dsp.TableType.Equals("columnMetaDataTableName"))
            {
                s_Return = ". Importing Column Metadata Table: " + dsp.InputTableName +
                    " -> " + dsp.NewTableName;
            }
            return s_Return;
        }

        private void ReadCSV_File()
        {
            string s_RStatement = string.Format("{0} <- read.csv(\"{1}\")",
                            dsp.NewTableName,
                            dsp.WorkDirectory + "/" + dsp.InputFileName);

            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Importing CSV",
                this.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;
        }

        private void ReadTSV_File()
        {
            string s_RStatement = string.Format("{0} <- read.table(" +
                            "file=\"{1}\", sep=\"\\t\", header=T)",
                            dsp.NewTableName,
                            dsp.WorkDirectory + "/" + dsp.InputFileName);

            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Importing TSV",
                this.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;
        }


        /// <summary>
        /// Unit Test for Importing a CSV file
        /// </summary>
        /// <returns>Information regarding the result of the UnitTest</returns>
        public clsTestResult TestCSV_FileImport()
        {
            dsp.GetParameters(ModuleName, Parameters);
            clsTestResult result = new clsTestResult(true, "");
            result.Module = ModuleName;

            try
            {
                if (!CheckPassedParameters())
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING CSV FILE: Not all required parameters were passed in!";
                    return result;
                }
                    
                ReadCSV_File();
                if (dsp.RowNames.Length > 0)
                    SetTableRowNames(dsp.NewTableName);

                // Confirm by testing if the new table exists within the environment
                if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.NewTableName))
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING CSV FILE: After importing csv, " +
                        "the new table name could not be found within the R workspace";
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }

                System.Data.DataTable dt = clsGenericRCalls.GetDataTable(s_RInstance, dsp.NewTableName);
                if (dt.Columns.Count != 35)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING CSV FILE: After importing csv, " +
                        "the new table was supposed to have 35 columns, and instead has " +
                        dt.Columns.Count;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
                if (dt.Rows.Count != 25)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING CSV FILE: After importing csv, " +
                        "the new table was supposed to have 25 columns, and instead has " +
                        dt.Rows.Count;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
            }
            catch (Exception exc)
            {
                result.IsSuccessful = false;
                result.Message = "ERROR IMPORTING CSV FILE: " + dsp.InputFileName + "\n\n" + exc.ToString();
                result.R_Statement = s_Current_R_Statement;
            }

            return result;
        }

        /// <summary>
        /// Unit Test for Importing a TSV file
        /// </summary>
        /// <returns>Information regarding the result of the UnitTest</returns>
        public clsTestResult TestTSV_FileImport()
        {
            dsp.GetParameters(ModuleName, Parameters);
            clsTestResult result = new clsTestResult(true, "");
            result.Module = ModuleName;

            try
            {
                if (!CheckPassedParameters())
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING TSV FILE: Not all required parameters were passed in!";
                    return result;
                }

                ReadTSV_File();

                if (dsp.RowNames.Length > 0)
                    SetTableRowNames(dsp.NewTableName);

                // Confirm by testing if the new table exists within the environment
                if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.NewTableName))
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING TSV FILE: After importing tsv, " +
                        "the new table name could not be found within the R workspace";
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }

                System.Data.DataTable dt = clsGenericRCalls.GetDataTable(s_RInstance, dsp.NewTableName);
                if (dt.Columns.Count != 35)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING CSV FILE: After importing tsv, " +
                        "the new table was supposed to have 35 columns, and instead has " +
                        dt.Columns.Count;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
                if (dt.Rows.Count != 25)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING CSV FILE: After importing tsv, " +
                        "the new table was supposed to have 25 columns, and instead has " +
                        dt.Rows.Count;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
            }
            catch (Exception exc)
            {
                result.IsSuccessful = false;
                result.Message = "ERROR IMPORTING TSV FILE: " + dsp.InputFileName + "\n\n" + exc.ToString();
                result.R_Statement = s_Current_R_Statement;
            }

            return result;
        }

        /// <summary>
        /// Unit Test for Importing a Data Table from a SQLite database
        /// </summary>
        /// <returns>Information regarding the result of the UnitTest</returns>
        public clsTestResult TestImportDataTableFromSQLite()
        {
            dsp.GetParameters(ModuleName, Parameters);
            clsTestResult result = new clsTestResult(true, "");
            result.Module = ModuleName;

            try
            {
                if (!CheckPassedParameters())
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING DATATABE FROM SQLITE: Not all required parameters were passed in!";
                    return result;
                }

                ConnectToSQLiteDatabase();
                GetDataTableFromSQLiteDB();
                if (dsp.RowNames.Length > 0)
                    SetTableRowNames(dsp.NewTableName);
                DisconnectFromDatabase();

                // Confirm by testing if the new table exists within the environment
                if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.NewTableName))
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING DATATABE FROM SQLITE: After importing DataTable, " +
                        "the new table name could not be found within the R workspace";
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }

                System.Data.DataTable dt = clsGenericRCalls.GetDataTable(s_RInstance, dsp.NewTableName);
                if (dt.Columns.Count != 66)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING DATA TABLE: After importing data table, " +
                        "the new table was supposed to have 66 columns, and instead has " +
                        dt.Columns.Count;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
                if (dt.Rows.Count != 1311)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING DATA TABLE: After importing data table, " +
                        "the new table was supposed to have 1,311 columns, and instead has " +
                        dt.Rows.Count;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
            }
            catch (Exception exc)
            {
                result.IsSuccessful = false;
                result.Message = "ERROR IMPORTING DATATABE FROM SQLITE: " + dsp.InputFileName + "\n\n" + exc.ToString();
                result.R_Statement = s_Current_R_Statement;
            }

            return result;
        }

        /// <summary>
        /// Unit Test for Importing a Row Metadata Table from a SQLite database
        /// </summary>
        /// <returns>Information regarding the result of the UnitTest</returns>
        public clsTestResult TestImportRowMetadataTableFromSQLite()
        {
            dsp.GetParameters(ModuleName, Parameters);
            clsTestResult result = new clsTestResult(true, "");
            result.Module = ModuleName;

            try
            {
                if (!CheckPassedParameters())
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING ROW METADATATABE FROM SQLITE: Not all required parameters were passed in!";
                    return result;
                }

                ConnectToSQLiteDatabase();
                GetRowMetadataFromSQLiteDB();
                if (dsp.RowNames.Length > 0)
                    SetTableRowNames(dsp.NewTableName);
                DisconnectFromDatabase();

                // Confirm by testing if the new table exists within the environment
                if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.NewTableName))
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING ROW METADATATABE FROM SQLITE: After importing DataTable, " +
                        "the new table name could not be found within the R workspace";
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }

                System.Data.DataTable dt = clsGenericRCalls.GetDataTable(s_RInstance, dsp.NewTableName);
                if (dt.Columns.Count != 5)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING ROW METADATA TABLE: After importing row metadata table, " +
                        "the new table was supposed to have 5 columns, and instead has " +
                        dt.Columns.Count;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
                if (dt.Rows.Count != 22439)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING ROW METADATA TABLE: After importing row metadata table, " +
                        "the new table was supposed to have 22,439 rows, and instead has " +
                        dt.Rows.Count;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
            }
            catch (Exception exc)
            {
                result.IsSuccessful = false;
                result.Message = "ERROR IMPORTING ROW METADATATABE FROM SQLITE: " + dsp.InputFileName + "\n\n" + exc.ToString();
                result.R_Statement = s_Current_R_Statement;
            }

            return result;
        }

        /// <summary>
        /// Unit Test for Importing a Column Metadata Table from a SQLite database
        /// </summary>
        /// <returns>Information regarding the result of the UnitTest</returns>
        public clsTestResult TestImportColumnMetadataTableFromSQLite()
        {
            dsp.GetParameters(ModuleName, Parameters);
            clsTestResult result = new clsTestResult(true, "");
            result.Module = ModuleName;

            try
            {
                if (!CheckPassedParameters())
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING COLUMN METADATATABE FROM SQLITE: Not all required parameters were passed in!";
                    return result;
                }

                ConnectToSQLiteDatabase();
                GetColumnMetadataFromSQLiteDB();
                if (dsp.RowNames.Length > 0)
                    SetTableRowNames(dsp.NewTableName);
                DisconnectFromDatabase();

                // Confirm by testing if the new table exists within the environment
                if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.NewTableName))
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING COLUMN METADATATABE FROM SQLITE: After importing DataTable, " +
                        "the new table name could not be found within the R workspace";
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }

                System.Data.DataTable dt = clsGenericRCalls.GetDataTable(s_RInstance, dsp.NewTableName);
                if (dt.Columns.Count != 13)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING column METADATA TABLE: After importing column metadata table, " +
                        "the new table was supposed to have 13 columns, and instead has " +
                        dt.Columns.Count;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
                if (dt.Rows.Count != 99)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR IMPORTING COLUMN METADATA TABLE: After importing column metadata table, " +
                        "the new table was supposed to have 99 columns, and instead has " +
                        dt.Rows.Count;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
            }
            catch (Exception exc)
            {
                result.IsSuccessful = false;
                result.Message = "ERROR IMPORTING COLUMN METADATATABE FROM SQLITE: " + dsp.InputFileName + "\n\n" + exc.ToString();
                result.R_Statement = s_Current_R_Statement;
            }

            return result;
        }
        #endregion
    }
}
