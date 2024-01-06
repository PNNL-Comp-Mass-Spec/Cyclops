/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
 * -----------------------------------------------------
 *
 * Licensed under the 2-Clause BSD License; you may not use this
 * file except in compliance with the License.  You may obtain
 * a copy of the License at https://opensource.org/licenses/BSD-2-Clause
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Cyclops.DataModules
{
    public class CorrelationHeatmap : BaseDataModule
    {
        // Ignore Spelling: Heatmap

        private readonly string m_ModuleName = "CorrelationHeatmap";
        private readonly string m_Description = "";

        /// <summary>
        /// Required parameters to run Aggregate
        /// </summary>
        private enum RequiredParameters
        {
            TableName, PlotFileName, Image, CorrelationListName, SkipTheFirstColumn
        }

        public string Horizontal { get; set; } = "TRUE";

        public int PointSize { get; set; } = 12;

        public string PlotFileName { get; set; }

        /// <summary>
        /// Generic constructor creating an CorrelationHeatmap Module
        /// </summary>
        public CorrelationHeatmap()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// CorrelationHeatmap module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public CorrelationHeatmap(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// CorrelationHeatmap module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public CorrelationHeatmap(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
            Parameters = ExportParameters;
        }

        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override bool PerformOperation()
        {
            var successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running " + ModuleName, ModuleName, StepNumber);

                if (CheckParameters())
                {
                    successful = RunCorrelationAnalysis();

                    if (successful)
                    {
                        successful = CorrelationHeatmapFunction();
                    }
                }
            }

            return successful;
        }

        /// <summary>
        /// Retrieves a dictionary of all parameters used by the module,
        /// and the corresponding default values
        /// </summary>
        /// <returns>Parameters used by module</returns>
        public override Dictionary<string, string> GetParametersTemplate()
        {
            var paramDictionary = new Dictionary<string, string>();

            foreach (var s in Enum.GetNames(typeof(RequiredParameters)))
            {
                paramDictionary.Add(s, "");
            }

            return paramDictionary;
        }

        /// <summary>
        /// Checks the parameters to ensure that all required keys are present
        /// </summary>
        /// <returns>True, if all required keys are included in the
        /// Parameters</returns>
        public override bool CheckParameters()
        {
            var successful = true;

            foreach (var s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogWarning("Required Field Missing: " + s, ModuleName, StepNumber);
                    return false;
                }
            }

            if (!Model.RCalls.ContainsObject(Parameters[nameof(RequiredParameters.TableName)]))
            {
                Model.LogError("R Environment does not contain the specified input table: " +
                    Parameters[nameof(RequiredParameters.TableName)],
                    ModuleName, StepNumber);
                successful = false;
            }

            if (Parameters.TryGetValue("Height", out var height))
            {
                Height = Convert.ToInt32(height);
            }

            if (Parameters.TryGetValue("Width", out var width))
            {
                Width = Convert.ToInt32(width);
            }

            if (Parameters.TryGetValue("Horizontal", out var horizontal))
            {
                Horizontal = horizontal;
            }

            if (Parameters.TryGetValue("PointSize", out var pointSize))
            {
                PointSize = Convert.ToInt32(pointSize);
            }

            if (Parameters.TryGetValue("Main", out var main))
            {
                Main = main;
            }

            if (Directory.Exists(Model.WorkDirectory))
            {
                const string plotDirectory = "Plots";

                if (!Directory.Exists(plotDirectory))
                {
                    Directory.CreateDirectory(plotDirectory);
                }

                var fileName = Parameters[nameof(RequiredParameters.PlotFileName)];
                PlotFileName = GenericRCalls.ConvertToRCompatiblePath(Path.Combine(plotDirectory, fileName));
            }

            return successful;
        }

        /// <summary>
        /// Run the Correlation Heatmap Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool CorrelationHeatmapFunction()
        {
            bool successful;

            var rCmd = "require(grDevices)\nrequire(gplots)\n";

            var imageType = Parameters[nameof(RequiredParameters.Image)];

            switch (imageType.ToLower())
            {
                case "esp":
                    rCmd += string.Format("postscript(filename='{0}', width={1}," +
                        "height={2}, pointsize={3})\n",
                        Model.WorkDirectory.Replace("\\", "/") + "/" + PlotFileName,
                        Width,
                        Height,
                        PointSize);
                    break;
                case "png":
                    rCmd += string.Format("png(filename='{0}', width={1}," +
                        "height={2}, pointsize={3})\n",
                        Model.WorkDirectory.Replace("\\", "/") + "/" + PlotFileName,
                        Width,
                        Height,
                        PointSize);
                    break;
                case "jpg":
                    rCmd += string.Format("jpg(filename='{0}', width={1}," +
                        "height={2}, pointsize={3})\n",
                        Model.WorkDirectory.Replace("\\", "/") + "/" + PlotFileName,
                        Width,
                        Height,
                        PointSize);
                    break;

                default:
                    Model.LogError("Unsupported image type for heatmap: " + imageType);
                    return false;
            }

            rCmd += GetHeatmapStatement();

            rCmd += "dev.off()\n";

            try
            {
                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while creating a Correlation Heatmap:\n" + ex);
                SaveCurrentREnvironment();
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Runs the Correlation analysis
        /// </summary>
        /// <returns>True, if correlation analysis completes successfully</returns>
        private bool RunCorrelationAnalysis()
        {
            bool successful;
            var tTable = Model.RCalls.GetTemporaryTableName("T_CorrAnalysis_");
            var SkipFirstColumn = Convert.ToBoolean(
                Parameters[nameof(RequiredParameters.SkipTheFirstColumn)]);

            var rCmd = string.Format(
                "require(Hmisc)\n" +
                "{0} <- rcorr(data.matrix({1}{2}){3})\n" +
                "{4} <- list(cor={0}, n={0}$n, prob={0}$P)\n" +
                "rm({0})\n",
                tTable,
                Parameters[nameof(RequiredParameters.TableName)],
                SkipFirstColumn ? "[,-1]" : "",
                Parameters.TryGetValue("Type", out var parameter) ? ", type=c(\"" +
                                                                    parameter + "\")" : "",
                Parameters[nameof(RequiredParameters.CorrelationListName)]);

            try
            {
                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while running " +
                    "'RunCorrelationAnalysis': " + ex, ModuleName,
                    StepNumber);
                SaveCurrentREnvironment();
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Constructs the R statement that produces the heatmap
        /// </summary>
        /// <returns>R Statement</returns>
        private string GetHeatmapStatement()
        {
            var rCmd = "BlueRed <- colorRampPalette(c('blue', 'white', 'red'))\n";
            rCmd += "cmap <- BlueRed(20)\n";
            rCmd += string.Format("{0} <- heatmap.2(data.matrix({1}$cor[1]$r), " +
                "main='{2}', " +
                "Rowv=F, " +
                "Colv=F, " +
                "dendrogram=c('none'), " +
                "col=cmap, " +
                "trace=c('none'), " +
                "scale=c('none'), " +
                "margins=c(10,10))\n" +
                "rm(BlueRed)\nrm(cmap)\n",
                "hm_" + Parameters[nameof(RequiredParameters.CorrelationListName)],
                Parameters[nameof(RequiredParameters.CorrelationListName)],
                Main);
            return rCmd;
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

        /// <summary>
        /// Retrieves the Type Description for automatically
        /// registering the module assembly
        /// </summary>
        /// <returns>Module's Description</returns>
        protected override string GetTypeDescription()
        {
            return Description;
        }
    }
}
