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

namespace Cyclops
{
    /// <summary>
    /// Controller class that interacts with and runs Cyclops
    /// </summary>
    public class CyclopsController : PRISM.clsEventNotifier
    {
        #region Members

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
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
            var cyclops = new CyclopsModel(Parameters)
            {
                OperationsDatabasePath = OperationsDatabasePath
            };

            RegisterEvents(cyclops);

            if (string.IsNullOrEmpty(WorkingDirectory) &&
                Parameters.ContainsKey("workDir"))
                WorkingDirectory = Parameters["workDir"];

            var b_Successful = cyclops.ModuleLoader.ReadWorkflow();
            if (!b_Successful)
                return false;

            var success = cyclops.Run();
            return success;
        }

        #endregion
    }
}
