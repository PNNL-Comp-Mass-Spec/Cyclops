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
using Mage;

namespace Cyclops.ExportModules
{
    /// <summary>
    /// Exports tables from R environment to SQLite database, CSV, TSV, MSAccess, or SQLServer
    /// </summary>
    public class clsExportTableModule : clsBaseExportModule
    {
        #region Members
        private ExportModules.cslExportParameterHandler esp =
            new ExportModules.cslExportParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        private string s_RInstance;
        #endregion

        #region Constructors
        /// <summary>
        /// Exports a data.frame or matrix from R to a designated target (TXT, CSV, etc.)
        /// </summary>
        public clsExportTableModule()
        {
            ModuleName = "Export Table Module";
        }
        /// <summary>
        /// Exports a data.frame or matrix from R to a designated target (TXT, CSV, etc.)
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsExportTableModule(string InstanceOfR)
        {
            ModuleName = "Export Table Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Exports a data.frame or matrix from R to a designated target (TXT, CSV, etc.)
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsExportTableModule(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Export Table Module";
            Model = TheCyclopsModel;
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties
        
        #endregion

        #region Methods
            
        /// <summary>
        /// Runs module
        /// </summary>
        public override void PerformOperation()
        {
            if (Model.SuccessRunningPipeline)
            {
                Model.IncrementStep(ModuleName);

                esp.GetParameters(ModuleName, Parameters);

                if (CheckPassedParameters())
                {
                    switch (esp.Source.ToUpper())
                    {
                        case "R":
                            ExportFromR();
                            break;
                        case "SQLITE":
                            ExportFromSQLite();
                            break;
                    }
                }
            }
        }

        private void ExportFromR()
        {
            string s_Command = "";

            switch (esp.Target.ToUpper())
            {
                case "SQLITE":
                    if (clsGenericRCalls.ContainsObject(s_RInstance, esp.TableName))
                    {
                        ConnectToSQLiteDatabase();
                        s_Command = string.Format("dbWriteTable(" +
                            "conn=con, name=\"{0}\", value=data.frame({1}))",
                            esp.NewTableName,
                            esp.TableName);

                        if (!clsGenericRCalls.Run(s_Command, s_RInstance,
                            "Exporting Table to SQLite",
                            Model.StepNumber, Model.NumberOfModules))
                            Model.SuccessRunningPipeline = false;

                        DisconnectFromDatabase();
                    }
                    else
                    {
                        traceLog.Info("EXPORT TABLE: table '" +
                            esp.TableName + "' was not found in the R workspace, " +
                            "and therefore was not exported to the sqlite database.");
                    }
                    break;
                case "CSV":
                    if (clsGenericRCalls.ContainsObject(s_RInstance, esp.TableName))
                    {
                        if (esp.HasFileName)
                        {
                            if (esp.IncludeRowNames)
                            {
                                s_Command = string.Format("jnb_Write(df={0}, " +
                                    "fileName=\"{1}\", firstColumnHeader=\"{2}\", " +
                                    "sepChar=\"{3}\")",
                                    esp.TableName,
                                    esp.WorkDirectory + "/" + esp.FileName,
                                    esp.RownamesColumnHeader,
                                    esp.SeparatingCharacter.Replace("\"", ""));
                            }
                            else
                            {
                                s_Command = string.Format("jnb_Write(df={0}, " +
                                    "fileName=\"{1}\", row.names=FALSE, " +
                                    "sepChar=\"{2}\")",
                                    esp.TableName,
                                    esp.WorkDirectory + "/" + esp.FileName,
                                    esp.SeparatingCharacter.Replace("\"", ""));
                            }

                            if (!clsGenericRCalls.Run(s_Command, s_RInstance,
                                "Exporting Table to CSV",
                                Model.StepNumber, Model.NumberOfModules))
                                Model.SuccessRunningPipeline = false;
                        }
                    }
                    else
                    {
                        traceLog.Info("EXPORT TABLE: table '" +
                               esp.TableName + "' was not found in the R workspace, " +
                               "and therefore was not exported to csv file.");
                    }
                    break;
                case "TSV":
                    if (clsGenericRCalls.ContainsObject(s_RInstance, esp.TableName))
                    {
                        if (esp.HasFileName)
                        {
                            if (esp.IncludeRowNames)
                            {
                                s_Command = string.Format("jnb_Write(df={0}, " +
                                    "fileName=\"{1}\", firstColumnHeader=\"{2}\", " +
                                    "sepChar=\"{3}\")",
                                    esp.TableName,
                                    esp.WorkDirectory + "/" + esp.FileName,
                                    esp.RownamesColumnHeader,
                                    esp.SeparatingCharacter);
                            }
                            else
                            {
                                s_Command = string.Format("jnb_Write(df={0}, " +
                                    "fileName=\"{1}\", row.names=FALSE, " +
                                    "sepChar=\"{2}\")",
                                    esp.TableName,
                                    esp.WorkDirectory + "/" + esp.FileName,
                                    esp.SeparatingCharacter);
                            }

                            if (!clsGenericRCalls.Run(s_Command, s_RInstance,
                                "Exporting Table to TSV",
                                Model.StepNumber, Model.NumberOfModules))
                                Model.SuccessRunningPipeline = false;
                        }
                    }
                    else
                    {
                        traceLog.Info("EXPORT TABLE: table '" +
                               esp.TableName + "' was not found in the R workspace, " +
                               "and therefore was not exported to tsv file.");
                    }
                    break;
                case "ACCESS":

                    break;
                case "SQLSERVER":

                    break;
            }
        }

        private void ExportFromSQLite()
        {
            if (esp.Target.ToLower().Equals("tsv") ||
                esp.Target.ToLower().Equals("txt") ||
                esp.Target.ToLower().Equals("csv"))
            {
                SQLiteReader reader = new SQLiteReader();
                reader.Database = Path.Combine(esp.WorkDirectory, esp.InputFileName);
                reader.SQLText = string.Format("SELECT * FROM {0}",
                    esp.TableName);

                DelimitedFileWriter writer = new DelimitedFileWriter();
                writer.FilePath = Path.Combine(esp.WorkDirectory, esp.FileName);
                writer.Delimiter = esp.SeparatingCharacter.Equals("\\t") ? "\t" : esp.SeparatingCharacter;


                traceLog.Info("Writing SQLite table out to " + esp.Target + "\n" +
                    "SQLite database: " + reader.Database + "\n" +
                    "Table name: " + esp.TableName + "\n" +
                    "Output filename: " + writer.FilePath);

                try
                {
                    ProcessingPipeline.Assemble("Writing Table out to Text", reader, writer).RunRoot(null);
                }
                catch (Exception exc)
                {
                    traceLog.Error("ERROR Exporting SQLite table to text: " + exc.ToString());
                }
            }
        }

        /// <summary>
        /// Determine is all the necessary parameters are being passed to the object
        /// </summary>
        /// <returns>Returns true import module can proceed</returns>
        public bool CheckPassedParameters()
        {
            bool b_2Param = true; 

            // NECESSARY PARAMETERS
            if (!esp.HasSource)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR ExportTable: 'source' was not found in the passed parameters");
                b_2Param = false;
            }
            if (!esp.HasTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR ExportTable: 'tableName' was not found in the passed parameters");
                b_2Param = false;
            }
            if (!esp.HasWorkDirectory)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR ExportTable: 'workDir' was not found in the passed parameters");
                b_2Param = false;
            }

            return b_2Param;
        }

        /// <summary>
        /// Creates a connection to a sqlite database
        /// </summary>
        /// <param name="RInstance">Instance of your R workspace</param>
        protected void ConnectToSQLiteDatabase()
        {
            string s_DatabaseFileName = "";
            if (esp.HasWorkDirectory & esp.HasFileName)
                s_DatabaseFileName = esp.WorkDirectory + "/" + esp.FileName;
            else if (esp.HasFileName)
                s_DatabaseFileName = esp.FileName;
            else
                s_DatabaseFileName = esp.WorkDirectory + "/Results.db3";

            if (File.Exists(s_DatabaseFileName))
            {
                string s_RStatement = string.Format(
                                "require(RSQLite)\n" +
                                "m <- dbDriver(\"SQLite\", max.con=25)\n" +
                                "con <- dbConnect(m, dbname = \"{0}\")",
                                    s_DatabaseFileName);

                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "Connecting to SQLite Database",
                    Model.StepNumber, Model.NumberOfModules))
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

            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Disconnecting from Database",
                Model.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;

            bool b_Disconnected = clsGenericRCalls.AssessBoolean(s_RInstance, "terminated");

            if (b_Disconnected)
            {
                s_RStatement = "rm(con)\nrm(m)\nrm(terminated)\nrm(rt)";
                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "Cleaning Database Connection",
                    Model.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;
            }
            else
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("Cyclops was unsuccessful at disconnecting from SQLITE database");
            }
        }
        #endregion
    }
}
