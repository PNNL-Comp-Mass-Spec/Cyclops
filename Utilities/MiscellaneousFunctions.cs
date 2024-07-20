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
        /// <param name="itemList">List of items to concatenate</param>
        /// <param name="delimiter">Delimiter</param>
        /// <param name="makeRCompliant">If this is a list for R (e.g. 'c(...)')</param>
        /// <returns></returns>
        public static string Concatenate(List<string> itemList, string delimiter, bool makeRCompliant)
        {
            var delimitedList = makeRCompliant ? "c(" : "";

            foreach (var item in itemList)
            {
                delimitedList += makeRCompliant ? "\"" + item + "\"" + delimiter : item + delimiter;
            }

            delimitedList = delimitedList.Substring(0, delimitedList.Length - delimiter.Length); // remove the last delimiter

            delimitedList += makeRCompliant ? ")" : "";

            return delimitedList;
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
                var writer = new StreamWriter(FileName);

                // write the headers to the file
                for (var columns = 0; columns < tableToSave.Columns.Count; columns++)
                {
                    if ((tableToSave.Columns.Count - 1) == columns)
                    {
                        writer.Write(tableToSave.Columns[columns] + "\n");
                    }
                    else
                    {
                        writer.Write(tableToSave.Columns[columns] + "\t");
                    }
                }

                // write the data to the file
                for (var rows = 0; rows < tableToSave.Rows.Count; rows++)
                {
                    for (var columns = 0; columns < tableToSave.Columns.Count; columns++)
                    {
                        if ((tableToSave.Columns.Count - 1) == columns)
                        {
                            writer.Write(tableToSave.Rows[rows][columns]
                                + "\n");
                        }
                        else
                        {
                            writer.Write(tableToSave.Rows[rows][columns]
                                + "\t");
                        }
                    }
                }

                writer.Close();
            }
            catch (IOException ex)
            {
                // TODO : Figure out how to handle this exception
                Console.WriteLine("IOException in SaveDataTable: " + ex.Message);
            }
        }
    }
}
