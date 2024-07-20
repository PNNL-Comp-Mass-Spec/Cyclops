/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://www.pnnl.gov/integrative-omics
 * -----------------------------------------------------
 *
 * Licensed under the 2-Clause BSD License; you may not use this
 * file except in compliance with the License.  You may obtain
 * a copy of the License at https://opensource.org/licenses/BSD-2-Clause
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
        // Ignore Spelling: sql

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

        /// <summary>
        /// Complete path and file name to your SQLite database (e.g. C:\Database\TestDB.db3)
        /// </summary>
        public string DatabaseFileName { get; set; }

        /// <summary>
        /// Converts Microsoft data types to SQLite data types
        /// </summary>
        /// <param name="MicrosoftDataType">Microsoft data type, e.g. "System.String"</param>
        /// <returns>Data type for SQLite database</returns>
        private string ConvertMS2SqliteDataType(string MicrosoftDataType)
        {
            switch (MicrosoftDataType)
            {
                case "System.String":
                    return "TEXT";

                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                    return "INTEGER";

                case "System.Double":
                case "System.Float":
                    return "DOUBLE";
            }

            return string.Empty;
        }

        /// <summary>
        /// Get the database type from a Microsoft data type string
        /// </summary>
        /// <param name="MicrosoftDataType">Microsoft data type string</param>
        /// <returns>DbType</returns>
        [Obsolete("Unused")]
        private DbType GetDatabaseType(string MicrosoftDataType)
        {
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

            return new DbType();
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
                {
                    return true;
                }
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
            var sql = string.Format(
                "CREATE TABLE {0} (",
                Table.TableName);

            var fileTypeList = new List<string>();

            foreach (DataColumn dc in Table.Columns)
            {
                var fieldType = FormatDataColumnName(dc.ColumnName) + " ";

                fieldType += ConvertMS2SqliteDataType(dc.DataType.FullName);

                fileTypeList.Add(fieldType);
            }

            sql += string.Join(", ", fileTypeList);

            sql += ");";

            return sql;
        }

        /// <summary>
        /// Formats a column name if it is an existing SQLite Keyword by
        /// adding quotes around the string
        /// </summary>
        /// <param name="ColumnName">ColumnName of a DataTable field</param>
        /// <returns>Formatted column name</returns>
        private string FormatDataColumnName(string ColumnName)
        {
            if (IsSQLiteKeyword(ColumnName))
            {
                ColumnName = "\"" + ColumnName + "\"";
            }

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
            var successful = true;

            try
            {
                using (var dbTrans = Conn.BeginTransaction())
                {
                    using (var cmd = Conn.CreateCommand())
                    {
                        var columnNames = new List<string>();
                        foreach (DataColumn dc in Table.Columns)
                        {
                            columnNames.Add(FormatDataColumnName(dc.ColumnName));
                        }

                        cmd.CommandText = string.Format(
                            "INSERT INTO {0}({1}) VALUES (@{2});",
                            Table.TableName,
                            string.Join(", ", columnNames),
                            string.Join(", @", columnNames));

                        foreach (var unused in columnNames)
                        {
                            var param = cmd.CreateParameter();
                            cmd.Parameters.Add(param);
                        }

                        foreach (DataRow dr in Table.Rows)
                        {
                            var idx = 0;
                            foreach (SQLiteParameter p in cmd.Parameters)
                            {
                                p.ParameterName = "@" + columnNames[idx];
                                p.SourceColumn = columnNames[idx];
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
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Inserts each table within the DataSet into the SQLite database
        /// </summary>
        /// <param name="Conn">Full path to the SQLite database</param>
        /// <param name="MainData">DataSet to enter into the database</param>
        /// <returns>True, if the function completes successfully</returns>
        [Obsolete("Unused")]
        private bool FillTables(SQLiteConnection Conn, DataSet MainData)
        {
            foreach (DataTable dt in MainData.Tables)
            {
                var successful = FillTable(Conn, dt);

                if (!successful)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a SQLite database, file is named by the property DatabaseFileName,
        /// automatically overwrites any existing file
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public override bool CreateDatabase()
        {
            var successful = true;

            if (DatabaseFileName == null)
            {
                return false;
            }

            SQLiteConnection.CreateFile(DatabaseFileName);

            if (!File.Exists(DatabaseFileName))
            {
                successful = false;
            }

            return successful;
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
            var successful = true;

            if (DatabaseFileName == null)
            {
                return false;
            }

            if (OverwriteExistingDatabase)
            {
                SQLiteConnection.CreateFile(DatabaseFileName);
            }
            else if (!File.Exists(DatabaseFileName))
            {
                SQLiteConnection.CreateFile(DatabaseFileName);
            }

            if (!File.Exists(DatabaseFileName))
            {
                successful = false;
            }

            return successful;
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
            var successful = true;

            var conn = new SQLiteConnection("Data Source=" + DatabaseFileName, true);

            try
            {
                conn.Open();

                if (TableExists(Table.TableName))
                {
                    var cmdDropIfExists = new SQLiteCommand(string.Format(
                        "DROP TABLE IF EXISTS {0};",
                        Table.TableName), conn);
                    cmdDropIfExists.ExecuteNonQuery();
                }

                var cmdCreateTable = new SQLiteCommand(SqliteCreateTableStatement(Table), conn);
                cmdCreateTable.ExecuteNonQuery();

                FillTable(conn, Table);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in WriteDataTableToDatabase: " + ex.Message);
                successful = false;
            }
            finally
            {
                conn.Close();
            }

            return successful;
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
            {
                return false;
            }

            foreach (DataTable dt in MainData.Tables)
            {
                var successful = WriteDataTableToDatabase(dt);

                if (!successful)
                {
                    return false;
                }
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
        /// <param name="tableName">Name of table</param>
        /// <returns>True if table is present, otherwise false</returns>
        public override bool TableExists(string tableName)
        {
            if (DatabaseFileName == null)
            {
                return false;
            }

            var infoTable = GetDatabaseInformation();
            foreach (DataRow dr in infoTable.Rows)
            {
                if (dr["tbl_name"].ToString().Equals(tableName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determine the names of the tables in the SQLite file
        /// </summary>
        /// <returns>DataTable with table names and table creation SQL (which includes column names and column types)</returns>
        public override DataTable GetDatabaseInformation()
        {
            if (DatabaseFileName == null)
            {
                return null;
            }

            var infoTable = new DataTable();

            const string sql = "SELECT * FROM sqlite_master WHERE type='table'";

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
                    cmd.CommandText = sql;
                    var reader = cmd.ExecuteReader();
                    infoTable.Load(reader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetDatabaseInformation: " + ex.Message);
                return null;
            }
            return infoTable;
        }

        /// <summary>
        /// Retrieves the entire database and stores it as a DataSet
        /// </summary>
        /// <returns>DataSet containing all tables in the database</returns>
        public override DataSet GetDatabase()
        {
            var ds = new DataSet(
                Path.GetFileNameWithoutExtension(DatabaseFileName));

            var tableNames = GetListOfTablesInDatabase();

            foreach (var tableName in tableNames)
            {
                ds.Tables.Add(GetTable(tableName));
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
            {
                return null;
            }

            var tableNames = new List<string>();

            try
            {
                var infoTable = GetDatabaseInformation();

                foreach (DataRow dr in infoTable.Rows)
                {
                    tableNames.Add(dr["tbl_name"].ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetListOfTablesInDatabase: " + ex.Message);
                return null;
            }

            return tableNames;
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
            {
                return false;
            }

            var successful = true;

            try
            {
                if (string.IsNullOrEmpty(IndexName))
                {
                    IndexName = "idx_" + Table + "_" + Column;
                }

                var sql = string.Format(
                    "CREATE INDEX {0} ON {1}({2});",
                    IndexName,
                    Table,
                    Column);

                var connStr = new SQLiteConnectionStringBuilder
                {
                    DataSource = DatabaseFileName
                };

                using (var conn = new SQLiteConnection(connStr.ToString(), true))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("IOException in WriteDataTableToDatabase: " + ex.Message);
                successful = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CreateIndex: " + ex.Message);
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Selects a table from a given query
        /// </summary>
        /// <param name="sql">SQLite query to generate the table that is returned</param>
        /// <returns>Table generated from the supplied SQLite query, null if query fails</returns>
        public override DataTable SelectTable(string sql)
        {
            if (DatabaseFileName == null)
            {
                return null;
            }

            var outTable = new DataTable();

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
                    cmd.CommandText = sql;

                    var reader = cmd.ExecuteReader();
                    outTable.Load(reader);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("IOException in SelectTable: " + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SelectTable: " + ex.Message);
                return null;
            }

            return outTable;
        }

        /// <summary>
        /// If a table exists in the database, this will remove the table
        /// </summary>
        /// <param name="TableName">Name of table to delete</param>
        /// <returns>True, if table is dropped successfully</returns>
        public override bool DropTable(string TableName)
        {
            if (DatabaseFileName == null)
            {
                return false;
            }

            var successful = true;

            if (TableExists(TableName))
            {
                var sql = "DROP TABLE " + TableName;

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
                        cmd.CommandText = sql;

                        var i = cmd.ExecuteNonQuery();
                        Console.WriteLine("Returned: " + i);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in DropTable: " + ex.Message);
                    successful = false;
                }
            }

            return successful;
        }

        /// <summary>
        /// Retrieves a table from the SQLite database and returns it as a DataTable
        /// </summary>
        /// <param name="tableName">Name of table to retrieve</param>
        /// <returns>Table from Database</returns>
        public override DataTable GetTable(string tableName)
        {
            if (DatabaseFileName == null)
            {
                return null;
            }

            var outTable = new DataTable();

            var sql = string.Format("SELECT * FROM {0};", tableName);

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
                    cmd.CommandText = sql;

                    var reader = cmd.ExecuteReader();
                    outTable.Load(reader);
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

            return outTable;
        }

        /// <summary>
        /// Useful method to execute a NonQuery on the Database
        /// </summary>
        /// <param name="sql">SQL command to issue</param>
        /// <returns>True, if the SQL statement completed successfully</returns>
        public override bool RunNonQuery(string sql)
        {
            if (string.IsNullOrEmpty(DatabaseFileName))
            {
                return false;
            }

            var successful = true;
            var outTable = new DataTable();

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
                    cmd.CommandText = sql;

                    var reader = cmd.ExecuteReader();
                    outTable.Load(reader);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("IOException in RunNonQuery: " + ex.Message);
                successful = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in RunNonQuery: " + ex.Message);
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Gets the column names of a table
        /// </summary>
        /// <param name="TableName">Name of Table</param>
        /// <returns>Fields within the Table</returns>
        public List<string> GetColumnNames(string TableName)
        {
            if (string.IsNullOrEmpty(DatabaseFileName))
            {
                return null;
            }

            var ColumnNames = new List<string>();
            var sql = string.Format(
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
                    cmd.CommandText = sql;

                    var reader = cmd.ExecuteReader();
                    var dt = new DataTable();
                    dt.Load(reader);

                    foreach (DataColumn dc in dt.Columns)
                    {
                        ColumnNames.Add(dc.ColumnName);
                    }
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
    }
}
