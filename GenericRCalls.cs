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
using System.Threading;
using RDotNet;

namespace Cyclops
{
    /// <summary>
    /// Structure for housing a Table's dimensions
    /// </summary>
    public struct TableInfo
    {
        public string Title { get; set; }
        public int Columns { get; set; }
        public int Rows { get; set; }
        public string ObjectType { get; set; }
    }

    /// <summary>
    /// Generic class for making generic calls to the R workspace.
    /// </summary>
    public class GenericRCalls
    {
        #region Members
        private REngine engine;
        private string m_RPackageLocation = "http://cran.cs.wwu.edu/";
        #endregion

        #region Properties
        /// <summary>
        /// Instance of R Workspace
        /// </summary>
        public string InstanceOfR { get; set; }

        /// <summary>
        /// Instance of the Model Class
        /// </summary>
        public CyclopsModel Model
        {
            get;
            set;
        }

        /// <summary>
        /// Website to download R packages for installation
        /// </summary>
        public string RPackageLocation
        {
            get { return m_RPackageLocation; }
            set { m_RPackageLocation = value; }
        }
        #endregion

        #region Constructors
        public GenericRCalls()
        {
        }

        public GenericRCalls(CyclopsModel CyclopsModel)
        {
            Model = CyclopsModel;
        }        
        #endregion

        #region Instantiating R Environment
        /// <summary>
        /// Instantiates R
        /// </summary>
        /// <returns></returns>
        public bool InstantiateR()
        {
            try
            {
                REngine.SetEnvironmentVariables();
                engine = REngine.GetInstance();
                engine.Initialize();
                return true;
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while instantiating R: " + ex, "Generic R Calls");
                return false;
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Main call method to run a command in the R environment
        /// </summary>
        /// <param name="Command">R Command to pass to the environment</param>
        /// <param name="SummaryStatement">Summary of the Command being issued, e.g. Name of Module</param>
        /// <param name="Step">Module Number running the command</param>
        /// <returns>True, if the command is issued successfully</returns>
        public bool Run(string Command, string SummaryStatement,
            int Step)
        {
            var b_Successful = true;

            try
            {
                Model.LogMessage(Command, SummaryStatement, Step);

                var se = engine.Evaluate(Command);                
            }
            catch (ParseException pe)
            {
                Model.LogError("ParseException encountered while running command:\n" +
                    Command + "\nParseException: " + pe.Message +
                    "\nInnerException: " + pe.InnerException,
                    SummaryStatement, Step);
                b_Successful = false;
            }
            catch (IOException ioe)
            {
                Model.LogError("IOException encountered while running command:\n" +
                    Command + "\nIOException: " + ioe.Message +
                    "\nInnerException: " + ioe.InnerException,
                    SummaryStatement, Step);
                b_Successful = false;
            }
            catch (AccessViolationException ave)
            {
                Model.LogError("AccessViolationException encountered while running command:\n" +
                    Command + "\nAccessViolationException: " + ave.Message +
                    "\nInnerException: " + ave.InnerException,
                    SummaryStatement, Step);
                b_Successful = false;
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while running command:\n" +
                    Command + "\nException: " + ex.Message +
                    "\nInnerException: " + ex.InnerException,
                    SummaryStatement, Step);
                b_Successful = false;
            }

            return b_Successful;
        }

        /// <summary>
        /// Loads an R Workspace into the environment
        /// </summary>
        /// <param name="WorkspaceFileName">Full path to the RData file</param>
        /// <param name="Module">Name of module calling LoadRWorkspace</param>
        /// <param name="Step">Module Number running LoadRWorkspace</param>
        /// <returns>True, if loaded successfully</returns>
        public bool LoadRWorkspace(string WorkspaceFileName,
            string Module, int Step)
        {
            WorkspaceFileName = WorkspaceFileName.Replace('\\', '/');
            var Command = string.Format(
                        "load(\"{0}\")",
                        WorkspaceFileName);

            return Run(Command, Module, Step);
        }

        /// <summary>
        /// Returns the version of R being run
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> Version()
        {
            var d_Return = new Dictionary<string, string>();

            try
            {
                Model.LogMessage("Retrieving R Version Information...");
                var df = engine.Evaluate("version").AsDataFrame();
                foreach (var s in df.ColumnNames)
                {
                    d_Return.Add(s, df[0, s].ToString());       
                }
            }
            catch (Exception ex)
            {
                Model.LogError("Error retrieving R version():\n" +
                    ex.Message + "\nInnerException: " +
                    ex.InnerException);
                return null;
            }
            return d_Return;
        }

        /// <summary>
        /// Returns all the objects in the R workspace
        /// </summary>
        /// <returns>List of all the objects in the R workspace</returns>
        public List<string> ls()
        {
            var l_Objects = new List<string>();

            var cv = engine.Evaluate("ls()").AsCharacter();
            foreach (var s in cv)
                l_Objects.Add(s);

            return l_Objects;
        }

        /// <summary>
        /// Determines if an object is present in the R Workspace or not
        /// </summary>
        /// <param name="ObjectName">Name of the object</param>
        /// <returns>True, if the object is present in the R workspace</returns>
        public bool ContainsObject(string ObjectName)
        {
            if (!ObjectName.Contains("$"))
            {
                var l_Objects = ls();
                if (l_Objects.Contains(ObjectName))
                    return true;
                else
                    return false;
            }
            else
            {
                var s_Split = ObjectName.Split('$');
                if (s_Split.Length == 2)
                {
                    var l_Objects = ls();
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
        }

        /// <summary>
        /// Removes an object from the R Workspace
        /// </summary>
        /// <param name="Object2Remove">Name of object you would like to remove</param>
        /// <returns>True, if successfully removed</returns>
        public bool RemoveObject(string Object2Remove)
        {
            var b_Successful = true;

            if (!string.IsNullOrEmpty(Object2Remove))
            {
                try
                {
                    engine.Evaluate(string.Format("rm({0})\n",
                        Object2Remove));
                }
                catch (Exception ex)
                {
                    Model.LogError("Exception caught while trying to remove an object:\n" +
                        "Object to remove: " + Object2Remove + "\n" +
                        "Exception: " + ex.Message + "\n" +
                        "InnerException: " + ex.InnerException);
                    b_Successful = false;
                }
            }

            return b_Successful;
        }

        /// <summary>
        /// Gets the class of an object in the R workspace
        /// </summary>
        /// <param name="ObjectName">Name of the object</param>
        /// <returns>class of the object</returns>
        public string GetClassOfObject(string ObjectName)
        {
            if (ContainsObject(ObjectName))
            {
                var s_RStatement = string.Format("class({0})", ObjectName);
                var cv = engine.Evaluate(s_RStatement).AsCharacter();
                return cv[0];
            }
            else 
                return null;
        }

        /// <summary>
        /// Return the current working directory
        /// </summary>
        /// <returns>Current working directory</returns>
        public string GetWorkingDirectory()
        {
            var s_RStatement = "getwd()";
            var cv = engine.Evaluate(s_RStatement).AsCharacter();
            return cv[0];
        }

        /// <summary>
        /// Determines if an object is of a specified class
        /// </summary>
        /// <param name="ObjectName">Name of the object</param>
        /// <param name="Class">Specified class to test</param>
        /// <returns>true if the object is of that class</returns>
        public  bool IsObjectOfClass(string ObjectName, string Class)
        {
            if (ContainsObject(ObjectName))
            {
                if (GetClassOfObject(ObjectName).Equals(Class))
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
        /// <param name="RStatement">Statement to evaluate to return TRUE or FALSE</param>
        /// <returns>TRUE or FALSE</returns>
        public bool AssessBoolean(string RStatement)
        {
            var b_Return = false;

            var cv = engine.Evaluate(RStatement).AsCharacter();
            if (cv[0].ToUpper().Equals("TRUE"))
                b_Return = true;
            return b_Return;
        }
        
        /// <summary>
        /// Installs a specified package into the R workspace
        /// </summary>
        /// <param name="Package">Name of the package</param>
        public void InstallPackage(string Package)
        {
            engine.Evaluate(string.Format(
                "install.packages('{0}', repos='{1}')",
                Package,
                RPackageLocation));
        }

        /// <summary>
        /// Determines if R has a specified package already installed or not
        /// </summary>
        /// <param name="Package">Name of your package</param>
        /// <returns>>TRUE or FALSE</returns>
        public bool IsPackageInstalled(string Package)
        {
            var b_Return = false;
            var s_RStatement = string.Format("jnbIsPackageInstalled('{0}')",
                Package);
            var cv = engine.Evaluate(s_RStatement).AsCharacter();
            if (cv[0].ToUpper().Equals("TRUE"))
                b_Return = true;
            return b_Return;
        }

        /// <summary>
        /// Quick way to save your work environment
        /// </summary>
        /// <param name="FileName">Name of the file you'd like to save the environment as</param>
        /// <returns>True, if the R environment is saved successfully</returns>
        public bool SaveEnvironment(string FileName)
        {
            var b_Successful = true;

            var Command = string.Format("save.image(file=\"{0}\")",
                FileName);
            Command = Command.Replace("\\", "/");
            try
            {
                engine.Evaluate(Command);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while Saving R " +
                    "environment:\n" + ex.Message);
                b_Successful = false;
            }

            return b_Successful;
        }

        /// <summary>
        /// Gets the row names for a data.frame or matrix in the R workspace
        /// </summary>
        /// <param name="ObjectName">Name of the data.frame or matrix</param>
        /// <returns>List of the rownames</returns>
        public List<string> GetRowNames(string ObjectName)
        {
            var l_Return = new List<string>();

            if (GetClassOfObject(ObjectName).Equals("data.frame") ||
                GetClassOfObject(ObjectName).Equals("matrix"))
            {
                var cv = engine.Evaluate(string.Format("rownames({0})",
                    ObjectName)).AsCharacter();

                for (var i = 0; i < cv.Length; i++)
                    l_Return.Add(cv[i]);
            }

            return l_Return;
        }

        /// <summary>
        /// Returns all the column names for a data.frame or matrix
        /// </summary>
        /// <param name="ObjectName">Name of the data.frame or matrix</param>
        /// <returns>Column Names</returns>
        public List<string> GetColumnNames(string ObjectName)
        {
            return GetColumnNames(ObjectName, false);
        }

        /// <summary>
        /// Returns the column names for a data.frame or matrix
        /// </summary>
        /// <param name="ObjectName">Name of the data.frame or matrix</param>
        /// <param name="Unique">True, if only unique column names are to be included</param>
        /// <returns>Column Names</returns>
        public List<string> GetColumnNames(string ObjectName, bool Unique)
        {
            var l_Columns = new List<string>();

            if (GetClassOfObject(ObjectName).Equals("data.frame") ||
                GetClassOfObject(ObjectName).Equals("matrix"))
            {
                var cv = engine.Evaluate(string.Format("colnames({0})",
                    ObjectName)).AsCharacter();

                for (var i = 0; i < cv.Length; i++)
                {
                    if (Unique)
                    {
                        if (!l_Columns.Contains(cv[i]))
                            l_Columns.Add(cv[i]);
                    }
                    else
                        l_Columns.Add(cv[i]);
                }
            }

            return l_Columns;
        }

        /// <summary>
        /// Gets a list of strings from R
        /// </summary>
        /// <param name="Vector">Vector to retrieve</param>
        /// <returns>List of strings in the returned Vector</returns>
        public List<string> GetCharacterVector(string Vector)
        {
            var l_Return = new List<string>();
            var cv = engine.Evaluate(Vector).AsCharacter();
            foreach (var s in cv)
            {
                l_Return.Add(s);
            }

            return l_Return;  
        }
        
        /// <summary>
        /// Gets the Dimensions of a data.frame or matrix
        /// </summary>
        /// <param name="ObjectName">Name of the data.frame or matrix</param>
        /// <returns>Dimensions of the Table</returns>
        public TableInfo GetDimensions(string ObjectName)
        {
            var td = new TableInfo
            {
                Title = ObjectName,
                Columns = 0,
                Rows = 0
            };

            if (ContainsObject(ObjectName))
            {
                if (GetClassOfObject(ObjectName).Equals("data.frame") ||
                    GetClassOfObject(ObjectName).Equals("matrix"))
                {
                    var dim = engine.Evaluate(
                        string.Format("dim({0})\n",
                        ObjectName)).AsInteger();
                    if (dim.Length == 2)
                    {
                        td.Rows = dim[0];
                        td.Columns = dim[1];
                    }
                }
            }            

            return td;
        }

        /// <summary>
        /// Gets the standard information about an object in R, 
        /// including the dimensions and the type of object
        /// </summary>
        /// <param name="ObjectName">Name of the object that you'd 
        /// like information for</param>
        /// <returns>Information about the object</returns>
        public TableInfo GetTableInfo(string ObjectName)
        {
            var td = GetDimensions(ObjectName);
            td.ObjectType = GetClassOfObject(ObjectName);            
            return td;
        }

        /// <summary>
        /// Get the number of rows in a data.frame or matrix
        /// </summary>
        /// <param name="TableName">Name of the table to count rows</param>
        /// <returns>Number of rows</returns>
        public int GetNumberOfRowsInTable(string TableName)
        {
            var td = GetDimensions(TableName);
            return td.Rows;
        }

        /// <summary>
        /// Get the number of row in a table. Optionally, the table can be
        /// filtered on a single column by a min value, max value, or both.
        /// </summary>
        /// <param name="TableName">Name of table to report # of rows</param>
        /// <param name="ColumnName">Name of column to optionally filter on</param>
        /// <param name="MinValue">Minimum value in column</param>
        /// <param name="MaxValue">Maximum value in column</param>
        /// <returns>Number of rows</returns>
        public int GetNumberOfRowsInTable(string TableName,
            string ColumnName, string MinValue, string MaxValue)
        {            
            var s_Filter = "";

            #region Construct the Filter
            if (!string.IsNullOrEmpty(ColumnName))
            {
                if (TableContainsColumn(TableName,
                    ColumnName))
                {
                    if (!string.IsNullOrEmpty(MinValue) &
                        !string.IsNullOrEmpty(MaxValue))
                    {
                        s_Filter = string.Format(
                            "[{0}[,'{1}'] > {2} & {0}[,'{1}'] < {3},]",
                            TableName,
                            ColumnName,
                            MinValue,
                            MaxValue);
                    }
                    else if (!string.IsNullOrEmpty(MinValue))
                    {
                        s_Filter = string.Format(
                            "[{0}[,'{1}'] > {2},]",
                            TableName,
                            ColumnName,
                            MinValue);
                    }
                    else if (!string.IsNullOrEmpty(MaxValue))
                    {
                        s_Filter = string.Format(
                            "[{0}[,'{1}'] < {2},]",
                            TableName,
                            ColumnName,
                            MaxValue);
                    }
                }                
            }
            #endregion

            var iv = engine.Evaluate(
                    string.Format("nrow({0}{1})\n",
                    TableName,
                    !string.IsNullOrEmpty(s_Filter) ? s_Filter : "")).AsInteger();

            if (iv.Length > 0)
                return iv[0];
            else
                return 0;
        }

        /// <summary>
        /// Get the number of columns in a data.frame or matrix
        /// </summary>
        /// <param name="TableName">Name of the table to count columns</param>
        /// <returns>Number of columns</returns>
        public int GetNumberOfColumnsInTable(string TableName)
        {
            var td = GetDimensions(TableName);
            return td.Columns;
        }

        /// <summary>
        /// Indicates if a table contains a specific column
        /// </summary>
        /// <param name="TableName">Name of table</param>
        /// <param name="ColumnName">Name of column</param>
        /// <returns>True, if table contains the column name</returns>
        public bool TableContainsColumn(string TableName, string ColumnName)
        {
            var b_IncludesColumn = false;

            if (ContainsObject(TableName))
            {
                var l_Columns = GetColumnNames(TableName);
                b_IncludesColumn = l_Columns.Contains(ColumnName);
            }

            return b_IncludesColumn;
        }

        /// <summary>
        /// Returns the length of a vector
        /// </summary>
        /// <param name="Vector">Vector to return length of</param>
        /// <returns>Length of Vector</returns>
        public int GetLengthOfVector(string Vector)
        {
            var s_Command = string.Format("length({0})",
                Vector);

            var iv = engine.Evaluate(s_Command).AsInteger();
            return iv[0];
        }

        /// <summary>
        /// Converts an data.frame or matrix from R into a DataTable in C#
        /// </summary>
        /// <param name="Table2Retrieve">Name of data.frame or matrix</param>
        /// <param name="IgnoreLs">If your table of interest is hidden within a list or object, set this to true</param>
        /// <returns>DataTable version of your data.frame or matrix</returns>
        public DataTable GetDataTable(string Table2Retrieve,
            bool IgnoreLs)
        {
            var dt_Return = new DataTable();

            if (IgnoreLs || ContainsObject(Table2Retrieve))
            {
                if (GetClassOfObject(Table2Retrieve).Equals("data.frame"))
                {
                    var dataset = engine.Evaluate(Table2Retrieve).AsDataFrame();

                    for (var i = 0; i < dataset.ColumnCount; i++)
                    {
                        var dc = new DataColumn(dataset.ColumnNames[i]);
                        if (dc.Namespace.Equals(""))
                        {
                            var j = i + 1;
                            dc.Namespace = j.ToString();
                        }
                        dt_Return.Columns.Add(dc);
                    }

                    for (var r = 0; r < dataset.RowCount; r++)
                    {
                        var df_Row = dataset.GetRow(r);

                        var s_Row = new object[df_Row.DataFrame.ColumnCount];
                        for (var i = 0; i < df_Row.DataFrame.ColumnCount; i++)
                        {
                            s_Row[i] = df_Row[i].ToString();
                        }
                        dt_Return.Rows.Add(s_Row);
                    }
                }
                else if (GetClassOfObject(Table2Retrieve).Equals("matrix"))
                {
                    var dataset = engine.Evaluate("data.frame(" + Table2Retrieve + ")").AsDataFrame();

                    for (var i = 0; i < dataset.ColumnCount; i++)
                    {
                        var dc = new DataColumn(dataset.ColumnNames[i]);
                        if (dc.Namespace.Equals(""))
                        {
                            var j = i + 1;
                            dc.Namespace = j.ToString();
                        }
                        dt_Return.Columns.Add(dc);
                    }

                    for (var r = 0; r < dataset.RowCount; r++)
                    {
                        var df_Row = dataset.GetRow(r);

                        var s_Row = new object[df_Row.DataFrame.ColumnCount];
                        for (var i = 0; i < df_Row.DataFrame.ColumnCount; i++)
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
        /// Converts an data.frame from R into a DataTable in C#
        /// </summary>
        /// <param name="Table2Retrieve">Name of data.frame</param>
        /// <param name="NameOfFirstColumn">Name of the Rowname column, defaults to "RowNames"</param>
        /// <returns>DataTable version of your data.frame</returns>
        public DataTable GetDataTableIncludingRowNames(
            string Table2Retrieve, string NameOfFirstColumn)
        {
            if (!ContainsObject(Table2Retrieve))
            {
                return null;
            }

            var l_Rownames = GetRowNames(Table2Retrieve);

            if (string.IsNullOrEmpty(NameOfFirstColumn))
                NameOfFirstColumn = "RowNames";

            var dt_Return = new DataTable();
            
            if (GetClassOfObject(Table2Retrieve).Equals("data.frame"))
            {
                var dataset = engine.Evaluate(Table2Retrieve).AsDataFrame();

                var dc_RowName = new DataColumn(NameOfFirstColumn);
                dt_Return.Columns.Add(dc_RowName);

                for (var i = 0; i < dataset.ColumnCount; i++)
                {
                    var dc = new DataColumn(dataset.ColumnNames[i]);
                    if (dc.Namespace.Equals(""))
                    {
                        var j = i + 1;
                        dc.Namespace = j.ToString();
                    }
                    dt_Return.Columns.Add(dc);
                }

                // iterate across the rows
                for (var r = 0; r < dataset.RowCount; r++)
                {
                    var df_Row = dataset.GetRow(r);

                    var s_Row = new object[df_Row.DataFrame.ColumnCount + 1];
                    s_Row[0] = l_Rownames[r];
                    for (var i = 0; i < df_Row.DataFrame.ColumnCount; i++)
                    {
                        s_Row[i + 1] = df_Row[i].ToString();
                    }
                    dt_Return.Rows.Add(s_Row);
                }
            }
            else if (GetClassOfObject(Table2Retrieve).Equals("matrix"))
            {
                var dataset = engine.Evaluate("data.frame(" 
                    + Table2Retrieve + ")").AsDataFrame();

                var dc_RowName = new DataColumn(NameOfFirstColumn);
                dt_Return.Columns.Add(dc_RowName);

                for (var i = 0; i < dataset.ColumnCount; i++)
                {
                    var dc = new DataColumn(dataset.ColumnNames[i]);
                    if (dc.Namespace.Equals(""))
                    {
                        var j = i + 1;
                        dc.Namespace = j.ToString();
                    }
                    dt_Return.Columns.Add(dc);
                }

                // iterate across the rows
                for (var r = 0; r < dataset.RowCount; r++)
                {
                    var df_Row = dataset.GetRow(r);

                    var s_Row = new object[df_Row.DataFrame.ColumnCount + 1];
                    s_Row[0] = l_Rownames[r];
                    for (var i = 0; i < df_Row.DataFrame.ColumnCount; i++)
                    {
                        s_Row[i + 1] = df_Row[i].ToString();
                    }
                    dt_Return.Rows.Add(s_Row);
                }
            }

            return dt_Return;
        }

        /// <summary>
        /// Converts a DataTable to a DataFrame in a given R workspace
        /// </summary>
        /// <param name="Table">The DataTable you want to convert to a data.frame</param>
        /// <param name="TableName">Name of the new data.frame</param>
        /// <returns>True, if the new data.frame is created successfully</returns>
        public bool WriteDataTableToR(DataTable Table, string TableName)
        {
            var Command = string.Format("{0} <- data.frame(",
                    TableName);

            for (var c = 0; c < Table.Columns.Count; c++)
            {
                Command += "\"" + Table.Columns[c].ColumnName + "\"=c(\"";

                for (var r = 0; r < Table.Rows.Count; r++)
                {
                    if (r < Table.Rows.Count - 1)
                    {
                        Command += Table.Rows[r][c] + "\",\"";
                    }
                    else
                    {
                        Command += Table.Rows[r][c] + "\")";
                    }
                }

                if (c < Table.Columns.Count - 1)
                {
                    Command += ",";
                }
                else
                {
                    Command += ")";
                }
            }

            try
            {
                engine.Evaluate(Command);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered in GenericRCalls " +
					"'SetDataFrame':\n" + ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the specified column to the row.names, and then deletes that column
        /// from the data.frame
        /// </summary>
        /// <param name="DataFrameName">Name of the Data Frame</param>
        /// <param name="ColumnIndex">Index of the column (first column starts at 1)</param>
        /// <returns>True, if the rownames are set successfully</returns>
        public bool SetDataFrameRowNames(
            string DataFrameName, int ColumnIndex)
        {
            var s_RStatement = string.Format("rownames({0}) <- {0}[,{1}]\n" +
                "{0} <- {0}[,-{1}]",
                DataFrameName, ColumnIndex.ToString());

            try
            {
                engine.Evaluate(s_RStatement);
            }
            catch (Exception ex)
            {
				// TODO : Handle this error
				Console.WriteLine("Error in GenericRCalls->SetDataFrameRowNames: " + ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the value of an object in the R environment
        /// </summary>
        /// <param name="ObjectName">Name of variable</param>
        /// <param name="Value">Value of variable</param>
        /// <returns>True, if the object's value is set successfully</returns>
        public bool SetObject(string ObjectName, string Value)
        {

            try
            {
                var Command = ObjectName + " <- " + Value;
                engine.Evaluate(Command);
            }
            catch (Exception ex)
            {
                // TODO : Handle this error
				Console.WriteLine("Error in GenericRCalls->SetObject: " + ex.Message);
                return false;
            }

            return true;
        }
        #endregion

        #region Random Name Generators
        /// <summary>
        /// Creates a random instance name for the R workspace. This is
        /// important if multiple instances are to be run concurrently
        /// </summary>
        /// <returns>Random instance name</returns>
        private string GetRInstanceName()
        {
            var s_RInstance = "";
            Thread.Sleep(2);
            var rnd = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            var chars = "2346789ABCDEFGHJKLMNPQRTUVWXYZabcdefghjkmnpqrtuvwxyz";

            for (var i = 0; i < 20; i++)
            {
                s_RInstance += chars.Substring(rnd.Next(chars.Length), 1);
            }

            return s_RInstance;
        }

        /// <summary>
        /// Generates a random temporary table name 
        /// </summary>
        /// <returns>random name for a temporary table</returns>
        public string GetTemporaryTableName()
        {
            return "tmpTable_" + GetRInstanceName();
        }

        /// <summary>
        /// Generates a random temporary table name 
        /// </summary>
        /// <param name="Prefix">Prefix appended to random name</param>
        /// <returns>random name for a temporary table</returns>
        public string GetTemporaryTableName(string Prefix)
        {
            return Prefix + GetRInstanceName();
        }
        #endregion
        
        #region TestCases
        /// <summary>
        /// Tests the connection with R through RdotNET (http://rdotnet.codeplex.com/)
        /// Results are written out to console
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool TestConnectionToR()
        {
            var b_Successful = true;

            // .NET Framework array to R vector.
            var group1 = engine.CreateNumericVector(new[] { 30.02, 29.99, 30.11, 29.97, 30.01, 29.99 });
            engine.SetSymbol("group1", group1);
            // Direct parsing from R script.
            var group2 = engine.Evaluate("group2 <- c(29.89, 29.93, 29.72, 29.98, 30.02, 29.98)").AsNumeric();

            // Test difference of mean and get the P-value.
            var testResult = engine.Evaluate("t.test(group1, group2)").AsList();
            var nv = testResult["p.value"].AsNumeric();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Group1: [{0}]", string.Join(", ", group1));
            Console.WriteLine("Group2: [{0}]", string.Join(", ", group2));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("P-value from t-test: ");
            if (nv.Length == 1)
            {
                Console.WriteLine(nv[0]);
            }
            Console.ForegroundColor = ConsoleColor.White;


            if (nv[0] > 0.09078 && nv[0] < 0.09076)
                b_Successful = false;
            
            return b_Successful;
        }
        #endregion
    }
}
