﻿/* Written by Joseph N. Brown
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
using System.IO;

using System.Data.SQLite;

namespace Cyclops
{
    public static class clsSQLiteHandler
    {
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
                Console.WriteLine("IOEXCEPTION: " + ioe.ToString());
            }
            catch (Exception exc)
            {
                Console.WriteLine("EXC: " + exc.ToString());
            }
            return dt;
        }
    }
}
