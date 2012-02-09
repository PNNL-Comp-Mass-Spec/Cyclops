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
using System.IO;

using RDotNet;
using log4net;

namespace Cyclops.ExportModules
{
    /// <summary>
    /// Saves the R workspace to an .RData file
    /// </summary>
    public class clsSaveEnvironment : clsBaseExportModule
    {
        private ExportModules.cslExportParameterHandler esp =
            new ExportModules.cslExportParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        protected string s_RInstance, s_Current_R_Statement = "";

        #region Constructors
        /// <summary>
        /// Module saves the R workspace to a designated file
        /// </summary>
        public clsSaveEnvironment()
        {
            ModuleName = "Save Module";
        }
        /// <summary>
        /// Module saves the R workspace to a designated file
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsSaveEnvironment(string InstanceOfR)
        {
            ModuleName = "Save Module";
            s_RInstance = InstanceOfR;            
        }
        /// <summary>
        /// Module saves the R workspace to a designated file
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsSaveEnvironment(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Save Module";
            Model = TheCyclopsModel;
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Methods
        /// <summary>
        ///  Runs module
        /// </summary>
        public override void PerformOperation()
        {
            traceLog.Info("Saving R Environment...");
            esp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                SaveToDefaultDirectory();
            }
        }

        /// <summary>
        /// Passes the parameters to the REngine to save the R workspace
        /// </summary>
        public void SaveToDefaultDirectory()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            string s_OutputFileName = "Results.RData", s_Command = "";
            
            if (esp.HasWorkDirectory & esp.HasFileName)
            {
                s_Command = string.Format("save.image(\"{0}\")",
                    esp.WorkDirectory + "/" + esp.FileName);
            }
            else if (esp.HasWorkDirectory)
            {
                s_Command = string.Format("save.image(\"{0}\")",
                     esp.WorkDirectory + "/" + s_OutputFileName);
            }
            else
            {
                s_Command = string.Format("save.image(\"{0}\")",
                     s_OutputFileName);
            }
            try
            {
                traceLog.Info("Saving R environment: " + s_Command);
                s_Current_R_Statement = s_Command;
                engine.EagerEvaluate(s_Command);
            }
            catch (Exception exc)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR SaveEnvironment: " + exc.ToString());
            }
        }

        /// <summary>
        /// Determine is all the necessary parameters are being passed to the object
        /// </summary>
        /// <returns>Returns true import module can proceed</returns>
        public bool CheckPassedParameters()
        {
            // NECESSARY PARAMETERS


            return true;
        }

        /// <summary>
        /// Unit Test for Importing a CSV file
        /// </summary>
        /// <returns>Information regarding the result of the UnitTest</returns>
        public clsTestResult TestSaveR_Workspace()
        {
            esp.GetParameters(ModuleName, Parameters);
            clsTestResult result = new clsTestResult(true, "");

            try
            {
                if (!CheckPassedParameters())
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR SAVING R WORKSPACE: Not all required parameters were passed in!";
                    return result;
                }

                SaveToDefaultDirectory();

                // Confirm by testing if the new table exists within the environment
                if (!File.Exists(System.IO.Path.Combine(esp.WorkDirectory, esp.FileName).Replace('\\', '/')))
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR SAVING R WORKSPACE: Could not find exported Workspace: " +
                        esp.FileName;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
            }
            catch (Exception exc)
            {
                result.IsSuccessful = false;
                result.Message = "ERROR SAVING R WORKSPACE: " + esp.FileName + "\n\n" + exc.ToString();
                result.R_Statement = s_Current_R_Statement;
            }

            return result;
        }
        #endregion
    }
}
