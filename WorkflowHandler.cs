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
        private bool m_WorkflowContainsOperations;

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
            var d_ModuleTableColumnHeaders =
                new Dictionary<string, string>
                {
	                {"Step", "System.Int32"},
	                {"Module", "System.String"},
	                {"Parameter", "System.String"},
	                {"Value", "System.String"}
                };

	        foreach (KeyValuePair<string, string>
                kvp_Module in d_ModuleTableColumnHeaders)
            {
                var dc = new DataColumn(kvp_Module.Key)
                {
	                DataType = Type.GetType(kvp_Module.Value)
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
            bool b_Successful = true;

            if (string.IsNullOrEmpty(InputWorkflowFileName))
            {
                Model.LogError("A workflow file must be supplied in order to read modules!",
                    "WorkflowHandler: ReadWorkflow()", 0);
                return false;
            }
            
			if (!File.Exists(InputWorkflowFileName) && !File.Exists(Path.Combine(Model.WorkDirectory, InputWorkflowFileName)))
            {
                Model.LogError("Cyclops cannot find the specified workflow file: " +
                    InputWorkflowFileName, "WorkflowHandler: ReadWorkflow()", 0);
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
                    b_Successful = ReadXMLWorkflow();
                    break;
                case WorkflowType.SQLite:
                    b_Successful = ReadSQLiteWorkflow();
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
			string InputWorkflowFilePath = "";

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
                XmlNodeList xnl_Modules = xml.SelectNodes("Cyclops/Module");

                // Clear the LinkedList
                Modules.Clear();

	            if (xnl_Modules != null)
	            {
		            foreach (XmlNode xn in xnl_Modules)
		            {
			            if (xn.Attributes != null)
			            {
				            switch (xn.Attributes["Type"].Value.ToUpper())
				            {
					            case "DATA":
						            Dictionary<string, string> DataParam = 
							            GetXMLParameters(xn);
						            DataModules.BaseDataModule dm =
							            DataModules.BaseDataModule.Create(
								            xn.Attributes["Name"].Value,
								            Model, DataParam);
						            dm.StepNumber = Convert.ToInt32(
							            xn.Attributes["Step"].Value);
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
								            xn.Attributes["Name"].Value,
								            Model, OperationParam);
						            if (!string.IsNullOrEmpty(
							            Model.OperationsDatabasePath))
						            {
							            om.OperationsDatabasePath =
								            Model.OperationsDatabasePath;
						            }
						            b_Successful = om.PerformOperation();
						            m_WorkflowContainsOperations = true;

						            break;
				            }
			            }

			            if (!b_Successful)
				            break;
		            }
	            }
            }
            catch (IOException ioe)
            {
                Model.LogError("IOException thrown while reading XML Workflow file: \n" +
					InputWorkflowFilePath + "\nIOException: " + ioe);
                b_Successful = false;
            }
            catch (Exception ex)
            {
                Model.LogError("Exception thrown while reading XML Workflow file: \n" +
					InputWorkflowFilePath + "\nException: " + ex);
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

            int i_Step = 1;
            foreach (DataModules.BaseDataModule bdm in Modules)
            {
                if (bdm != null)
                    b_Successful = bdm.PerformOperation();
                else
                    Model.LogError("Cyclops did not detect a module at step number: " +
                        i_Step);

                if (!b_Successful)
                    return false;

                i_Step++;
            }

            return true;
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
            var d_Parameters = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);
            XmlNodeList xnl_Parameters = Node.SelectNodes("Parameter");

	        if (xnl_Parameters != null)
	        {
		        foreach (XmlNode xn in xnl_Parameters)
		        {
			        if (xn.Attributes != null)
			        {
				        d_Parameters.Add(xn.Attributes["key"].Value,
				                         xn.Attributes["value"].Value);
			        }
		        }
	        }

	        return d_Parameters;
        }

        public bool ReadSQLiteWorkflow()
        {
            bool b_Successful;
            try
            {
                sql.DatabaseFileName = InputWorkflowFileName;
                DataTable dt_Workflow = sql.GetTable(WorkflowTableName);

                b_Successful = ReadDataTableWorkflow(dt_Workflow);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while reading SQLite workflow:\n" + ex);
                b_Successful = false;
            }

            return b_Successful;
        }

		public bool ReadDataTableWorkflow(DataTable workflowTableName)
        {

			int? MaxSteps = GetMaximumStepsInWorkflowDataTable(workflowTableName);
            if (MaxSteps == null)
                return false;

			// Clear the LinkedList
			Modules.Clear();

			for (int i = 0; i < MaxSteps; i++)
			{
				int r = i + 1;
				DataRow[] rows = workflowTableName.Select(
					string.Format("Step = {0}",
					              r));
				Dictionary<string, string> Param = GetParametersFromDataRows(rows, r);
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
            int i_MaxStepNumber = 0;

            var h_Steps = new HashSet<int>();

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

        private Dictionary<string, string> GetParametersFromDataRows(IEnumerable<DataRow> Rows, int stepNumber)
        {
            var Param = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (DataRow dr in Rows)
            {
	            string keyName = dr["Parameter"].ToString();
	            if (Param.ContainsKey(keyName))
		            throw new DuplicateNameException("Parameter " + keyName + " is specified twice for step " + stepNumber);
	            
				Param.Add(keyName, dr["Value"].ToString());
            }

            return Param;
        }

        private strModuleInfo GetModuleNameFromRows(DataRow[] rows, int Step)
        {
            var mi = new strModuleInfo();
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

                // Write modules out to file
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
                var Settings = new XmlWriterSettings
                {
	                Indent = true,
	                IndentChars = "   ",
	                NewLineOnAttributes = false,
	                NewLineChars = "\n"
                };

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
                Model.LogError("InvalidOperationException thrown while writing XML Workflow:\n" + ioe);
                b_Successful = false;
            }
            catch (IOException ie)
            {
                Model.LogError("IOException thrown while writing XML Workflow:\n" + ie);
                b_Successful = false;
            }
            catch (Exception ex)
            {
                Model.LogError("Exception thrown while writing XML Workflow:\n" + ex);
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
                var d_Columns = new Dictionary<string, string>
                {
	                {"Step", "System.Int32"},
	                {"Module", "System.String"},
	                {"Parameter", "System.String"},
	                {"Value", "System.String"}
                };

	            var dt_Workflow = new DataTable(WorkflowTableName);
                foreach (KeyValuePair<string, string> kvp in d_Columns)
                {
                    var dc = new DataColumn(kvp.Key)
                    {
	                    DataType = Type.GetType(kvp.Value)
                    };
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
            catch (Exception ex)
            {
                Model.LogError("Error writing workflow out to SQLite database:\n" + ex);
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

            foreach (DataModules.BaseDataModule m in Modules)
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

        public bool ContainsStep(int StepNumber)
        {
            foreach (DataModules.BaseDataModule bdm in Modules)
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
		        for (int Step = NewModule.StepNumber + 1;
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
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
