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
    /// Constructs histogram plots and saves the image file
    /// </summary>
    public class clsHistogram : clsBaseVisualizationModule
    {
        protected string s_RInstance, s_TableName="",
            s_NewFileName="", s_Width="1200", s_Height="1200",
            s_DataColumns="", s_Background="white", 
            s_AddRug="TRUE", s_AddDist="FALSE", 
            s_FontSize="12", s_HistType="standard", s_FileType="",
            s_ColorBars = "89c6ff", s_ColorBackground = "5FAE27";
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

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
        #endregion

        #region Properties
        
        #endregion

        #region Methods
        /// <summary>
        ///  Runs module
        /// </summary>
        public override void PerformOperation()
        {
            if (CheckPassedParameters())
            {
                CreatePlotsFolder();

                if (clsGenericRCalls.ContainsObject(s_RInstance, s_TableName))
                {
                    CreateHistogram();
                }
                else
                {
                    traceLog.Error("ERROR Histogram class: " + s_TableName + " not found in the R workspace.");
                }
            }
        }

        /// <summary>
        /// Checks the dictionary to ensure all the necessary parameters are present
        /// </summary>
        /// <returns>True if all necessary parameters are present</returns>
        protected bool CheckPassedParameters()
        {
            // NON-NECESSARY PARAMETERS
            if (Parameters.ContainsKey("width"))
            {
                s_Width = Parameters["width"];
            }
            else
            {
                traceLog.Warn("Histogram class: 'width' was not found in the passed parameters, continuing the process...");
            }
            if (Parameters.ContainsKey("height"))
            {
                s_Height = Parameters["height"];
            }
            else
            {
                traceLog.Warn("Histogram class: 'height' was not found in the passed parameters, continuing the process...");
            }
            if (Parameters.ContainsKey("addRug"))
            {
                s_AddRug = Parameters["addRug"];
            }
            else
            {
                traceLog.Warn("Histogram class: 'addRug' was not found in the passed parameters, continuing the process...");
            }
            if (Parameters.ContainsKey("addDist"))
            {
                s_AddDist = Parameters["addDist"];
            }
            else
            {
                traceLog.Warn("Histogram class: 'addDist' was not found in the passed parameters, continuing the process...");
            }
            if (Parameters.ContainsKey("colorBars"))
            {
                s_ColorBars = Parameters["colorBars"];
            }
            else
            {
                traceLog.Warn("Histogram class: 'colorBars' was not found in the passed parameters, continuing the process...");
            }
            if (Parameters.ContainsKey("colorBackground"))
            {
                s_ColorBackground = Parameters["colorBackground"];
            }
            else
            {
                traceLog.Warn("Histogram class: 'colorBackground' was not found in the passed parameters, continuing the process...");
            }

            //NECESSARY PARAMETERS
            if (Parameters.ContainsKey("tableName"))
            {
                s_TableName = Parameters["tableName"];
            }
            else
            {
                traceLog.Error("ERROR Histogram class: 'tableName' was not found in the passed parameters");
                return false;
            }
            if (Parameters.ContainsKey("newFileName"))
            {
                s_NewFileName = Parameters["newFileName"];
            }
            else
            {
                traceLog.Error("ERROR Histogram class: 'newFileName' was not found in the passed parameters");
                return false;
            }


            if (Parameters.ContainsKey("hist_type"))
            {
                s_HistType = Parameters["hist_type"];
            }
            else
            {
                traceLog.Error("ERROR Histogram class: 'hist_type' was not found in the passed parameters");
                return false;
            }
            if (Parameters.ContainsKey("file_type"))
            {
                s_HistType = Parameters["file_type"];
            }
            else
            {
                traceLog.Error("ERROR Histogram class: 'file_type' was not found in the passed parameters");
                return false;
            }

            return true;
        }



        private void CreateHistogram()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            string s_RStatement = "";

            switch (s_HistType)
            {
                case "standard":
                    s_RStatement = string.Format("plot_hist(Data={0}, " +
                         "file=\"{1}\", Data.Columns={2}, IMGwidth={3}, " +
                         "IMGheight={4}, FNTsize={5}, colF={6}, colB={7})",
                         s_TableName,
                         s_NewFileName,
                         s_DataColumns,
                         s_Width,
                         s_Height,
                         s_FontSize,
                         s_ColorBars,
                         s_ColorBackground);
                    break;
            }
            try
            {
                traceLog.Info("Histogram type: " + s_HistType);
                traceLog.Info("Performing Histogram: " + s_RStatement);
                engine.EagerEvaluate(s_RStatement);
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR Performing Histogram: " + exc.ToString());
            }
        }
        #endregion
    }
}
