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
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using System.IO;

using log4net;


namespace Cyclops
{
    /// <summary>
    /// Model class serves as the entry point for the Cyclops DLL
    /// </summary>
    public class CyclopsModel : MessageEventBase
    {
        #region Members
        private Dictionary<string, string> m_CyclopsParameters = new Dictionary<string, string>();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        private bool m_SuccessRunningPipeline = true;
        private int m_StepNumber = 0, m_TotalNumberOfModules = 0;
        private WorkflowHandler m_WorkflowHandler;
        private GenericRCalls m_RCalls;
        #endregion

        #region Properties
        /// <summary>
        /// The executing version of Cyclops
        /// </summary>
        public string CyclopsVersion
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        /// <summary>
        /// Parameters to run Cyclops
        /// </summary>
        public Dictionary<string, string> CyclopsParameters
        {
            get { return m_CyclopsParameters; }
            set { m_CyclopsParameters = value; }
        }

        /// <summary>
        /// Current step that Cyclops is processing
        /// </summary>
        public int CurrentStepNumber
        {
            get { return m_StepNumber; }
            set { m_StepNumber = value; }
        }

        /// <summary>
        /// Total number of steps (modules) in the pipeline
        /// </summary>
        public int TotalNumberOfSteps
        {
            get { return m_TotalNumberOfModules; }
            set { m_TotalNumberOfModules = value; }
        }

        /// <summary>
        /// Flag that indicates the pipeline is running successfully
        /// </summary>
        public bool PipelineCurrentlySuccessful
        {
            get { return m_SuccessRunningPipeline; }
            set { m_SuccessRunningPipeline = value; }
        }

        /// <summary>
        /// Instance of the object that makes all calls to R
        /// </summary>
        public GenericRCalls RCalls
        {
            get { return m_RCalls; }
            set { m_RCalls = value; }
        }

        /// <summary>
        /// Loader that manages the root node, module assembly and IO.
        /// </summary>
        public WorkflowHandler ModuleLoader
        {
            get { return m_WorkflowHandler; }
            set { m_WorkflowHandler = value; }
        }

        /// <summary>
        /// Primary working directory
        /// </summary>
        public string WorkDirectory { get; set; }

        /// <summary>
        /// RData file to work from
        /// </summary>
        public string RWorkEnvironment { get; set; }

        /// <summary>
        /// SQLite database to work from
        /// </summary>
        public string SQLiteDatabase { get; set; }

        /// <summary>
        /// Path to SQLite database that contains the table to
        /// run a Cyclops Workflow
        /// </summary>
        public string OperationsDatabasePath { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Generic Cyclops Constructor, instantiates the R work environment
        /// </summary>
        public CyclopsModel()
        {
            m_RCalls = new GenericRCalls(this);
            m_WorkflowHandler = new WorkflowHandler(this);
            RCalls.InstantiateR();
            ReportMessage("Running Cyclops Version: " + CyclopsVersion);            
        }
        
        /// <summary>
        /// Primary Cyclops Constructor, instantiates the R work environment 
        /// and sets the parameters for running Cyclops. 
        /// </summary>
        /// <param name="ParametersForCyclops">Parameters to run Cyclops</param>
        public CyclopsModel(Dictionary<string, string> ParametersForCyclops)
        {
            m_RCalls = new GenericRCalls(this);
            m_WorkflowHandler = new WorkflowHandler(this);
            RCalls.InstantiateR();
            CyclopsParameters = ParametersForCyclops;

            if (CyclopsParameters.ContainsKey("workDir"))
            {

                WorkDirectory = CyclopsParameters["workDir"];
            }
            else
            {
                ReportError("Parameters passed into Cyclops did NOT contain a " +
                    "working directory 'workDir'");
                PipelineCurrentlySuccessful = false;
            }

            if (CyclopsParameters.ContainsKey("CyclopsWorkflowName"))
            {

                m_WorkflowHandler.InputWorkflowFileName =
                    CyclopsParameters["CyclopsWorkflowName"];
            }
            else
            {
                ReportError("Parameters passed into Cyclops did NOT contain a " +
                    "workflow file name 'CyclopsWorkflowName'");
                PipelineCurrentlySuccessful = false;
            }

            ReportMessage("Running Cyclops Version: " + CyclopsVersion);
        }


        public CyclopsModel(string XMLWorkflow, string WorkDirectory)
        {
            m_RCalls = new GenericRCalls(this);
            m_WorkflowHandler = new WorkflowHandler(this);
            m_WorkflowHandler.InputWorkflowFileName = XMLWorkflow;
            this.WorkDirectory = WorkDirectory;
            RCalls.InstantiateR();
            ReportMessage("Running Cyclops Version: " + CyclopsVersion);
        }
        #endregion

        #region Logging Methods

        public void LogError(string Message)
        {
            ReportError(Message);
        }

        public void LogError(string Message, string Module)
        {
            ReportError(Message, Module);
        }

        public void LogError(string Message, string Module, int? Step)
        {
            ReportError(Message, Module, (int)Step);
        }

        public void LogWarning(string Message)
        {
            ReportWarning(Message);
        }

        public void LogWarning(string Message, string Module)
        {
            ReportWarning(Message, Module);
        }

        public void LogWarning(string Message, string Module, int? Step)
        {
            ReportWarning(Message, Module, (int)Step);
        }

        public void LogMessage(string Message)
        {
            ReportMessage(Message);
        }

        public void LogMessage(string Message, string Module)
        {
            ReportMessage(Message, Module);
        }

        public void LogMessage(string Message, string Module, int? Step)
        {
            ReportMessage(Message, Module, (int)Step);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Main Logging method in Cyclops
        /// </summary>
        /// <param name="level"></param>
        /// <param name="Message"></param>
        public static void TraceLogWrite(LogTools.LogLevels level, string Message)
        {
            ILog traceLog = LogManager.GetLogger("TraceLog");
            switch (level)
            {
                case LogTools.LogLevels.INFO:
                    traceLog.Info(Message);
                    break;
                case LogTools.LogLevels.WARN:
                    traceLog.Warn(Message);
                    break;
                case LogTools.LogLevels.ERROR:
                    traceLog.Error(Message);
                    break;
            }
        }

        /// <summary>
        /// Main method to run the Cyclops Workflow
        /// </summary>
        /// <returns>True, if the workflow completes successfully</returns>
        public bool Run()
        {
            if (m_WorkflowHandler.Count > 0)
                return m_WorkflowHandler.RunWorkflow();
            else
            {
                LogWarning("No modules were detected during the Workflow Run()");
                return true;
            }
        }

        public bool WriteOutWorkflow(string FileName, WorkflowType OutputWorkflowType)
        {
            return m_WorkflowHandler.WriteWorkflow(FileName, OutputWorkflowType);
        }

        /// <summary>
        /// Testing method for Cyclop Modules
        /// </summary>
        /// <returns>True, if pipeline runs successfully</returns>
        public bool TestMethod()
        {
            bool b_Successful = true;
                        
            try
            {
                string s_Module = "MissedCleavageSummary";                
            }
            catch (Exception exc)
            {
                LogError("Exception caught while creating modules: " +
                    exc.ToString());
                b_Successful = false;
            }
            return b_Successful;
        }


        
        #endregion
    }
}
