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
    /// <summary>
    /// Plots a boxplot using the BoxPlot function
    /// 
    /// Parameters include:
    /// tableName:      Name of table in R workspace as source for plot
    /// plotFileName:   Name of the file to return in the Plots directory
    /// 
    /// backgroundColor:    Color of plot background        Defaults to white
    /// width:              Width of the plot in pixels     Defaults to 1200
    /// height:             Height of the plot in pixels    Defaults to 1200
    /// fontSize:           Size of font text in plot       Defaults to 12
    /// resolution:         Resolution of plot              Defaults to 600
    /// TODO: ADD THE OTHER PARAMTERS
    /// </summary>
    public class clsBoxPlot : clsBaseVisualizationModule
    {
        #region Members
        protected string s_RInstance;
        VisualizationModules.clsVisualizationParameterHandler vgp = 
            new VisualizationModules.clsVisualizationParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Constructors
        /// <summary>
        /// Develops a boxplot from a table
        /// </summary>
        public clsBoxPlot()
        {
            ModuleName = "Boxplot Module";
        }
        /// <summary>
        /// Develops a boxplot from a table
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsBoxPlot(string InstanceOfR)
        {
            ModuleName = "Boxplot Module";
            s_RInstance = InstanceOfR;            
        }
        /// <summary>
        /// Develops a boxplot from a table
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsBoxPlot(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Boxplot Module";
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
                    vgp.PlotDirectory = CreatePlotsFolder();
                    vgp.ResetPlotFileName();

                    if (clsGenericRCalls.ContainsObject(s_RInstance, vgp.TableName))
                    {
                        // Perform Boxplot
                        CreateBoxPlot();
                    }
                    else
                    {
                        traceLog.Error("ERROR Boxplot class: " + vgp.TableName + " not found in the R workspace.");
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

            //NECESSARY PARAMETERS
            if (!vgp.HasTableName)
            {
                traceLog.Error("ERROR Boxplot class: 'tableName' was not found in the passed parameters");
                b_2Param = false;
                Model.SuccessRunningPipeline = false;
            }
            if (!vgp.HasPlotFileName)
            {
                traceLog.Error("ERROR Boxplot class: 'plotFileName' was not found in the passed parameters");
                b_2Param = false;
                Model.SuccessRunningPipeline = false;
            }

            // if the table does not exist, do not throw an cyclops error, but do not plot boxplots
            // this is because Linear Regression only runs if there is a consolidation factor
            // if the linear regression table does not exist, then don't run boxplot
            if (!clsGenericRCalls.ContainsObject(s_RInstance, vgp.TableName))
            {
                //Model.SuccessRunningPipeline = false;
                traceLog.Info("ERROR Boxplot class: '" + vgp.TableName +
                    "' was not found in the R workspace!");
                b_2Param = false;
            }

            return b_2Param;
        }

        private void CreateBoxPlot()
        {
            string s_RStatement = "";

            // If a Factor to color by has been chosen, make sure that the
            // Factor Table contains the column.
            bool b_FactorIsPresent = true;
            if (!vgp.FixedEffect.Equals(""))
            {
                b_FactorIsPresent = clsGenericRCalls.TableContainsColumn(s_RInstance, vgp.ColumnFactorTable, vgp.FixedEffect);
                if (!b_FactorIsPresent)
                {
                    traceLog.Error("ERROR in BoxPlot: The factor table (" + vgp.ColumnFactorTable + ") " +
                        "does NOT contain the factor " + vgp.FixedEffect);
                    Model.SuccessRunningPipeline = false;
                }
            }
                        
            string s_TmpTable = GetTemporaryTableName();
            if (vgp.SkipTheFirstColumn.ToLower().Equals("true"))
            {
                s_RStatement += string.Format("{0} <- {1}[,-1]\n",
                    s_TmpTable,
                    vgp.TableName);
            }
            else
            {
                s_RStatement += string.Format("{0} <- {1}\n",
                    s_TmpTable,
                    vgp.TableName);
            }

            s_RStatement += string.Format("Boxplots(x={0}, Columns={1}, " +
                "file=\"{2}\", colorByFactor={3}, colorFactorTable={4}, " +
                "colorFactorName={5}, " +
                "outliers={6}, color=\"{7}\", bkground=\"{8}\", labelscale={9}, " +
                "boxwidth={10}, showcount={11}, showlegend={12}, stamp={13}, " +
                "do.ylim={14}, ymin={15}, ymax={16}, ylabel=\"{17}\", " +
                "IMGwidth={18}, IMGheight={19}, FNTsize={20}, res={21})\n" +
                "rm({0})\n",
                s_TmpTable,                                             // 0
                vgp.DataColumns.Length > 0 ? "\"" + vgp.DataColumns + "\"" : "NULL",  // 1
                vgp.PlotFileName,                                       // 2
                vgp.ColorByFactor,                                      // 3
                vgp.ColumnFactorTable,                                  // 4
                vgp.ConsolidationFactor.Length > 0 ? "\"" + vgp.ConsolidationFactor + "\"": "NULL",  // 5
                vgp.Outliers,                                           // 6
                vgp.Color,                                              // 7
                vgp.BackgroundColor,                                    // 8
                vgp.LabelScale,                                         // 9
                vgp.BoxWidth,                                           // 10
                vgp.ShowCount,                                          // 11
                vgp.ShowLegend,                                         // 12
                vgp.Stamp,                                              // 13
                vgp.DoYLim,                                             // 14
                vgp.yMin,                                               // 15
                vgp.yMax,                                               // 16
                vgp.yLabel,                                             // 17
                vgp.Width,                                              // 18
                vgp.Height,                                             // 19
                vgp.FontSize,                                           // 20
                vgp.Resolution);                                        // 21


            if (Directory.Exists(Path.GetDirectoryName(vgp.PlotFileName)))
                traceLog.Info(Path.GetDirectoryName(vgp.PlotFileName) + " exists, and available to be written to...");
            else
                traceLog.Error(Path.GetDirectoryName(vgp.PlotFileName) + " DOES NOT exist!");

            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Performing Boxplot",
                Model.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;
        }
        #endregion
    }
}
