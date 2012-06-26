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
using System.IO;
using System.Diagnostics;

using log4net;
using RDotNet;

// Configure log4net using the .log4net file
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Logging.config", Watch = true)]

namespace Cyclops
{
    /// <summary>
    /// Model class serves as the entry point for the Cyclops DLL
    /// </summary>
    public class clsCyclopsModel
    {
        #region Members
        private DataModules.clsBaseDataModule root = null, currentNode = null;
        private REngine engine;
        private string s_RInstance;
        private Dictionary<string, string> d_CyclopsParameters = new Dictionary<string, string>();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        private bool b_SuccessRunningPipeline = true;
        #endregion

        #region Constructors
        /// <summary>
        /// Basic constructor for the Model class
        /// </summary>
        public clsCyclopsModel()
        {
            s_RInstance = "rCore";            
        }

        /// <summary>
        /// Constructor that requires the parameters for running cyclops
        /// </summary>
        /// <param name="ParametersForCyclops">Parameters for running cyclops</param>
        public clsCyclopsModel(Dictionary<string, string> ParametersForCyclops)
        {
            string value = "";
            d_CyclopsParameters = ParametersForCyclops;
            CyclopsParameters.TryGetValue(clsCyclopsParametersKey.GetParameterName("PipelineID"),
                out value);
            if (value != null && value.Length > 0)
            {
                s_RInstance = value;
            }
            else
            {
                s_RInstance = "rCore";
            }

            CyclopsParameters.TryGetValue(clsCyclopsParametersKey.GetParameterName("RDLL"),
                out value);
            if (value != null && value.Length > 0)
            {
                REngine.SetDllDirectory(value);
            }
            else
            {
                REngine.SetDllDirectory(@"C:\Program Files\R\R-2.13.1\bin\i386");
            }
        }

        /// <summary>
        /// Constructor that requires the path to R DLL
        /// </summary>
        /// <param name="RDLL">Path to R DLL</param>
        public clsCyclopsModel(string RDLL)
        {
            REngine.SetDllDirectory(RDLL);
            s_RInstance = "rCore";
        }
        #endregion

        #region Properties
        /// <summary>
        /// The total number of modules to be run in pipeline.
        /// </summary>
        public int NumberOfModules
        {
            get;
            set;
        }

        /// <summary>
        /// Flag to indicate if the pipeline completed successfully.
        /// </summary>
        public bool SuccessRunningPipeline
        {
            get
            {
                return b_SuccessRunningPipeline;
            }
            set
            {
                b_SuccessRunningPipeline = value;
            }

        }
        /// <summary>
        /// Root module of Cyclops Pipeline
        /// </summary>
        public DataModules.clsBaseDataModule Root
        {
            get { return root; }
            set { root = value; }
        }
        /// <summary>
        /// Pointer to current module in Cyclops Pipeline
        /// </summary>
        public DataModules.clsBaseDataModule CurrentNode
        {
            get { return currentNode; }
            set { currentNode = value; }
        }
        /// <summary>
        /// Dictionary of Parameters for running Cyclops
        /// </summary>
        public Dictionary<string, string> CyclopsParameters
        {
            get { return d_CyclopsParameters; }
            set { d_CyclopsParameters = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the path the R DLL
        /// </summary>
        /// <param name="RDLL">Path to the R DLL</param>
        public void SetREngineDLL(string RDLL)
        {
            REngine.SetDllDirectory(RDLL);
        }

        /// <summary>
        /// Creates a new instance of the R workspace
        /// </summary>
        public bool CreateInstanceOfR()
        {
            try
            {
                engine = REngine.CreateInstance(s_RInstance, new[] { "-q" }); // quiet mode
                return true;
            }
            catch (Exception exc)
            {
                return false;
            }
        }

        /// <summary>
        /// Loads a R workspace
        /// </summary>
        /// <param name="Workspace"></param>
        public void CreateInstanceOfR_AndLoadWorkspace(string Workspace)
        {
            engine = REngine.CreateInstance(s_RInstance, new[] { "-q" }); // quiet mode
            engine.EagerEvaluate(string.Format("load({0})", Workspace));
        }

        /// <summary>
        /// Reads an XML workflow provided in CyclopsParameters and assembles 
        /// the modules for the pipeline
        /// </summary>
        public void AssembleModulesFromXML()
        {            
            traceLog.Info("Establishing connection to R...");

            try
            {
                CreateInstanceOfR();
                traceLog.Info("Connection with R established successfully!");
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR establishing connection with R: " + exc.ToString());
            }
            
            clsCyclopsXMLReader reader = new clsCyclopsXMLReader(this, CyclopsParameters);
            
            string value = "";
            CyclopsParameters.TryGetValue(clsCyclopsParametersKey.GetParameterName("Workflow"),
                out value);
            if (value != null && value.Length > 0)
            {
                string s_Path = "";
                CyclopsParameters.TryGetValue(clsCyclopsParametersKey.GetParameterName("workDir"),
                    out s_Path);
                value = Path.Combine(s_Path, value);
                if (File.Exists(value))
                {
                    traceLog.Info("Assembling Modules from XML: " + value);
                    root = reader.ReadXML_Workflow(value,
                        s_RInstance);
                }
            }
            else
            {
                traceLog.Error("Name of Cyclops XML workflow was not passed in properly to the Parameters.");
            }
        }

        /// <summary>
        /// Read the XML workflow file and assemble the pipeline
        /// </summary>
        /// <param name="WorkFlowFile">Full path to the XML file</param>
        public void AssembleModulesFromXML(string WorkFlowFile)
        {
            traceLog.Info("Establishing connection to R...");

            try
            {
                CreateInstanceOfR();
                traceLog.Info("Connection with R established successfully!");
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR establishing connection with R: " + exc.ToString());
            }

            traceLog.Info("Assembing Modules from XML: " + WorkFlowFile + ".");

            clsCyclopsXMLReader reader = new clsCyclopsXMLReader(this, CyclopsParameters);

            root = reader.ReadXML_Workflow(WorkFlowFile, s_RInstance);
        }

        /// <summary>
        /// Read the XML workflow file and assemble the pipeline
        /// </summary>
        /// <param name="WorkFlowFile">Full path to the XML file</param>
        /// <param name="RDLL">Path to R</param>
        public void AssembleModulesFromXML(string WorkFlowFile, string RDLL)
        {
            traceLog.Info("Establishing connection to R...");
            try
            {
                CreateInstanceOfR();
                traceLog.Info("Connection with R established successfully!");
                traceLog.Info("Setting RDLL: " + RDLL);
                SetREngineDLL(RDLL);
                traceLog.Info("RDLL set successfully!");
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR establishing connection with R & setting RDLL: " + exc.ToString());
            }

            traceLog.Info("Assembing Modules from XML: " + WorkFlowFile + ".");



            clsCyclopsXMLReader xmlReader = new clsCyclopsXMLReader(this, CyclopsParameters);

            root = xmlReader.ReadXML_Workflow(WorkFlowFile, s_RInstance);
        }

        /// <summary>
        /// Runs the Cyclops Pipeline
        /// </summary>
        public bool Run()
        {
            traceLog.Info("Running Cyclops Workflow.");

            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            if (root != null)
            {
                root.PerformOperation();
                engine.Close();
                traceLog.Info(string.Format("Cyclops completed {0}",
                    SuccessRunningPipeline ? "successfully!" : "unsuccessfully."));
                return SuccessRunningPipeline;
            }
            else
            {
                traceLog.Error("Cyclops was unable to find the root module.");
                engine.Close();
                return false;
            }
        }

        public void PrintPipeLineModules()
        {
            if (root != null)
            {
                root.PrintModule();
            }
            else
            {
                Console.WriteLine("There were no modules in the pipeline!");
            }
        }

        private bool CreateLogTableInR()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            string s_Command = "t_log <- data.frame(" +
                "\"DateTime\"=Sys.time()" +             // record time of call
                ",\"Module\"=\"CyclopsModel\"" +        // Module that ran function
                ",\"Target\"=\"\"" +                    // Target table
                ",\"Function\"=\"Create R Session\"" +  // Type of function called
                ", \"Statement\"=\"\")";                // R statement passed
            try
            {
                engine.EagerEvaluate(s_Command);

                return true;
            }
            catch (Exception exc)
            {
                // TODO : Handle exception

                return false;
            }
        }
        #endregion
    }

    public struct CyclopsPipelineStatusManager
    {
        public string ModuleName;
    }
}
