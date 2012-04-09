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
using System.IO;
using System.Linq;
using System.Text;

using log4net;

namespace Cyclops
{
    /// <summary>
    /// Static class to commonly used functions
    /// </summary>
    public static class clsMiscFunctions
    {
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        /// <summary>
        /// Concatenates the items in the list.
        /// </summary>
        /// <param name="MyList">List of items to concatenate</param>
        /// <param name="Sep">Delimiter</param>
        /// <param name="MakeRCompliant">If this is a list for R (e.g. 'c(...)')</param>
        /// <returns></returns>
        public static string Concatenate(List<string> MyList, string Sep, bool MakeRCompliant)
        {
            string s_Return = "";

            s_Return = MakeRCompliant ? "c(" : "";

            foreach (string s in MyList)
            {
                s_Return += MakeRCompliant ? "\"" + s + "\"" + Sep : s + Sep;
            }

            s_Return = s_Return.Substring(0, s_Return.Length - Sep.Length); // remove the last Sep

            s_Return += MakeRCompliant ? ")" : "";

            return s_Return;
        }

        /// <summary>
        /// Saves the DataTable as a tab-delimited text file
        /// </summary>
        /// <param name="TheDataTable">DataTable to Save</param>
        /// <param name="FileName">Where the DataTable will be saved to</param>
        public static void SaveDataTable(DataTable TheDataTable, string FileName)
        {
            try
            {
                StreamWriter sw_Writer = new StreamWriter(FileName);

                // write the headers to the file
                for (int columns = 0; columns < TheDataTable.Columns.Count; columns++)
                {
                    if ((TheDataTable.Columns.Count - 1) == columns)
                    {
                        sw_Writer.Write(TheDataTable.Columns[columns].ToString() + "\n");
                    }
                    else
                    {
                        sw_Writer.Write(TheDataTable.Columns[columns].ToString() + "\t");
                    }
                }

                // write the data to the file
                for (int rows = 0; rows < TheDataTable.Rows.Count; rows++)
                {
                    for (int columns = 0; columns < TheDataTable.Columns.Count; columns++)
                    {
                        if ((TheDataTable.Columns.Count - 1) == columns)
                        {
                            sw_Writer.Write(TheDataTable.Rows[rows][columns].ToString()
                                + "\n");
                        }
                        else
                        {
                            sw_Writer.Write(TheDataTable.Rows[rows][columns].ToString()
                                + "\t");
                        }
                    }
                }

                sw_Writer.Close();
            }
            catch (IOException ioe)
            {
                traceLog.Error("Miscellaneous Functions Error writing datatable to file: " + ioe.ToString());
            }
        }
    }
}
