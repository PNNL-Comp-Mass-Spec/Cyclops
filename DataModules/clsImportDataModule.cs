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
        //private Dictionary<string, string> d_CyclopsParameters = new Dictionary<string, string>();

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
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="InstanceOfR">Path to R DLL</param>
        /// <param name="ParametersForCyclops">Parameters to run Cyclops Pipeline</param>
        //public clsImportDataModule(string InstanceOfR, Dictionary<string, string> ParametersForCyclops)
        //{
        //    ModuleName = "Import Module";
        //    s_RInstance = InstanceOfR;
        //    d_CyclopsParameters = ParametersForCyclops;
        //}
        #endregion

        #region Members
        
        #endregion

        #region Properties
        
        #endregion

        #region Functions

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
                            }
                            if (Parameters.ContainsKey("columnMetaDataTableName"))
                            {
                                GetColumnMetadataFromSQLiteDB(s_RInstance);
                            }
                            if (Parameters.ContainsKey("rowMetaDataTableName"))
                            {
                                GetRowMetadataFromSQLiteDB(s_RInstance);
                            }
                        }
                        engine.EagerEvaluate(s_RStatement);

                        DisconnectFromDatabase(s_RInstance);
                        
                    }
                    catch (ParseException pe)
                    {
                        Console.WriteLine("Error Processing step:\n" + pe.ToString());
                    }
                    catch (AccessViolationException ave)
                    {
                        Console.WriteLine("Access Violation Exception caught:\n" +
                            ave.ToString());
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine("General Exception caught:\n" + exc.ToString());
                    }                    
                    break;
                case (int)ImportDataType.CSV:
                    string s_Read = string.Format("{0} <- read.csv(\"{1}\")",
                        Parameters["newDataTableName"],
                        Parameters["workDir"].ToString().Replace('\\', '/') + "/" + Parameters["dataTableName"]);
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
            REngine engine = REngine.GetInstanceFromID(RInstance);

            string s_InputFileName = "";
            if (!Parameters.ContainsKey(clsCyclopsParametersKey.GetParameterName(
                    "InputFileName")))
            {
                if (Parameters.ContainsKey("workDir"))
                {
                    //s_InputFileName = Path.Combine(Parameters["workDir"].ToString(), "Results.db3");
                    s_InputFileName = Parameters["workDir"].ToString() + "/Results.db3";
                }
                else
                {
                    s_InputFileName = "Results.db3";
                }
            }
            else
            {
                s_InputFileName = Parameters[clsCyclopsParametersKey.GetParameterName(
                        "InputFileName")].ToString().Replace('\\', '/');
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
                    engine.EagerEvaluate(s_RStatement);
                }
                catch (IOException exc)
                {
                    // TODO, handle exception
                }
                catch (AccessViolationException ave)
                {
                    // TODO, handle exception
                }
                catch (Exception ex)
                {
                    // TODO, handle exception
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
            engine.EagerEvaluate(s_RStatement);

            bool b_Disconnected = clsGenericRCalls.AssessBoolean(RInstance, "terminated");

            if (b_Disconnected)
            {
                s_RStatement = "rm(con)\nrm(m)\nrm(terminated)";
                engine.EagerEvaluate(s_RStatement);
            }
            else
            {
                // TODO: throw an exception
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
            engine.EagerEvaluate(s_RStatement);
        }
        #endregion
    }
}
