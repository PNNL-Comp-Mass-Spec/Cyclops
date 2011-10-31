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
    /// Exports tables from R environment to SQLite database, CSV, TSV, MSAccess, or SQLServer
    /// </summary>
    public class clsExportTableModule : clsBaseExportModule
    {
        public enum ExportDataType { SQLite, CSV, TSV, MSAccess, SQLServer };
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

        #region Properties
        
        #endregion

        #region Functions
        /// <summary>
        /// Sets the DataType that the object is going to pull the data from
        /// </summary>
        /// <param name="DataType">ImportDataType</param>
        public void SetDataType(ExportDataType DataType)
        {
            dataType = (int)DataType;
        }

        /// <summary>
        /// Given the list of parameters from the Dictionary Parameters,
        /// determine the source the data should be export out to and set the DataType
        /// </summary>
        public void SetDataTypeFromParameters()
        {
            string s_DataType = Parameters["source"].ToString();
            switch (s_DataType)
            {
                case "sqlite":
                    SetDataType(ExportDataType.SQLite);
                    break;
                case "msAccess":
                    SetDataType(ExportDataType.MSAccess);
                    break;
                case "csv":
                    SetDataType(ExportDataType.CSV);
                    break;
                case "tsv":
                    SetDataType(ExportDataType.TSV);
                    break;
                case "sqlServer":
                    SetDataType(ExportDataType.SQLServer);
                    break;
            }
        }

        /// <summary>
        /// Runs module
        /// </summary>
        public override void PerformOperation()
        {
            // Determine what source the data needs to export out to
            SetDataTypeFromParameters();

            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            string s_Command = "";

            switch (dataType)
            {
                case (int)ExportDataType.SQLite:

                    break;
                case (int)ExportDataType.CSV:
                    string s_FileName = Parameters["fileName"];
                    if (Path.GetDirectoryName(s_FileName).Equals("") &
                        Parameters.ContainsKey("workDir"))
                    {
                        s_FileName = Parameters["workDir"] + "/" + s_FileName;
                    }
                    s_FileName = s_FileName.Replace('\\', '/');
                    s_Command = string.Format("write.csv({0}, file=\"{1}\")",
                        Parameters["tableName"],
                        s_FileName);
                    break;
                case (int)ExportDataType.TSV:

                    break;
                case (int)ExportDataType.MSAccess:

                    break;
                case (int)ExportDataType.SQLServer:

                    break;
            }

            engine.EagerEvaluate(s_Command);
        }
        #endregion
    }
}
