﻿/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: http://omics.pnl.gov/software
 * -----------------------------------------------------
 *
 * Licensed under the Apache License, Version 2.0; you may not use this
 * file except in compliance with the License.  You may obtain a copy of the
 * License at https://opensource.org/licenses/Apache-2.0
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Cyclops.DataModules
{
    public class CorrelationHeatmap : BaseDataModule
    {
        #region Members
        private readonly string m_ModuleName = "CorrelationHeatmap";
        private readonly string m_Description = "";

        /// <summary>
        /// Required parameters to run Aggregate
        /// </summary>
        private enum RequiredParameters
        {
            TableName, PlotFileName, Image, CorrelationListName, SkipTheFirstColumn
        }

        #endregion

        #region Properties
        public string Horizontal { get; set; } = "TRUE";

        public int PointSize { get; set; } = 12;

        public string PlotFileName { get; set; }
        #endregion

        #region Constructors
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
        #endregion

        #region Methods
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
                        successful = CorrelationHeatmapFunction();
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

            if (!Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.TableName.ToString()]))
            {
                Model.LogError("R Environment does not contain the " +
                    "specified input table: " +
                    Parameters[RequiredParameters.TableName.ToString()],
                    ModuleName, StepNumber);
                successful = false;
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
                var plotDirectory = "Plots";
                if (!Directory.Exists(plotDirectory))
                    Directory.CreateDirectory(plotDirectory);

                var fileName = Parameters[RequiredParameters.PlotFileName.ToString()];
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

            var imageType = Parameters[RequiredParameters.Image.ToString()];

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
                Parameters[RequiredParameters.SkipTheFirstColumn.ToString()]);

            var rCmd = string.Format(
                "require(Hmisc)\n" +
                "{0} <- rcorr(data.matrix({1}{2}){3})\n" +
                "{4} <- list(cor={0}, n={0}$n, prob={0}$P)\n" +
                "rm({0})\n",
                tTable,
                Parameters[RequiredParameters.TableName.ToString()],
                SkipFirstColumn ? "[,-1]" : "",
                Parameters.ContainsKey("Type") ? ", type=c(\"" +
                    Parameters["Type"] + "\")" : "",
                Parameters[RequiredParameters.CorrelationListName.ToString()]);

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
                "hm_" + Parameters[RequiredParameters.CorrelationListName.ToString()],
                Parameters[RequiredParameters.CorrelationListName.ToString()],
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
        #endregion
    }
}

