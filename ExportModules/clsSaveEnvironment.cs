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

namespace Cyclops
{
    public class clsSaveEnvironment : clsBaseExportModule
    {
        protected string s_RInstance;

        #region Constructors
        public clsSaveEnvironment()
        {
            ModuleName = "Save Module";
        }
        public clsSaveEnvironment(string InstanceOfR)
        {
            ModuleName = "Save Module";
            s_RInstance = InstanceOfR;            
        }
        #endregion

        #region Functions
        public override void PerformOperation()
        {
            SaveToDefaultDirectory(s_RInstance);
            //REngine engine = REngine.GetInstanceFromID(s_RInstance);

            //string s_OutputFileName = "";

            //if (Parameters.ContainsKey(clsCyclopsParametersKey.GetParameterName(
            //        "OutputDirectory")))
            //{
            //    s_OutputFileName = Parameters[clsCyclopsParametersKey.GetParameterName(
            //            "OutputDirectory")].ToString().Replace('\\', '/') +
            //            Path.DirectorySeparatorChar +
            //            Parameters["outputFileName"].ToString();
            //}
            //else
            //{
            //    s_OutputFileName = Parameters["outputFileName"].ToString();
            //}


            //string s_Command = string.Format("save.image(\"{0}\")",
            //                    s_OutputFileName);
            //try
            //{
            //    engine.EagerEvaluate(s_Command);
            //}
            //catch (Exception exc)
            //{
            //    Console.WriteLine("ERROR while attempting to save R workspace: " + exc.ToString());
            //}
        }

        public void SaveToDefaultDirectory(string InstanceOfR)
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            string s_OutputFileName = "Results.RData";
            string s_Path = "", s_Command = "";
            if (Parameters.ContainsKey("workDir"))
                s_Path = Parameters["workDir"].ToString();
            if (s_Path != null && s_Path.Length > 0)
            {
                //s_Command = string.Format("save.image(\"{0}\")",
                //    Path.Combine(s_Path, s_OutputFileName));
                s_Command = string.Format("save.image(\"{0}\")",
                    s_Path + "/" + s_OutputFileName);
            }
            else
            {
                s_Command = string.Format("save.image(\"{0}\")",
                                    s_OutputFileName);
            }
            try
            {
                s_Command = s_Command.Replace('\\', '/');
                engine.EagerEvaluate(s_Command);
            }
            catch (Exception exc)
            {
                Console.WriteLine("ERROR while attempting to save R workspace: " + exc.ToString());
            }
        }
        #endregion
    }
}
