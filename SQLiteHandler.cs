/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: http://omics.pnl.gov/software
 * -----------------------------------------------------
 *
 * Licensed under the Apache License, Version 2.0; you may not use this
 * file except in compliance with the License.  You may obtain a copy of the
 * License at https://opensource.org/licenses/Apache-2.0
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Cyclops
{
    /// <summary>
    /// Wrapper class to make it easier to work with SQLite Databases
    /// </summary>
    public class SQLiteHandler : DatabaseHandler
    {
        #region Members
        private readonly string[] m_SQLiteKeywords = {
            "ABORT", "ACTION", "ADD", "AFTER", "ALL", "ALTER", "ANALYZE", "AND",
            "AS", "ASC", "ATTACH", "AUTOINCREMENT", "BEFORE", "BEGIN", "BETWEEN",
            "BY", "CASCADE", "CASE", "CAST", "CHECK", "COLLATE", "COLUMN", "COMMIT",
            "CONFLICT", "CONSTRAINT", "CREATE", "CROSS", "CURRENT_DATE", "CURRENT_TIME",
            "CURRENT_TIMESTAMP", "DATABASE", "DEFAULT", "DEFERRABLE", "DEFERRED",
            "DELETE", "DESC", "DETACH", "DISTINCT", "DROP", "EACH", "ELSE", "END",
            "ESCAPE", "EXCEPT", "EXCLUSIVE", "EXISTS", "EXPLAIN", "FAIL", "FOR",
            "FOREIGN", "FROM", "FULL", "GLOB", "GROUP", "HAVING", "IF", "IGNORE",
            "IMMEDIATE", "IN", "INDEX", "INDEXED", "INITIALLY", "INNER", "INSERT",
            "INSTEAD", "INTERSECT", "INTO", "IS", "ISNULL", "JOIN", "KEY", "LEFT",
            "LIKE", "LIMIT", "MATCH", "NATURAL", "NO", "NOT", "NOTNULL", "NULL",
            "OF", "OFFSET", "ON", "OR", "ORDER", "OUTER", "PLAN", "PRAGMA", "PRIMARY",
            "QUERY", "RAISE", "REFERENCES", "REGEXP", "REINDEX", "RELEASE", "RENAME",
            "REPLACE", "RESTRICT", "RIGHT", "ROLLBACK", "ROW", "SAVEPOINT", "SELECT",
            "SET", "TABLE", "TEMP", "TEMPORARY", "THEN", "TO", "TRANSACTION", "TRIGGER",
            "UNION", "UNIQUE", "UPDATE", "USING", "VACUUM", "VALUES", "VIEW", "VIRTUAL",
            "WHEN", "WHERE"
        };
        #endregion

        #region Properties
        /// <summary>
        /// Complete path and file name to your SQLite database (e.g. C:\Database\TestDB.db3)
        /// </summary>
        public string DatabaseFileName { get; set; }
        #endregion

        #region Private Methods
        /// <summary>
        /// Converts Microsoft data types to SQLite data types
        /// </summary>
        /// <param name="MicrosoftDataType">Microsoft data type, e.g. "System.String"</param>
        /// <returns>Data type for SQLite database</returns>
        private string ConvertMS2SqliteDataType(string MicrosoftDataType)
        {
            var s_Return = "";
            switch (MicrosoftDataType)
            {
                case "System.String":
                    s_Return = "TEXT";
                    break;
                case "System.Int16":
                    s_Return = "INTEGER";
                    break;
                case "System.Int32":
                    s_Return = "INTEGER";
                    break;
                case "System.Int64":
                    s_Return = "INTEGER";
                    break;
                case "System.Double":
                    s_Return = "DOUBLE";
                    break;
                case "System.Float":
                    s_Return = "DOUBLE";
                    break;
            }

            return s_Return;
        }

        /// <summary>
        /// Get the database type from a Microsoft data type string
        /// </summary>
        /// <param name="MicrosoftDataType">Microsoft data type string</param>
        /// <returns>DbType</returns>
        private DbType GetDatabaseType(string MicrosoftDataType)
        {
            var db = new DbType();

            switch (MicrosoftDataType)
            {
                case "System.String":
                    return DbType.String;
                case "System.Int16":
                    return DbType.Int16;
                case "System.Int32":
                    return DbType.Int32;
                case "System.Int64":
                    return DbType.Int64;
                case "System.Double":
                    return DbType.Double;
                case "System.Float":
                    return DbType.Double;
            }

            return db;
        }

        /// <summary>
        /// Checks to see if the Word is currently a SQLite keyword
        /// </summary>
        /// <param name="Word">Word to test</param>
        /// <returns>True, if the word is a SQLite Keyword</returns>
        private bool IsSQLiteKeyword(string Word)
        {
            foreach (var s in m_SQLiteKeywords)
            {
                if (Word.ToUpper().Equals(s))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Generates the SQLite statement to create a table within the database.
        /// </summary>
        /// <param name="Table">Table to insert into the database</param>
        /// <returns>SQL Command for creating the table</returns>
        private string SqliteCreateTableStatement(DataTable Table)
        {
            var s_Command = string.Format(
                "CREATE TABLE {0} (",
                Table.TableName);

            var l_FieldType = new List<string>();

            foreach (DataColumn dc in Table.Columns)
            {
                var s_FieldType = FormatDataColumnName(dc.ColumnName) + " ";

                s_FieldType += ConvertMS2SqliteDataType(dc.DataType.FullName);

                l_FieldType.Add(s_FieldType);
            }

            s_Command += string.Join(", ", l_FieldType);

            s_Command += ");";

            return s_Command;
        }

        /// <summary>
        /// Formats a column name if it is an existing SQLite Keyword by
        /// adding quotes around the string
        /// </summary>
        /// <param name="ColumnName">ColumnName of a DataTable field</param>
        /// <returns>Formated column name</returns>
        private string FormatDataColumnName(string ColumnName)
        {
            if (IsSQLiteKeyword(ColumnName))
                ColumnName = "\"" + ColumnName + "\"";

            return ColumnName;
        }

        /// <summary>
        /// Inserts a single DataTable into the SQLite database
        /// </summary>
        /// <param name="Conn">Full path to the SQLite database</param>
        /// <param name="Table">DataTable to insert into the database, TableName is used to name the table in the SQLite database</param>
        /// <returns>True, if the function completes successfully</returns>
        private bool FillTable(SQLiteConnection Conn, DataTable Table)
        {
            var b_Successful = true;

            try
            {
                using (var dbTrans = Conn.BeginTransaction())
                {
                    using (var cmd = Conn.CreateCommand())
                    {
                        var l_Col = new List<string>();
                        foreach (DataColumn dc in Table.Columns)
                            l_Col.Add(FormatDataColumnName(dc.ColumnName));

                        cmd.CommandText = string.Format(
                            "INSERT INTO {0}({1}) VALUES (@{2});",
                            Table.TableName,
                            string.Join(", ", l_Col),
                            string.Join(", @", l_Col));

                        foreach (var s in l_Col)
                        {
                            var param = cmd.CreateParameter();
                            cmd.Parameters.Add(param);
                        }

                        foreach (DataRow dr in Table.Rows)
                        {
                            var idx = 0;
                            foreach (SQLiteParameter p in cmd.Parameters)
                            {
                                p.ParameterName = "@" + l_Col[idx];
                                p.SourceColumn = l_Col[idx];
                                p.Value = dr[idx];
                                idx++;
                            }

                            cmd.ExecuteNonQuery();
                        }
                    }

                    dbTrans.Commit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in FillTable: " + ex.Message);
                b_Successful = false;
            }

            return b_Successful;
        }

        /// <summary>
        /// Inserts each table within the DataSet into the SQLite database
        /// </summary>
        /// <param name="Conn">Full path to the SQLite database</param>
        /// <param name="MainData">DataSet to enter into the database</param>
        /// <returns>True, if the function completes successfully</returns>
        private bool FillTables(SQLiteConnection Conn, DataSet MainData)
        {

            foreach (DataTable dt in MainData.Tables)
            {
                var b_Successful = FillTable(Conn, dt);

                if (!b_Successful)
                    return false;
            }

            return true;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates a SQLite database, file is named by the property DatabaseFileName,
        /// automatically overwrites any existing file
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public override bool CreateDatabase()
        {
            var b_Successful = true;

            if (DatabaseFileName == null)
                return false;

            SQLiteConnection.CreateFile(DatabaseFileName);

            if (!File.Exists(DatabaseFileName))
                b_Successful = false;

            return b_Successful;
        }

        /// <summary>
        /// Creates a SQLite database, file is named by the property DatabaseFileName
        /// </summary>
        /// <param name="OverwriteExistingDatabase">If true, this function
        /// will create delete existing SQLite database and create a blank one
        /// in its place</param>
        /// <returns>True, if the function completes successfully</returns>
        public bool CreateDatabase(bool OverwriteExistingDatabase)
        {
            var b_Successful = true;

            if (DatabaseFileName == null)
                return false;

            if (OverwriteExistingDatabase)
                SQLiteConnection.CreateFile(DatabaseFileName);
            else if (!File.Exists(DatabaseFileName))
                SQLiteConnection.CreateFile(DatabaseFileName);

            if (!File.Exists(DatabaseFileName))
                b_Successful = false;

            return b_Successful;
        }

        /// <summary>
        /// Creates a SQLite database
        /// </summary>
        /// <param name="FileNameOfDatabase">Path and name of database (e.g. C:\Database\TestDB.db3)</param>
        /// <param name="OverwriteExistingDatabase">If true, this function
        /// will create delete existing SQLite database and create a blank one
        /// in its place</param>
        /// <returns>True, if the function completes successfully</returns>
        public bool CreateDatabase(string FileNameOfDatabase, bool OverwriteExistingDatabase)
        {
            DatabaseFileName = FileNameOfDatabase;

            return CreateDatabase(OverwriteExistingDatabase);
        }

        /// <summary>
        /// Writes a DataTable out to the SQLite database
        /// </summary>
        /// <param name="Table">DataTable to write to database</param>
        /// <returns>True, if the function completes successfully</returns>
        public bool WriteDataTableToDatabase(
            DataTable Table)
        {
            var b_Successful = true;

            var Conn = new SQLiteConnection("Data Source=" + DatabaseFileName, true);

            var retval = 0;

            try
            {
                Conn.Open();


                if (TableExists(Table.TableName))
                {
                    var Cmd = new SQLiteCommand(string.Format(
                        "DROP TABLE IF EXISTS {0};",
                        Table.TableName), Conn);
                    retval = Cmd.ExecuteNonQuery();
                }

                var cmd_Table = new SQLiteCommand(SqliteCreateTableStatement(Table), Conn);
                retval = cmd_Table.ExecuteNonQuery();

                FillTable(Conn, Table);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in WriteDataTableToDatabase: " + ex.Message);
                b_Successful = false;
            }
            finally
            {
                Conn.Close();
            }

            return b_Successful;
        }

        /// <summary>
        /// Commits the tables within a DataSet to a SQLite database
        /// </summary>
        /// <param name="MainData">the DataSet to commit to the SQLite database</param>
        /// <returns>True, if the data is committed successfully</returns>
        public override bool WriteDatasetToDatabase(
            DataSet MainData)
        {
            if (DatabaseFileName == null)
                return false;

            foreach (DataTable dt in MainData.Tables)
            {
                var b_Successful = WriteDataTableToDatabase(dt);

                if (!b_Successful)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Commits the tables within a DataSet to a SQLite database
        /// </summary>
        /// <param name="DatabasePath">Path to the database to commit to</param>
        /// <param name="MainData">the DataSet to commit to the SQLite database</param>
        /// <returns>True, if the data is committed successfully</returns>
        public bool WriteDatasetToDatabase(string DatabasePath, DataSet MainData)
        {
            DatabaseFileName = DatabasePath;

            return WriteDatasetToDatabase(MainData);
        }

        /// <summary>
        /// Determines if a table is present in the database or not
        /// </summary>
        /// <param name="TableName">Name of table</param>
        /// <returns>True if table is present, otherwise false</returns>
        public override bool TableExists(string TableName)
        {
            if (DatabaseFileName == null)
                return false;

            var dt_Tables = GetDatabaseInformation();
            foreach (DataRow dr in dt_Tables.Rows)
            {
                if (dr["tbl_name"].ToString().Equals(TableName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the table information regarding the database from sqlite_master, such as table names etc.
        /// </summary>
        /// <returns>Datatable of all the information</returns>
        public override DataTable GetDatabaseInformation()
        {
            if (DatabaseFileName == null)
                return null;

            var dt_Info = new DataTable();
            var s_Command =
            "SELECT * FROM " +
            "sqlite_master WHERE type='table'";

            try
            {
                var connStr = new SQLiteConnectionStringBuilder()
                {
                    DataSource = DatabaseFileName
                };

                using (var conn = new SQLiteConnection(connStr.ToString(), true))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = s_Command;
                    var reader = cmd.ExecuteReader();
                    dt_Info.Load(reader);
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetDatabaseInformation: " + ex.Message);
                return null;
            }
            return dt_Info;
        }


        /// <summary>
        /// Retrieves the entire database and stores it
        /// as a DataSet
        /// </summary>
        /// <returns>DataSet containing all tables in the database</returns>
        public override DataSet GetDatabase()
        {
            var ds = new DataSet(
                Path.GetFileNameWithoutExtension(DatabaseFileName));

            var l_Tables = GetListOfTablesInDatabase();

            foreach (var s in l_Tables)
            {
                ds.Tables.Add(GetTable(s));
            }

            return ds;
        }

        /// <summary>
        /// Gets a list of the table names in the database
        /// </summary>
        /// <returns>List of tables names in the SQLite database</returns>
        public override List<string> GetListOfTablesInDatabase()
        {
            if (DatabaseFileName == null)
                return null;

            var l_Tables = new List<string>();

            try
            {
                var dt_Info = GetDatabaseInformation();

                foreach (DataRow dr in dt_Info.Rows)
                {
                    l_Tables.Add(dr["tbl_name"].ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetListOfTablesInDatabase: " + ex.Message);
                return null;
            }

            return l_Tables;
        }

        /// <summary>
        /// Creates an index in the database
        /// </summary>
        /// <param name="Table">Table name</param>
        /// <param name="Column">Name of Column to index within the table</param>
        /// <param name="IndexName">Name of index, if null/blank, one will be automatically generated</param>
        /// <returns>True, if index is created successfully</returns>
        public override bool CreateIndex(string Table, string Column, string IndexName)
        {
            if (DatabaseFileName == null)
                return false;

            var b_Successful = true;

            try
            {
                if (string.IsNullOrEmpty(IndexName))
                    IndexName = "idx_" + Table + "_" + Column;

                var s_Command = string.Format(
                    "CREATE INDEX {0} ON {1}({2});",
                    IndexName,
                    Table,
                    Column);

                //traceLog.Info("SQLite Handler Creating Index: " + s_Command);

                var connStr = new SQLiteConnectionStringBuilder()
                {
                    DataSource = DatabaseFileName
                };

                using (var conn = new SQLiteConnection(connStr.ToString(), true))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = s_Command;

                    cmd = conn.CreateCommand();
                    cmd.CommandText = s_Command;
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("IOException in WriteDataTableToDatabase: " + ex.Message);
                b_Successful = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CreateIndex: " + ex.Message);
                b_Successful = false;
            }

            return b_Successful;
        }

        /// <summary>
        /// Selects a table from a given query
        /// </summary>
        /// <param name="Command">SQLite query to generate the table that is returned</param>
        /// <returns>Table generated from the supplied SQLite query, null if query fails</returns>
        public override DataTable SelectTable(string Command)
        {
            if (DatabaseFileName == null)
                return null;

            var dt_Return = new DataTable();

            try
            {
                var connStr = new SQLiteConnectionStringBuilder()
                {
                    DataSource = DatabaseFileName
                };

                using (var conn = new SQLiteConnection(connStr.ToString(), true))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = Command;

                    cmd = conn.CreateCommand();
                    cmd.CommandText = Command;
                    var reader = cmd.ExecuteReader();
                    dt_Return.Load(reader);
                    conn.Close();
                }

            }
            catch (IOException ex)
            {
                Console.WriteLine("IOException in SelecTable: " + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SelectTable: " + ex.Message);
                return null;
            }

            return dt_Return;
        }

        /// <summary>
        /// If a table exists in the database, this will remove the table
        /// </summary>
        /// <param name="TableName">Name of table to delete</param>
        /// <returns>True, if table is dropped successfully</returns>
        public override bool DropTable(string TableName)
        {
            if (DatabaseFileName == null)
                return false;

            var b_Successful = true;

            if (TableExists(TableName))
            {
                var s_Command = "DROP TABLE " + TableName;

                try
                {
                    var connStr = new SQLiteConnectionStringBuilder()
                    {
                        DataSource = DatabaseFileName
                    };

                    using (var conn = new SQLiteConnection(connStr.ToString(), true))
                    {
                        conn.Open();

                        var cmd = conn.CreateCommand();
                        cmd.CommandText = s_Command;
                        var i = cmd.ExecuteNonQuery();
                        Console.WriteLine("Returned: " + i);
                        conn.Close();

                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in DropTable: " + ex.Message);
                    b_Successful = false;
                }
            }

            return b_Successful;
        }

        /// <summary>
        /// Retrieves a table from the SQLite database and returns it as a DataTable
        /// </summary>
        /// <param name="TableName">Name of table to retrieve</param>
        /// <returns>Table from Database</returns>
        public override DataTable GetTable(string TableName)
        {
            if (DatabaseFileName == null)
                return null;

            var dt_Return = new DataTable();

            var Command = string.Format("SELECT * FROM {0};", TableName);

            try
            {
                var connStr = new SQLiteConnectionStringBuilder()
                {
                    DataSource = DatabaseFileName
                };

                using (var conn = new SQLiteConnection(connStr.ToString(), true))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = Command;

                    cmd = conn.CreateCommand();
                    cmd.CommandText = Command;
                    //SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                    //da.Fill(dt_Return);
                    var reader = cmd.ExecuteReader();
                    dt_Return.Load(reader);
                    conn.Close();
                }

            }
            catch (IOException ex)
            {
                Console.WriteLine("IOException in GetTable: " + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetTable: " + ex.Message);
                return null;
            }

            return dt_Return;
        }

        /// <summary>
        /// Useful method to execute a NonQuery on the Database
        /// </summary>
        /// <param name="Command">SQL command to issue</param>
        /// <returns>True, if the SQL statement completed successfully</returns>
        public override bool RunNonQuery(string Command)
        {
            if (string.IsNullOrEmpty(DatabaseFileName))
                return false;

            var b_Successful = true;
            var dt_Return = new DataTable();

            try
            {
                var connStr = new SQLiteConnectionStringBuilder()
                {
                    DataSource = DatabaseFileName
                };

                using (var conn = new SQLiteConnection(connStr.ToString(), true))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = Command;

                    var reader = cmd.ExecuteReader();
                    dt_Return.Load(reader);
                    conn.Close();
                }

            }
            catch (IOException ex)
            {
                Console.WriteLine("IOException in RunNonQuery: " + ex.Message);
                b_Successful = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in RunNonQuery: " + ex.Message);
                b_Successful = false;
            }

            return b_Successful;
        }

        /// <summary>
        /// Gets the column names of a table
        /// </summary>
        /// <param name="TableName">Name of Table</param>
        /// <returns>Fields within the Table</returns>
        public List<string> GetColumnNames(string TableName)
        {
            if (string.IsNullOrEmpty(DatabaseFileName))
                return null;

            var ColumnNames = new List<string>();
            var Command = string.Format(
                "SELECT * FROM {0} LIMIT 1",
                TableName);

            try
            {
                var connStr = new SQLiteConnectionStringBuilder()
                {
                    DataSource = DatabaseFileName
                };

                using (var conn = new SQLiteConnection(connStr.ToString(), true))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = Command;

                    var reader = cmd.ExecuteReader();
                    var dt = new DataTable();
                    dt.Load(reader);
                    conn.Close();

                    foreach (DataColumn dc in dt.Columns)
                        ColumnNames.Add(dc.ColumnName);
                }
            }
            catch (Exception ex)
            {
                // TODO : Handle exception
                Console.WriteLine("Error in GetColumnNames: " + ex.Message);
                return null;
            }

            return ColumnNames;
        }
        #endregion
    }
}
