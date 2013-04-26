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
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
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
        private LinkedList<DataModules.BaseDataModule> m_Modules =
            new LinkedList<DataModules.BaseDataModule>();         
        private SQLiteHandler sql = new SQLiteHandler();
        private int m_ModuleCount = 0;
        private WorkflowType m_InputType, 
            m_OutputType;

        private string m_SQLiteWorkflowTableName = "T_Workflow";

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
        public WorkflowType InputWorkflowType
        {
            get { return m_InputType; }
            set { m_InputType = value; }
        }
        /// <summary>
        /// Type of workflow to write out
        /// </summary>
        public WorkflowType OutputWorkflowType 
        {
            get { return m_OutputType; }
            set { m_OutputType = value; }
        }
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
        public int Count
        {
            get { return Modules.Count; }
        }

        /// <summary>
        /// Name of the table in the database to run
        /// </summary>
        public string WorkflowTableName
        {
            get { return m_SQLiteWorkflowTableName; }
            set { m_SQLiteWorkflowTableName = value; }
        }

        /// <summary>
        /// SQLite database handled by Cyclops
        /// </summary>
        public SQLiteHandler SQLiteDatabase
        {
            get { return sql; }
            set { sql = value; }
        }

        public LinkedList<DataModules.BaseDataModule> Modules
        {
            get { return m_Modules; }
            set { m_Modules = value; }
        }

        public DataTable WorkflowTable
        {
            get { return m_ModulesTable; }
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
            Dictionary<string, string> d_ModuleTableColumnHeaders =
                new Dictionary<string, string>();
            d_ModuleTableColumnHeaders.Add("Step", "System.Int32");
            d_ModuleTableColumnHeaders.Add("Module", "System.String");
            d_ModuleTableColumnHeaders.Add("Parameter", "System.String");
            d_ModuleTableColumnHeaders.Add("Value", "System.String");

            foreach (KeyValuePair<string, string>
                kvp_Module in d_ModuleTableColumnHeaders)
            {
                DataColumn dc = new DataColumn(kvp_Module.Key);
                dc.DataType = System.Type.GetType(kvp_Module.Value);
                m_ModulesTable.Columns.Add(dc);
            }
        }

        /// <summary>
        /// Reads a workflow and assembles the modules
        /// </summary>
        /// <returns>True, if the workflow is read successfully</returns>
        public bool ReadWorkflow()
        {
            bool b_Successful = true;

            if (string.IsNullOrEmpty(InputWorkflowFileName))
            {
                Model.LogError("A workflow file must be supplied in order to read modules!");
                return false;
            }

            if (Path.GetExtension(InputWorkflowFileName).ToLower().Equals(".xml"))
                InputWorkflowType = WorkflowType.XML;
            else if (Path.GetExtension(InputWorkflowFileName).ToLower().Equals(".db") ||
                Path.GetExtension(InputWorkflowFileName).ToLower().Equals(".db3"))
                InputWorkflowType = WorkflowType.SQLite;

            /// TODO : Read modules from file
            switch (InputWorkflowType)
            {
                case WorkflowType.XML:
                    b_Successful = ReadXMLWorkflow();
                    break;
                case WorkflowType.SQLite:
                    b_Successful = ReadSQLiteWorkflow();
                    break;
            }

            if (Count == 0)
            {
                Model.LogError(string.Format("No modules were assembled from the workflow:\n" +
                    "Please check that the settings " +
                    "below are correct:\nInput File: {0}\nType: {1}",
                    InputWorkflowFileName,
                    InputWorkflowType));
                return false;
            }

            return b_Successful;
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

        public bool ReadWorkflow(string TheWorkflowFileName, string TableName,
            WorkflowType Type)
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
            bool b_Successful = true;
			string InputWorkflowFilePath = InputWorkflowFileName;

            try
            {
                //InputWorkflowFilePath = Path.Combine(Model.WorkDirectory, InputWorkflowFileName);
				
				XmlDocument xml = new XmlDocument();
                xml.Load(InputWorkflowFilePath);
                XmlNodeList xnl_Modules = xml.SelectNodes("Cyclops/Module");

                // Clear the LinkedList
                Modules.Clear();

                foreach (XmlNode xn in xnl_Modules)
                {
                    switch (xn.Attributes["Type"].Value.ToString().ToUpper())
                    {
                        case "DATA":
                            Dictionary<string, string> DataParam = 
                                GetXMLParameters(xn);
                            DataModules.BaseDataModule dm =
                                DataModules.BaseDataModule.Create(
                                xn.Attributes["Name"].Value.ToString(),
                                Model, DataParam);
                            dm.StepNumber = Convert.ToInt32(
                                xn.Attributes["Step"].Value.ToString());
                            dm = AddParameters(dm);

                            // Only add the module if that particular step
                            // is available
                            if (!HasStep(dm.StepNumber))
                                Modules.AddLast(dm);
                            else
                            {
                                Model.LogError("Error occurred while trying to " +
                                    "add Step: " + dm.StepNumber + ", Module: " +
                                    dm.ModuleName + ". This Step is already " +
                                    "taken in the workflow. Please check your " +
                                    "workflow so the same step number is not " +
                                    "used twice!");

                                return false;
                            }

                            AddModulesToDataTables(dm);
                            break;
                        case "OPERATION":
                            Dictionary<string, string> OperationParam =
                                GetXMLParameters(xn);                                                       
                            Operations.BaseOperationModule om =
                                Operations.BaseOperationModule.Create(
                                xn.Attributes["Name"].Value.ToString(),
                                Model, OperationParam);
                            if (!string.IsNullOrEmpty(
                                Model.OperationsDatabasePath))
                            {
                                om.OperationsDatabasePath =
                                    Model.OperationsDatabasePath;
                            }
                            b_Successful = om.PerformOperation();
                            
                            break;
                    }
                }
            }
            catch (IOException ioe)
            {
                Model.LogError("IOException thrown while reading XML Workflow file: \n" +
					InputWorkflowFilePath + "\nIOException: " + ioe.ToString());
                b_Successful = false;
            }
            catch (Exception exc)
            {
                Model.LogError("Exception thrown while reading XML Workflow file: \n" +
					InputWorkflowFilePath + "\nException: " + exc.ToString());
                b_Successful = false;
            }

            return b_Successful;
        }

        private void AddModulesToDataTables(DataModules.BaseDataModule Module)
        {
            foreach (KeyValuePair<string, string> kvp in Module.Parameters)
            {
                DataRow dr_Module = m_ModulesTable.NewRow();
                dr_Module["Step"] = Module.StepNumber;
                dr_Module["Module"] = Module.ModuleName;
                dr_Module["Parameter"] = kvp.Key;
                dr_Module["Value"] = kvp.Value;

                m_ModulesTable.Rows.Add(dr_Module);
            }
        }

        /// <summary>
        /// Runs the Module Workflow
        /// </summary>
        /// <returns>True, if the workflow completes successfully</returns>
        public bool RunWorkflow()
        {
            bool b_Successful = true;

            foreach (DataModules.BaseDataModule bdm in Modules)
            {
                b_Successful = bdm.PerformOperation();

                if (!b_Successful)
                    return false;
            }

            return b_Successful;
        }

        private DataModules.BaseDataModule AddParameters(
            DataModules.BaseDataModule Module)
        {
            foreach (KeyValuePair<string, string> kvp in Model.CyclopsParameters)
            {
                Module.Parameters.Add(kvp.Key, kvp.Value);
            }

            return Module;
        }

        private Dictionary<string, string> GetXMLParameters(XmlNode Node)
        {
            Dictionary<string, string> d_Parameters = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);
            XmlNodeList xnl_Parameters = Node.SelectNodes("Parameter");

            foreach (XmlNode xn in xnl_Parameters)
            {
                d_Parameters.Add(xn.Attributes["key"].Value.ToString(),
                    xn.Attributes["value"].Value.ToString());
            }

            return d_Parameters;
        }

        public bool ReadSQLiteWorkflow()
        {
            bool b_Successful = true;
            try
            {
                sql.DatabaseFileName = InputWorkflowFileName;
                DataTable dt_Workflow = sql.GetTable(WorkflowTableName);

                b_Successful = ReadDataTableWorkflow(dt_Workflow);
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while reading SQLite workflow:\n" +
                    exc.ToString());
                b_Successful = false;
            }

            return b_Successful;
        }

        public bool ReadDataTableWorkflow(DataTable WorkflowTable)
        {
            bool b_Successful = true;

            int? MaxSteps = GetMaximumStepsInWorkflowDataTable(WorkflowTable);
            if (MaxSteps == null)
                return false;
            else
            {
                // Clear the LinkedList
                Modules.Clear();

                for (int i = 0; i < MaxSteps; i++)
                {
                    int r = i + 1;
                    DataRow[] rows = WorkflowTable.Select(
                        string.Format("Step = {0}",
                        r));
                    Dictionary<string, string> Param = GetParametersFromDataRows(rows);
                    strModuleInfo mi = GetModuleNameFromRows(rows, r);

                    DataModules.BaseDataModule bdm = DataModules.BaseDataModule.Create(
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
            }

            return b_Successful;
        }

        /// <summary>
        /// Gets the Maximum number of steps in a DataTable workflow. If any steps are
        /// missing, the function returns null.
        /// </summary>
        /// <param name="Table">Workflow Table</param>
        /// <returns>Max Steps, or null if intermediate steps are missing</returns>
        private int? GetMaximumStepsInWorkflowDataTable(DataTable Table)
        {
            int i_MaxStepNumber = 0;

            HashSet<int> h_Steps = new HashSet<int>();

            foreach (DataRow dr in Table.Rows)
            {
                string s_Step = dr["Step"].ToString();
                if (!string.IsNullOrEmpty(s_Step))
                {
                    int i = Convert.ToInt32(s_Step);
                    if (!h_Steps.Contains(i))
                        h_Steps.Add(i);
                    if (i > i_MaxStepNumber)
                        i_MaxStepNumber = i;
                }
            }

            for (int i = 0; i < i_MaxStepNumber; i++)
            {
                int j = i + 1;
                if (!h_Steps.Contains(j))
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

            return i_MaxStepNumber;
        }

        private Dictionary<string, string> GetParametersFromDataRows(DataRow[] Rows)
        {
            Dictionary<string, string> Param = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (DataRow dr in Rows)
            {
                Param.Add(dr["Parameter"].ToString(), dr["Value"].ToString());
            }

            return Param;
        }

        private strModuleInfo GetModuleNameFromRows(DataRow[] rows, int Step)
        {
            strModuleInfo mi = new strModuleInfo();
            if (rows.Length > 0)
            {
                mi.ModuleName = rows[0]["Module"].ToString();
            }
            // Now check that the other rows have the same values
            foreach (DataRow dr in rows)
            {
                if (!mi.ModuleName.Equals(dr["Module"].ToString()))
                {
                    Model.LogWarning("Warning reading workflow info from SQLite:\n" +
                        "Step " + Step + " has multiple Modules");
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
            bool b_Successful = true;

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

                /// TODO : Write modules out to file
                switch (OutputWorkflowType)
                {
                    case WorkflowType.XML:
                        b_Successful = WriteModulesOutAsXML();
                        break;
                    case WorkflowType.SQLite:
                        b_Successful = WriteModulesOutToSQLite();
                        break;
                }
            }
            else
            {
                Model.LogError("No modules to write out!");
                return false;
            }

            return b_Successful;
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
            bool b_Successful = true;

            try
            {
                XmlWriterSettings Settings = new XmlWriterSettings();
                Settings.Indent = true;
                Settings.IndentChars = "   ";
                Settings.NewLineOnAttributes = false;
                Settings.NewLineChars = "\n";

                using (XmlWriter writer = XmlWriter.Create(OutputWorkflowFileName, Settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Cyclops");

                    foreach (DataModules.BaseDataModule bdm in Modules)
                    {
                        bdm.WriteModuleToXML(writer);
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            catch (InvalidOperationException ioe)
            {
                Model.LogError("InvalidOperationException thrown while writing XML Workflow:\n" +
                    ioe.ToString());
                b_Successful = false;
            }
            catch (IOException ie)
            {
                Model.LogError("IOException thrown while writing XML Workflow:\n" +
                    ie.ToString());
                b_Successful = false;
            }
            catch (Exception exc)
            {
                Model.LogError("Exception thrown while writing XML Workflow:\n" +
                    exc.ToString());
                b_Successful = false;
            }

            return b_Successful;
        }

        /// <summary>
        /// Writes the workflow out to a table in a SQLite database
        /// The Database is the OutputWorkflowFileName, and the 
        /// Table is named by the m_SQLiteWorkflowTableName variable
        /// </summary>
        /// <returns>True, if the table is exported successfully</returns>
        private bool WriteModulesOutToSQLite()
        {
            bool b_Successful = true;
            try
            {
                Dictionary<string, string> d_Columns = new Dictionary<string, string>();
                d_Columns.Add("Step", "System.Int32");
                d_Columns.Add("Module", "System.String");
                d_Columns.Add("Parameter", "System.String");
                d_Columns.Add("Value", "System.String");

                DataTable dt_Workflow = new DataTable(WorkflowTableName);
                foreach (KeyValuePair<string, string> kvp in d_Columns)
                {
                    DataColumn dc = new DataColumn(kvp.Key);
                    dc.DataType = System.Type.GetType(kvp.Value);
                    dt_Workflow.Columns.Add(dc);
                }

                foreach (DataModules.BaseDataModule bdm in Modules)
                {
                    bdm.WriteModuleToDataTable(dt_Workflow);
                }

                sql.DatabaseFileName = OutputWorkflowFileName;
                sql.CreateDatabase(OutputWorkflowFileName, false);
                sql.WriteDataTableToDatabase(dt_Workflow);
            }
            catch (Exception exc)
            {
                Model.LogError("Error writing workflow out to SQLite database:\n" +
                    exc.ToString());
                b_Successful = false;
            }

            return b_Successful;
        }

        /// <summary>
        /// Get the module at a particular step
        /// </summary>
        /// <param name="StepNumber">Step to retrieve module for</param>
        /// <returns>Module at the indicated step number</returns>
        public DataModules.BaseDataModule GetModule(int StepNumber)
        {
            foreach (DataModules.BaseDataModule m in Modules)
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
            bool b_HasStep = false;

            foreach (DataModules.BaseDataModule m in Modules)
            {
                if (m.StepNumber == StepNumber)
                    return true;
            }

            return b_HasStep;
        }

        /// <summary>
        /// Get the Maximum step number for the modules
        /// </summary>
        /// <returns>Maximum Step Number</returns>
        public int GetMaxStep()
        {
            int i_Max = 0;

            foreach (DataModules.BaseDataModule m in Modules)
            {
                if (m.StepNumber > i_Max)
                    i_Max = m.StepNumber;
            }

            return i_Max;
        }

        /// <summary>
        /// Removes a module from the List of Modules
        /// </summary>
        /// <param name="Module2Remove">Module to remove</param>
        //public void RemoveModuleFromWorkflow(DataModules.BaseDataModule Module2Remove)
        //{
        //    // Remove from m_ModulesTable
        //    // Update StepNumber of subsequent modules
            
        //    // Remove from LinkedList
        //    // Update StepNumber of subsequent modules
        //}

        /// <summary>
        /// Removes a module from the List of Modules
        /// </summary>
        /// <param name="StepNumber">Step to remove module</param>
        public void RemoveModuleFromWorkflow(int StepNumber)
        {
            var node = Modules.First;
            bool b_Removed = false;
            while (node != null)
            {
                var NextNode = node.Next;
                if (node.Value.StepNumber == StepNumber)
                {
                    Modules.Remove(node);
                    b_Removed = true;
                }
                else if (b_Removed)
                    node.Value.StepNumber--;
                node = NextNode;
            }

        }

        // Create the OnPropertyChanged method to raise the event 
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
