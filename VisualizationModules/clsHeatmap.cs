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
using log4net;

namespace Cyclops.VisualizationModules
{
    public class clsHeatmap : clsBaseVisualizationModule
    {
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        protected string s_RInstance;
        private VisualizationModules.clsVisualizationParameterHandler vgp =
            new VisualizationModules.clsVisualizationParameterHandler();

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
            vgp.Parameters = Parameters;

            if (CheckPassedParameters())
            {
                CreatePlotsFolder();

                if (clsGenericRCalls.ContainsObject(s_RInstance, vgp.TableName))
                {
                    if (vgp.HasImageType)
                    {
                        PrepareImageFile();
                    }
                    BuildHeatmapPlot();
                    if (vgp.HasImageType)
                    {
                        CleanUpImageFile();
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
            // NON-NECESSARY PARAMETERS
            if (!vgp.HasTableName)
            {
                traceLog.Error("Heatmap class: 'tableName' was not found in the passed parameters");
                b_2Param = false;
            }

            return b_2Param;
        }

        private void LoadLibraries()
        {
            traceLog.Info("Heatmap Module: Loading Required Libraries...");
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            // load the necessary packages
            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "grDevices"))
                clsGenericRCalls.InstallPackage(s_RInstance, "grDevices");
            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "gplots"))
                clsGenericRCalls.InstallPackage(s_RInstance, "gplots");

            try
            {
                string s_RStatement = "require(gplots)\nrequire(grDevices)\n";
                                
                traceLog.Info(s_RStatement);
                engine.EagerEvaluate(s_RStatement);
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR Heatmap Module while loading libraries: " + exc.ToString());
            }
        }

        private void PrepareImageFile()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            string s_RStatement = "";
            if (vgp.HasImageType)
            {
                if (vgp.ImageType.Equals("eps"))
                {
                    s_RStatement += string.Format("postscript(\"{0}\", width={1}," +
                        "height={2}, horizontal={3}, pointsize={4})\n",
                        vgp.PlotFileName,
                        vgp.Width,
                        vgp.Height,
                        vgp.Horizontal,
                        vgp.PointSize);
                }
                else if (vgp.ImageType.Equals("png"))
                {
                    s_RStatement += string.Format("png(\"" +

                        "{0}\", width={1}," +
                        "height={2}, pointsize={3})\n",
                        vgp.PlotFileName,
                        vgp.Width,
                        vgp.Height,
                        vgp.PointSize);
                }
                else if (vgp.ImageType.Equals("jpg"))
                {
                    s_RStatement += string.Format("jpg(\"{0}\", width={1}," +
                        "height={2}, pointsize={3})\n",
                        vgp.PlotFileName,
                        vgp.Width,
                        vgp.Height,
                        vgp.PointSize);
                }

                try
                {
                    traceLog.Info("Heatmap Module: Preparing image file: " + s_RStatement);
                    engine.EagerEvaluate(s_RStatement);
                }
                catch (Exception exc)
                {
                    traceLog.Error("ERROR Heatmap Module while preparing image file: " + exc.ToString());
                }
            }
        }

        private void BuildHeatmapPlot()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            string s_RStatement = string.Format(
                "myColorRamp <- colorRampPalette({0})\n",
                vgp.HeatmapColors);
            s_RStatement += string.Format(
                "cmap <- myColorRamp({0})\n",
                vgp.HeatmapColorScaleDegree);
            s_RStatement += string.Format(
                "cmap <- seq({0}, {1}, length={2}+1)\n",
                vgp.HeatmapScaleMin,
                vgp.HeatmapScaleMax,
                vgp.HeatmapColorScaleDegree);
            s_RStatement += string.Format("hm_{0} <- heatmap.2(x=data.matrix({0})," +
            "Rowv={1}, Colv={2}, distfun={3}, hclustfun={4}, dendrogram={5}," +
            "symm={6}, scale={7}, na.rm={8}, col=cmap, trace={9}, " +
            "breaks=colscale, main={10})\n",
                vgp.TableName,              // 0
                vgp.HeatmapRowDendrogram,   // 1
                vgp.HeatmapColDendrogram,   // 2
                vgp.HeatmapDist,            // 3
                vgp.HeatmapClusterFunction, // 4
                vgp.HeatmapDrawDendrogram,  // 5
                vgp.HeatmapSymm,            // 6
                vgp.HeatmapScale,           // 7
                vgp.HeatmapRemoveNA,        // 8 
                vgp.HeatmapTrace,           // 9
                vgp.Main);                  // 10

            s_RStatement += "rm(myColorRamp)\nrm(cmap)";

            try
            {
                traceLog.Info("Heatmap Module building heatmap plot: " + s_RStatement);
                engine.EagerEvaluate(s_RStatement);
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR Heatmap Module building heatmap plot: " + exc.ToString());
            }
        }

        private void CleanUpImageFile()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            string s_RStatement = "dev.off()";

            try
            {
                traceLog.Info("Heatmap Module: Cleaning up Image File: " + s_RStatement);
                engine.EagerEvaluate(s_RStatement);
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR Heatmap Module during cleaning up image file: " + exc.ToString());
            }
        }
        #endregion
    }
}
