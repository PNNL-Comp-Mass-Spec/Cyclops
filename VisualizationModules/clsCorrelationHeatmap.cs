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
using System.Linq;
using System.Text;

using log4net;
using RDotNet;

namespace Cyclops.VisualizationModules
{
    /// <summary>
    /// Produces correlation matrices, significant matrices, and correlation heatmap
    /// </summary>
    public class clsCorrelationHeatmap : clsBaseVisualizationModule
    {
        #region Members
        protected string s_RInstance;
        private VisualizationModules.clsVisualizationParameterHandler vgp =
            new VisualizationModules.clsVisualizationParameterHandler();

        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Constructors
        /// <summary>
        /// Module to construct a correlation heatmap
        /// </summary>
        public clsCorrelationHeatmap()
        {
            ModuleName = "Correlation Heatmap Module";
        }

        /// <summary>
        /// Module to construct a correlation heatmap
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsCorrelationHeatmap(string InstanceOfR)
        {
            ModuleName = "Correlation Heatmap Module";
            s_RInstance = InstanceOfR;
        }

        /// <summary>
        /// Module to construct a correlation heatmap
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsCorrelationHeatmap(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            Model = TheCyclopsModel;            
            ModuleName = "Correlation Heatmap Module";            
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

                traceLog.Info("Producing Correlation Heatmap...");

                if (CheckPassedParameters())
                {
                    CreatePlotsFolder();

                    if (clsGenericRCalls.ContainsObject(s_RInstance, vgp.TableName))
                    {
                        LoadLibraries();
                        RunCorrelationAnalysis();
                        CreatePlotFile();
                    }
                    else
                    {
                        traceLog.Error("ERROR Correlation Heatmap module: " +
                            vgp.TableName + " table was not found in the R workspace.\n" +
                            "The Correlation Plot analysis can not continue without this " +
                            "table present.");
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
            if (!vgp.HasTableName)
            {
                traceLog.Error("Correlation Heatmap class: 'tableName' was not found in the passed parameters");
                b_2Param = false;
            }

            if (!vgp.HasImageType)
            {
                traceLog.Error("Correlation Heatmap class: 'image' was not found in the passed parameters");
                b_2Param = false;
            }

            if (!vgp.HasPlotFileName)
            {
                traceLog.Error("Correlation Heatmap class: 'plotFileName' was not found in the passed parameters");
                b_2Param = false;
            }

            if (string.IsNullOrEmpty(vgp.CorrelationListName))
            {
                traceLog.Error("ERROR Correlation Heatmap class: 'correlationListName' was not found in the passed parameters");
                b_2Param = false;
            }

            return b_2Param;
        }

        private void LoadLibraries()
        {
            string s_RStatement = "require(Hmisc)\n" +
                "require(gplots)\nrequire(grDevices)\n";

            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Loading Libraries for Correlation Heatmap",
                Model.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;
        }

        private void RunCorrelationAnalysis()
        {
            string s_TemporaryTableName = GetTemporaryTableName();

            string s_RStatement = string.Format(
                "require(Hmisc)\n" +
                "{0} <- rcorr(data.matrix({1}{2}){3})\n" +
                "{4} <- list(cor={0}$r, n={0}$n, prob={0}$P)\n" +
                "rm({0})\n",
                s_TemporaryTableName,                
                vgp.TableName,
                vgp.SkipTheFirstColumn.ToLower().Equals("true") ? "[,-1]" : "",
                !string.IsNullOrEmpty(vgp.Type) ? ", type=c(\"" + vgp.Type + "\")" : "",
                vgp.CorrelationListName);

            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Performing Correlation Analysis",
                Model.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;
        }

        /// <summary>
        /// Constructs and Runs the R statement to save the heatmap plot
        /// </summary>
        private void CreatePlotFile()
        {
            string s_RStatement = "";

            string s_TmpPath = Path.Combine(vgp.WorkDir, "Plots", Path.GetFileName(vgp.PlotFileName));

            if (Parameters.ContainsKey("image"))
            {
                if (Parameters["image"].Equals("eps"))
                {
                    s_RStatement += string.Format("postscript(filename=\"{0}\", width={1}," +
                        "height={2}, horizontal={3}, pointsize={4})\n",
                        s_TmpPath,
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
                        s_TmpPath,
                        vgp.Width,
                        vgp.Height,
                        vgp.PointSize);
                }
                else if (Parameters["image"].Equals("jpg"))
                {
                    s_RStatement += string.Format("jpg(filename=\"{0}\", width={1}," +
                        "height={2}, pointsize={3})\n",
                        s_TmpPath,
                        vgp.Width,
                        vgp.Height,
                        vgp.PointSize);
                }

                s_RStatement += GetHeatmapStatement();

                s_RStatement += "dev.off()\n";
                s_RStatement = s_RStatement.Replace("\\", "/");

                if (Directory.Exists(Path.GetDirectoryName(vgp.PlotFileName)))
                    traceLog.Info(Path.GetDirectoryName(vgp.PlotFileName) + " exists, and available to be written to...");
                else
                    traceLog.Error(Path.GetDirectoryName(vgp.PlotFileName) + " DOES NOT exist!");

                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "Saving Correlation Heatmap",
                    Model.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;
            }
        }

        /// <summary>
        /// Constructs the R statement that produces the heatmap
        /// </summary>
        /// <returns>R Statement</returns>
        private string GetHeatmapStatement()
        {
            string s_Return = "BlueRed <- colorRampPalette(c(\"blue\", \"white\", \"red\"))\n"; // color Ramp
            s_Return += "cmap <- BlueRed(20)\n";
            s_Return += string.Format("heatmap.2({0}$cor, " +
                "main=\"{1}\", " +
                "Rowv=F, " +
                "Colv=F, " +
                "dendrogram=c(\"none\"), " +
                "col=cmap, " + 
                "trace=c(\"none\"), " +
                "scale=c(\"none\"), " + 
                "margins=c(10,10))\n",
                vgp.CorrelationListName,
                vgp.Main);
            return s_Return;
        }
        #endregion
    }
}
