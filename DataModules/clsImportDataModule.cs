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

namespace Cyclops
{
    /// <summary>
    /// Class pulls tables from SQLite, CSV, TSV, MSAccess, or SQLServer and 
    /// adds them as DataFrames in R. Designed to created ExpressionSets in 
    /// R for further data analysis.
    /// </summary>
    public class clsImportDataModule : clsBaseDataModule
    {
        public enum ImportDataType { SQLite, CSV, TSV, MSAccess, SQLServer };
        private int dataType;
        private string s_RInstance;
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        #region Constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        public clsImportDataModule()
        {            
            ModuleName = "Import Module";
        }
        /// <summary>
        /// Constructor that requires the Name of the R instance
        /// </summary>
        /// <param name="InstanceOfR">Path to R DLL</param>
        public clsImportDataModule(string InstanceOfR)
        {
            ModuleName = "Import Module";
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
        /// Sets the DataType that the object is going to pull the data from
        /// </summary>
        /// <param name="DataType">ImportDataType</param>
        public void SetDataType(ImportDataType DataType)
        {
            dataType = (int)DataType;
        }

        /// <summary>
        /// Given the list of parameters from the Dictionary Parameters,
        /// determine where the source of the incoming data is and set the DataType
        /// </summary>
        public void SetDataTypeFromParameters()
        {
            string s_DataType = Parameters["source"].ToString();
            switch (s_DataType)
            {
                case "sqlite":
                    SetDataType(ImportDataType.SQLite);
                    break;
                case "msAccess":
                    SetDataType(ImportDataType.MSAccess);
                    break;
                case "csv":
                    SetDataType(ImportDataType.CSV);
                    break;
                case "tsv":
                    SetDataType(ImportDataType.TSV);
                    break;
                case "sqlServer":
                    SetDataType(ImportDataType.SQLServer);
                    break;
            }
        }

        /// <summary>
        ///  Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            traceLog.Info("Cyclops Importing Data From " + Parameters["source"].ToString() + ".");

            // Determine what source the data is coming from
            SetDataTypeFromParameters();

            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            switch (dataType)
            {
                case (int)ImportDataType.SQLite:
                    
                    try
                    {
                        string s_RStatement = "";
                        ConnectToSQLiteDatabase(s_RInstance);
                        
                        /// To create an ExpressionSet, the following data must be supplied:
                        /// ASSAY_DATA: F X S Matrix of expression values. F rows (e.g. peptides), S columns (e.g. MS runs)
                        /// PHENO_DATA: S X V Dataframe describing what the MS runs correspond to
                        /// PHENO_DATA_LINK: Name of the column in PHENO_DATA that links to the AssayData
                        /// ROW_DATA: F X Z Dataframe describing the rows (e.g. peptide to proteins linkage)                        
                        if (Parameters.ContainsKey("expressionSet"))
                        {
                            /// Pull in the AssayData
                            GetAssayDataFromSQLiteDB(s_RInstance);

                            /// Pull in the Phenotype Data
                            GetPhenotypeDataFromSQLiteDB(s_RInstance);

                            /// Pull in the Features Data
                            GetFeatureDataFromSQLiteDB(s_RInstance);

                            /// Build the ExpressionSet
                            s_RStatement = string.Format("eData <- new(\"ExpressionSet\", exprs=tmpData, " +
                                    "phenoData=pData, featureData=fData)\n" +
                                    "rm(tmpData)\n" +
                                    "rm(pData)\n" +
                                    "rm(fData)");                            
                        }
                        else
                        {
                            if (Parameters.ContainsKey("dataTableName"))
                            {
                                GetDataTableFromSQLiteDB(s_RInstance);
                                SetTableRowNames(s_RInstance, Parameters["newDataTableName"]);
                                
                            }
                            if (Parameters.ContainsKey("columnMetaDataTableName"))
                            {
                                GetColumnMetadataFromSQLiteDB(s_RInstance);
                                SetTableRowNames(s_RInstance, Parameters["newColumnMetaDataTableName"]);
                                
                            }
                            if (Parameters.ContainsKey("rowMetaDataTableName"))
                            {
                                GetRowMetadataFromSQLiteDB(s_RInstance);
                                SetTableRowNames(s_RInstance, Parameters["newRowMetaDataTableName"]);                                
                            }
                        }

                        traceLog.Info("Importing Table: " + s_RStatement);
                        engine.EagerEvaluate(s_RStatement);

                        DisconnectFromDatabase(s_RInstance);
                        
                    }
                    catch (ParseException pe)
                    {
                        //clsLogTools.WriteLog(clsLogTools.LoggerTypes.LogFile, clsLogTools.LogLevels.ERROR,
                        //    "Cyclops encountered a ParseException while reading from SQLite DB: " +
                        //    pe.ToString() + ".");
                    }
                    catch (AccessViolationException ave)
                    {
                        //clsLogTools.WriteLog(clsLogTools.LoggerTypes.LogFile, clsLogTools.LogLevels.ERROR,
                        //    "Cyclops encountered an AccessViolationException while reading from SQLite DB: " +
                        //    ave.ToString() + ".");
                    }
                    catch (Exception exc)
                    {
                        //clsLogTools.WriteLog(clsLogTools.LoggerTypes.LogFile, clsLogTools.LogLevels.ERROR,
                        //    "Cyclops encountered an Exception while reading from SQLite DB: " +
                        //    exc.ToString() + ".");
                    }                    
                    break;
                case (int)ImportDataType.CSV:
                    string s_Read = string.Format("{0} <- read.csv(\"{1}\")",
                        Parameters["newDataTableName"],
                        Parameters["workDir"].Replace('\\', '/') + "/" + Parameters["dataTableName"]);

                    traceLog.Info("Importing CSV: " + s_Read);
                    engine.EagerEvaluate(s_Read);
                    break;
                case (int)ImportDataType.TSV:

                    break;
                case (int)ImportDataType.SQLServer:

                    break;
                case (int)ImportDataType.MSAccess:

                    break;
            }

            RunChildModules();
        }

        /// <summary>
        /// Creates a connection to a sqlite database
        /// </summary>
        /// <param name="RInstance">Instance of your R workspace</param>
        protected void ConnectToSQLiteDatabase(string RInstance)
        {
            //clsLogTools.WriteLog(clsLogTools.LoggerTypes.LogFile, clsLogTools.LogLevels.INFO,
            //    "Cyclops connecting to SQLite database.");

            REngine engine = REngine.GetInstanceFromID(RInstance);

            string s_InputFileName = "";
            if (!Parameters.ContainsKey(clsCyclopsParametersKey.GetParameterName(
                    "InputFileName")))
            {
                if (Parameters.ContainsKey("workDir"))
                {
                    //s_InputFileName = Path.Combine(Parameters["workDir"].ToString(), "Results.db3");
                    s_InputFileName = Parameters["workDir"] + "/Results.db3";
                }
                else
                {
                    s_InputFileName = "Results.db3";
                }
            }
            else
            {
                s_InputFileName = Parameters[clsCyclopsParametersKey.GetParameterName(
                        "InputFileName")].Replace('\\', '/');
            }

            if (File.Exists(s_InputFileName))
            {
                string s_RStatement = string.Format(
                                "require(RSQLite)\n" +
                                "m <- dbDriver(\"SQLite\", max.con=25)\n" +
                                "con <- dbConnect(m, dbname = \"{0}\")",
                                    s_InputFileName);
                s_RStatement = s_RStatement.Replace('\\', '/');

                try
                {
                    traceLog.Info("Connecting to SQLite Database: " + s_RStatement);
                    engine.EagerEvaluate(s_RStatement);
                }
                catch (IOException exc)
                {
                    traceLog.Error("Cyclops encountered an IOException while connecting to SQLite database: " +
                        exc.ToString() + ".");
                }
                catch (AccessViolationException ave)
                {
                    traceLog.Error("Cyclops encountered an AccessViolationException while connecting to SQLite database: " +
                        ave.ToString() + ".");
                }
                catch (Exception ex)
                {
                    traceLog.Error("Cyclops encountered an Exception while connecting to SQLite database:" +
                        ex.ToString() + ".");
                }
            }
        }

        /// <summary>
        /// Terminates the connection to the SQLite database, releasing control of the database.
        /// </summary>
        /// <param name="RInstance">Instance of the R Workspace</param>
        public void DisconnectFromDatabase(string RInstance)
        {            
            REngine engine = REngine.GetInstanceFromID(RInstance);
            
            string s_RStatement = "terminated <- dbDisconnect(con)";

            traceLog.Info("Disconnecting from Database: " + s_RStatement);
            engine.EagerEvaluate(s_RStatement);

            bool b_Disconnected = clsGenericRCalls.AssessBoolean(RInstance, "terminated");

            if (b_Disconnected)
            {
                s_RStatement = "rm(con)\nrm(m)\nrm(terminated)\nrm(rt)";
                traceLog.Info("Cleaning Database Connection: " + s_RStatement);
                engine.EagerEvaluate(s_RStatement);
            }
            else
            {
                traceLog.Error("Cyclops was unsuccessful at disconnecting from SQLITE database");
            }
        }


        /// <summary>
        /// Retrieves a table from the SQLite database and converts it to
        /// an AssayData table that can be easily entered into an
        /// ExpressionSet
        /// </summary>
        /// <param name="RInstance">Instance of the R Workspace</param>
        protected void GetAssayDataFromSQLiteDB(string RInstance)
        {
            REngine engine = REngine.GetInstanceFromID(RInstance);
            // Pull in the AssayData
            string s_RStatement = string.Format("library(\"Biobase\")\n" +
                "rt <- dbSendQuery(con, \"SELECT * FROM {0}\")\n" +
                "tmpData <- fetch(rt, n=-1)\n" +
                "tmpData <- as.matrix(tmpData)\n" +
                "rownames(tmpData) <- as.numeric(tmpData[,1])\n" +
                "tmpData <- tmpData[,-1]\n" +
                "DataCleaning(tmpData)",
                Parameters["assayData"]);

            traceLog.Info("Retrieving Assay Data from SQLite: " + s_RStatement);
            engine.EagerEvaluate(s_RStatement);             
        }

        /// <summary>
        /// Gets the Phenotypic metadata (column metadata) for the 
        /// Expression set
        /// </summary>
        /// <param name="RInstance">Instance of the R Workspace</param>
        protected void GetPhenotypeDataFromSQLiteDB(string RInstance)
        {
            REngine engine = REngine.GetInstanceFromID(RInstance);
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

            traceLog.Info("Retrieving Phenotype Data from SQLite: " + s_RStatement);
            engine.EagerEvaluate(s_RStatement);
        }

        /// <summary>
        /// Gets the Feature metadata (row metadata) for the 
        /// Expression set
        /// </summary>
        /// <param name="s_RInstance">Instance of the R Workspace</param>
        public void GetFeatureDataFromSQLiteDB(string s_RInstance)
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
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

            traceLog.Info("Retrieving Feature Data from SQLite: " + s_RStatement);
            engine.EagerEvaluate(s_RStatement);
        }

        /// <summary>
        /// Pulls a data table from a SQLite database, and cleans it up
        /// so there are no row duplicates
        /// </summary>
        /// <param name="RInstance">Instance of the R Workspace</param>
        public void GetDataTableFromSQLiteDB(string RInstance)
        {
            REngine engine = REngine.GetInstanceFromID(RInstance);
            string s_RStatement = string.Format(
                 "rt <- dbSendQuery(con, \"SELECT * FROM {0}\")\n" +
                 "{1} <- fetch(rt, n = -1)\n" +
                 "dbClearResult(rt)\n" +
                 "DataCleaning({1})",
                 Parameters["dataTableName"],
                 Parameters["newDataTableName"]);

            traceLog.Info("Retrieving Table from SQLite: " + s_RStatement);
            engine.EagerEvaluate(s_RStatement);
        }

        /// <summary>
        /// Pulls a column metadata table from a SQLite database, and
        /// cleans it up so there are no row duplicates
        /// </summary>
        /// <param name="RInstance">Instance of the R Workspace</param>
        public void GetColumnMetadataFromSQLiteDB(string RInstance)
        {
            REngine engine = REngine.GetInstanceFromID(RInstance);
            string s_RStatement = string.Format(
                  "rt <- dbSendQuery(con, \"SELECT * FROM {0}\")\n" +
                  "{1} <- fetch(rt, n = -1)\n" +
                  "dbClearResult(rt)\n" +
                  "DataCleaning({1})",
                  Parameters["columnMetaDataTableName"],
                  Parameters["newColumnMetaDataTableName"]);

            traceLog.Info("Retrieving Column Metadata from SQLite: " + s_RStatement);
            engine.EagerEvaluate(s_RStatement);
        }

        /// <summary>
        /// Pulls the row metadata table from a SQLite database, and
        /// cleans it up so there are no row duplicates
        /// </summary>
        /// <param name="RInstance">Instance of the R Workspace</param>
        public void GetRowMetadataFromSQLiteDB(string RInstance)
        {
            REngine engine = REngine.GetInstanceFromID(RInstance);
            string s_RStatement = string.Format(
                                    "rt <- dbSendQuery(con, \"SELECT * FROM {0}\")\n" +
                                    "{1} <- fetch(rt, n = -1)\n" +
                                    "dbClearResult(rt)\n" +
                                    "DataCleaning({1})",
                                    Parameters["rowMetaDataTableName"],
                                    Parameters["newRowMetaDataTableName"]);

            traceLog.Info("Retrieving Row Metadata from SQLite: " + s_RStatement);
            engine.EagerEvaluate(s_RStatement);
        }

        /// <summary>
        /// Sets the rownames for a table in R and removes that column from the table
        /// </summary>
        /// <param name="RInstance">>Instance of the R Workspace</param>
        /// <param name="TableName">Name of the table</param>
        public void SetTableRowNames(string RInstance, string TableName)
        {
            if (Parameters.ContainsKey("rowNames"))
            {
                REngine engine = REngine.GetInstanceFromID(RInstance);
                string s_RStatement = string.Format(
                    "rownames({0}) <- {0}[,{1}]\n" +
                    "{0} <- {0}[,-{1}]",
                    TableName,
                    Parameters["rowNames"]);

                traceLog.Info("Setting Rownames on Table: " + s_RStatement);
                engine.EagerEvaluate(s_RStatement);
            }
        }

        private string GetModuleNameExtension()
        {
            string s_Return = "";
            if (Parameters.ContainsKey("dataTableName"))
            {
                s_Return = ". Importing DataTable: " + Parameters["dataTableName"] + 
                    " -> " + Parameters["newDataTableName"];
            }
            else if (Parameters.ContainsKey("rowMetaDataTableName"))
            {
                s_Return = ". Importing Row Metadata Table: " + Parameters["rowMetaDataTableName"] +
                    " -> " + Parameters["newRowMetaDataTableName"];
            }
            else if (Parameters.ContainsKey("columnMetaDataTableName"))
            {
                s_Return = ". Importing Column Metadata Table: " + Parameters["columnMetaDataTableName"] +
                    " -> " + Parameters["newColumnMetaDataTableName"];
            }
            return s_Return;
        }

        /// <summary>
        /// Checks the dictionary to ensure all the necessary parameters are present
        /// </summary>
        /// <returns>True if all necessary parameters are present</returns>
        protected bool CheckPassedParameters()
        {
            // NECESSARY PARAMETERS


            return true;
        }
        #endregion
    }
}
