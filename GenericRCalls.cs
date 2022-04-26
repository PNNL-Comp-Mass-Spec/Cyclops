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
    ///
    public class GenericRCalls
    {
        // Ignore Spelling: colnames, rownames

        private REngine engine;

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
        public string RPackageLocation { get; set; } = "http://cran.fhcrc.org/";

        public GenericRCalls()
        {
        }

        public GenericRCalls(CyclopsModel CyclopsModel)
        {
            Model = CyclopsModel;
        }

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

                // When this is True, RDotNet displays the results of R method calls in the console
                // This can lead to thousands of rows of data being displayed
                engine.AutoPrint = false;

                return true;
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while instantiating R: " + ex, "Generic R Calls");
                return false;
            }
        }

        /// <summary>
        /// Main call method to run a command in the R environment
        /// </summary>
        /// <param name="rCmd">R Command to pass to the environment</param>
        /// <param name="summaryStatement">Summary of the Command being issued, e.g. Name of Module</param>
        /// <param name="step">Step number for the command</param>
        /// <returns>True, if the command is issued successfully</returns>
        public bool Run(string rCmd, string summaryStatement, int step)
        {
            var successful = true;

            try
            {
                Model.LogMessage(rCmd, summaryStatement, step);

                engine.Evaluate(rCmd);
            }
            catch (ParseException pe)
            {
                Model.LogError("ParseException encountered while running command:\n" +
                               rCmd + "\nParseException: " + pe.Message +
                               "\nInnerException: " + pe.InnerException,
                               summaryStatement, step);
                successful = false;
            }
            catch (IOException ioe)
            {
                Model.LogError("IOException encountered while running command:\n" +
                               rCmd + "\nIOException: " + ioe.Message +
                               "\nInnerException: " + ioe.InnerException,
                    summaryStatement, step);
                successful = false;
            }
            catch (AccessViolationException ave)
            {
                Model.LogError("AccessViolationException encountered while running command:\n" +
                               rCmd + "\nAccessViolationException: " + ave.Message +
                               "\nInnerException: " + ave.InnerException,
                               summaryStatement, step);
                successful = false;
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while running command:\n" +
                               rCmd + "\nException: " + ex.Message +
                               "\nInnerException: " + ex.InnerException,
                               summaryStatement, step);
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Loads an R Workspace into the environment
        /// </summary>
        /// <param name="WorkspaceFileName">Full path to the RData file</param>
        /// <param name="Module">Name of module calling LoadRWorkspace</param>
        /// <param name="Step">Step number for the command</param>
        /// <returns>True, if loaded successfully</returns>
        public bool LoadRWorkspace(string WorkspaceFileName, string Module, int Step)
        {
            var filePathForR = ConvertToRCompatiblePath(WorkspaceFileName);

            var rCmd = string.Format("load(\"{0}\")", filePathForR);

            return Run(rCmd, Module, Step);
        }

        /// <summary>
        /// Returns the version of R being run
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> Version()
        {
            var rVersionInfo = new Dictionary<string, string>();

            try
            {
                Model.LogMessage("Retrieving R Version Information...");
                var df = engine.Evaluate("version").AsDataFrame();
                foreach (var s in df.ColumnNames)
                {
                    rVersionInfo.Add(s, df[0, s].ToString());
                }
            }
            catch (Exception ex)
            {
                Model.LogError("Error retrieving R version():\n" +
                    ex.Message + "\nInnerException: " +
                    ex.InnerException);
                return null;
            }
            return rVersionInfo;
        }

        /// <summary>
        /// Returns all the objects in the R workspace
        /// </summary>
        /// <returns>List of all the objects in the R workspace</returns>
        public List<string> GetObjectNames()
        {
            var objectList = new List<string>();

            var evalResult = engine.Evaluate("ls()").AsCharacter();
            foreach (var s in evalResult)
            {
                objectList.Add(s);
            }

            return objectList;
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
                var objectList = GetObjectNames();
                return objectList.Contains(ObjectName);
            }

            var nameParts = ObjectName.Split('$');
            if (nameParts.Length == 2)
            {
                var objectList = GetObjectNames();
                return objectList.Contains(nameParts[0]);
            }

            return false;
        }

        /// <summary>
        /// Removes an object from the R Workspace
        /// </summary>
        /// <param name="Object2Remove">Name of object you would like to remove</param>
        /// <returns>True, if successfully removed</returns>
        public bool RemoveObject(string Object2Remove)
        {
            var successful = true;

            if (!string.IsNullOrEmpty(Object2Remove))
            {
                try
                {
                    engine.Evaluate(string.Format("rm({0})\n", Object2Remove));
                }
                catch (Exception ex)
                {
                    Model.LogError("Exception caught while trying to remove an object:\n" +
                        "Object to remove: " + Object2Remove + "\n" +
                        "Exception: " + ex.Message + "\n" +
                        "InnerException: " + ex.InnerException);
                    successful = false;
                }
            }

            return successful;
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
                var rCmd = string.Format("class({0})", ObjectName);
                var evalResult = engine.Evaluate(rCmd).AsCharacter();
                return evalResult[0];
            }

            return null;
        }

        /// <summary>
        /// Replace any backslash characters in the file path with forward slash characters
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <returns></returns>
        public static string ConvertToRCompatiblePath(string inputFilePath)
        {
            return inputFilePath.Replace('\\', '/');
        }

        /// <summary>
        /// Return the current working directory
        /// </summary>
        /// <returns>Current working directory</returns>
        public string GetWorkingDirectory()
        {
            const string rCmd = "getwd()";
            var evalResult = engine.Evaluate(rCmd).AsCharacter();
            return evalResult[0];
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
                return GetClassOfObject(ObjectName).Equals(Class);
            }

            return false;
        }

        /// <summary>
        /// Evaluates a logical statement in R, and returns the result as a boolean
        /// </summary>
        /// <param name="RStatement">Statement to evaluate to return TRUE or FALSE</param>
        /// <returns>TRUE or FALSE</returns>
        public bool AssessBoolean(string RStatement)
        {
            var evalResult = engine.Evaluate(RStatement).AsCharacter();
            return evalResult[0].Equals("TRUE", StringComparison.OrdinalIgnoreCase);
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
            var rCmd = string.Format("jnbIsPackageInstalled('{0}')", Package);
            var evalResult = engine.Evaluate(rCmd).AsCharacter();
            return evalResult[0].Equals("TRUE", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Quick way to save your work environment
        /// </summary>
        /// <param name="filePath">Name of the file you'd like to save the environment as</param>
        /// <returns>True, if the R environment is saved successfully</returns>
        public bool SaveEnvironment(string filePath)
        {
            var successful = true;

            var rCmd = string.Format("save.image(file=\"{0}\")", ConvertToRCompatiblePath(filePath));

            try
            {
                engine.Evaluate(rCmd);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while Saving R environment:\n" + ex.Message);
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Gets the row names for a data.frame or matrix in the R workspace
        /// </summary>
        /// <param name="ObjectName">Name of the data.frame or matrix</param>
        /// <returns>List of the row names</returns>
        public List<string> GetRowNames(string ObjectName)
        {
            var rowNames = new List<string>();

            if (GetClassOfObject(ObjectName).Equals("data.frame") ||
                GetClassOfObject(ObjectName).Equals("matrix"))
            {
                var matrixRows = engine.Evaluate(string.Format("rownames({0})", ObjectName)).AsCharacter();

                rowNames.AddRange(matrixRows);
            }

            return rowNames;
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
            var columnList = new List<string>();

            if (GetClassOfObject(ObjectName).Equals("data.frame") ||
                GetClassOfObject(ObjectName).Equals("matrix"))
            {
                var matrixColumns = engine.Evaluate(string.Format("colnames({0})", ObjectName)).AsCharacter();

                foreach (var column in matrixColumns)
                {
                    if (Unique)
                    {
                        if (!columnList.Contains(column))
                        {
                            columnList.Add(column);
                        }
                    }
                    else
                    {
                        columnList.Add(column);
                    }
                }
            }

            return columnList;
        }

        /// <summary>
        /// Gets a list of strings from R
        /// </summary>
        /// <param name="Vector">Vector to retrieve</param>
        /// <returns>List of strings in the returned Vector</returns>
        public List<string> GetCharacterVector(string Vector)
        {
            var stringList = new List<string>();
            var evalResult = engine.Evaluate(Vector).AsCharacter();

            foreach (var s in evalResult)
            {
                stringList.Add(s);
            }

            return stringList;
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
                    var dim = engine.Evaluate(string.Format("dim({0})\n", ObjectName)).AsInteger();
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
        /// <param name="tableName">Name of table to report # of rows</param>
        /// <param name="columnName">Name of column to optionally filter on</param>
        /// <param name="minValue">Minimum value in column</param>
        /// <param name="maxValue">Maximum value in column</param>
        /// <returns>Number of rows</returns>
        public int GetNumberOfRowsInTable(string tableName,
            string columnName, string minValue, string maxValue)
        {
            var dataFilter = "";

            if (!string.IsNullOrEmpty(columnName) && TableContainsColumn(tableName, columnName))
            {
                if (!string.IsNullOrEmpty(minValue) &&
                    !string.IsNullOrEmpty(maxValue))
                {
                    dataFilter = string.Format(
                        "[{0}[,'{1}'] > {2} & {0}[,'{1}'] < {3},]",
                        tableName,
                        columnName,
                        minValue,
                        maxValue);
                }
                else if (!string.IsNullOrEmpty(minValue))
                {
                    dataFilter = string.Format(
                        "[{0}[,'{1}'] > {2},]",
                        tableName,
                        columnName,
                        minValue);
                }
                else if (!string.IsNullOrEmpty(maxValue))
                {
                    dataFilter = string.Format(
                        "[{0}[,'{1}'] < {2},]",
                        tableName,
                        columnName,
                        maxValue);
                }
            }

            var evalResult = engine.Evaluate(
                    string.Format("nrow({0}{1})\n",
                    tableName,
                    !string.IsNullOrEmpty(dataFilter) ? dataFilter : "")).AsInteger();

            if (evalResult.Length > 0)
            {
                return evalResult[0];
            }

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
            var includesColumn = false;

            if (ContainsObject(TableName))
            {
                var colNames = GetColumnNames(TableName);
                includesColumn = colNames.Contains(ColumnName);
            }

            return includesColumn;
        }

        /// <summary>
        /// Returns the length of a vector
        /// </summary>
        /// <param name="Vector">Vector to return length of</param>
        /// <returns>Length of Vector</returns>
        public int GetLengthOfVector(string Vector)
        {
            var rCmd = string.Format("length({0})", Vector);

            var evalResult = engine.Evaluate(rCmd).AsInteger();
            return evalResult[0];
        }

        /// <summary>
        /// Converts an data.frame or matrix from R into a DataTable in C#
        /// </summary>
        /// <param name="Table2Retrieve">Name of data.frame or matrix</param>
        /// <param name="IgnoreLs">If your table of interest is hidden within a list or object, set this to true</param>
        /// <returns>DataTable version of your data.frame or matrix</returns>
        public DataTable GetDataTable(string Table2Retrieve, bool IgnoreLs)
        {
            var outTable = new DataTable();

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
                        outTable.Columns.Add(dc);
                    }

                    for (var r = 0; r < dataset.RowCount; r++)
                    {
                        var dataFrameRow = dataset.GetRow(r);

                        var newRow = new object[dataFrameRow.DataFrame.ColumnCount];
                        for (var i = 0; i < dataFrameRow.DataFrame.ColumnCount; i++)
                        {
                            newRow[i] = dataFrameRow[i].ToString();
                        }
                        outTable.Rows.Add(newRow);
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
                        outTable.Columns.Add(dc);
                    }

                    for (var r = 0; r < dataset.RowCount; r++)
                    {
                        var dataFrameRow = dataset.GetRow(r);

                        var newRow = new object[dataFrameRow.DataFrame.ColumnCount];
                        for (var i = 0; i < dataFrameRow.DataFrame.ColumnCount; i++)
                        {
                            newRow[i] = dataFrameRow[i].ToString();
                        }
                        outTable.Rows.Add(newRow);
                    }
                }
            }

            return outTable;
        }

        /// <summary>
        /// Converts an data.frame from R into a DataTable in C#
        /// </summary>
        /// <param name="Table2Retrieve">Name of data.frame</param>
        /// <param name="NameOfFirstColumn">Name of the row name column, defaults to "RowNames"</param>
        /// <returns>DataTable version of your data.frame</returns>
        public DataTable GetDataTableIncludingRowNames(
            string Table2Retrieve, string NameOfFirstColumn)
        {
            if (!ContainsObject(Table2Retrieve))
            {
                return null;
            }

            var rowNames = GetRowNames(Table2Retrieve);

            if (string.IsNullOrEmpty(NameOfFirstColumn))
            {
                NameOfFirstColumn = "RowNames";
            }

            var outTable = new DataTable();

            if (GetClassOfObject(Table2Retrieve).Equals("data.frame"))
            {
                var dataset = engine.Evaluate(Table2Retrieve).AsDataFrame();

                var keyColumn = new DataColumn(NameOfFirstColumn);
                outTable.Columns.Add(keyColumn);

                for (var i = 0; i < dataset.ColumnCount; i++)
                {
                    var newColumn = new DataColumn(dataset.ColumnNames[i]);
                    if (newColumn.Namespace.Equals(""))
                    {
                        var j = i + 1;
                        newColumn.Namespace = j.ToString();
                    }
                    outTable.Columns.Add(newColumn);
                }

                // iterate across the rows
                for (var r = 0; r < dataset.RowCount; r++)
                {
                    var dataFrameFow = dataset.GetRow(r);

                    var newRow = new object[dataFrameFow.DataFrame.ColumnCount + 1];
                    newRow[0] = rowNames[r];
                    for (var i = 0; i < dataFrameFow.DataFrame.ColumnCount; i++)
                    {
                        newRow[i + 1] = dataFrameFow[i].ToString();
                    }
                    outTable.Rows.Add(newRow);
                }
            }
            else if (GetClassOfObject(Table2Retrieve).Equals("matrix"))
            {
                var dataset = engine.Evaluate("data.frame("
                    + Table2Retrieve + ")").AsDataFrame();

                var keyColumn = new DataColumn(NameOfFirstColumn);
                outTable.Columns.Add(keyColumn);

                for (var i = 0; i < dataset.ColumnCount; i++)
                {
                    var newColumn = new DataColumn(dataset.ColumnNames[i]);
                    if (newColumn.Namespace.Equals(""))
                    {
                        var j = i + 1;
                        newColumn.Namespace = j.ToString();
                    }
                    outTable.Columns.Add(newColumn);
                }

                // iterate across the rows
                for (var r = 0; r < dataset.RowCount; r++)
                {
                    var dataFrameRow = dataset.GetRow(r);

                    var newRow = new object[dataFrameRow.DataFrame.ColumnCount + 1];
                    newRow[0] = rowNames[r];
                    for (var i = 0; i < dataFrameRow.DataFrame.ColumnCount; i++)
                    {
                        newRow[i + 1] = dataFrameRow[i].ToString();
                    }
                    outTable.Rows.Add(newRow);
                }
            }

            return outTable;
        }

        /// <summary>
        /// Converts a DataTable to a DataFrame in a given R workspace
        /// </summary>
        /// <param name="Table">The DataTable you want to convert to a data.frame</param>
        /// <param name="TableName">Name of the new data.frame</param>
        /// <returns>True, if the new data.frame is created successfully</returns>
        public bool WriteDataTableToR(DataTable Table, string TableName)
        {
            var rCmd = string.Format("{0} <- data.frame(", TableName);

            for (var c = 0; c < Table.Columns.Count; c++)
            {
                rCmd += "\"" + Table.Columns[c].ColumnName + "\"=c(\"";

                for (var r = 0; r < Table.Rows.Count; r++)
                {
                    if (r < Table.Rows.Count - 1)
                    {
                        rCmd += Table.Rows[r][c] + "\",\"";
                    }
                    else
                    {
                        rCmd += Table.Rows[r][c] + "\")";
                    }
                }

                if (c < Table.Columns.Count - 1)
                {
                    rCmd += ",";
                }
                else
                {
                    rCmd += ")";
                }
            }

            try
            {
                engine.Evaluate(rCmd);
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
        /// <returns>True, if the row names are set successfully</returns>
        public bool SetDataFrameRowNames(
            string DataFrameName, int ColumnIndex)
        {
            var rCmd = string.Format("rownames({0}) <- {0}[,{1}]\n" +
                "{0} <- {0}[,-{1}]",
                DataFrameName, ColumnIndex.ToString());

            try
            {
                engine.Evaluate(rCmd);
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
                var rCmd = ObjectName + " <- " + Value;
                engine.Evaluate(rCmd);
            }
            catch (Exception ex)
            {
                // TODO : Handle this error
                Console.WriteLine("Error in GenericRCalls->SetObject: " + ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a random instance name for the R workspace. This is
        /// important if multiple instances are to be run concurrently
        /// </summary>
        /// <returns>Random instance name</returns>
        private string GetRInstanceName()
        {
            var rInstance = "";
            Thread.Sleep(2);
            var rnd = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            const string chars = "2346789ABCDEFGHJKLMNPQRTUVWXYZabcdefghjkmnpqrtuvwxyz";

            for (var i = 0; i < 20; i++)
            {
                rInstance += chars.Substring(rnd.Next(chars.Length), 1);
            }

            return rInstance;
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

        /// <summary>
        /// Tests the connection with R through RdotNET (http://rdotnet.codeplex.com/)
        /// Results are written out to console
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool TestConnectionToR()
        {
            var successful = true;

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
            {
                successful = false;
            }

            return successful;
        }
    }
}
