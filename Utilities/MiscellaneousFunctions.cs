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
        #region Methods
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
            catch (IOException ex)
            {
                // TODO : Figure out how to handle this exception
                Console.WriteLine("IOException in SaveDataTable: " + ex.Message);
            }
        }
        #endregion
    }
}
