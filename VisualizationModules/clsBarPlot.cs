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

using RDotNet;
using log4net;

namespace Cyclops.VisualizationModules
{
    public class clsBarPlot : clsBaseVisualizationModule
    {
        #region Members
        protected string s_RInstance;
        VisualizationModules.clsVisualizationParameterHandler vgp = 
            new VisualizationModules.clsVisualizationParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Constructors
        /// <summary>
        /// Develops a barplot from a table
        /// </summary>
        public clsBarPlot()
        {
            ModuleName = "Barplot Module";
        }
        /// <summary>
        /// Develops a barplot from a table
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsBarPlot(string InstanceOfR)
        {
            ModuleName = "Barplot Module";
            s_RInstance = InstanceOfR;            
        }
        /// <summary>
        /// Develops a barplot from a table
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsBarPlot(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Barplot Module";
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
            vgp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                vgp.PlotDirectory = CreatePlotsFolder();
                vgp.ResetPlotFileName();

                if (clsGenericRCalls.ContainsObject(s_RInstance, vgp.TableName))
                {
                    // Perform Barplot
                    CreateBarPlot();
                }
                else
                {
                    traceLog.Error("ERROR Barplot class: " + vgp.TableName + " not found in the R workspace.");
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

            //NECESSARY PARAMETERS
            if (!vgp.HasTableName)
            {
                traceLog.Error("ERROR Barplot class: 'tableName' was not found in the passed parameters");
                b_2Param = false;
                Model.SuccessRunningPipeline = false;
            }
            if (!vgp.HasPlotFileName)
            {
                traceLog.Error("ERROR Barplot class: 'plotFileName' was not found in the passed parameters");
                b_2Param = false;
                Model.SuccessRunningPipeline = false;
            }

            return b_2Param;
        }

        private void CreateBarPlot()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            string s_RStatement = "";

            if (string.IsNullOrEmpty(vgp.Mode))
            {
                s_RStatement += string.Format("plotBars(" +
                    "x={0}, Data.Column=\"{1}\", " +
                    "file=\"{2}\", " +
                    "bkground=\"{3}\", " +
                    "takeLog={4}, " +
                    "base={5}, " +
                    "names.arg=\"{6}\", " +
                    "xLab=\"{7}\", " +
                    "yLab=\"{8}\", " +
                    "title=\"{9}\", " +
                    "col={10}, " +
                    "IMGwidth={11}, " +
                    "IMGheight={12}, " +
                    "FNTsize={13}, " +
                    "res={14})",
                    vgp.TableName,                                          // 0
                    vgp.DataColumns,                                        // 1
                    vgp.PlotFileName,                                       // 2
                    vgp.BackgroundColor,                                    // 3
                    vgp.Log,                                                // 4
                    vgp.LogBase,                                            // 5
                    vgp.Names,                                              // 6
                    vgp.xLabel,                                             // 7
                    vgp.yLabel,                                             // 8
                    vgp.Main,                                               // 9
                    vgp.BarColor.Equals("NULL") ? "NULL" : "\"" + vgp.BarColor + "\"",// 10
                    vgp.Width,                                              // 11
                    vgp.Height,                                             // 12
                    vgp.FontSize,                                           // 13
                    vgp.Resolution                                          // 14
                    );
            }
            else if (vgp.Mode.Equals("iterator"))
            {
                string s_TmpTable = GetTemporaryTableName();

                s_RStatement += string.Format("{0} <- " +
                    "data.frame(Cleavage=c(\"Tryptic\", " +
                    "\"Partial\", \"NonTryptic\"), " +
                    "Frequency=c(sum({1}$Tryptic), " +
                    "sum({1}$PartTryptic), " +
                    "sum({1}$NonTryptic)))\n\n",
                    s_TmpTable,
                    vgp.TableName);

                s_RStatement += string.Format("plotBars(" +
                    "x={0}, Data.Column=\"{1}\", " +
                    "file=\"{2}\", " +
                    "bkground=\"{3}\", " +
                    "takeLog={4}, " +
                    "base={5}, " +
                    "names.arg=\"{6}\", " +
                    "xLab=\"{7}\", " +
                    "yLab=\"{8}\", " +
                    "title=\"{9}\", " +
                    "col={10}, " +
                    "IMGwidth={11}, " +
                    "IMGheight={12}, " +
                    "FNTsize={13}, " +
                    "res={14})\n" +
                    "rm({0})\n",
                    s_TmpTable,                                          // 0
                    vgp.DataColumns,                                        // 1
                    vgp.PlotFileName,                                       // 2
                    vgp.BackgroundColor,                                    // 3
                    vgp.Log,                                                // 4
                    vgp.LogBase,                                            // 5
                    vgp.Names,                                              // 6
                    vgp.xLabel,                                             // 7
                    vgp.yLabel,                                             // 8
                    vgp.Main,                                               // 9
                    vgp.BarColor.Equals("NULL") ? "NULL" : "\"" + vgp.BarColor + "\"",// 10
                    vgp.Width,                                              // 11
                    vgp.Height,                                             // 12
                    vgp.FontSize,                                           // 13
                    vgp.Resolution                                          // 14
                    );
            }
                        
            try
            {
                traceLog.Info("Performing Barplot: " + s_RStatement);
                if (Directory.Exists(Path.GetDirectoryName(vgp.PlotFileName)))
                    traceLog.Info(Path.GetDirectoryName(vgp.PlotFileName) + " exists, and available to be written to...");
                else
                    traceLog.Error(Path.GetDirectoryName(vgp.PlotFileName) + " DOES NOT exist!");

                    engine.EagerEvaluate(s_RStatement);

                if (File.Exists(vgp.PlotFileName))
                    traceLog.Info("Barplot was written out to: " + vgp.PlotFileName);
                else
                    traceLog.Error("Unable to find the plot file: " + vgp.PlotFileName);
                FileInfo fi = new FileInfo(vgp.PlotFileName);
                if (fi.Length > 0)
                    traceLog.Info(vgp.PlotDirectory + " contains data: " + fi.Length);
                else
                    traceLog.Error(vgp.PlotDirectory + " is empty!");
            }
            catch (Exception exc)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR Performing Barplot: " + exc.ToString());
            }
        }
        #endregion
    }
}
