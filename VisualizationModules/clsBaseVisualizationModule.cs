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
using System.IO;

using log4net;

namespace Cyclops.VisualizationModules
{
    /// <summary>
    /// Base class for visualization modules
    /// </summary>
    public abstract class clsBaseVisualizationModule : clsBaseModule
    {
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        #region Properties
        // instance of the model class
        public clsCyclopsModel Model
        {
            get;
            set;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates a directory to store image files
        /// </summary>
        public bool CreatePlotsFolder()
        {
            bool b_DirectoryCreated = false;
            if (Parameters.ContainsKey("workDir"))
            {
                string s_WorkDir = Parameters["workDir"];
                s_WorkDir += "/Plots";
                s_WorkDir = s_WorkDir.Replace('\\', '/');
                if (!Directory.Exists(s_WorkDir))
                {
                    traceLog.Info("Creating Plots Directory");
                    Directory.CreateDirectory(s_WorkDir);
                    b_DirectoryCreated = true;
                }
                b_DirectoryCreated = true;
            }
            else
            {
                if (!Directory.Exists("Plots"))
                {
                    Directory.CreateDirectory("Plots");
                    b_DirectoryCreated = true;
                }
            }

            return b_DirectoryCreated;
        }
        #endregion
    }
}
