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
using System.Data;

using Mage;

namespace Cyclops
{
    /// <summary>
    /// Mage module that can fucntion as root module in pipeline
    /// and serve contents of a .Net DataTable to standard tabular output
    /// </summary>
    public class clsMageDataTableSource : BaseModule
    {
        /// <summary>
        /// The .Net DataTable object that will be sourced to standard tabular output
        /// </summary>
        public DataTable SourceTable { get; set; }

        /// <summary>
        /// Serve the contents of the DataTable to standard tabular output
        /// (override of Mage BaseModule stub function)
        /// </summary>
        /// <param name="state"></param>
        public override void Run(object state)
        {
            // tell listeners about the columns
            List<MageColumnDef> columnDefs = GetMageColumnDefs();
            OnColumnDefAvailable(new MageColumnEventArgs(columnDefs.ToArray()));

            // send each data row to listeners
            foreach (DataRow row in SourceTable.Rows)
            {
                OnDataRowAvailable(new MageDataEventArgs(row.ItemArray));
            }

            // tell listeners that all data rows have been set
            if (!Abort)
            {
                OnDataRowAvailable(new MageDataEventArgs(null));
            }
        }

        /// <summary>
        /// Convert column definitions in DataTable object
        /// to their eqivalent Mage column definitions
        /// </summary>
        /// <returns></returns>
        private List<MageColumnDef> GetMageColumnDefs()
        {
            List<MageColumnDef> columnDefs = new List<MageColumnDef>();
            foreach (DataColumn srcCol in SourceTable.Columns)
            {
                string field = srcCol.ColumnName;
                //DataColumn.DataType type = srcCol.DataType;
                string type = "text";
                switch (srcCol.DataType.ToString())
                {
                    case "System.Int16":
                        type = "integer";
                        break;
                    case "System.Int32":
                        type = "integer";
                        break;
                    case "System.Int64":
                        type = "integer";
                        break;
                    case "System.Double":
                        type = "numeric";
                        break;
                    case "System.Decimal":
                        type = "numeric";
                        break;
                    case "System.Single":
                        type = "numeric";
                        break;
                    case "System.Data.SqlTypes.SqlInt16":
                        type = "integer";
                        break;
                    case "System.Data.SqlTypes.SqlInt32":
                        type = "integer";
                        break;
                    case "System.Data.SqlTypes.SqlInt64":
                        type = "integer";
                        break;
                    case "System.Data.SqlTypes.SqlDouble":
                        type = "numeric";
                        break;
                    // more data types
                }
                MageColumnDef colDef = new MageColumnDef(field, type, "10");
                columnDefs.Add(colDef);
            }
            return columnDefs;
        }
    }
}
