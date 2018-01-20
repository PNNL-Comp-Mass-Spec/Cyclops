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
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Xml;

namespace Cyclops
{
    public enum WorkflowType { XML, SQLite };
    /// <summary>
    /// This class is responsible for reading and writing workflows, from
    /// XML and SQLite
    /// </summary>
    public class WorkflowHandler : INotifyPropertyChanged
    {
        #region Members

        private bool m_WorkflowContainsOperations;

        private struct strModuleInfo
        {
            public string ModuleName { get; set; }
        }

        private DataTable m_ModulesTable = new DataTable("T_Modules");

        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Properties

        /// <summary>
        /// Type of workflow to read in
        /// </summary>
        public WorkflowType InputWorkflowType { get; set; }

        /// <summary>
        /// Type of workflow to write out
        /// </summary>
        public WorkflowType OutputWorkflowType { get; set; }

        /// <summary>
        /// Instance of the CyclopsModel controller
        /// </summary>
        public CyclopsModel Model { get; set; }
        /// <summary>
        /// File name for the input workflow
        /// </summary>
        public string InputWorkflowFileName { get; set; }
        /// <summary>
        /// File name for the output workflow
        /// </summary>
        public string OutputWorkflowFileName { get; set; }

        /// <summary>
        /// The number of modules in the workflow
        /// </summary>
        public int Count => Modules.Count;

        /// <summary>
        /// Name of the table in the database to run
        /// </summary>
        public string WorkflowTableName { get; set; } = "T_Workflow";

        /// <summary>
        /// SQLite database handled by Cyclops
        /// </summary>
        public SQLiteHandler SQLiteDatabase { get; set; } = new SQLiteHandler();

        public LinkedList<DataModules.BaseDataModule> Modules { get; set; } = new LinkedList<DataModules.BaseDataModule>();

        public DataTable WorkflowTable
        {
            get => m_ModulesTable;
            set
            {
                m_ModulesTable = value;
                OnPropertyChanged("WorkflowTable");
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        ///
        /// </summary>
        /// <param name="TheModel"></param>
        public WorkflowHandler(CyclopsModel TheModel)
        {
            Model = TheModel;

            AddDataColumnsToWorkflowTables();
        }
        #endregion

        #region Methods

        private void AddDataColumnsToWorkflowTables()
        {
            var moduleTableColumnHeaders =
                new Dictionary<string, string>
                {
                    {"Step", "System.Int32"},
                    {"Module", "System.String"},
                    {"Parameter", "System.String"},
                    {"Value", "System.String"}
                };

            foreach (var module in moduleTableColumnHeaders)
            {
                var dc = new DataColumn(module.Key)
                {
                    DataType = Type.GetType(module.Value)
                };
                m_ModulesTable.Columns.Add(dc);
            }
        }

        /// <summary>
        /// Reads a workflow and assembles the modules
        /// </summary>
        /// <returns>True, if the workflow is read successfully</returns>
        public bool ReadWorkflow()
        {
            var successful = true;

            if (string.IsNullOrEmpty(InputWorkflowFileName))
            {
                Model.LogError("A workflow file must be supplied in order to read modules!",
                               "WorkflowHandler: ReadWorkflow()", 0);
                return false;
            }

            if (!File.Exists(InputWorkflowFileName) && !File.Exists(Path.Combine(Model.WorkDirectory, InputWorkflowFileName)))
            {
                Model.LogError("Cyclops cannot find the specified workflow file: " + InputWorkflowFileName,
                               "WorkflowHandler: ReadWorkflow()", 0);
                return false;
            }

            if (Path.GetExtension(InputWorkflowFileName).ToLower().Equals(".xml"))
                InputWorkflowType = WorkflowType.XML;
            else if (Path.GetExtension(InputWorkflowFileName).ToLower().Equals(".db") ||
                Path.GetExtension(InputWorkflowFileName).ToLower().Equals(".db3"))
                InputWorkflowType = WorkflowType.SQLite;

            // Read modules from file
            switch (InputWorkflowType)
            {
                case WorkflowType.XML:
                    successful = ReadXMLWorkflow();
                    break;
                case WorkflowType.SQLite:
                    successful = ReadSQLiteWorkflow();
                    break;
            }

            // Operations do not add to the overall Count for the primary Model
            // So, exclude instances that are running operations.
            if (Count == 0 && !m_WorkflowContainsOperations)
            {
                Model.LogError(string.Format("No modules were assembled from the workflow:\n" +
                    "Please check that the settings " +
                    "below are correct:\nInput File: {0}\nType: {1}",
                    InputWorkflowFileName,
                    InputWorkflowType));
                return false;
            }

            return successful;
        }

        /// <summary>
        /// Reads a workflow and assembles the modules
        /// </summary>
        /// <param name="TheWorkflowFileName">Name of the workflow file</param>
        /// <returns>True, if the workflow is read successfully</returns>
        public bool ReadWorkflow(string TheWorkflowFileName)
        {
            InputWorkflowFileName = TheWorkflowFileName;
            return ReadWorkflow();
        }

        public bool ReadWorkflow(string TheWorkflowFileName, WorkflowType Type)
        {
            InputWorkflowFileName = TheWorkflowFileName;
            InputWorkflowType = Type;
            return ReadWorkflow();
        }

        public bool ReadWorkflow(string TheWorkflowFileName, string TableName, WorkflowType Type)
        {
            InputWorkflowFileName = TheWorkflowFileName;
            WorkflowTableName = TableName;
            InputWorkflowType = Type;
            return ReadWorkflow();
        }

        /// <summary>
        /// Reads an XML workflow and assembles modules from the file
        /// </summary>
        /// <returns>True, if the modules are assembled correctly</returns>
        private bool ReadXMLWorkflow()
        {
            var successful = true;
            var InputWorkflowFilePath = "";

            try
            {
                if (File.Exists(InputWorkflowFileName))
                    InputWorkflowFilePath = InputWorkflowFileName;
                else if (Directory.Exists(Model.WorkDirectory) &&
                    File.Exists(Path.Combine(Model.WorkDirectory, InputWorkflowFileName)))
                    InputWorkflowFilePath = Path.Combine(Model.WorkDirectory, InputWorkflowFileName);
                else
                {
                    Model.LogError("Cyclops could not find the XML workflow file: \n" +
                        "Working directory: " + Model.WorkDirectory + "\n" +
                        "XML Workflow File: " + InputWorkflowFileName);
                    return false;
                }

                var xml = new XmlDocument();
                xml.Load(InputWorkflowFilePath);
                var moduleNodes = xml.SelectNodes("Cyclops/Module");

                // Clear the LinkedList
                Modules.Clear();

                if (moduleNodes != null)
                {
                    foreach (XmlNode xn in moduleNodes)
                    {
                        if (xn.Attributes != null)
                        {
                            switch (xn.Attributes["Type"].Value.ToUpper())
                            {
                                case "DATA":
                                    var DataParam = GetXMLParameters(xn);
                                    var dataModule = DataModules.BaseDataModule.Create(
                                        xn.Attributes["Name"].Value,
                                        Model, DataParam);

                                    dataModule.StepNumber = Convert.ToInt32(
                                        xn.Attributes["Step"].Value);
                                    dataModule = AddParameters(dataModule);

                                    // Only add the module if that particular step
                                    // is available
                                    if (!HasStep(dataModule.StepNumber))
                                        Modules.AddLast(dataModule);
                                    else
                                    {
                                        Model.LogError("Error occurred while trying to " +
                                                       "add Step: " + dataModule.StepNumber + ", Module: " +
                                                       dataModule.ModuleName + ". This Step is already " +
                                                       "taken in the workflow. Please check your " +
                                                       "workflow so the same step number is not " +
                                                       "used twice!");

                                        return false;
                                    }

                                    AddModulesToDataTables(dataModule);
                                    break;

                                case "OPERATION":
                                    var OperationParam = GetXMLParameters(xn);

                                    var operationsModule = Operations.BaseOperationModule.Create(
                                        xn.Attributes["Name"].Value,
                                        Model, OperationParam);

                                    if (!string.IsNullOrEmpty(
                                        Model.OperationsDatabasePath))
                                    {
                                        operationsModule.OperationsDatabasePath =
                                            Model.OperationsDatabasePath;
                                    }
                                    successful = operationsModule.PerformOperation();
                                    m_WorkflowContainsOperations = true;

                                    break;
                            }
                        }

                        if (!successful)
                            break;
                    }
                }
            }
            catch (IOException ioe)
            {
                Model.LogError("IOException thrown while reading XML Workflow file: \n" +
                    InputWorkflowFilePath + "\nIOException: " + ioe);
                successful = false;
            }
            catch (Exception ex)
            {
                Model.LogError("Exception thrown while reading XML Workflow file: \n" +
                    InputWorkflowFilePath + "\nException: " + ex);
                successful = false;
            }

            return successful;
        }

        private void AddModulesToDataTables(BaseModule module)
        {
            foreach (var param in module.Parameters)
            {
                var moduleRow = m_ModulesTable.NewRow();
                moduleRow["Step"] = module.StepNumber;
                moduleRow["Module"] = module.ModuleName;
                moduleRow["Parameter"] = param.Key;
                moduleRow["Value"] = param.Value;

                m_ModulesTable.Rows.Add(moduleRow);
            }
        }

        /// <summary>
        /// Runs the Module Workflow
        /// </summary>
        /// <returns>True, if the workflow completes successfully</returns>
        public bool RunWorkflow()
        {
            var successful = true;

            var step = 1;
            foreach (var module in Modules)
            {
                if (module != null)
                    successful = module.PerformOperation();
                else
                    Model.LogError("Cyclops did not detect a module at step number: " + step);

                if (!successful)
                    return false;

                step++;
            }

            return true;
        }

        private DataModules.BaseDataModule AddParameters(
            DataModules.BaseDataModule Module)
        {
            foreach (var kvp in Model.CyclopsParameters)
            {
                Module.Parameters.Add(kvp.Key, kvp.Value);
            }

            return Module;
        }

        private Dictionary<string, string> GetXMLParameters(XmlNode Node)
        {
            var paramDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var parameters = Node.SelectNodes("Parameter");

            if (parameters != null)
            {
                foreach (XmlNode paramNode in parameters)
                {
                    if (paramNode.Attributes != null)
                    {
                        paramDictionary.Add(paramNode.Attributes["key"].Value,
                                            paramNode.Attributes["value"].Value);
                    }
                }
            }

            return paramDictionary;
        }

        public bool ReadSQLiteWorkflow()
        {
            bool successful;
            try
            {
                SQLiteDatabase.DatabaseFileName = InputWorkflowFileName;
                var workflow = SQLiteDatabase.GetTable(WorkflowTableName);

                successful = ReadDataTableWorkflow(workflow);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while reading SQLite workflow:\n" + ex);
                successful = false;
            }

            return successful;
        }

        public bool ReadDataTableWorkflow(DataTable workflowTableName)
        {

            var MaxSteps = GetMaximumStepsInWorkflowDataTable(workflowTableName);
            if (MaxSteps == null)
                return false;

            // Clear the LinkedList
            Modules.Clear();

            for (var i = 0; i < MaxSteps; i++)
            {
                var r = i + 1;
                var rows = workflowTableName.Select(
                    string.Format("Step = {0}", r));
                var Param = GetParametersFromDataRows(rows, r);
                var mi = GetModuleNameFromRows(rows, r);

                var bdm = DataModules.BaseDataModule.Create(
                    mi.ModuleName, Model, Param);

                if (bdm != null)
                {
                    bdm.StepNumber = r;

                    bdm = AddParameters(bdm);

                    // Only add the module if that particular step
                    // is available
                    if (!HasStep(bdm.StepNumber))
                        Modules.AddLast(bdm);
                    else
                    {
                        Model.LogError("Error occurred while trying to " +
                                       "add Step: " + bdm.StepNumber + ", Module: " +
                                       bdm.ModuleName + ". This Step is already " +
                                       "taken in the workflow. Please check your " +
                                       "workflow so the same step number is not " +
                                       "used twice!");

                        return false;
                    }

                    AddModulesToDataTables(bdm);
                }
                else
                {
                    Model.LogError("Error occurred while assembling modules:\n" +
                                   mi.ModuleName + " module does not exist! Please check the " +
                                   "version of Cyclops and reassemble your workflow");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the Maximum number of steps in a DataTable workflow. If any steps are
        /// missing, the function returns null.
        /// </summary>
        /// <param name="Table">Workflow Table</param>
        /// <returns>Max Steps, or null if intermediate steps are missing</returns>
        private int? GetMaximumStepsInWorkflowDataTable(DataTable Table)
        {
            var maxStepNumber = 0;

            var stepList = new HashSet<int>();

            foreach (DataRow dr in Table.Rows)
            {
                var step = dr["Step"].ToString();
                if (!string.IsNullOrEmpty(step))
                {
                    var i = Convert.ToInt32(step);
                    if (!stepList.Contains(i))
                        stepList.Add(i);
                    if (i > maxStepNumber)
                        maxStepNumber = i;
                }
            }

            for (var i = 0; i < maxStepNumber; i++)
            {
                var j = i + 1;
                if (!stepList.Contains(j))
                {
                    Model.LogError(string.Format("ERROR: While reading workflow from " +
                        "SQLite database, {0}, Step number {1} was missing from " +
                        "{2} table.",
                        InputWorkflowFileName,
                        j,
                        WorkflowTableName));
                    return null;
                }
            }

            return maxStepNumber;
        }

        private Dictionary<string, string> GetParametersFromDataRows(IEnumerable<DataRow> Rows, int stepNumber)
        {
            var Param = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var dr in Rows)
            {
                var keyName = dr["Parameter"].ToString();
                if (Param.ContainsKey(keyName))
                    throw new DuplicateNameException("Parameter " + keyName + " is specified twice for step " + stepNumber);

                Param.Add(keyName, dr["Value"].ToString());
            }

            return Param;
        }

        private strModuleInfo GetModuleNameFromRows(IReadOnlyList<DataRow> rows, int step)
        {
            var mi = new strModuleInfo();
            if (rows.Count > 0)
            {
                mi.ModuleName = rows[0]["Module"].ToString();
            }
            // Now check that the other rows have the same values
            foreach (var dr in rows)
            {
                if (!mi.ModuleName.Equals(dr["Module"].ToString()))
                {
                    Model.LogWarning("Warning reading workflow info from SQLite:\n" +
                        "Step " + step + " has multiple Modules");
                }
            }
            return mi;
        }

        /// <summary>
        /// Writes the modules out to a workflow
        /// </summary>
        /// <returns>True, if the workflow modules are written out successfully</returns>
        public bool WriteWorkflow()
        {
            var successful = true;

            if (Count > 0)
            {
                if (string.IsNullOrEmpty(OutputWorkflowFileName))
                {
                    Model.LogError("Please supply a file to write workflow to!");
                    return false;
                }

                if (Path.GetExtension(OutputWorkflowFileName).ToLower().Equals(".xml"))
                    OutputWorkflowType = WorkflowType.XML;
                else if (Path.GetExtension(OutputWorkflowFileName).ToLower().Equals(".db") ||
                    Path.GetExtension(OutputWorkflowFileName).ToLower().Equals(".db3"))
                    OutputWorkflowType = WorkflowType.SQLite;

                // Write modules out to file
                switch (OutputWorkflowType)
                {
                    case WorkflowType.XML:
                        successful = WriteModulesOutAsXML();
                        break;
                    case WorkflowType.SQLite:
                        successful = WriteModulesOutToSQLite();
                        break;
                }
            }
            else
            {
                Model.LogError("No modules to write out!");
                return false;
            }

            return successful;
        }


        /// <summary>
        /// Writes the modules out to a workflow
        /// </summary>
        /// <param name="TheOutputWorkflowFileName">File to write modules out to</param>
        /// <returns>True, if the workflow modules are written out successfully</returns>
        public bool WriteWorkflow(string TheOutputWorkflowFileName)
        {
            OutputWorkflowFileName = TheOutputWorkflowFileName;
            return WriteWorkflow();
        }

        /// <summary>
        /// Writes the modules out to a workflow
        /// </summary>
        /// <param name="TheOutputWorkflowFileName">File to write modules out to</param>
        /// <param name="Type">Type of file to write modules to</param>
        /// <returns>True, if the workflow modules are written out successfully</returns>
        public bool WriteWorkflow(string TheOutputWorkflowFileName, WorkflowType Type)
        {
            OutputWorkflowFileName = TheOutputWorkflowFileName;
            OutputWorkflowType = Type;
            return WriteWorkflow();
        }

        /// <summary>
        /// Writes the Modules in Cyclops out to XML
        /// </summary>
        /// <returns>True, if the XML file is written correctly</returns>
        private bool WriteModulesOutAsXML()
        {
            var successful = true;

            try
            {
                var Settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "   ",
                    NewLineOnAttributes = false,
                    NewLineChars = "\n"
                };

                using (var writer = XmlWriter.Create(OutputWorkflowFileName, Settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Cyclops");

                    foreach (var bdm in Modules)
                    {
                        bdm.WriteModuleToXML(writer);
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            catch (InvalidOperationException ioe)
            {
                Model.LogError("InvalidOperationException thrown while writing XML Workflow:\n" + ioe);
                successful = false;
            }
            catch (IOException ie)
            {
                Model.LogError("IOException thrown while writing XML Workflow:\n" + ie);
                successful = false;
            }
            catch (Exception ex)
            {
                Model.LogError("Exception thrown while writing XML Workflow:\n" + ex);
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Writes the workflow out to a table in a SQLite database
        /// The Database is the OutputWorkflowFileName, and the
        /// Table is named by the m_SQLiteWorkflowTableName variable
        /// </summary>
        /// <returns>True, if the table is exported successfully</returns>
        private bool WriteModulesOutToSQLite()
        {
            var successful = true;
            try
            {
                var columnSpecs = new Dictionary<string, string>
                {
                    {"Step", "System.Int32"},
                    {"Module", "System.String"},
                    {"Parameter", "System.String"},
                    {"Value", "System.String"}
                };

                var workflow = new DataTable(WorkflowTableName);
                foreach (var columnSpec in columnSpecs)
                {
                    var newColumn = new DataColumn(columnSpec.Key)
                    {
                        DataType = Type.GetType(columnSpec.Value)
                    };
                    workflow.Columns.Add(newColumn);
                }

                foreach (var module in Modules)
                {
                    module.WriteModuleToDataTable(workflow);
                }

                SQLiteDatabase.DatabaseFileName = OutputWorkflowFileName;
                SQLiteDatabase.CreateDatabase(OutputWorkflowFileName, false);
                SQLiteDatabase.WriteDataTableToDatabase(workflow);
            }
            catch (Exception ex)
            {
                Model.LogError("Error writing workflow out to SQLite database:\n" + ex);
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Get the module at a particular step
        /// </summary>
        /// <param name="StepNumber">Step to retrieve module for</param>
        /// <returns>Module at the indicated step number</returns>
        public DataModules.BaseDataModule GetModule(int StepNumber)
        {
            foreach (var m in Modules)
            {
                if (m.StepNumber == StepNumber)
                    return m;
            }

            // if Step number is not found, return null
            return null;
        }

        /// <summary>
        /// Indicates if a particular step number is present in the Modules
        /// </summary>
        /// <param name="StepNumber">Step number to query for</param>
        /// <returns>True, if the step number is present</returns>
        public bool HasStep(int StepNumber)
        {

            foreach (var m in Modules)
            {
                if (m.StepNumber == StepNumber)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get the Maximum step number for the modules
        /// </summary>
        /// <returns>Maximum Step Number</returns>
        public int GetMaxStep()
        {
            var maxStep = 0;

            foreach (var module in Modules)
            {
                if (module.StepNumber > maxStep)
                    maxStep = module.StepNumber;
            }

            return maxStep;
        }

        /// <summary>
        /// Removes a module from the List of Modules
        /// </summary>
        /// <param name="StepNumber">Step to remove module</param>
        public void RemoveModuleFromWorkflow(int StepNumber)
        {
            var node = Modules.First;
            var removed = false;
            while (node != null)
            {
                var NextNode = node.Next;
                if (node.Value.StepNumber == StepNumber)
                {
                    Modules.Remove(node);
                    removed = true;
                }
                else if (removed)
                    node.Value.StepNumber--;
                node = NextNode;
            }

        }

        public bool ContainsStep(int StepNumber)
        {
            foreach (var bdm in Modules)
            {
                if (bdm.StepNumber == StepNumber)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddNewModuleToWorkflow(
            DataModules.BaseDataModule NewModule)
        {
            if (NewModule.StepNumber > Modules.Count)
            {
                Modules.AddLast(NewModule);
                return;
            }

            var Module2Displace = GetModule(NewModule.StepNumber);
            var NNext = Modules.Find(Module2Displace);
            if (NNext != null)
            {
                Modules.AddBefore(NNext, Module2Displace);

                // increment new StepNumbers
                for (var Step = NewModule.StepNumber + 1;
                    Step < Modules.Count - 1; Step++)
                {
                    var Module2Modify = GetModule(Step);
                    var NextModule = GetModule(Step + 1);
                    NNext = Modules.Find(NextModule);
                    Modules.Remove(Module2Modify);
                    Module2Modify.StepNumber++;
                    if (NNext != null)
                    {
                        Modules.AddBefore(NNext, Module2Modify);
                    }
                }
            }

            // Modify the step number of the last module
            var LastModule = GetModule(Modules.Count);
            Modules.Remove(LastModule);
            LastModule.StepNumber++;
            Modules.AddLast(LastModule);
        }

        public void MoveStepUp(int StepNumber)
        {
            // Do not run if Step does not exist, or if already
            // the first step
            if (!ContainsStep(StepNumber) | StepNumber == 1)
                return;

            var Module2Move = GetModule(StepNumber);
            Modules.Remove(Module2Move);
            Module2Move.StepNumber--;
            var PreviousNode = GetModule(StepNumber - 1);
            var PNode = Modules.Find(PreviousNode);
            if (PNode != null)
            {
                Modules.AddBefore(PNode, Module2Move);
                Modules.Remove(PreviousNode);
                PreviousNode.StepNumber = PreviousNode.StepNumber + 1;
            }
            PNode = Modules.Find(Module2Move);
            if (PNode != null)
            {
                Modules.AddAfter(PNode, PreviousNode);
            }
        }

        public void MoveStepBack(int StepNumber)
        {
            // Do not run if Step does not exist, or if already
            // the first step
            if (!ContainsStep(StepNumber) | StepNumber == Modules.Count)
                return;

            var Module2Move = GetModule(StepNumber);
            Modules.Remove(Module2Move);
            Module2Move.StepNumber++;
            var NextNode = GetModule(StepNumber + 1);
            var PNode = Modules.Find(NextNode);
            if (PNode != null)
            {
                Modules.AddAfter(PNode, Module2Move);
                Modules.Remove(NextNode);
                NextNode.StepNumber = NextNode.StepNumber - 1;
            }
            PNode = Modules.Find(Module2Move);
            if (PNode != null)
            {
                Modules.AddBefore(PNode, NextNode);
            }
        }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
