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

using System.Collections.Generic;
using System.Data;

namespace Cyclops
{
    /// <summary>
    /// Base class for Wrapper classes for handling databases
    /// </summary>
    public abstract class DatabaseHandler
    {
        #region Methods
        /// <summary>
        /// Creates a database,
        /// automatically overwrites any existing file
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public abstract bool CreateDatabase();

        /// <summary>
        /// Gets the table information regarding the database
        /// </summary>
        /// <returns>Datatable of all the information</returns>
        public abstract DataTable GetDatabaseInformation();

        /// <summary>
        /// Retrieves all tables from the database
        /// </summary>
        /// <returns>Database is stored in a DataSet</returns>
        public abstract DataSet GetDatabase();

        /// <summary>
        /// Commits the tables within a DataSet to the database
        /// </summary>
        /// <param name="MainData">DataSet to commit to the database</param>
        /// <returns>True, if the data is committed successfully</returns>
        public abstract bool WriteDatasetToDatabase(DataSet MainData);

        /// <summary>
        /// Determines if a table is present in the database or not
        /// </summary>
        /// <param name="TableName">Name of table</param>
        /// <returns>True if table is present, otherwise false</returns>
        public abstract bool TableExists(string TableName);

        /// <summary>
        /// Retrieves a table from the database and returns it as a DataTable
        /// </summary>
        /// <param name="TableName">Name of table to retrieve</param>
        /// <returns>Table from Database</returns>
        public abstract DataTable GetTable(string TableName);

        /// <summary>
        /// Selects a table from a given query
        /// </summary>
        /// <param name="sql">SQL query to generate the table that is returned</param>
        /// <returns>Table generated from the supplied SQL query, null if query fails</returns>
        public abstract DataTable SelectTable(string sql);

        /// <summary>
        /// Useful method to execute a NonQuery on the Database
        /// </summary>
        /// <param name="sql">SQL command to issue</param>
        /// <returns>True, if the SQL statement completed successfully</returns>
        public abstract bool RunNonQuery(string sql);

        /// <summary>
        /// If a table exists in the database, this will remove the table
        /// </summary>
        /// <param name="TableName">Name of table to delete</param>
        /// <returns>True, if table is dropped successfully</returns>
        public abstract bool DropTable(string TableName);

        /// <summary>
        /// Creates an index in the database
        /// </summary>
        /// <param name="Table">Table name</param>
        /// <param name="Column">Name of Column to index within the table</param>
        /// <param name="IndexName">Name of index</param>
        /// <returns>True, if index is created successfully</returns>
        public abstract bool CreateIndex(
            string Table, string Column, string IndexName);

        /// <summary>
        /// Gets a list of the table names in the database
        /// </summary>
        /// <returns>List of tables names in the SQLite database</returns>
        public abstract List<string> GetListOfTablesInDatabase();
        #endregion
    }
}
