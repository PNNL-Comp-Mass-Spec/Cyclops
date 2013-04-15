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
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Cyclops.DataModules
{
    public class CorrelationHeatmap : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "CorrelationHeatmap",
            m_Horizontal = "TRUE";
        /// <summary>
        /// Required parameters to run Aggregate
        /// </summary>
        private enum RequiredParameters
        {
            TableName, PlotFileName, Image, CorrelationListName,
            SkipTheFirstColumn
        }

        private int m_PointSize = 12;
        #endregion

        #region Properties
        public string Horizontal
        {
            get { return m_Horizontal; }
            set { m_Horizontal = value; }
        }

        public int PointSize
        {
            get { return m_PointSize; }
            set { m_PointSize = value; }
        }

        public string PlotFileName { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an CorrelationHeatmap Module
        /// </summary>
        public CorrelationHeatmap()
        {
            ModuleName = m_ModuleName;
        }

        /// <summary>
        /// CorrelationHeatmap module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public CorrelationHeatmap(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
        }

        /// <summary>
        /// CorrelationHeatmap module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public CorrelationHeatmap(CyclopsModel CyclopsModel,
            Dictionary<string, string> ExportParameters)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
            Parameters = ExportParameters;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running " + ModuleName,
                        ModuleName, StepNumber);

                if (CheckParameters())
                {
                    bool b_Successful = RunCorrelationAnalysis();

                    if (b_Successful)
                        b_Successful = CorrelationHeatmapFunction();

                    Model.PipelineCurrentlySuccessful = b_Successful;
                }

                RunChildModules();
            }
        }

        /// <summary>
        /// Checks the parameters to ensure that all required keys are present
        /// </summary>
        /// <returns>True, if all required keys are included in the
        /// Parameters</returns>
        public override bool CheckParameters()
        {
            bool b_Successful = true;

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogWarning("Required Field Missing: " + s,
                        ModuleName, StepNumber);
                    b_Successful = false;
                    return b_Successful;
                }
            }

            if (!Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.TableName.ToString()]))
            {
                Model.LogError("R Environment does not contain the " +
                    "specified input table: " +
                    Parameters[RequiredParameters.TableName.ToString()],
                    ModuleName, StepNumber);
                b_Successful = false;
            }

            if (Parameters.ContainsKey("Height"))
                Height = Convert.ToInt32(Parameters["Height"]);
            if (Parameters.ContainsKey("Width"))
                Width = Convert.ToInt32(Parameters["Width"]);
            if (Parameters.ContainsKey("Horizontal"))
                Horizontal = Parameters["Horizontal"];
            if (Parameters.ContainsKey("PointSize"))
                PointSize = Convert.ToInt32(Parameters["PointSize"]);
            if (Parameters.ContainsKey("Main"))
                Main = Parameters["Main"];

            if (Directory.Exists(Model.WorkDirectory))
            {
                string s_PlotDirectory = "Plots";
                if (!Directory.Exists(s_PlotDirectory))
                    Directory.CreateDirectory(s_PlotDirectory);
                PlotFileName = Path.Combine(s_PlotDirectory,
                    Parameters[RequiredParameters.PlotFileName.ToString()]).Replace("\\", "/");
            }

            return b_Successful;
        }

        /// <summary>
        /// Run the Correlation Heatmap Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool CorrelationHeatmapFunction()
        {
            bool b_Successful = true;

            string Command = "require(grDevices)\nrequire(gplots)\n";
                        
            switch (Parameters[RequiredParameters.Image.ToString()].ToLower())
            {
                case "esp":
                    Command += string.Format("postscript(filename='{0}', width={1}," +
                        "height={2}, pointsize={3})\n",
                        Model.WorkDirectory.Replace("\\", "/") + "/" + PlotFileName,
                        Width,
                        Height,
                        PointSize);
                    break;
                case "png":
                    Command += string.Format("png(filename='{0}', width={1}," +
                        "height={2}, pointsize={3})\n",
                        Model.WorkDirectory.Replace("\\", "/") + "/" + PlotFileName,
                        Width,
                        Height,
                        PointSize);
                    break;
                case "jpg":
                    Command += string.Format("jpg(filename='{0}', width={1}," +
                        "height={2}, pointsize={3})\n",
                        Model.WorkDirectory.Replace("\\", "/") + "/" + PlotFileName,
                        Width,
                        Height,
                        PointSize);
                    break;
            }

            Command += GetHeatmapStatement();

            Command += "dev.off()\n";
            
            try
            {
                b_Successful = Model.RCalls.Run(Command, ModuleName, StepNumber);
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while creating a " +
                    "Correlation Heatmap:\n" + exc.ToString());
                SaveCurrentREnvironment();
                b_Successful = false;
            }

            return b_Successful;
        }        

        /// <summary>
        /// Runs the Correlation analysis
        /// </summary>
        /// <returns>True, if correlation analysis completes successfully</returns>
        private bool RunCorrelationAnalysis()
        {
            bool b_Successful = true;
            string s_TmpTable = Model.RCalls.GetTemporaryTableName("T_CorrAnalysis_");
            bool SkipFirstColumn = Convert.ToBoolean(
                Parameters[RequiredParameters.SkipTheFirstColumn.ToString()]);

            string Command = string.Format(
                "require(Hmisc)\n" +
                "{0} <- rcorr(data.matrix({1}{2}){3})\n" +
                "{4} <- list(cor={0}, n={0}$n, prob={0}$P)\n" +
                "rm({0})\n",
                s_TmpTable,
                Parameters[RequiredParameters.TableName.ToString()],
                SkipFirstColumn ? "[,-1]" : "",
                Parameters.ContainsKey("Type") ? ", type=c(\"" +
                    Parameters["Type"] + "\")" : "",
                Parameters[RequiredParameters.CorrelationListName.ToString()]);

            try
            {
                b_Successful = Model.RCalls.Run(Command, ModuleName, StepNumber);
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while running " +
                    "'RunCorrelationAnalysis': " + exc.ToString(), ModuleName,
                    StepNumber);
                SaveCurrentREnvironment();
                b_Successful = false;
            }

            return b_Successful;
        }

        /// <summary>
        /// Constructs the R statement that produces the heatmap
        /// </summary>
        /// <returns>R Statement</returns>
        private string GetHeatmapStatement()
        {
            string s_Return = "BlueRed <- colorRampPalette(c('blue', 'white', 'red'))\n";
            s_Return += "cmap <- BlueRed(20)\n";
            s_Return += string.Format("{0} <- heatmap.2(data.matrix({1}$cor[1]$r), " +
                "main='{2}', " +
                "Rowv=F, " +
                "Colv=F, " +
                "dendrogram=c('none'), " +
                "col=cmap, " +
                "trace=c('none'), " +
                "scale=c('none'), " +
                "margins=c(10,10))\n" +
                "rm(BlueRed)\nrm(cmap)\n",
                "hm_" + Parameters[RequiredParameters.CorrelationListName.ToString()],
                Parameters[RequiredParameters.CorrelationListName.ToString()],
                Main);
            return s_Return;
        }

        /// <summary>
        /// Retrieves the Default Value
        /// </summary>
        /// <returns>Default Value</returns>
        protected override string GetDefaultValue()
        {
            return "false";
        }

        /// <summary>
        /// Retrieves the Type Name for automatically 
        /// registering the module assembly
        /// </summary>
        /// <returns>Module's Name</returns>
        protected override string GetTypeName()
        {
            return ModuleName;
        }
        #endregion
    }
}

