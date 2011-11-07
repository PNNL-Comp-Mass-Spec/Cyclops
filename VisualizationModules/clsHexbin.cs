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
using RDotNet;

namespace Cyclops
{
    /// <summary>
    /// Constructs and save a hexbin plot
    /// </summary>
    public class clsHexbin : clsBaseVisualizationModule
    {
        protected string s_RInstance;
        //protected Dictionary<string, string> d_PlotParameters = new Dictionary<string,string>();

        #region Constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        public clsHexbin()
        {
            ModuleName = "Hexin Module";
        }
        /// <summary>
        /// Basic constructor that passes in the R workspace
        /// </summary>
        /// <param name="InstanceOfR"></param>
        public clsHexbin(string InstanceOfR)
        {
            ModuleName = "Hexbin Module";
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
            // Construct the R statement
            string s_RStatement = "";
            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "hexbin"))
                clsGenericRCalls.InstallPackage(s_RInstance, "hexbin");

            s_RStatement = "require(hexbin)";
            engine.EagerEvaluate(s_RStatement);
            s_RStatement = "";

            s_RStatement += "bin <- hexbin(";

            if (Parameters["absLogX"].Equals("true"))
            {
                s_RStatement += string.Format("abs(log(as.numeric({0}${1}), 10)), ",
                    Parameters["tableName"],
                    Parameters["xColumn"]);
            }
            else
            {
                s_RStatement += string.Format("as.numeric({0}${1}), ",
                    Parameters["tableName"],
                    Parameters["xColumn"]);
            }

            if (Parameters["absLogY"].Equals("true"))
            {
                s_RStatement += string.Format("abs(log(as.numeric({0}${1}), 10)), ",
                    Parameters["tableName"],
                    Parameters["yColumn"]);
            }
            else
            {
                s_RStatement += string.Format("as.numeric({0}${1}), ",
                    Parameters["tableName"],
                    Parameters["yColumn"]);
            }

            s_RStatement += string.Format("xbins={0})\n",
                Parameters["bins"]);

            engine.EagerEvaluate(s_RStatement);
            s_RStatement = "";

            if (Parameters.ContainsKey("image"))
            {
                if (Parameters["image"].Equals("eps"))
                {
                    s_RStatement += string.Format("postscript(filename=\"{0}\", width={1}," +
                        "height={2}, horizontal={3}, pointsize={4})\n",
                        Parameters["plotFileName"],
                        Parameters["width"],
                        Parameters["height"],
                        Parameters["horizontal"],
                        Parameters["pointsize"]);
                }
                else if (Parameters["image"].Equals("png"))
                {
                    s_RStatement += string.Format("png(filename=\"" +
                        
                        "{0}/Plots/{1}\", width={2}," +
                        "height={3}, pointsize={4})\n",
                        Parameters["workDir"].Replace('\\', '/'),
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
                engine.EagerEvaluate(s_RStatement);
                s_RStatement = "";

                s_RStatement += string.Format("plot(bin, xlab=\"{0}\", ylab=\"{1}\", main=\"{2}\", style=\"{3}\")\n",
                    Parameters["xLab"],
                    Parameters["yLab"],
                    Parameters["main"],
                    "colorscale"); // can always come back and change up the style of the plot

                s_RStatement += "dev.off()\nrm(bin)";
                engine.EagerEvaluate(s_RStatement);
            }
        }
        #endregion
    }
}
