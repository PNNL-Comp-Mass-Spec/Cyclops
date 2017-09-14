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
        abstract public bool CreateDatabase();

        /// <summary>
        /// Gets the table information regarding the database
        /// </summary>
        /// <returns>Datatable of all the information</returns>
        abstract public DataTable GetDatabaseInformation();

        /// <summary>
        /// Retrieves all tables from the database
        /// </summary>
        /// <returns>Database is stored in a DataSet</returns>
        abstract public DataSet GetDatabase();

        /// <summary>
        /// Commits the tables within a DataSet to the database
        /// </summary>
        /// <param name="MainData">DataSet to commit to the database</param>
        /// <returns>True, if the data is committed successfully</returns>
        abstract public bool WriteDatasetToDatabase(DataSet MainData);

        /// <summary>
        /// Determines if a table is present in the database or not
        /// </summary>
        /// <param name="TableName">Name of table</param>
        /// <returns>True if table is present, otherwise false</returns>
        abstract public bool TableExists(string TableName);

        /// <summary>
        /// Retrieves a table from the database and returns it as a DataTable
        /// </summary>
        /// <param name="TableName">Name of table to retrieve</param>
        /// <returns>Table from Database</returns>
        abstract public DataTable GetTable(string TableName);

        /// <summary>
        /// Selects a table from a given query
        /// </summary>
        /// <param name="Command">SQL query to generate the table that is returned</param>
        /// <returns>Table generated from the supplied SQL query, null if query fails</returns>
        abstract public DataTable SelectTable(string Command);

        /// <summary>
        /// Useful method to execute a NonQuery on the Database
        /// </summary>
        /// <param name="Command">SQL command to issue</param>
        /// <returns>True, if the SQL statement completed successfully</returns>
        abstract public bool RunNonQuery(string Command);

        /// <summary>
        /// If a table exists in the database, this will remove the table
        /// </summary>
        /// <param name="TableName">Name of table to delete</param>
        /// <returns>True, if table is dropped successfully</returns>
        abstract public bool DropTable(string TableName);

        /// <summary>
        /// Creates an index in the database
        /// </summary>
        /// <param name="Table">Table name</param>
        /// <param name="Column">Name of Column to index within the table</param>
        /// <param name="IndexName">Name of index</param>
        /// <returns>True, if index is created successfully</returns>
        abstract public bool CreateIndex(
            string Table, string Column, string IndexName);

        /// <summary>
        /// Gets a list of the table names in the database
        /// </summary>
        /// <returns>List of tables names in the SQLite database</returns>
        abstract public List<string> GetListOfTablesInDatabase();
        #endregion
    }
}
