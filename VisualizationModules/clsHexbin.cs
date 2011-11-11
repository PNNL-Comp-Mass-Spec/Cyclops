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
using log4net;

namespace Cyclops
{
    /// <summary>
    /// Constructs and save a hexbin plot
    /// </summary>
    public class clsHexbin : clsBaseVisualizationModule
    {
        protected string s_RInstance;
        private string s_TableName="", s_xColumn="", s_yColumn="", 
            s_Bins="100", s_Image="png",s_PlotFileName="", 
            s_Width="480", s_Height="480", s_Horizontal="TRUE", 
            s_PointSize="12", s_WorkDir="", s_Xlab="", s_Ylab="", 
            s_Main="";
        private bool b_AbsLogX = false, b_AbsLogY = false, b_PlotsDir = false;
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

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
        /// <param name="InstanceOfR">Instance of R Workspace</param>
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
            traceLog.Info("Producing Hexbin Plot...");

            bool b_Param = CheckPassedParameters();

            if (b_Param)
            {
                CreatePlotsFolder();

                REngine engine = REngine.GetInstanceFromID(s_RInstance);
                // Construct the R statement
                string s_RStatement = "";
                if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "hexbin"))
                    clsGenericRCalls.InstallPackage(s_RInstance, "hexbin");

                s_RStatement = "require(hexbin)";
                try
                {
                    traceLog.Info("Loading Hexbin libraries: " + s_RStatement);
                    engine.EagerEvaluate(s_RStatement);
                }
                catch (Exception exc)
                {
                    traceLog.Error("ERROR loading Hexbin libraries: " + exc.ToString());
                }

                BinData();


                CreatePlotFile();
            }     
        }

        /// <summary>
        /// Checks the dictionary to ensure all the necessary parameters are present
        /// </summary>
        /// <returns>True if all necessary parameters are present</returns>
        protected bool CheckPassedParameters()
        {
            // NON-NECESSARY PARAMETERS
            if (Parameters.ContainsKey("absLogX"))
            {
                string abx = Parameters["absLogX"];
                b_AbsLogX = true ? abx.Equals("true") : false;
            }
            if (Parameters.ContainsKey("absLogY"))
            {
                string aby = Parameters["absLogY"];
                b_AbsLogY = true ? aby.Equals("true") : false;
            }
            if (Parameters.ContainsKey("plotDir"))
            {
                string pd = Parameters["plotDir"];
                b_PlotsDir = true ? pd.Equals("true") : false;
            }

            if (Parameters.ContainsKey("bins"))
                s_Bins = Parameters["bins"];
            else
            {
                traceLog.Error("Hexbin class: 'bins' was not found in the passed parameters, continuing the process...");
            }

            if (Parameters.ContainsKey("width"))
                s_Width = Parameters["width"];
            else
            {
                traceLog.Error("Hexbin class: 'width' was not found in the passed parameters, continuing the process...");
            }
            if (Parameters.ContainsKey("height"))
                s_Height = Parameters["height"];
            else
            {
                traceLog.Error("Hexbin class: 'height' was not found in the passed parameters, continuing the process...");
            }
            if (Parameters.ContainsKey("horizontal"))
                s_Horizontal = Parameters["horizontal"];
            else
            {
                traceLog.Error("Hexbin class: 'horizontal' was not found in the passed parameters, continuing the process...");
            }
            if (Parameters.ContainsKey("pointsize"))
                s_PointSize = Parameters["pointsize"];
            else
            {
                traceLog.Error("Hexbin class: 'pointsize' was not found in the passed parameters, continuing the process...");
            }

            if (Parameters.ContainsKey("xLab"))
                s_Xlab = Parameters["xLab"];
            else
            {
                traceLog.Error("Hexbin class: 'xLab' was not found in the passed parameters, continuing the process...");
            }
            if (Parameters.ContainsKey("yLab"))
                s_Ylab = Parameters["yLab"];
            else
            {
                traceLog.Error("Hexbin class: 'yLab' was not found in the passed parameters, continuing the process...");
            }
            if (Parameters.ContainsKey("main"))
                s_Main = Parameters["main"];
            else
            {
                traceLog.Error("Hexbin class: 'main' was not found in the passed parameters, continuing the process...");
            }


            // NECESSARY PARAMETERS
            if (Parameters.ContainsKey("tableName"))
                s_TableName = Parameters["tableName"];
            else
            {
                traceLog.Error("Hexbin class: 'tableName' was not found in the passed parameters");
                return false;
            }
            if (Parameters.ContainsKey("xColumn"))
                s_xColumn = Parameters["xColumn"];
            else
            {
                traceLog.Error("Hexbin class: 'xColumn' was not found in the passed parameters");
                return false;
            }
            if (Parameters.ContainsKey("yColumn"))
                s_yColumn = Parameters["yColumn"];
            else
            {
                traceLog.Error("Hexbin class: 'yColumn' was not found in the passed parameters");
                return false;
            }
            
            if (Parameters.ContainsKey("image"))
                s_Image = Parameters["image"];
            else
            {
                traceLog.Error("Hexbin class: 'image' was not found in the passed parameters");
                return false;
            }

            if (Parameters.ContainsKey("workDir") &
                Parameters.ContainsKey("plotFileName") &
                b_PlotsDir)
            {
                s_PlotFileName = Parameters["workDir"].Replace('\\', '/') + 
                    "/Plots/" + Parameters["plotFileName"];
            }
            else if (Parameters.ContainsKey("workDir") &
                Parameters.ContainsKey("plotFileName"))
            {
                s_PlotFileName = Parameters["workDir"].Replace('\\', '/') +
                    "/" + Parameters["plotFileName"];
            }
            else if (Parameters.ContainsKey("plotFileName"))
                s_PlotFileName = Parameters["plotFileName"];
            else
            {
                traceLog.Error("Hexbin class: 'plotFileName' was not found in the passed parameters");
                return false;
            }

            return true;
        }

        protected void BinData()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            string s_RStatement = "bin <- hexbin(";

            if (b_AbsLogX)
            {
                s_RStatement += string.Format("abs(log(as.numeric({0}${1}), 10)), ",
                    s_TableName,
                    s_xColumn);
            }
            else
            {
                s_RStatement += string.Format("as.numeric({0}${1}), ",
                    s_TableName,
                    s_xColumn);
            }

            if (b_AbsLogY)
            {
                s_RStatement += string.Format("abs(log(as.numeric({0}${1}), 10)), ",
                    s_TableName,
                    s_yColumn);
            }
            else
            {
                s_RStatement += string.Format("as.numeric({0}${1}), ",
                    s_TableName,
                    s_yColumn);
            }

            s_RStatement += string.Format("xbins={0})\n",
                s_Bins);

            try
            {
                traceLog.Info("Binning Data for Hexbin: " + s_RStatement);
                engine.EagerEvaluate(s_RStatement);
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR Binning Data for Hexbin: " + exc.ToString());
            }
        }

        protected void CreatePlotFile()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            string s_RStatement = "";
            if (Parameters.ContainsKey("image"))
            {
                if (Parameters["image"].Equals("eps"))
                {
                    s_RStatement += string.Format("postscript(filename=\"{0}\", width={1}," +
                        "height={2}, horizontal={3}, pointsize={4})\n",
                        s_PlotFileName,
                        s_Width,
                        s_Height,
                        s_Horizontal,
                        s_PointSize);
                }
                else if (Parameters["image"].Equals("png"))
                {
                    s_RStatement += string.Format("png(filename=\"" +

                        "{0}\", width={1}," +
                        "height={2}, pointsize={3})\n",
                        s_PlotFileName,
                        s_Width,
                        s_Height,
                        s_PointSize);
                }
                else if (Parameters["image"].Equals("jpg"))
                {
                    s_RStatement += string.Format("jpg(\"{0}\", width={1}," +
                        "height={2}, pointsize={3})\n",
                        s_PlotFileName,
                        s_Width,
                        s_Height,
                        s_PointSize);
                }
                try
                {
                    traceLog.Info("Creating Hexbin Image File: " + s_RStatement);
                    engine.EagerEvaluate(s_RStatement);
                }
                catch (Exception exc)
                {
                    traceLog.Error("ERROR Creating Hexbin Image File: " + exc.ToString());
                }
                s_RStatement = "";

                s_RStatement += string.Format("plot(bin, xlab=\"{0}\", ylab=\"{1}\", main=\"{2}\", style=\"{3}\")\n",
                    s_Xlab,
                    s_Ylab,
                    s_Main,
                    "colorscale"); // can always come back and change up the style of the plot

                s_RStatement += "dev.off()\nrm(bin)";

                try
                {
                    traceLog.Info("Saving Hexbin plot: " + s_RStatement);
                    engine.EagerEvaluate(s_RStatement);
                }
                catch (Exception exc)
                {
                    traceLog.Error("ERROR Saving Hexbin Plot: " + exc.ToString());
                }
            }  
        }
        #endregion
    }
}
