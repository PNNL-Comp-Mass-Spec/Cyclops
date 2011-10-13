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

using RDotNet;

namespace Cyclops
{
    /// <summary>
    /// Model class serves as the entry point for the Cyclops DLL
    /// </summary>
    public class clsCyclopsModel
    {
        private clsBaseDataModule root = null, currentNode = null;
        private REngine engine;
        private string s_RInstance;
        private string s_Version = "0.1.0.1";
        private Dictionary<string, string> d_CyclopsParameters = new Dictionary<string, string>();

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
            s_RInstance = value.Length > 0 ? value : "rCore";
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

        #region Members
        /// <summary>
        /// Root module of Cyclops Pipeline
        /// </summary>
        public clsBaseDataModule Root
        {
            get { return root; }
            set { root = value; }
        }
        /// <summary>
        /// Pointer to current module in Cyclops Pipeline
        /// </summary>
        public clsBaseDataModule CurrentNode
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
        /// <summary>
        /// Retrieves the current Cyclops Version
        /// </summary>
        public string Version
        {
            get { return s_Version; }
        }
        #endregion

        #region Functions
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
        public void CreateInstanceOfR()
        {
            engine = REngine.CreateInstance(s_RInstance, new[] { "-q" }); // quiet mode
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
        /// Read the XML workflow file and assemble the pipeline
        /// </summary>
        /// <param name="WorkFlowFile">Full path to the XML file</param>
        public void AssembleModulesFromXML(string WorkFlowFile)
        {
            CreateInstanceOfR();

            clsCyclopsXMLReader reader = new clsCyclopsXMLReader();

            root = reader.ReadXML_Workflow(WorkFlowFile, s_RInstance);
        }

        /// <summary>
        /// Read the XML workflow file and assemble the pipeline
        /// </summary>
        /// <param name="WorkFlowFile">Full path to the XML file</param>
        /// <param name="RDLL">Path to R</param>
        public void AssembleModulesFromXML(string WorkFlowFile, string RDLL)
        {
            SetREngineDLL(RDLL);
            CreateInstanceOfR();

            clsCyclopsXMLReader xmlReader = new clsCyclopsXMLReader(CyclopsParameters);

            root = xmlReader.ReadXML_Workflow(WorkFlowFile, s_RInstance);
        }

        /// <summary>
        /// Runs the Cyclops Pipeline
        /// </summary>
        public void Run()
        {
            if (root != null)
                root.PerformOperation();
        }
        #endregion
    }
}
