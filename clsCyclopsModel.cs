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
    public class clsCyclopsModel
    {
        private clsBaseDataModule root = null, currentNode = null;
        private REngine engine;
        private string s_RInstance;

        #region Constructors
        public clsCyclopsModel()
        {
            s_RInstance = "rCore";
        }

        public clsCyclopsModel(string RDLL)
        {
            REngine.SetDllDirectory(RDLL);
            s_RInstance = "rCore";
        }
        #endregion

        #region Members
        public clsBaseDataModule Root
        {
            get { return root; }
            set { root = value; }
        }
        public clsBaseDataModule CurrentNode
        {
            get { return currentNode; }
            set { currentNode = value; }
        }
        #endregion

        #region Functions
        public void SetREngineDLL(string RDLL)
        {
            REngine.SetDllDirectory(RDLL);
        }

        public void CreateInstanceOfR()
        {
            engine = REngine.CreateInstance(s_RInstance, new[] { "-q" }); // quiet mode
        }

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

            clsCyclopsXMLReader xmlReader = new clsCyclopsXMLReader();

            root = xmlReader.ReadXML_Workflow(WorkFlowFile, s_RInstance);
        }

        public void Run()
        {
            if (root != null)
                root.PerformOperation();
        }
        #endregion
    }
}
