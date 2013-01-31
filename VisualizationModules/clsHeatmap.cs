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
        #region Members
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        protected string s_RInstance;
        private VisualizationModules.clsVisualizationParameterHandler vgp =
            new VisualizationModules.clsVisualizationParameterHandler();
        private bool b_FilteredSuccessfully = false;
        #endregion

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
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsHeatmap(string InstanceOfR)
        {
            ModuleName = "Heatmap Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Modules that builds a heatmap
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsHeatmap(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            Model = TheCyclopsModel;            
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
            if (Model.SuccessRunningPipeline)
            {
                Model.IncrementStep(ModuleName);
                vgp.GetParameters(ModuleName, Parameters);

                if (CheckPassedParameters())
                {
                    //vgp.PlotDirectory = CreatePlotsFolder();
                    //vgp.ResetPlotFileName();

                    //if (vgp.Mode.ToLower().Equals("filterpvals"))
                    //{
                    //    FilterPvals();

                    //    if (b_FilteredSuccessfully)
                    //        BuildHeatmapPlot();
                    //}
                    //else
                    //{
                    //    if (vgp.HasImageType)
                    //    {
                    //        PrepareImageFile();
                    //    }

                    //    BuildHeatmapPlot();
                    //    if (vgp.HasImageType)
                    //    {
                    //        CleanUpImageFile();
                    //    }
                    //}
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
            if (!clsGenericRCalls.ContainsObject(s_RInstance, vgp.TableName))
            {
                traceLog.Error("Heatmap class: 'tableName' was not found in the R environment");
                b_2Param = false;
            }
            if (!vgp.HasPlotFileName)
            {
                traceLog.Error("ERROR Heatmap class: 'plotFileName' was not found in the passed parameters");
                b_2Param = false;
            }

            return b_2Param;
        }

        private bool CheckForFilterParameters()
        {
            bool b_2Param = true;
            if (string.IsNullOrEmpty(vgp.SignificanceTable))
            {
                traceLog.Error("Heatmap class: 'significanceTable' was not found in the passed parameters");
                b_2Param = false;
            }
            if (!clsGenericRCalls.ContainsObject(s_RInstance, vgp.SignificanceTable))
            {
                traceLog.Error("Heatmap class: 'significanceTable' was not found in the R environment");
                b_2Param = false;
            }
            if (string.IsNullOrEmpty(vgp.PValueColumn))
            {
                traceLog.Error("Heatmap class: 'pValueColumn' was not found in the passed parameters");
                b_2Param = false;
            }
            if (!clsGenericRCalls.TableContainsColumn(s_RInstance, vgp.SignificanceTable, vgp.PValueColumn))
            {
                traceLog.Error("Heatmap class: 'significanceTable' does not contain the column: " + 
                    vgp.PValueColumn);
                b_2Param = false;
            }
            if (string.IsNullOrEmpty(vgp.FilteredTableName))
            {
                traceLog.Error("Heatmap class: 'filteredTableName' was not found in the passed parameters");                    
                b_2Param = false;
            }
            if (vgp.HeatmapColors.StartsWith("\"") &
                vgp.HeatmapColors.EndsWith("\""))
            {
                vgp.HeatmapColors = vgp.HeatmapColors.Substring(1, vgp.HeatmapColors.Length - 2);
            }
            
            return b_2Param;
        }

        private void LoadLibraries()
        {
            // load the necessary packages
            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "grDevices"))
                clsGenericRCalls.InstallPackage(s_RInstance, "grDevices");
            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "gplots"))
                clsGenericRCalls.InstallPackage(s_RInstance, "gplots");

            string s_RStatement = "require(gplots)\nrequire(grDevices)\n";
            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Loading Libraries for Constructing Heatmap",
                this.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;
        }

        /// <summary>
        /// Filters the input table based on p-values from a significance table
        /// based on a threshold p-value, so only significant features are plotted
        /// The'TableName' parameter becomes the new 'significant' table of proteins  
        /// </summary>
        private void FilterPvals()
        {
            if (CheckForFilterParameters())
            {
                string s_RStatement = string.Format(
                    "{0} <- {1}[{1}[,'{2}'] < {3},]\n"+
                    "{0} <- {4}[which(rownames({4})%in%rownames({0})),]\n",
                    vgp.FilteredTableName,
                    vgp.SignificanceTable,
                    vgp.PValueColumn,
                    vgp.PValue,
                    vgp.TableName);

                b_FilteredSuccessfully = clsGenericRCalls.Run(
                    s_RStatement,
                    s_RInstance, "Filtering For P-value < " +
                    vgp.PValue, this.StepNumber,
                    Model.NumberOfModules);
            }
        }

        private void PrepareImageFile()
        {
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

                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "Saving Heatmap",
                    this.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;
            }
        }

        private void BuildHeatmapPlot()
        {
            string s_RStatement = string.Format(
                "myColorRamp <- colorRampPalette({0})\n",
                vgp.HeatmapColors);
            s_RStatement += string.Format(
                "cmap <- myColorRamp({0})\n",
                vgp.HeatmapColorScaleDegree);
            s_RStatement += string.Format(
                "colscale <- seq({0}, {1}, length={2}+1)\n",
                vgp.HeatmapScaleMin,
                vgp.HeatmapScaleMax,
                vgp.HeatmapColorScaleDegree);
            //s_RStatement += string.Format("hm_{0} <- heatmap.2(x=data.matrix({0})," +
            //"Rowv={1}, Colv={2}, distfun={3}, hclustfun={4}, dendrogram={5}," +
            //"symm={6}, scale={7}, na.rm={8}, col=cmap, trace={9}, " +
            //"breaks=colscale, main={10})\n",
            //    vgp.TableName,              // 0
            //    vgp.HeatmapRowDendrogram,   // 1
            //    vgp.HeatmapColDendrogram,   // 2
            //    vgp.HeatmapDist,            // 3
            //    vgp.HeatmapClusterFunction, // 4
            //    vgp.HeatmapDrawDendrogram,  // 5
            //    vgp.HeatmapSymm,            // 6
            //    vgp.HeatmapScale,           // 7
            //    vgp.HeatmapRemoveNA,        // 8 
            //    vgp.HeatmapTrace,           // 9
            //    vgp.Main);                  // 10

            s_RStatement += string.Format(
                "{0} <- jnb_Heatmap(x={1}, " +
                "file={2}, clusterRows={3}, " +
                "clusterCols={4}, distFunc={5}, " +
                "hclustFunc={6}, " +
                "bkground={7}, " +
                "IMGwidth={8}, IMGheight={9}, " +
                "FNTsize={10}, plotRes={11}, title={12}, " +
                "plotScale={13}, plotMargins={14}, " +
                "plotTrace={15}, labRow={16}, " +
                "nullReplacement={17}, " +
                "zeroReplacement={18}, " +
                "plotBreaks=colscale, " +
                "colMap=cmap)\n",
                !string.IsNullOrEmpty(vgp.HeatmapResultsTableName) ?
                    vgp.HeatmapResultsTableName :
                    "hm_" + vgp.TableName,                  // 0
                vgp.Mode.ToLower().Equals("filterpvals") ?
                    vgp.FilteredTableName : vgp.TableName,  // 1
                "'" + vgp.PlotFileName + "'",               // 2
                vgp.HeatmapClusterRows,                     // 3
                vgp.HeatmapClusterColumns,                  // 4
                vgp.HeatmapDist,                            // 5
                vgp.HeatmapClusterFunction,                 // 6
                "'" + vgp.BackgroundColor + "'",            // 7
                vgp.Width,                                  // 8
                vgp.Height,                                 // 9
                vgp.FontSize,                               // 10
                vgp.Resolution,                             // 11
                "'" + vgp.Main + "'",                       // 12
                "'" + vgp.HeatmapScale + "'",               // 13
                vgp.Margin,                                 // 14
                "'" + vgp.HeatmapTrace + "'",               // 15
                vgp.HeatmapIncludeRowLabels,                // 16
                vgp.NullReplacement,                        // 17
                vgp.ZeroReplacement                         // 18
                );

            s_RStatement += "rm(myColorRamp)\nrm(cmap)";

            clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Constructing Heatmap",
                this.StepNumber, Model.NumberOfModules);

            //if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
            //    "Constructing Heatmap",
            //    this.StepNumber, Model.NumberOfModules))
            //    Model.SuccessRunningPipeline = false;
        }

        private void CleanUpImageFile()
        {            
            string s_RStatement = "dev.off()";

            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Cleaning up Heatmap Analysis",
                this.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;
        }
        #endregion
    }
}
