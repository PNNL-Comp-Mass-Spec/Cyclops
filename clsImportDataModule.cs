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

        #region Constructors
        public clsImportDataModule()
        {
            ModuleName = "Import Module";
        }
        public clsImportDataModule(string InstanceOfR)
        {
            ModuleName = "Import Module";
            s_RInstance = InstanceOfR;
        }
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

        public override void PerformOperation()
        {
            Console.WriteLine("******* IMPORTING DATA! *************");
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            switch (dataType)
            {
                case (int)ImportDataType.SQLite:
                    
                    try
                    {
                        string s_RStatement = "";
                        ConnectToSQLiteDatabase(s_RInstance);

                        //string s_LogFile = @"C:\Users\Joe\Documents\Visual Studio 2010\Projects\Cyclops\LogFiles\ImportFileLog.txt";
                        //StreamWriter sw = new StreamWriter(s_LogFile);
                        //sw.WriteLine(s_RStatement);
                        //sw.Close();

                        /// To create an ExpressionSet, the following data must be supplied:
                        /// ASSAY_DATA: F X S Matrix of expression values. F rows (e.g. peptides), S columns (e.g. MS runs)
                        /// PHENO_DATA: S X V Dataframe describing what the MS runs correspond to
                        /// PHENO_DATA_LINK: Name of the column in PHENO_DATA that links to the AssayData
                        /// ROW_DATA: F X Z Dataframe describing the rows (e.g. peptide to proteins linkage)                        
                        if (Parameters.ContainsKey("expressionSet"))
                        {
                            //s_RStatement = string.Format(
                            //    "{0} <- jnb_CreateExpressionSetFromSQLite(" +
                            //    "sqlitedb={1}, assayData={2}, phenoData={3}," +
                            //    "featureData={4})",
                            //    Parameters["EXPRESSION_SET"],
                            //    Parameters["PATH"],
                            //    Parameters["ASSAY_DATA"],
                            //    Parameters["PHENO_DATA"],
                            //    Parameters["ROW_DATA"]);

                            if (Parameters["source"].Equals("sqlite"))
                            {
                                /// Pull in the AssayData
                                GetAssayDataFromSQLiteDB(s_RInstance);
                                //sw = File.AppendText(s_LogFile);
                                //sw.WriteLine(s_RStatement);

                                /// Pull in the Phenotype Data
                                GetPhenotypeDataFromSQLiteDB(s_RInstance);
                                //sw.WriteLine(s_RStatement);

                                /// Pull in the Features Data
                                GetFeatureDataFromSQLiteDB(s_RInstance);
                                //sw.WriteLine(s_RStatement);   

                                /// Build the ExpressionSet
                                s_RStatement = string.Format("eData <- new(\"ExpressionSet\", exprs=tmpData, " +
                                        "phenoData=pData, featureData=fData)");
                                //sw.WriteLine(s_RStatement);
                                //sw.Close();
                                engine.EagerEvaluate(s_RStatement);
                            }

                            //NumericVector gv_Data = engine.EagerEvaluate("dim(tmpData)").AsNumeric();
                            //Console.WriteLine("Data dimensions:");
                            //foreach (double d in gv_Data)
                            //{
                            //    Console.WriteLine(d);
                            //}
                            //NumericVector gv_Pheno = engine.EagerEvaluate("dim(tmpPheno)").AsNumeric();
                            //Console.WriteLine("Phenotypic dimensions:");
                            //foreach (double d in gv_Pheno)
                            //{
                            //    Console.WriteLine(d);
                            //}
                            //Console.WriteLine("Feature dimensions:");
                            //NumericVector gv_Features = engine.EagerEvaluate("dim(tmpFeatures)").AsNumeric();
                            //foreach (double d in gv_Features)
                            //{
                            //    Console.WriteLine(d);
                            //}


                        }
                        else
                        {
                            if (Parameters.ContainsKey("dataTableName"))
                            {
                                s_RStatement = string.Format(
                                    "rt <- dbSendQuery(con, \"SELECT * FROM {0}\")\n" +
                                    "{1} <- fetch(rt, n = -1)\n" +
                                    "dbClearResult(rt)",
                                    Parameters["dataTableName"],
                                    Parameters["newDataTableName"]);
                                engine.EagerEvaluate(s_RStatement);
                            }
                            if (Parameters.ContainsKey("columnMetaDataTableName"))
                            {
                                s_RStatement = string.Format(
                                    "rt <- dbSendQuery(con, \"SELECT * FROM {0}\")\n" +
                                    "{1} <- fetch(rt, n = -1)\n" +
                                    "dbClearResult(rt)",
                                    Parameters["columnMetaDataTableName"],
                                    Parameters["newColumnMetaDataTableName"]);
                                engine.EagerEvaluate(s_RStatement);
                            }
                            if (Parameters.ContainsKey("rowMetaDataTableName"))
                            {
                                s_RStatement = string.Format(
                                    "rt <- dbSendQuery(con, \"SELECT * FROM {0}\")\n" +
                                    "{1} <- fetch(rt, n = -1)\n" +
                                    "dbClearResult(rt)",
                                    Parameters["rowMetaDataTableName"],
                                    Parameters["newRowMetaDataTableName"]);
                                engine.EagerEvaluate(s_RStatement);
                            }
                            s_RStatement = "rm(con)\n" +
                                "rm(m)\n" +
                                "rm(rt)\n";
                        }
                        engine.EagerEvaluate(s_RStatement);
                        //engine.EagerEvaluate("save.image(\"G:/Visual Studio 2010/Projects/Cyclops/CyclopsApp/Documents/test.RData\")");
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
                    Console.WriteLine("Process with R completed!");
                    break;
                case (int)ImportDataType.CSV:
                    string s_Read = string.Format("{0} <- read.csv(\"{1}\")",
                        Parameters["dataTableName"],
                        Parameters["path"].ToString().Replace('\\', '/'));
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

        protected void ConnectToSQLiteDatabase(string RInstance)
        {
            REngine engine = REngine.GetInstanceFromID(RInstance);
            string s_RStatement = string.Format(
                            "require(RSQLite)\n" +
                            "m <- dbDriver(\"SQLite\", max.con=25)\n" +
                            "con <- dbConnect(m, dbname = \"{0}\")",
                                Parameters["path"].ToString().Replace('\\', '/'));
            engine.EagerEvaluate(s_RStatement);
        }

        protected void GetAssayDataFromSQLiteDB(string RInstance)
        {
            REngine engine = REngine.GetInstanceFromID(RInstance);
            // Pull in the AssayData
            string s_RStatement = string.Format("library(\"Biobase\")\n" +
                "rt <- dbSendQuery(con, \"SELECT * FROM {0}\")\n" +
                "tmpData <- fetch(rt, n=-1)\n" +
                "tmpData <- as.matrix(tmpData)\n" +
                "rownames(tmpData) <- as.numeric(tmpData[,1])\n" +
                "tmpData <- tmpData[,-1]",
                Parameters["assayData"]);
            engine.EagerEvaluate(s_RStatement);
             
        }

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

        public string GetFeatureDataFromSQLiteDB(string s_RInstance)
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
            return s_RStatement;
        }
        #endregion
    }
}
