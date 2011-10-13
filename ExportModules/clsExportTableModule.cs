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
    public class clsExportTableModule : clsBaseExportModule
    {
        public enum ImportDataType { SQLite, CSV, TSV, MSAccess, SQLServer };
        private Dictionary<string, string> d_Tables2Export = new Dictionary<string, string>();
        private string s_Path2SaveTables;
        
        private int dataType;

        private string s_RInstance;

        #region Constructors
        public clsExportTableModule()
        {
            ModuleName = "Export Table Module";
        }
        public clsExportTableModule(string InstanceOfR)
        {
            ModuleName = "Export Table Module";
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Members

        #endregion

        #region Properties
        public Dictionary<string, string> Tables2Export
        {
            get { return d_Tables2Export; }
        }

        public string Path2SaveTables
        {
            get { return s_Path2SaveTables; }
            set { s_Path2SaveTables = value; }
        }
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
        /// Adds the name of a table to the list to save
        /// </summary>
        /// <param name="TableName">Name of table to save.</param>
        public void AddTable2Save(string TableName, string NewTableName)
        {
            d_Tables2Export.Add(TableName, NewTableName);
        }

        public override void PerformOperation()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            switch (dataType)
            {
                case (int)ImportDataType.CSV:
                    foreach (KeyValuePair<string, string> kvp in d_Tables2Export)
                    {
                        string s_Command = string.Format("write.csv({0}, file=\"{1}\")",
                            kvp.Key, Path2SaveTables + Path.PathSeparator + kvp.Value);
                        //engine.EagerEvaluate(s_Command);
                    }
                    break; // CSV
                case (int)ImportDataType.TSV:

                    break; // TSV
            }
        }
        #endregion
    }
}
