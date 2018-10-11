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

namespace Cyclops
{
    /// <summary>
    /// Controller class that interacts with and runs Cyclops
    /// </summary>
    public class CyclopsController : PRISM.EventNotifier
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
        /// <param name="paramDictionary">Paramters for running Cyclops</param>
        public CyclopsController(
            Dictionary<string, string> paramDictionary)
        {
            Parameters = paramDictionary;

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

            var successful = cyclops.ModuleLoader.ReadWorkflow();
            if (!successful)
                return false;

            var success = cyclops.Run();
            return success;
        }

        #endregion
    }
}
