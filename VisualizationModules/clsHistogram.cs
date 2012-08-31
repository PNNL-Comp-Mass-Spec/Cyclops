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
    /// Constructs histogram plots and saves the image file
    /// </summary>
    public class clsHistogram : clsBaseVisualizationModule
    {
        #region Members
        protected string s_RInstance;
        VisualizationModules.clsVisualizationParameterHandler vgp = 
            new VisualizationModules.clsVisualizationParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        public clsHistogram()
        {
            ModuleName = "Histogram Module";
        }
        /// <summary>
        /// Basic constructor that passes in the R workspace
        /// </summary>
        /// <param name="InstanceOfR">Instance of R Workspace</param>
        public clsHistogram(string InstanceOfR)
        {
            ModuleName = "Histogram Module";
            s_RInstance = InstanceOfR;            
        }
        /// <summary>
        /// Module plots a histogram graph to the Plots directory
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsHistogram(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Histogram Module";
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
                        CreateHistogram();
                    }
                    else
                    {
                        traceLog.Error("ERROR Histogram class: " + vgp.TableName + " not found in the R workspace.");
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
                traceLog.Error("ERROR Histogram class: 'tableName' was not found in the passed parameters");
                b_2Param = false;
            }
            if (!vgp.HasPlotFileName)
            {
                traceLog.Error("ERROR Histogram class: 'plotFileName' was not found in the passed parameters");
                b_2Param = false;
            }

            return b_2Param;
        }



        private void CreateHistogram()
        {
            string s_RStatement = "";

            switch (vgp.HistogramType)
            {
                case "standard":
                    s_RStatement = string.Format("plot_hist(Data={0}, " +
                         "file=\"{1}\", Data.Columns={2}, " +
                         "IMGwidth={3}, " +
                         "IMGheight={4}, FNTsize={5}, colF=\"{6}\", colB=\"{7}\")",
                         vgp.TableName,
                         vgp.PlotFileName,
                         vgp.DataColumns,
                         vgp.Width,
                         vgp.Height,
                         vgp.FontSize,
                         vgp.BarColor,
                         vgp.BackgroundColor);
                    break;
            }

            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Performing Histogram",
                Model.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;
        }
        #endregion
    }
}
