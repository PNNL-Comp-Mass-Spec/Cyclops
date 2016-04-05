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
using log4net;

namespace Cyclops
{
    /// <summary>
    /// Controller class that interacts with and runs Cyclops
    /// </summary>
    public class CyclopsController
    {
        #region Members
        private readonly ILog traceLog = LogManager.GetLogger("TraceLog");
        private Dictionary<string, string> m_Parameters =
            new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);
        private CyclopsModel m_Cyclops;
        #endregion

        #region Properties
        /// <summary>
        /// Main directory Cyclops uses to pull files and export files
        /// Sets the path for the log file
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Path to the workflow file
        /// </summary>
        public string WorkFlowFileName { get; set; }

        /// <summary>
        /// Parameters to pass to Cyclops. These are the main parameter to 
        /// run Cyclops, not the individual modules
        /// </summary>
        public Dictionary<string, string> Parameters
        {
            get { return m_Parameters; }
            set { m_Parameters = value; }
        }

        /// <summary>
        /// Path to SQLite database that contains the table to
        /// run a Cyclops Workflow
        /// </summary>
        public string OperationsDatabasePath { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Generic empty constructor
        /// </summary>
        public CyclopsController()
        {
        }

        /// <summary>
        /// Main constructor to call to control Cyclops
        /// </summary>
        /// <param name="CyclopsParameters">Paramters for running Cyclops</param>
        public CyclopsController(
            Dictionary<string, string> CyclopsParameters)
        {
            Parameters = CyclopsParameters;

            if (Parameters.ContainsKey("workDir"))
                WorkingDirectory = Parameters["workDir"];
        }
        #endregion

        #region Methods
        /// <summary>
        /// Main method to run Cyclops
        /// </summary>
        /// <returns>True, if Cyclops completes successfully</returns>
        public bool Run()
        {
            m_Cyclops = new CyclopsModel(Parameters)
            {
                OperationsDatabasePath = OperationsDatabasePath
            };
            AttachEvents(m_Cyclops);
            if (string.IsNullOrEmpty(WorkingDirectory) &&
                Parameters.ContainsKey("workDir"))
                WorkingDirectory = Parameters["workDir"];
            //CreateLogFile();

            var b_Successful = m_Cyclops.ModuleLoader.ReadWorkflow();
			if (!b_Successful)
				return false;

            return m_Cyclops.Run();
        }

        /// <summary>
        /// Attaches the Error, Warning, and Message events to the local event handler
        /// </summary>
        /// <param name="Model"></param>
        private void AttachEvents(CyclopsModel Model)
        {
            Model.ErrorEvent += CyclopsEngineErrorEvent;
            Model.WarningEvent += CyclopsEngineWarningEvent;
            Model.MessageEvent += CyclopsEngineMessageEvent;
        }
        #endregion

        #region EventHandlers
        private void CyclopsEngineErrorEvent(object sender, MessageEventArgs e)
        {
            traceLog.Error("Step: " + e.Step + "\n" +
                e.Message + "\nModule: " + e.Module);
        }

        private void CyclopsEngineWarningEvent(object sender, MessageEventArgs e)
        {
            traceLog.Warn("Step: " + e.Step + "\n" +
                e.Message + "\nModule: " + e.Module);
        }

        private void CyclopsEngineMessageEvent(object sender, MessageEventArgs e)
        {            
            traceLog.Info("Step: " + e.Step + "\n" +
                e.Message + "\nModule: " + e.Module);       
        }
        #endregion
    }
}
