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
using RDotNet;

namespace Cyclops
{
    public class clsHeatmap : clsBaseVisualizationModule
    {
        protected string s_RInstance;

        #region Constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        public clsHeatmap()
        {
            ModuleName = "Heatmap Module";
        }
        /// <summary>
        /// Basic constructor that passes in the R workspace
        /// </summary>
        /// <param name="InstanceOfR"></param>
        public clsHeatmap(string InstanceOfR)
        {
            ModuleName = "Heatmap Module";
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        /// <summary>
        ///  Runs module
        /// </summary>
        public override void PerformOperation()
        {
            CreatePlotsFolder();

            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            // load the necessary packages
            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "grDevices"))
                clsGenericRCalls.InstallPackage(s_RInstance, "grDevices");
            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "gplots"))
                clsGenericRCalls.InstallPackage(s_RInstance, "gplots");

            string s_RStatement = "require(gplots)\nrequire(grDevices)\n";

            // Set the color of the heatmap
            s_RStatement += string.Format("crp <- colorRampPalette(c({0}))\n",
                "\"blue\",\"white\",\"red\"");

            if (Parameters.ContainsKey("image"))
            {
                if (Parameters["image"].Equals("eps"))
                {
                    s_RStatement += string.Format("postscript(\"{0}\", width={1}," +
                        "height={2}, horizontal={3}, pointsize={4})\n",
                        Parameters["plotFileName"],
                        Parameters["width"],
                        Parameters["height"],
                        Parameters["horizontal"],
                        Parameters["pointsize"]);
                }
                else if (Parameters["image"].Equals("png"))
                {
                    s_RStatement += string.Format("png(\"" +

                        "{0}/Plots/{1}\", width={2}," +
                        "height={3}, pointsize={4})\n",
                        Parameters["workDir"],
                        Parameters["plotFileName"],
                        Parameters["width"],
                        Parameters["height"],
                        Parameters["pointsize"]);
                }
                else if (Parameters["image"].Equals("jpg"))
                {
                    s_RStatement += string.Format("jpg(\"{0}\", width={1}," +
                        "height={2}, pointsize={3})\n",
                        Parameters["plotFileName"],
                        Parameters["width"],
                        Parameters["height"],
                        Parameters["pointsize"]);
                }

                

                s_RStatement += "dev.off()";
                engine.EagerEvaluate(s_RStatement);
            }
        }

        /// <summary>
        /// Checks the dictionary to ensure all the necessary parameters are present
        /// </summary>
        /// <returns>True if all necessary parameters are present</returns>
        protected bool CheckPassedParameters()
        {
            // NON-NECESSARY PARAMETERS



            return true;
        }
        #endregion
    }
}
