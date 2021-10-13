/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
 * -----------------------------------------------------
 *
 * Licensed under the 2-Clause BSD License; you may not use this
 * file except in compliance with the License.  You may obtain
 * a copy of the License at https://opensource.org/licenses/BSD-2-Clause
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Cyclops.Utilities
{
    /// <summary>
    /// Static class to commonly used functions
    /// </summary>
    public static class MiscellaneousFunctions
    {
        /// <summary>
        /// Concatenates the items in the list.
        /// </summary>
        /// <param name="MyList">List of items to concatenate</param>
        /// <param name="Sep">Delimiter</param>
        /// <param name="MakeRCompliant">If this is a list for R (e.g. 'c(...)')</param>
        /// <returns></returns>
        public static string Concatenate(List<string> MyList, string Sep, bool MakeRCompliant)
        {
            var s_Return = MakeRCompliant ? "c(" : "";

            foreach (var s in MyList)
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
        /// <param name="tableToSave">DataTable to Save</param>
        /// <param name="FileName">Where the DataTable will be saved to</param>
        public static void SaveDataTable(DataTable tableToSave, string FileName)
        {
            try
            {
                var sw_Writer = new StreamWriter(FileName);

                // write the headers to the file
                for (var columns = 0; columns < tableToSave.Columns.Count; columns++)
                {
                    if ((tableToSave.Columns.Count - 1) == columns)
                    {
                        sw_Writer.Write(tableToSave.Columns[columns] + "\n");
                    }
                    else
                    {
                        sw_Writer.Write(tableToSave.Columns[columns] + "\t");
                    }
                }

                // write the data to the file
                for (var rows = 0; rows < tableToSave.Rows.Count; rows++)
                {
                    for (var columns = 0; columns < tableToSave.Columns.Count; columns++)
                    {
                        if ((tableToSave.Columns.Count - 1) == columns)
                        {
                            sw_Writer.Write(tableToSave.Rows[rows][columns]
                                + "\n");
                        }
                        else
                        {
                            sw_Writer.Write(tableToSave.Rows[rows][columns]
                                + "\t");
                        }
                    }
                }

                sw_Writer.Close();
            }
            catch (IOException ex)
            {
                // TODO : Figure out how to handle this exception
                Console.WriteLine("IOException in SaveDataTable: " + ex.Message);
            }
        }
    }
}
