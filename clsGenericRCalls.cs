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
using System.Text;
using System.Threading;

using log4net;
using RDotNet;

namespace Cyclops
{
    /// <summary>
    /// Static class for making generic calls to the R workspace.
    /// </summary>
    public static class clsGenericRCalls
    {
        #region Enums
        public enum ReturnTypes { TablesAndVectors, Tables,
            Vectors, Functions, Lists, TablesVectorsAndLists };
        #endregion

        #region Functions
        /// <summary>
        /// Creates a new instance of the R workspace
        /// </summary>
        /// <param name="engine">Engine to create instance for</param>
        /// <param name="InstanceName">Name of Instance</param>
        public static bool CreateInstance(REngine engine, string InstanceName)
        {
            ILog traceLog = LogManager.GetLogger("TraceLog");

            traceLog.Info("Creating Instance of R Workspace...");
            try
            {
                engine = REngine.CreateInstance(InstanceName, new[] { "-q" }); // quiet mode
                return true;
            }
            catch (ParseException pe)
            {
                traceLog.Error("ERROR Parse Exception Creating Instance " +
                    "\n" + pe.ToString());
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR Exception Creating Instance " +
                    "\n" + exc.ToString());
            }

            return false;
        }

        /// <summary>
        /// Creates a new instance of the R workspace
        /// </summary>
        /// <param name="InstanceName">Name of Instance</param>
        /// <returns></returns>
        public static bool CreateInstance(string InstanceName)
        {
            ILog traceLog = LogManager.GetLogger("TraceLog");

            traceLog.Info("Creating Instance of R Workspace...");
            try
            {
                REngine engine = REngine.CreateInstance(InstanceName, new[] { "-q" }); // quiet mode
                return true;
            }
            catch (ParseException pe)
            {
                traceLog.Error("ERROR Parse Exception Creating Instance " +
                    "\n" + pe.ToString());
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR Exception Creating Instance " +
                    "\n" + exc.ToString());
            }

            return false;
        }

        /// <summary>
        /// Main call method to run a command in the R environment
        /// </summary>
        /// <param name="Command">R Command to pass to the environment</param>
        /// <param name="Instance">Instance of the R workspace</param>
        /// <param name="SummaryStatement">Summary of the Command being issued, e.g. Name of Module</param>
        public static bool Run(string Command, 
            string Instance, string SummaryStatement,
            int? Step, int? TotalSteps)
        {
            REngine engine = REngine.GetInstanceFromID(Instance);
            ILog traceLog = LogManager.GetLogger("TraceLog");
            StringWriter sw = new StringWriter();
            Console.SetOut(sw);

            traceLog.Info(string.Format(
                "{0}{1}\n{2}\n{3}\n\n",
                Step == null ? "" : "Step " + Step + " of ",
                TotalSteps == null ? "" : TotalSteps + ": ",
                SummaryStatement,
                Command));
            try
            {
                engine.EagerEvaluate(Command);

                return true;
            }
            catch (ParseException pe)
            {
                traceLog.Error("ERROR ParseException Running " +
                    SummaryStatement + "\n" + pe.ToString() + "\n\n" +
                    "R ERROR MESSAGE: " + sw.ToString() + "\n\n");                
            }
            catch (IOException ioe)
            {
                traceLog.Error("ERROR IOException Running " +
                    SummaryStatement + "\n" + ioe.ToString() + "\n\n" +
                    "R ERROR MESSAGE: " + sw.ToString() + "\n\n");
            }
            catch (AccessViolationException ave)
            {
                traceLog.Error("ERROR AccessViolationException Running " +
                    SummaryStatement + "\n" + ave.ToString() + "\n\n" +
                    "R ERROR MESSAGE: " + sw.ToString() + "\n\n");
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR Exception Running " +
                    SummaryStatement + "\n" + exc.ToString() + "\n\n" +
                    "R ERROR MESSAGE: " + sw.ToString() + "\n\n");
            }

            return false;
        }

        /// <summary>
        /// Runs a call in the R environment without logging
        /// </summary>
        /// <param name="Command">R Command to pass to the environment</param>
        /// <param name="Instance">Instance of the R workspace</param>
        /// <returns>null if completed successfully, otherwise error message</returns>
        public static string RunWithoutLogging(string Command,
            string Instance, string SummaryStatement)
        {
            string s_Return = null;

            REngine engine = REngine.GetInstanceFromID(Instance);
            StringWriter sw = new StringWriter();
            Console.SetOut(sw);

            try
            {
                engine.EagerEvaluate(Command);
            }
            catch (ParseException pe)
            {
                s_Return = "ERROR ParseException Running " +
                    SummaryStatement + "\n" + pe.ToString() + "\n\n" +
                    "R ERROR MESSAGE: " + sw.ToString() + "\n\n";
            }
            catch (IOException ioe)
            {
                s_Return = "ERROR IOException Running " +
                    SummaryStatement + "\n" + ioe.ToString() + "\n\n" +
                    "R ERROR MESSAGE: " + sw.ToString() + "\n\n";
            }
            catch (AccessViolationException ave)
            {
                s_Return = "ERROR AccessViolationException Running " +
                    SummaryStatement + "\n" + ave.ToString() + "\n\n" +
                    "R ERROR MESSAGE: " + sw.ToString() + "\n\n";
            }
            catch (Exception exc)
            {
                s_Return = "ERROR Exception Running " +
                    SummaryStatement + "\n" + exc.ToString() + "\n\n" +
                    "R ERROR MESSAGE: " + sw.ToString() + "\n\n";
            }

            return s_Return;
        }

        /// <summary>
        /// Loads an R Workspace into the environment
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        /// <param name="FilePath">Full path to the RData file</param>
        /// <returns>True if loaded successfully</returns>
        public static bool LoadRWorkspace(string InstanceOfR,
            string FilePath)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);

            FilePath = FilePath.Replace('\\', '/');
            string s_Command = string.Format(
                        "load(\"{0}\")",
                        FilePath);

            return Run(s_Command, InstanceOfR,
                "Loading R Workspace",
                null, null);
        }

        /// <summary>
        /// Returns the version of R being run
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        /// <returns></returns>
        public static Dictionary<string, string> Version(string InstanceOfR)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            Dictionary<string, string> d_Return = new Dictionary<string, string>();
            GenericVector gv = engine.GetSymbol("Version").AsList();

            if (gv == null)
            {
                Console.Write("Is null!");
            }
            else
            {
                foreach (SymbolicExpression s in gv)
                {
                    Console.Write(s.ToString());
                }
            }
            return d_Return;
        }

        /// <summary>
        /// Removes an object from the R Workspace
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="Object2Remove">Name of object you would like to remove</param>
        /// <returns>true if successfully removed</returns>
        public static bool RemoveObject(string InstanceOfR, string Object2Remove)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            
            if (ContainsObject(InstanceOfR, Object2Remove))
            {
                string s_RStatement = string.Format(
                    "rm({0})\n",
                    Object2Remove);
                engine.EagerEvaluate(s_RStatement);
                return true;
            }

            return false;
        }

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
        /// Return objects in the R workspace that meet the
        /// acceptable criteria
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="TypeOfObjects">Criteria for acceptable</param>
        /// <returns></returns>
        public static List<string> GetObjects(string InstanceOfR,
            ReturnTypes TypeOfObjects)
        {
            List<string> l_All = ls(InstanceOfR);

            foreach (string l in l_All)
            {
                string s = GetClassOfObject(InstanceOfR, l);
                if (!AcceptableType(s, TypeOfObjects))
                    l_All.Remove(s);
            }

            return l_All;
        }

        public static bool IsNull(string InstanceOfR,
            string ObjectName)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            bool b_Return = false;

            if (!Exists(InstanceOfR, ObjectName))
                return true;
       
            CharacterVector cv = engine.EagerEvaluate(
                string.Format("is.null({0})",
                ObjectName)).AsCharacter();

            List<string> l_Return = new List<string>();
            foreach (string s in cv)
            {
                l_Return.Add(s);
            }

            if (l_Return[0].Equals("TRUE"))
                b_Return = true;
            else
                b_Return = false;

            return b_Return;
        }

        /// <summary>
        /// Indicates if the given type meets the 
        /// specified criteria
        /// </summary>
        /// <param name="Type">R type</param>
        /// <param name="AcceptableType">Criteria</param>
        /// <returns></returns>
        public static bool AcceptableType(string Type,
            ReturnTypes AcceptableType)
        {
            switch (AcceptableType)
            {
                case ReturnTypes.TablesVectorsAndLists:
                    switch (Type)
                    {
                        case "data.frame":
                            return true;
                        case "matrix":
                            return true;
                        case "list":
                            return true;
                        case "character":
                            return true;
                        case "numeric":
                            return true;
                    }
                    break;
                case ReturnTypes.Functions:
                    switch (Type)
                    {
                        case "function":
                            return true;
                    }
                    break;
                case ReturnTypes.Lists:
                    switch (Type)
                    {
                        case "list":
                            return true;
                    }
                    break;
                case ReturnTypes.Tables:
                    switch (Type)
                    {
                        case "data.frame":
                            return true;
                        case "matrix":
                            return true;
                    }
                    break;
                case ReturnTypes.Vectors:
                    switch (Type)
                    {
                        case "character":
                            return true;
                        case "numeric":
                            return true;
                    }
                    break;
                case ReturnTypes.TablesAndVectors:
                    switch (Type)
                    {
                        case "data.frame":
                            return true;
                        case "matrix":
                            return true;
                        case "character":
                            return true;
                        case "numeric":
                            return true;
                    }
                    break;
            }
            return false;
        }

        public static List<string> ConnectLoadAndGetObjects(string InstanceOfR, 
            string FilePath, ReturnTypes TypeOfObjects)
        {
            List<string> l_Return = new List<string>();

            StringWriter sw = new StringWriter();
            ILog traceLog = LogManager.GetLogger("TraceLog");
            string SummaryStatement = "Connecting, Loading, and Retrieving Objects";

            traceLog.Info("Creating Instance of R Workspace...");
            try
            {
                REngine engine = REngine.CreateInstance(InstanceOfR, new[] { "-q" }); // quiet mode
                FilePath = FilePath.Replace('\\', '/');
                string s_Command = string.Format(
                            "load(\"{0}\")\n",
                            FilePath);
                s_Command += "ls()";
                CharacterVector cv = engine.EagerEvaluate(s_Command).AsCharacter();
                foreach (string s in cv)
                {
                    string s_Class = GetClassOfObject(InstanceOfR, s);
                    if (AcceptableType(s_Class, TypeOfObjects))
                        l_Return.Add(s);
                }
            }
            catch (ParseException pe)
            {
                traceLog.Error("ERROR ParseException Running " +
                    SummaryStatement + "\n" + pe.ToString() + "\n\n" +
                    "R ERROR MESSAGE: " + sw.ToString() + "\n\n");  
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR Exception Running " +
                    SummaryStatement + "\n" + exc.ToString() + "\n\n" +
                    "R ERROR MESSAGE: " + sw.ToString() + "\n\n");
            }

            return l_Return;
        }

        public static string GetStatement(string InstanceOfR,
            string Command)
        {
            string s_Return = null;
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            CharacterVector cv = engine.EagerEvaluate(Command).AsCharacter();
            foreach (string s in cv)
            {
                s_Return += s + ";";
            }
            return s_Return.Trim();
        }

        /// <summary>
        /// Returns the dimensions for an object in the R workspace
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="ObjectName">Object to get dimensions of</param>
        /// <returns>List of dimensions, rows [0], columns [1]</returns>
        public static List<int> GetDimensions(string InstanceOfR, string ObjectName)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            List<int> l_Return = new List<int>();
            IntegerVector iv = engine.EagerEvaluate("dim(" + ObjectName + ")").AsInteger();
            foreach (int i in iv)
            {
                l_Return.Add(i);
            }
            return l_Return;
        }

        /// <summary>
        /// Searches the column names of the specified Object for columns with the 
        /// specified Search Term, and returns the indices.
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="ObjectName">Table to search headers for</param>
        /// <param name="SearchTerm">Term to look for in the headers</param>
        /// <returns>Indicies that contain the search term</returns>
        public static List<int> SearchColumnNames(string InstanceOfR, string ObjectName, string SearchTerm)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            List<int> l_Return = new List<int>();
            IntegerVector iv = engine.EagerEvaluate("grep('" + SearchTerm + 
                "', colnames(" + ObjectName + "))").AsInteger();
            foreach (int i in iv)
            {
                l_Return.Add(i);
            }
            return l_Return;
        }

        /// <summary>
        /// Returns the factor levels for a given vector
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="ObjectName">Vector to get factor levels for</param>
        /// <returns>factor levels</returns>
        public static List<string> GetFactorLevels(string InstanceOfR, string ObjectName)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            List<string> l_Return = new List<string>();
            CharacterVector cv = engine.EagerEvaluate("levels(factor(" + ObjectName + "))").AsCharacter();
            foreach (string s in cv)
            {
                l_Return.Add(s);
            }
            return l_Return;
        }

        /// <summary>
        /// Returns the minimum value of an object in the R workspace,
        /// with the exception of NA values.
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="ObjectName">Object to return minimum of</param>
        /// <returns></returns>
        public static double? GetMinimumValue(string InstanceOfR, string ObjectName)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);

            string s_Command = string.Format("min({0}, na.rm=TRUE)");
            NumericVector nv = engine.EagerEvaluate(s_Command).AsNumeric();
            return nv[0];
        }

        /// <summary>
        /// Returns the maximum value of an object in the R workspace,
        /// with the exception of NA values.
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="ObjectName">Object to return maximum of</param>
        /// <returns>Maximum Value</returns>
        public static double? GetMaximumValue(string InstanceOfR, string ObjectName)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);

            string s_Command = string.Format("max({0}, na.rm=TRUE)");
            NumericVector nv = engine.EagerEvaluate(s_Command).AsNumeric();
            return nv[0];
        }

        /// <summary>
        /// Returns the length of a vector
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="Vector">Vector to return length of</param>
        /// <returns>Length of Vector</returns>
        public static Int32 GetLengthOfVector(string InstanceOfR, string Vector)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);

            string s_Command = string.Format("length({0})",
                Vector);
            IntegerVector iv = engine.EagerEvaluate(s_Command).AsInteger();
            return iv[0];
        }

        public static List<string> GetCharacterVector(string InstanceOfR, string Vector)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            List<string> l_Return = new List<string>();
            CharacterVector cv = engine.EagerEvaluate(Vector).AsCharacter();
            foreach (string s in cv)
            {
                l_Return.Add(s);
            }

            return l_Return;
        }

        /// <summary>
        /// Get the number of unique entries for a column in a given table
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="TableName">Name of the Table</param>
        /// <param name="ColumnName">Name of Column in Table</param>
        /// <returns>Number of unique entries in the column</returns>
        public static int GetUniqueLengthOfColumn(string InstanceOfR,
            string TableName, string ColumnName)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);

            string s_Command = string.Format("length(unique({0}${1}))",
                TableName,
                ColumnName);
            IntegerVector iv = engine.EagerEvaluate(s_Command).AsInteger();
            return iv[0];
        }

        /// <summary>
        /// Retrieves the unique elements within a column in a table
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="TableName">Name of the Table</param>
        /// <param name="ColumnName">Name of Column in Table</param>
        /// <returns>Unique elements within the column</returns>
        public static List<string> GetUniqueColumnElementsWithinTable(string InstanceOfR,
            string TableName, string ColumnName)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);

            string s_Command = string.Format("unique(as.character({0}${1}))",
                TableName,
                ColumnName);
            CharacterVector cv = engine.EagerEvaluate(s_Command).AsCharacter();
            List<string> l_Return = new List<string>();
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
        /// Tests whether an object exists in the workspace
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="ObjectName">Name of object</param>
        /// <returns>TRUE if object exists</returns>
        public static bool Exists(string InstanceOfR, string ObjectName)
        {
            bool b_Return = false;
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            string RStatement = "exists(\"" + ObjectName + "\")";
            CharacterVector cv = engine.EagerEvaluate(RStatement).AsCharacter();
            List<string> l_Return = new List<string>();
            foreach (string s in cv)
            {
                l_Return.Add(s);
            }

            if (l_Return[0].Equals("TRUE"))
                b_Return = true;

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
            if (!ObjectName.Contains("$"))
            {
                List<string> l_Objects = ls(InstanceOfR);
                if (l_Objects.Contains(ObjectName))
                    return true;
                else
                    return false;
            }
            else
            {
                string[] s_Split = ObjectName.Split('$');
                if (s_Split.Length == 2)
                {
                    List<string> l_Objects = ls(InstanceOfR);
                    if (l_Objects.Contains(s_Split[0]))
                        return true;
                    else
                        return false;
                }
                else
                {                    
                    return false;  
                }                    
            }
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
        /// Return the current working directory
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <returns>Current working directory</returns>
        public static string GetWorkingDirectory(string InstanceOfR)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            string s_RStatement = string.Format("getwd()");
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

        /// <summary>
        /// Installs a specified package into the R workspace
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="Package">Name of the package</param>
        public static void InstallPackage(string InstanceOfR, string Package)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            engine.EagerEvaluate("install.packages(\"" + Package + "\")");            
        }

        /// <summary>
        /// Determines if R has a specified package already installed or not
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="Package">Name of your package</param>
        /// <returns>>TRUE or FALSE</returns>
        public static bool IsPackageInstalled(string InstanceOfR, string Package)
        {
            bool b_Return = false;
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            string s_RStatement = string.Format("jnbIsPackageInstalled(\"{0}\")",
                Package);
            CharacterVector cv = engine.EagerEvaluate(s_RStatement).AsCharacter();
            if (cv[0].Equals("TRUE"))
                b_Return = true;
            return b_Return;
        }

        /// <summary>
        /// Converts an data.frame from R into a DataTable in C#
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="TheDataFrame">Name of data.frame</param>
        /// <returns>DataTable version of your data.frame</returns>
        public static DataTable GetDataTable(string InstanceOfR, string TheDataFrame)
        {
            DataTable dt_Return = new DataTable();
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
                        
            if (GetClassOfObject(InstanceOfR, TheDataFrame).Equals("data.frame"))
            {
                string s_TempTableName = GetTemporaryTableName();
                string s_Cmd = string.Format(
                    "{0} <- data.frame(sapply({1}, function(x) " +
                    "as.character(unlist(x))), stringsAsFactors=F)\n",
                    s_TempTableName,
                    TheDataFrame);
                engine.EagerEvaluate(s_Cmd);

                DataFrame dataset = engine.EagerEvaluate(s_TempTableName).AsDataFrame();
                
                for (int i = 0; i < dataset.ColumnCount; i++)
                {
                    DataColumn dc = new DataColumn(dataset.ColumnNames[i]);
                    if (dc.Namespace.Equals(""))
                    {
                        int j = i + 1;
                        dc.Namespace = j.ToString();
                    }
                    dt_Return.Columns.Add(dc);
                }
                for (int r = 0; r < dataset.RowCount; r++)
                {
                    DataFrameRow df_Row = dataset.GetRow(r);

                    string[] s_Row = new string[df_Row.DataFrame.ColumnCount];
                    for (int i = 0; i < df_Row.DataFrame.ColumnCount; i++)
                    {
                        s_Row[i] = df_Row[i].ToString();
                    }
                    dt_Return.Rows.Add(s_Row);
                }

                engine.EagerEvaluate(string.Format("rm({0})\n",
                    s_TempTableName));
            }
            else if (GetClassOfObject(InstanceOfR, TheDataFrame).Equals("matrix"))
            {
                DataFrame dataset = engine.EagerEvaluate("data.frame(" + TheDataFrame + ")").AsDataFrame();

                for (int i = 0; i < dataset.ColumnCount; i++)
                {
                    DataColumn dc = new DataColumn(dataset.ColumnNames[i]);
                    if (dc.Namespace.Equals(""))
                    {
                        int j = i + 1;
                        dc.Namespace = j.ToString();
                    }
                    dt_Return.Columns.Add(dc);
                }

                for (int r = 0; r < dataset.RowCount; r++)
                {
                    DataFrameRow df_Row = dataset.GetRow(r);

                    string[] s_Row = new string[df_Row.DataFrame.ColumnCount];
                    for (int i = 0; i < df_Row.DataFrame.ColumnCount; i++)
                    {
                        s_Row[i] = df_Row[i].ToString();
                    }
                    dt_Return.Rows.Add(s_Row);
                }
            }



            return dt_Return;
        }


        /// <summary>
        /// Converts an data.frame from R into a DataTable in C#
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>        
        /// <param name="TheDataFrame">Name of data.frame</param>
        /// <param name="NameOfFirstColumn">Name of the Rowname column, defaults to "RowNames"</param>
        /// <returns>DataTable version of your data.frame</returns>
        public static DataTable GetDataTableIncludingRownames(string InstanceOfR, string TheDataFrame, 
            string NameOfFirstColumn)
        {
            if (!ContainsObject(InstanceOfR, TheDataFrame))
            {                
                return null;
            }

            List<string> l_Rownames = GetRowNames(InstanceOfR, TheDataFrame);

            if (string.IsNullOrEmpty(NameOfFirstColumn))
                NameOfFirstColumn = "RowNames";
            
            DataTable dt_Return = new DataTable();
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);

            if (GetClassOfObject(InstanceOfR, TheDataFrame).Equals("data.frame"))
            {
                DataFrame dataset = engine.EagerEvaluate(TheDataFrame).AsDataFrame();

                DataColumn dc_RowName = new DataColumn(NameOfFirstColumn);
                dt_Return.Columns.Add(dc_RowName);

                for (int i = 0; i < dataset.ColumnCount; i++)
                {
                    DataColumn dc = new DataColumn(dataset.ColumnNames[i]);
                    if (dc.Namespace.Equals(""))
                    {
                        int j = i + 1;
                        dc.Namespace = j.ToString();
                    }
                    dt_Return.Columns.Add(dc);
                }

                // iterate across the rows
                for (int r = 0; r < dataset.RowCount; r++)
                {
                    DataFrameRow df_Row = dataset.GetRow(r);

                    string[] s_Row = new string[df_Row.DataFrame.ColumnCount+1];
                    s_Row[0] = l_Rownames[r];
                    for (int i = 0; i < df_Row.DataFrame.ColumnCount; i++)
                    {
                        s_Row[i+1] = df_Row[i].ToString();
                    }
                    dt_Return.Rows.Add(s_Row);
                }
            }
            else if (GetClassOfObject(InstanceOfR, TheDataFrame).Equals("matrix"))
            {
                DataFrame dataset = engine.EagerEvaluate("data.frame(" + TheDataFrame + ")").AsDataFrame();

                DataColumn dc_RowName = new DataColumn(NameOfFirstColumn);
                dt_Return.Columns.Add(dc_RowName);

                for (int i = 0; i < dataset.ColumnCount; i++)
                {
                    DataColumn dc = new DataColumn(dataset.ColumnNames[i]);
                    if (dc.Namespace.Equals(""))
                    {
                        int j = i + 1;
                        dc.Namespace = j.ToString();
                    }
                    dt_Return.Columns.Add(dc);
                }

                // iterate across the rows
                for (int r = 0; r < dataset.RowCount; r++)
                {
                    DataFrameRow df_Row = dataset.GetRow(r);

                    string[] s_Row = new string[df_Row.DataFrame.ColumnCount + 1];
                    s_Row[0] = l_Rownames[r];
                    for (int i = 0; i < df_Row.DataFrame.ColumnCount; i++)
                    {
                        s_Row[i+1] = df_Row[i].ToString();
                    }
                    dt_Return.Rows.Add(s_Row);
                }
            }

            return dt_Return;
        }


        /// <summary>
        /// Gets the row names for a data.frame or matrix in the R workspace
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        /// <param name="ObjectName">Name of the data.frame or matrix</param>
        /// <returns>List of the rownames</returns>
        public static List<string> GetRowNames(string InstanceOfR, string ObjectName)
        {
            List<string> l_Return = new List<string>();

            if (GetClassOfObject(InstanceOfR, ObjectName).Equals("data.frame") ||
                GetClassOfObject(InstanceOfR, ObjectName).Equals("matrix"))
            {
                REngine engine = REngine.GetInstanceFromID(InstanceOfR);

                CharacterVector cv = engine.EagerEvaluate(string.Format("rownames({0})",
                    ObjectName)).AsCharacter();

                for (int i = 0; i < cv.Length; i++)
                    l_Return.Add(cv[i]);
            }
            return l_Return;
        }

        /// <summary>
        /// Converts an data.frame from R into a DataTable in C#
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="TheDataFrame">Name of data.frame</param>
        /// <param name="IgnoreLs">If you table of interest is hidden within a list or object, set this to true</param>
        /// <returns>DataTable version of your data.frame</returns>
        public static DataTable GetDataTable(string InstanceOfR, string TheDataFrame,
            bool IgnoreLs)
        {
            DataTable dt_Return = new DataTable();
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);

            if (IgnoreLs || ContainsObject(InstanceOfR, TheDataFrame))
            {
                if (GetClassOfObject(InstanceOfR, TheDataFrame).Equals("data.frame"))
                {
                    DataFrame dataset = engine.EagerEvaluate(TheDataFrame).AsDataFrame();

                    for (int i = 0; i < dataset.ColumnCount; i++)
                    {
                        DataColumn dc = new DataColumn(dataset.ColumnNames[i]);
                        if (dc.Namespace.Equals(""))
                        {
                            int j = i + 1;
                            dc.Namespace = j.ToString();
                        }
                        dt_Return.Columns.Add(dc);
                    }

                    for (int r = 0; r < dataset.RowCount; r++)
                    {
                        DataFrameRow df_Row = dataset.GetRow(r);

                        string[] s_Row = new string[df_Row.DataFrame.ColumnCount];
                        for (int i = 0; i < df_Row.DataFrame.ColumnCount; i++)
                        {
                            s_Row[i] = df_Row[i].ToString();
                        }
                        dt_Return.Rows.Add(s_Row);
                    }
                }
                else if (GetClassOfObject(InstanceOfR, TheDataFrame).Equals("matrix"))
                {
                    DataFrame dataset = engine.EagerEvaluate("data.frame(" +TheDataFrame + ")").AsDataFrame();

                    for (int i = 0; i < dataset.ColumnCount; i++)
                    {
                        DataColumn dc = new DataColumn(dataset.ColumnNames[i]);
                        if (dc.Namespace.Equals(""))
                        {
                            int j = i + 1;
                            dc.Namespace = j.ToString();
                        }
                        dt_Return.Columns.Add(dc);
                    }

                    for (int r = 0; r < dataset.RowCount; r++)
                    {
                        DataFrameRow df_Row = dataset.GetRow(r);

                        string[] s_Row = new string[df_Row.DataFrame.ColumnCount];
                        for (int i = 0; i < df_Row.DataFrame.ColumnCount; i++)
                        {
                            s_Row[i] = df_Row[i].ToString();
                        }
                        dt_Return.Rows.Add(s_Row);
                    }
                }
            }

            return dt_Return;
        }

        /// <summary>
        /// Converts a DataTable to a DataFrame in a given R workspace
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="Table">The DataTable you want to convert to a data.frame</param>
        /// <param name="DataFrameName">Name of the new data.frame</param>
        public static void SetDataFrame(string InstanceOfR, DataTable Table, string DataFrameName)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            string s_RStatement = string.Format("{0} <- data.frame(",
                DataFrameName);

            for (int c = 0; c < Table.Columns.Count; c++)
            {
                s_RStatement += "\"" + Table.Columns[c].ColumnName + "\"=c(\"";

                for (int r = 0; r < Table.Rows.Count; r++)
                {
                    if (r < Table.Rows.Count - 1)
                    {
                        s_RStatement += Table.Rows[r][c].ToString() + "\",\"";
                    }
                    else
                    {
                        s_RStatement += Table.Rows[r][c].ToString() + "\")";
                    }
                }

                if (c < Table.Columns.Count - 1)
                {
                    s_RStatement += ",";
                }
                else
                {
                    s_RStatement += ")";
                }
            }

            try
            {
                engine.EagerEvaluate(s_RStatement);
            }
            catch (Exception exc)
            {
                // TODO: Handle problems creating the data.frame
            }
        }

        /// <summary>
        /// Quick way to save your work environment
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="WorkspaceFileName">Name of the file you'd like to save the environment as</param>
        public static void SaveEnvironment(string InstanceOfR, string WorkspaceFileName)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            string s_RStatement = string.Format("save.image(file=\"{0}\")",
                WorkspaceFileName);

            try
            {
                engine.EagerEvaluate(s_RStatement);
            }
            catch (Exception exc)
            {
                // TODO : Handle problems saving image
            }
        }

        /// <summary>
        /// Sets the specified column to the row.names, and then deletes that column
        /// from the data.frame
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="DataFrameName">Name of the Data Frame</param>
        /// <param name="ColumnIndex">Index of the column (first column starts at 1)</param>
        public static void SetDataFrameRowNames(string InstanceOfR,
            string DataFrameName, int ColumnIndex)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            string s_RStatement = string.Format("rownames({0}) <- {0}[,{1}]\n" +
                "{0} <- {0}[,-{1}]",
                DataFrameName, ColumnIndex.ToString());

            try
            {
                engine.EagerEvaluate(s_RStatement);
            }
            catch (Exception exc)
            {
                // TODO : Handle problems saving image
            }
        }

        /// <summary>
        /// Converts a DataTable to a Matrix in a given R workspace
        /// </summary>
        /// <param name="InstanceOfR">Instance of your R workspace</param>
        /// <param name="Table">The DataTable you want to convert to a data.frame</param>
        /// <param name="MatrixName">Name of the new matrix</param>
        public static void SetMatrix(string InstanceOfR, DataTable Table, string MatrixName)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            string s_RStatement = string.Format("{0} <- matrix(",
                MatrixName);

            string[] s_Headers = new string[Table.Columns.Count];
            for (int i = 0; i < Table.Columns.Count; i++)
            {
                s_Headers[i] = Table.Columns[i].ColumnName;
            }

            try
            {
            }
            catch (Exception exc)
            {
                // TODO: Handle problems creating the data.frame
            }
        }

        public static string SetObject(string InstanceOfR, string ObjectName,
            string Value)
        {
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);
            string Command = ObjectName + " <- " + Value;
            try
            {
                engine.EagerEvaluate(Command);
                return null;
            }
            catch (ParseException pe)
            {
                return pe.ToString();
            }
            catch (Exception exc)
            {
                return exc.ToString();
            }
        }

        /// <summary>
        /// Sets the System Environment Variable R_HOME
        /// </summary>
        /// <param name="rHome">Version of R being run, 
        /// if you don't know pass Null or empty string and 
        /// the function will search for it</param>
        /// <param name="ThirtyTwoBit">True if using 32-bit
        /// RdotNet dll, False if 64-bit</param>
        public static void Set_R_Home_Variable(string rVersion, bool IsThirtyTwoBit)
        {
            ILog traceLog = LogManager.GetLogger("TraceLog");

            string rHome = System.Environment.GetEnvironmentVariable("R_HOME"),
                s_ProgramFilesPath = @"C:\Program Files",
                s_BitFolderPath = IsThirtyTwoBit ? @"\bin\i386" : @"\bin\x64",
                s_FinalPath = "";
            try
            {
                if (!string.IsNullOrEmpty(rVersion))
                {
                    if (!rVersion.StartsWith("R-"))
                        rVersion = "R-" + rVersion;

                    rVersion = Path.Combine(s_ProgramFilesPath,
                        "R", rVersion);
                }

                if (string.IsNullOrEmpty(rHome))
                {
                    // if nothing is passed in or the version
                    // does not exist,
                    // grab the latest version on the 
                    // local machine
                    if (string.IsNullOrEmpty(rVersion) ||
                        !Directory.Exists(rVersion))
                    {
                        string s = @"C:\Program Files\R";
                        string[] d = Directory.GetDirectories(s);

                        for (int i = 0; i < d.Length; i++)
                        {
                            if (i == 0)
                                rVersion = d[i];
                            else
                            {
                                int c = string.Compare(rVersion, d[i]);
                                if (c < 0)
                                    rVersion = d[i];
                            }
                        }
                    }

                    rHome = rVersion;

                }

                System.Environment.SetEnvironmentVariable("R_HOME", rHome);
                s_FinalPath = System.Environment.GetEnvironmentVariable("PATH") +
                    ";" + rHome + s_BitFolderPath;
                System.Environment.SetEnvironmentVariable("PATH",
                    s_FinalPath);
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR Setting R_HOME to " +
                    Path.Combine(rHome, s_BitFolderPath) +
                    "\nERROR: " + exc.ToString());
            }

            traceLog.Info("Setting \"R_HOME\":\n\t" +
                rHome + s_BitFolderPath + "\n\n");
        }

        private static string GetTemporaryTableName()
        {
            string s_Suffix = "";
            Thread.Sleep(2);
            Random rnd = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            string chars = "2346789ABCDEFGHJKLMNPQRTUVWXYZabcdefghjkmnpqrtuvwxyz";

            for (int i = 0; i < 20; i++)
            {
                s_Suffix += chars.Substring(rnd.Next(chars.Length), 1);
            }

            return "tmpTable_" + s_Suffix;
        }
        #endregion

    }
}
