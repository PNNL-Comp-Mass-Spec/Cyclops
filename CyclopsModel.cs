/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
 * -----------------------------------------------------
 *
 * Licensed under the 2-Clause BSD License; you may not use this
 * file except in compliance with the License.  You may obtain
 * a copy of the License at https://opensource.org/licenses/BSD-2-Clause
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;
using PRISM;

namespace Cyclops
{
    /// <summary>
    /// Model class serves as the entry point for the Cyclops DLL
    /// </summary>
    public class CyclopsModel : EventNotifier
    {
        #region Members

        #endregion

        #region Properties
        /// <summary>
        /// The executing version of Cyclops
        /// </summary>
        public string CyclopsVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// Parameters to run Cyclops
        /// </summary>
        public Dictionary<string, string> CyclopsParameters { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Current step that Cyclops is processing
        /// </summary>
        public int CurrentStepNumber { get; set; }

        /// <summary>
        /// Flag that indicates the pipeline is running successfully
        /// </summary>
        public bool PipelineCurrentlySuccessful { get; set; } = true;

        /// <summary>
        /// Instance of the object that makes all calls to R
        /// </summary>
        public GenericRCalls RCalls { get; set; }

        /// <summary>
        /// Loader that manages the root node, module assembly and IO.
        /// </summary>
        public WorkflowHandler ModuleLoader { get; set; }

        /// <summary>
        /// Primary working directory
        /// </summary>
        public string WorkDirectory { get; set; }

        /// <summary>
        /// RData file to work from
        /// </summary>
        public string RWorkEnvironment { get; set; }

        /// <summary>
        /// Path to SQLite database that contains the table to
        /// run a Cyclops Workflow
        /// </summary>
        /// <remarks>Default file name: Cyclops_Operations.db3</remarks>
        public string OperationsDatabasePath { get; set; }
        #endregion

        #region Constructors

        /// <summary>
        /// Primary Cyclops Constructor, instantiates the R work environment
        /// and sets the parameters for running Cyclops.
        /// </summary>
        /// <param name="ParametersForCyclops">Parameters to run Cyclops</param>
        public CyclopsModel(Dictionary<string, string> ParametersForCyclops)
        {
            RCalls = new GenericRCalls(this);
            ModuleLoader = new WorkflowHandler(this);
            RCalls.InstantiateR();
            CyclopsParameters = ParametersForCyclops;

            if (CyclopsParameters.ContainsKey("workDir"))
            {

                WorkDirectory = CyclopsParameters["workDir"];
            }
            else
            {
                OnErrorEvent("Parameters passed into Cyclops did NOT contain a " +
                    "working directory 'workDir'");
                PipelineCurrentlySuccessful = false;
            }

            if (CyclopsParameters.ContainsKey("CyclopsWorkflowName"))
            {

                ModuleLoader.InputWorkflowFileName =
                    CyclopsParameters["CyclopsWorkflowName"];
            }
            else
            {
                OnErrorEvent("Parameters passed into Cyclops did NOT contain a " +
                    "workflow file name 'CyclopsWorkflowName'");
                PipelineCurrentlySuccessful = false;
            }

            OnStatusEvent("Running Cyclops Version: " + CyclopsVersion);
        }

        [Obsolete("Unused")]
        public CyclopsModel(string XMLWorkflow, string WorkDirectory)
        {
            RCalls = new GenericRCalls(this);
            ModuleLoader = new WorkflowHandler(this)
            {
                InputWorkflowFileName = XMLWorkflow
            };
            this.WorkDirectory = WorkDirectory;
            RCalls.InstantiateR();
            OnStatusEvent("Running Cyclops Version: " + CyclopsVersion);
        }
        #endregion

        #region Logging Methods

        private string FormatStatusMessage(string message, string module, int? step = null)
        {
            if (string.IsNullOrEmpty(module))
                return message;

            if (step == null)
                return module + ": " + message;

            // Example messages:
            // LoadRSourceFiles, step 1: Running LoadRSourceFiles
            // LoadRSourceFiles, step 1: source('C:/DMS_WorkDir/R_Scripts/ZRollup.R')
            // ExportTable, step 5: Running ExportTable
            return module + ", step " + step + ": " + message;
        }

        public void LogError(string message)
        {
            OnErrorEvent(message);
        }

        public void LogError(string message, string module)
        {
            OnErrorEvent(FormatStatusMessage(message, module));
        }

        public void LogError(string message, string module, int step)
        {
            OnErrorEvent(FormatStatusMessage(message, module, step));
        }

        public void LogWarning(string message)
        {
            OnWarningEvent(message);
        }

        public void LogWarning(string message, string module, int step)
        {
            OnWarningEvent(FormatStatusMessage(message, module, step));
        }

        public void LogMessage(string message)
        {
            OnStatusEvent(message);
        }

        public void LogMessage(string message, string module, int step)
        {
            OnStatusEvent(FormatStatusMessage(message, module, step));
        }
        #endregion

        #region Methods

        /// <summary>
        /// Main method to run the Cyclops Workflow
        /// </summary>
        /// <returns>True, if the workflow completes successfully</returns>
        public bool Run()
        {
            if (ModuleLoader.Count > 0)
                return ModuleLoader.RunWorkflow();

            LogWarning("No modules were detected during the Workflow Run()");
            return true;
        }

        [Obsolete("Unused")]
        public bool WriteOutWorkflow(string FileName, WorkflowType OutputWorkflowType)
        {
            return ModuleLoader.WriteWorkflow(FileName, OutputWorkflowType);
        }

        /// <summary>
        /// Testing method for Cyclops Modules
        /// </summary>
        /// <returns>True, if pipeline runs successfully</returns>
        [Obsolete("Unused")]
        public bool TestMethod()
        {
            var successful = true;

            try
            {
                Console.WriteLine("MissedCleavageSummary");
            }
            catch (Exception ex)
            {
                LogError("Exception caught while creating modules: " + ex.Message);
                successful = false;
            }
            return successful;
        }



        #endregion
    }
}
