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

namespace Cyclops.MiscModules
{
    public static class clsMiscFunctions
    {
        #region Functions

        /// <summary>
        /// Collapse a list of strings into a single string with each element
        /// separated by the specified delimiter
        /// </summary>
        /// <param name="List2Collapse">List to collapse</param>
        /// <param name="Delimiter">Delimiter to separate elements</param>
        /// <returns>Collapsed string</returns>
        public static string Collapse(List<string> List2Collapse, char Delimiter)
        {
            string s_Return = "";
            foreach (string s in List2Collapse)
            {
                s_Return += s + Delimiter;
            }
            s_Return = s_Return.Remove(s_Return.Length - 1);
            return s_Return;
        }

        /// <summary>
        /// Collapse an array of strings into a single string with each element
        /// separated by the specified delimiter
        /// </summary>
        /// <param name="Array2Collapse">String array to collapse</param>
        /// <param name="Delimiter">Delimiter to separate elements</param>
        /// <returns>Collapsed string</returns>
        public static string Collapse(string[] Array2Collapse, char Delimiter)
        {
            string s_Return = "";
            foreach (string s in Array2Collapse)
            {
                s_Return += s + Delimiter;
            }
            s_Return = s_Return.Remove(s_Return.Length - 1);
            return s_Return;
        }
        #endregion
    }
}
