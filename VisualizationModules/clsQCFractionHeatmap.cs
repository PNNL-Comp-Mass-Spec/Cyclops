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
    /// <summary>
    /// Plots QC Fractions heatmap by calling the ja_QCFractionsHeat function in QualityControl.R
    /// 
    /// Parameters include:
    /// tableName:          Name of table in R workspace as source for plot
    /// plotFileName:       Name of the file to return in the Plots directory
    /// 
    /// backgroundColor:    Color of plot background        Defaults to white
    /// width:              Width of the plot in pixels     Defaults to 1200
    /// height:             Height of the plot in pixels    Defaults to 1200
    /// fontSize:           Size of font text in plot       Defaults to 12
    /// resolution:         Resolution of plot              Defaults to 600
    /// </summary>
    public class clsQCFractionHeatmap : clsBaseVisualizationModule
    {
        #region Members
        protected string s_RInstance;
        VisualizationModules.clsVisualizationParameterHandler vgp = 
            new VisualizationModules.clsVisualizationParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Constructors
        /// <summary>
        /// Plots the QC fraction heatmap
        /// </summary>
        public clsQCFractionHeatmap()
        {
            ModuleName = "QC Fraction Heatmap Module";
        }
        /// <summary>
        /// Plots the QC fraction heatmap
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsQCFractionHeatmap(string InstanceOfR)
        {
            ModuleName = "QC Fraction Heatmap Module";
            s_RInstance = InstanceOfR;            
        }
        /// <summary>
        /// Plots the QC fraction heatmap
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsQCFractionHeatmap(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "QC Fraction Heatmap Module";
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
                        RunHeatmap();
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

            return b_2Param;
        }

        private void RunHeatmap()
        {
            string s_RStatement = string.Format("ja_QCFractionsHeat(zz={0}$myMatrix1, " +
                "file=\"{1}\", bkground=\"{2}\", IMGwidth={3}, IMGheight={4}, " +
                "FNTsize={5}, res={6})",
                vgp.TableName,
                vgp.PlotFileName,
                vgp.BackgroundColor,
                vgp.Width,
                vgp.Height,
                vgp.FontSize,
                vgp.Resolution);

            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Performing QC Fraction Heatmap",
                Model.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;
        }
        #endregion
    }
}
