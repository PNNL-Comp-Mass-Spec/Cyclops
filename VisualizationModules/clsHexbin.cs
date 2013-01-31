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

namespace Cyclops.VisualizationModules
{
    /// <summary>
    /// Constructs and save a hexbin plot
    /// </summary>
    public class clsHexbin : clsBaseVisualizationModule
    {
        #region Members
        protected string s_RInstance;
        private VisualizationModules.clsVisualizationParameterHandler vgp =
            new VisualizationModules.clsVisualizationParameterHandler();

        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Constructors
        /// <summary>
        /// Module plots a hexbin graph to the Plots directory
        /// </summary>
        public clsHexbin()
        {
            ModuleName = "Hexin Module";
        }
        /// <summary>
        /// Module plots a hexbin graph to the Plots directory
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsHexbin(string InstanceOfR)
        {
            ModuleName = "Hexbin Module";
            s_RInstance = InstanceOfR;            
        }
        /// <summary>
        /// Module plots a hexbin graph to the Plots directory
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsHexbin(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Hexbin Module";
            Model = TheCyclopsModel;             
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
            if (Model.SuccessRunningPipeline)
            {
                Model.IncrementStep(ModuleName);

                vgp.GetParameters(ModuleName, Parameters);

                if (CheckPassedParameters())
                {
                    CreatePlotsFolder();

                    if (clsGenericRCalls.ContainsObject(s_RInstance, vgp.TableName))
                    {
                        // Construct the R statement
                        string s_RStatement = "";

                        if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "hexbin"))
                            clsGenericRCalls.InstallPackage(s_RInstance, "hexbin");

                        s_RStatement = "require(hexbin)";

                        if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                            "Loading Libraries for Hexbin Plot",
                            this.StepNumber, Model.NumberOfModules))
                            Model.SuccessRunningPipeline = false;

                        BinData();

                        CreatePlotFile();
                    }
                    else
                    {
                        traceLog.Error("ERROR Hexbin class: " + vgp.TableName + " does not exist in the R workspace.");
                    }
                }
            }     
        }

        /// <summary>
        /// Checks the dictionary to ensure all the necessary parameters are present
        /// </summary>
        /// <returns>True if all necessary parameters are present</returns>
        protected bool CheckPassedParameters()
        {
            bool b_2Param = true;

            // NECESSARY PARAMETERS
            if (string.IsNullOrEmpty(vgp.TableName))
            {
                traceLog.Error("Hexbin class: 'tableName' was not found in the passed parameters");
                b_2Param = false;
            }
            if (!clsGenericRCalls.ContainsObject(s_RInstance, vgp.TableName))
            {
                traceLog.Error("Hexbin class: " + vgp.TableName + " was not found in the R workspace");
                b_2Param = false;
            }
            if (string.IsNullOrEmpty(vgp.xColumn))
            {
                traceLog.Error("Hexbin class: 'xColumn' was not found in the passed parameters");
                b_2Param = false;
            }
            if (!clsGenericRCalls.TableContainsColumn(s_RInstance, vgp.TableName,
                vgp.xColumn))
            {
                traceLog.Error(string.Format("Hexbin class: " +
                    "xColumn: '{0}' was not found in the table: '{1}'",
                    vgp.xColumn,
                    vgp.TableName));
                b_2Param = false;
            }
            if (string.IsNullOrEmpty(vgp.yColumn))
            {
                traceLog.Error("Hexbin class: 'yColumn' was not found in the passed parameters");
                b_2Param = false;
            }
            if (!clsGenericRCalls.TableContainsColumn(s_RInstance, vgp.TableName,
                vgp.yColumn))
            {
                traceLog.Error(string.Format("Hexbin class: " +
                    "yColumn: '{0}' was not found in the table: '{1}'",
                    vgp.yColumn,
                    vgp.TableName));
                b_2Param = false;
            }
            if (string.IsNullOrEmpty(vgp.ImageType))
            {
                traceLog.Error("Hexbin class: 'image' was not found in the passed parameters");
                b_2Param = false;
            }

            if (string.IsNullOrEmpty(vgp.PlotFileName))
            {
                traceLog.Error("Hexbin class: 'plotFileName' was not found in the passed parameters");
                b_2Param = false;
            }

            return b_2Param;
        }

        protected void BinData()
        {
            string s_TmpTable = base.GetTemporaryTableName();
            string s_RStatement = string.Format("{0} <- data.frame({1})\n" +
                "bin <- hexbin(",
                s_TmpTable,
                vgp.TableName);

            if (vgp.AbsLogX)
            {
                s_RStatement += string.Format("abs(log(as.numeric({0}${1}), 10)), ",
                    s_TmpTable,
                    vgp.xColumn);
            }
            else
            {
                s_RStatement += string.Format("as.numeric({0}${1}), ",
                    s_TmpTable,
                    vgp.xColumn);
            }

            if (vgp.AbsLogY)
            {
                s_RStatement += string.Format("abs(log(as.numeric({0}${1}), 10)), ",
                    s_TmpTable,
                    vgp.yColumn);
            }
            else
            {
                s_RStatement += string.Format("as.numeric({0}${1}), ",
                    s_TmpTable,
                    vgp.yColumn);
            }

            s_RStatement += string.Format("xbins={0})\n" +
                "rm({1})",
                vgp.Bins,
                s_TmpTable);

            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Binning Data for Hexbin",
                this.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;
        }

        protected void CreatePlotFile()
        {
            string s_RStatement = "";
            if (Parameters.ContainsKey("image"))
            {
                if (Parameters["image"].Equals("eps"))
                {
                    s_RStatement += string.Format("postscript(filename=\"{0}\", width={1}," +
                        "height={2}, horizontal={3}, pointsize={4})\n",
                        vgp.PlotFileName,
                        vgp.Width,
                        vgp.Height,
                        vgp.Horizontal,
                        vgp.PointSize);
                }
                else if (Parameters["image"].Equals("png"))
                {
                    s_RStatement += string.Format("png(filename=\"" +

                        "{0}\", width={1}," +
                        "height={2}, pointsize={3})\n",
                        vgp.PlotFileName,
                        vgp.Width,
                        vgp.Height,
                        vgp.PointSize);
                }
                else if (Parameters["image"].Equals("jpg"))
                {
                    s_RStatement += string.Format("jpg(\"{0}\", width={1}," +
                        "height={2}, pointsize={3})\n",
                        vgp.PlotFileName,
                        vgp.Width,
                        vgp.Height,
                        vgp.PointSize);
                }

                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "Creating Hexbin Image File",
                    this.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;

                s_RStatement = "";

                s_RStatement += string.Format("plot(bin, xlab=\"{0}\", ylab=\"{1}\", main=\"{2}\", style=\"{3}\")\n",
                    vgp.xLabel,
                    vgp.yLabel,
                    vgp.Main,
                    "colorscale"); // can always come back and change up the style of the plot

                s_RStatement += "dev.off()\nrm(bin)";

                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "Saving Hexbin Plot",
                    this.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;
            }  
        }
        #endregion
    }
}
