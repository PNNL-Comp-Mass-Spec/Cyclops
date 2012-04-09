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
using System.Data;
using System.Data.SQLite;
using System.IO;

using System.Data.SQLite;

using log4net;

namespace Cyclops
{
    public static class clsSQLiteHandler
    {
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        /// <summary>
        /// Retrieves a table from SQLite Database, and returns it in the 
        /// form of a DataTable.
        /// 
        /// http://msdn.microsoft.com/en-us/magazine/ff898405.aspx
        /// Found the solution to the SQLite problem right here.
        /// </summary>
        /// <param name="Command">SQL statement to grab table information</param>
        /// <param name="Connection">Connection to the SQLite Database</param>
        /// <returns></returns>
        public static DataTable GetDataTable(string Command, string Connection)
        {
            DataTable dt = new DataTable();

            try
            {
                var connStr = new SQLiteConnectionStringBuilder()
                {
                    DataSource = Connection
                };

                using (SQLiteConnection conn = new SQLiteConnection(connStr.ToString()))
                {
                    conn.Open();
                    SQLiteCommand cmd = conn.CreateCommand();
                    cmd.CommandText = Command;

                    cmd = conn.CreateCommand();
                    cmd.CommandText = Command;
                    SQLiteDataReader reader = cmd.ExecuteReader();
                    dt.Load(reader);
                    conn.Close();
                }

            }
            catch (IOException ioe)
            {
                traceLog.Error("SQLite Handler IOException in GetDataTable(): " +
                    ioe.ToString());
            }
            catch (Exception exc)
            {
                traceLog.Error("SQLite Handler Exception in GetDataTable(): " +
                    exc.ToString());
            }
            return dt;
        }

        /// <summary>
        /// Creates a table within a SQLite database
        /// </summary>
        /// <param name="Connection">Path and Filename of the SQLite database</param>
        /// <param name="TableName">Name of the new table to create</param>
        /// <param name="AutoID">Create an autoincrementing primary key in the first column of the table</param>
        /// <param name="Columns">Keys are column names and values are the type of variable</param>
        public static void CreateTable(string Connection, string TableName, bool AutoID, Dictionary<string, string> Columns)
        {
            string s_Command = "CREATE " + TableName + "(" +
                (AutoID ? "ID INTEGER PRIMARY KEY AUTOINCREMENT, " : "");

            foreach (string k in Columns.Keys)
            {
                s_Command += k + " " + Columns[k] + ", ";
            }
            s_Command = s_Command.Substring(0, s_Command.Length - 2);   // removes the last comma and space
            s_Command += ");";
 
            try
            {
                var connStr = new SQLiteConnectionStringBuilder()
                {
                    DataSource = Connection
                };

                using (SQLiteConnection conn = new SQLiteConnection(connStr.ToString()))
                {
                    conn.Open();
                    SQLiteCommand cmd = conn.CreateCommand();
                    cmd.CommandText = s_Command;

                    cmd = conn.CreateCommand();
                    cmd.CommandText = s_Command;
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

            }
            catch (IOException ioe)
            {
                traceLog.Error("SQLite Handler IOException in CreateTable(): " +
                    ioe.ToString());
            }
            catch (Exception exc)
            {
                traceLog.Error("SQLite Handler Exception in CreateTable(): " +
                    exc.ToString());
            }
        }

        /// <summary>
        /// Runs a NonQuery against the specified database
        /// </summary>
        /// <param name="Command">Command to execute</param>
        /// <param name="Connection">Path and Filename of the SQLite database</param>
        public static void RunNonQuery(string Command, string Connection)
        {
            try
            {
                var connStr = new SQLiteConnectionStringBuilder()
                {
                    DataSource = Connection
                };

                using (SQLiteConnection conn = new SQLiteConnection(connStr.ToString()))
                {
                    conn.Open();
                    SQLiteCommand cmd = conn.CreateCommand();
                    cmd.CommandText = Command;

                    cmd = conn.CreateCommand();
                    cmd.CommandText = Command;
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

            }
            catch (IOException ioe)
            {
                traceLog.Error("SQLite Handler IOException in RunNonQuery(): " +
                    ioe.ToString());
            }
            catch (Exception exc)
            {
                traceLog.Error("SQLite Handler Exception in RunNonQuery(): " +
                    exc.ToString());
            }
        }

        /// <summary>
        /// Creates an index on a column within a table
        /// </summary>
        /// <param name="Connection">Path and Filename of the SQLite database</param>
        /// <param name="Table">Table name</param>
        /// <param name="Column">Name of Column to index within the table</param>
        /// <param name="IndexName">Name of the index, if null/blank, one will be automatically generated</param>
        public static void CreateIndex(string Connection,
            string Table, string Column, string IndexName)
        {
            try
            {
                if (string.IsNullOrEmpty(IndexName))
                    IndexName = "idx_" + Table + "_" + Column;

                string s_Command = string.Format(
                    "CREATE INDEX {0} ON {1}({2})",
                    IndexName,
                    Table,
                    Column);

                traceLog.Info("SQLite Handler Creating Index: " + s_Command);

                var connStr = new SQLiteConnectionStringBuilder()
                {
                    DataSource = Connection
                };

                using (SQLiteConnection conn = new SQLiteConnection(connStr.ToString()))
                {
                    conn.Open();
                    SQLiteCommand cmd = conn.CreateCommand();
                    cmd.CommandText = s_Command;

                    cmd = conn.CreateCommand();
                    cmd.CommandText = s_Command;
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (IOException ioe)
            {
                traceLog.Error("SQLite Handler IOException in CreateIndex(): " +
                    ioe.ToString());
            }
            catch (Exception exc)
            {
                traceLog.Error("SQLite Handler Exception in CreateIndex(): " +
                    exc.ToString());
            }
        }

        /// <summary>
        /// DataTable to SQLite Table, if the SQLite table does not exist, it 
        /// automatically creates that table and updates it.
        /// </summary>
        /// <param name="PathToDatabase">Full path the the sqlite database</param>
        /// <param name="Table">Source DataTable</param>
        /// <param name="TableName">Name of table in SQLite database</param>
        public static void WriteDataTableToSQLiteTable(string PathToDatabase,
            DataTable Table, string TableName)
        {
            try
            {
                // make the Mage module that will source the .Net DataTable object
                clsMageDataTableSource source = new clsMageDataTableSource();
                source.SourceTable = Table;

                // make the Mage module that will write data to SQLite database
                Mage.SQLiteWriter writer = new Mage.SQLiteWriter();
                writer.DbPath = PathToDatabase;
                writer.TableName = TableName;

                // build and run Mage pipeline
                Mage.ProcessingPipeline.Assemble("Write_Pipeline", source, writer).RunRoot(null);
            }
            catch (IOException ioe)
            {
                traceLog.Error("SQLite Handler IOException in WriteDataTableToSQLiteTable(): " +
                    ioe.ToString());
            }
            catch (Exception exc)
            {
                traceLog.Error("SQLite Handler Exception in WriteDataTableToSQLiteTable(): " +
                    exc.ToString());
            }
        }

        /// <summary>
        /// Tab-delimited Text File to SQLite Table, if the SQLite table does not exist,
        /// it automatically creates that table and updates it.
        /// </summary>
        /// <param name="PathToDatabase">Full path the the sqlite database</param>
        /// <param name="PathToTextFile">Full path the the tab-delimited text file</param>
        /// <param name="TableName">Name of table in SQLite database</param>
        public static void WriteTabDelimitedTextFileToSQLiteTable(string PathToDatabase,
            string PathToTextFile, string TableName)
        {
            try
            {
                // Create a delimited file reader and write a new table with this info to database
                Mage.DelimitedFileReader fileReader = new Mage.DelimitedFileReader();
                fileReader.FilePath = PathToTextFile;

                // make the Mage module that will write the text file to SQLite database
                Mage.SQLiteWriter writer = new Mage.SQLiteWriter();
                writer.DbPath = PathToDatabase;
                writer.TableName = TableName;

                // build and run Mage pipeline
                Mage.ProcessingPipeline.Assemble("Write_Pipeline", fileReader, writer).RunRoot(null);
            }
            catch (IOException ioe)
            {
                traceLog.Error("SQLite Handler IOException in WriteTabDelimitedTextFileToSQLiteTable(): " +
                    ioe.ToString());
            }
            catch (Exception exc)
            {
                traceLog.Error("SQLite Handler Exception in WriteTabDelimitedTextFileToSQLiteTable(): " +
                    exc.ToString());
            }
        }
    }
}
