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

using RDotNet;
using log4net;

namespace Cyclops.DataModules
{
    /// <summary>
    /// Given a pre-existing RData Workspace, this module will load that workspace into the R environment.
    /// </summary>
    public class clsLoadRWorkspaceModule : clsBaseDataModule
    {
        private string s_RInstance, s_Current_R_Statement = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        
        #region Constructors
        /// <summary>
        /// Loads in existing R workspace
        /// </summary>
        public clsLoadRWorkspaceModule()
        {
            ModuleName = "Load Workspace Module";
        }
        /// <summary>
        /// Loads in existing R workspace
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsLoadRWorkspaceModule(string InstanceOfR)
        {
            ModuleName = "Load Workspace Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Loads in existing R workspace
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsLoadRWorkspaceModule(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Load Workspace Module";
            Model = TheCyclopsModel;
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            dsp.GetParameters(ModuleName, Parameters);
            traceLog.Info("Loading R Workspace: " + dsp.InputFileName.Replace('\\', '/'));

            LoadWorkspace();

            RunChildModules();
        }

        /// <summary>
        /// Checks the dictionary to ensure all the necessary parameters are present
        /// </summary>
        /// <returns>True if all necessary parameters are present</returns>
        protected bool CheckPassedParameters()
        {
            bool b_2Pass = true;

            // NECESSARY PARAMETERS
            if (!dsp.HasInputFileName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("Load R Workspace class: 'inputFileName': \"" +
                    dsp.InputFileName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        protected void LoadWorkspace()
        {           
            if (CheckPassedParameters())
            {
                REngine engine = REngine.GetInstanceFromID(s_RInstance);

                if (!dsp.InputFileName.Equals(String.Empty))
                {
                    string s_FileNameOfRFile2Load = dsp.InputFileName;

                    // Smart checker...
                    if (!System.IO.File.Exists(s_FileNameOfRFile2Load))
                        s_FileNameOfRFile2Load = System.IO.Path.Combine(
                            dsp.WorkDirectory,
                            dsp.InputFileName);

                    s_FileNameOfRFile2Load = s_FileNameOfRFile2Load.Replace('\\', '/');

                    string s_Command = string.Format(
                        "load(\"{0}\")",
                        s_FileNameOfRFile2Load);
                    traceLog.Info("LOADING SOURCE FILE: " + s_Command);
                    s_Current_R_Statement = s_Command;
                    engine.EagerEvaluate(s_Command);
                }
            }
        }

        /// <summary>
        /// Unit Test for Loading an R Workspace
        /// </summary>
        /// <returns>Information regarding the result of the UnitTest</returns>
        public clsTestResult TestLoadRWorkspace()
        {
            dsp.GetParameters(ModuleName, Parameters);
            clsTestResult result = new clsTestResult(true, "");
            
            int i_NumberOfObjectsInWorkspace = 9;

            try
            {
                if (!CheckPassedParameters())
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR LOADING R WORKSPACE: Not all required parameters were passed in!";                      
                    return result;
                }

                LoadWorkspace();

                // Confirm by testing the number of objects in the R workspace
                List<string> l_ObjectsSupposed2BeThere = new List<string>();
                l_ObjectsSupposed2BeThere.Add("T_Column_Metadata");
                l_ObjectsSupposed2BeThere.Add("T_Data_0pt1percent");
                //l_ObjectsSupposed2BeThere.Add("T_Data_10percent");
                //l_ObjectsSupposed2BeThere.Add("T_Data_1percent");
                //l_ObjectsSupposed2BeThere.Add("T_Data_5percent");
                //l_ObjectsSupposed2BeThere.Add("T_Long_0pt1percent");
                //l_ObjectsSupposed2BeThere.Add("T_Long_10percent");
                //l_ObjectsSupposed2BeThere.Add("T_Long_1percent");
                //l_ObjectsSupposed2BeThere.Add("T_Long_5percent");

                List<string> l_Missing = new List<string>();
                List<string> s_Objects = clsGenericRCalls.ls(s_RInstance);
                foreach (string s in l_ObjectsSupposed2BeThere)
                {
                    if (!s_Objects.Contains(s))
                    {
                        l_Missing.Add(s);
                    }
                }
                if (l_Missing.Count > 0)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR LOADING R WORKSPACE:\n" +
                        "The following tables were missing in the workspace:\n";
                    foreach (string s in l_Missing)
                    {
                        result.Message += s + "\n";
                    }
                    result.Module = ModuleName;
                    result.R_Statement = s_Current_R_Statement;
                }
            }
            catch (Exception exc)
            {
                result.IsSuccessful = false;
                result.Module = ModuleName;
                result.Message = "ERROR LOADING R WORKSPACE: " + dsp.InputFileName + "\n\n" + exc.ToString();
                result.R_Statement = s_Current_R_Statement;
            }

            return result;
        }
        #endregion
    }
}
