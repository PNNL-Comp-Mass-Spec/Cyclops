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
using System.Linq;
using System.Text;

using RDotNet;
using log4net;

namespace Cyclops.VisualizationModules
{
    public class clsBoxPlot : clsBaseVisualizationModule
    {
        protected string s_RInstance;
        VisualizationModules.clsVisualizationParameterHandler vgp = 
            new VisualizationModules.clsVisualizationParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

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
            vgp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                CreatePlotsFolder();

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
            }
            if (!vgp.HasPlotFileName)
            {
                traceLog.Error("ERROR Boxplot class: 'plotFileName' was not found in the passed parameters");
                b_2Param = false;
            }

            return b_2Param;
        }

        private void CreateBoxPlot()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            string s_RStatement = "";

            s_RStatement += string.Format("Boxplots(x={0}, Columns={1}, " +
                "file=\"{2}\", colorByFactor={3}, colorFactor={4}${5}, " +
                "outliers={6}, color=\"{7}\", bkground=\"{8}\", labelscale={9}, " +
                "boxwidth={10}, showcount={11}, showlegend={12}, stamp={13}, " +
                "do.ylim={14}, ymin={15}, ymax={16}, ylabel=\"{17}\", " +
                "IMGwidth={18}, IMGheight={19}, FNTsize={20}, res={21})",
                vgp.TableName,                      // 0
                vgp.DataColumns,                    // 1
                vgp.WorkDir + "/" + vgp.FileName,   // 2
                vgp.ColorByFactor,                  // 3
                vgp.ColumnFactorTable,              // 4
                vgp.FixedEffect,                    // 5
                vgp.Outliers,                       // 6
                vgp.Color,                          // 7
                vgp.BackgroundColor,                // 8
                vgp.LabelScale,                     // 9
                vgp.BoxWidth,                       // 10
                vgp.ShowCount,                      // 11
                vgp.ShowLegend,                     // 12
                vgp.Stamp,                          // 13
                vgp.DoYLim,                         // 14
                vgp.yMin,                           // 15
                vgp.yMax,                           // 16
                vgp.yLabel,                         // 17
                vgp.Width,                          // 18
                vgp.Height,                         // 19
                vgp.FontSize,                       // 20
                vgp.Resolution);                    // 21
                        
            try
            {
                traceLog.Info("Performing Boxplot: " + s_RStatement);
                traceLog.Info("Working directory: " + clsGenericRCalls.GetWorkingDirectory(s_RInstance));
                engine.EagerEvaluate(s_RStatement);
            }
            catch (Exception exc)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR Performing Boxplot: " + exc.ToString());
            }
        }
        #endregion
    }
}
