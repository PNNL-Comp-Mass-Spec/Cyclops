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
using System.Text;

using RDotNet;

namespace Cyclops
{
    /// <summary>
    /// Static class for making generic calls to the R workspace.
    /// </summary>
    public static class clsGenericRCalls
    {
        #region Functions
        /// <summary>
        /// Returns all the objects in the R workspace
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <returns>List of all the datasets in the R workspace</returns>
        public static List<string> ls(string InstanceOfR)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            List<string> l_Return = new List<string>();
            CharacterVector cv = engine.EagerEvaluate("ls()").AsCharacter();
            foreach (string s in cv)
            {
                l_Return.Add(s);
            }
            return l_Return;
        }

        /// <summary>
        /// Returns the names of the columns for the given table
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="TableName">Name of your object</param>
        /// <returns>List of the columns for the table</returns>
        public static List<string> GetColumnNames(string InstanceOfR, string TableName)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            List<string> l_Return = new List<string>();
            string s_Command = string.Format("colnames({0})",
                TableName);
            CharacterVector cv = engine.EagerEvaluate(s_Command).AsCharacter();
            foreach (string s in cv)
            {
                l_Return.Add(s);
            }
            return l_Return;
        }

        /// <summary>
        /// Performs a check to determine if a table contains a given column name
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="TableName">Name of your object</param>
        /// <param name="ColumnName">Name of column wish to test</param>
        /// <returns>True: table contains a column with given name; False: table does not contain a column with given name</returns>
        public static bool TableContainsColumn(string InstanceOfR, string TableName, string ColumnName)
        {
            bool b_Return = false;
            List<string> l_Columns = GetColumnNames(InstanceOfR, TableName);
            b_Return = l_Columns.Contains(ColumnName) ? true : false;
            return b_Return;
        }

        /// <summary>
        /// Determines if an object is present in the R Workspace or not
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="ObjectName">Name of the object</param>
        /// <returns>true if the object is present in the R workspace</returns>
        public static bool ContainsObject(string InstanceOfR, string ObjectName)
        {
            List<string> l_Objects = ls(InstanceOfR);
            if (l_Objects.Contains(ObjectName))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Gets the class of an object in the R workspace
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="ObjectName">Name of the object</param>
        /// <returns>class of the object</returns>
        public static string GetClassOfObject(string InstanceOfR, string ObjectName)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            string s_RStatement = string.Format("class({0})", ObjectName);
            CharacterVector cv = engine.EagerEvaluate(s_RStatement).AsCharacter();
            return cv[0].ToString();
        }

        /// <summary>
        /// Determines if an object is of a specified class
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="ObjectName">Name of the object</param>
        /// <param name="Class">Specified class to test</param>
        /// <returns>true if the object is of that class</returns>
        public static bool IsObjectOfClass(string InstanceOfR, string ObjectName, string Class)
        {
            if (ContainsObject(InstanceOfR, ObjectName))
            {
                if (GetClassOfObject(InstanceOfR, ObjectName).Equals(Class))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        /// <summary>
        /// Evaluates a logical statement in R, and returns the result as a boolean
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="RStatement">Statement to evaluate to return TRUE or FALSE</param>
        /// <returns>TRUE or FALSE</returns>
        public static bool AssessBoolean(string InstanceOfR, string RStatement)
        {
            bool b_Return = false;
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            CharacterVector cv = engine.EagerEvaluate(RStatement).AsCharacter();
            if (cv[0].Equals("TRUE"))
                b_Return = true;
            return b_Return;
        }
        #endregion

    }
}
