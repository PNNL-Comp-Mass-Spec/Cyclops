﻿/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://www.pnnl.gov/integrative-omics
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
    public class BoxPlot : BaseDataModule
    {
        private readonly string m_ModuleName = "BoxPlot";
        private readonly string m_Description = "";
        private string m_DataColumns = "NULL";
        private string m_ColorByFactor = "FALSE";
        private string m_ColumnFactorTable = "NULL";
        private string m_FactorColumn = "NULL";
        private string m_Outliers = "TRUE";
        private string m_Color = "cornflowerblue";
        private string m_LabelScale = "0.8";
        private string m_BoxWidth = "1";
        private string m_ShowCount = "TRUE";
        private string m_ShowLegend = "TRUE";
        private string m_Stamp = "NULL";
        private string m_DoYLim = "FALSE";
        private string m_yMin = "NULL";
        private string m_yMax = "NULL";

        /// <summary>
        /// Required parameters to run BoxPlot Module
        /// </summary>
        private enum RequiredParameters
        {
            TableName, PlotFileName
        }

        public string PlotFileName { get; set; }

        /// <summary>
        /// Generic constructor creating an BoxPlot Module
        /// </summary>
        public BoxPlot()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// BoxPlot module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public BoxPlot(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// BoxPlot module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public BoxPlot(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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

                Model.LogMessage("Running BoxPlot", ModuleName, StepNumber);

                if (CheckParameters())
                {
                    successful = BoxPlotFunction();
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
            foreach (var s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogError("Required Field Missing: " + s, ModuleName, StepNumber);
                    return false;
                }
            }

            if (Parameters.TryGetValue("ColorByFactor", out var parameter))
            {
                m_ColorByFactor = parameter.ToUpper();
            }

            if (Parameters.ContainsKey("ColumnFactorTable") && Model.RCalls.ContainsObject(Parameters["ColumnFactorTable"]))
            {
                m_ColumnFactorTable = Parameters["ColumnFactorTable"];
            }

            if (Parameters.ContainsKey("FactorColumn") && Model.RCalls.TableContainsColumn(
                    Parameters["ColumnFactorTable"],
                    Parameters["FactorColumn"]))
            {
                m_FactorColumn = Parameters["FactorColumn"];
            }

            if (!string.IsNullOrEmpty(m_ColumnFactorTable) &&
                !string.IsNullOrEmpty(m_FactorColumn) &&
                m_ColorByFactor.Equals("FALSE"))
            {
                m_ColorByFactor = "TRUE";
            }

            if (Parameters.TryGetValue("BackgroundColor", out var backgroundColor))
            {
                BackgroundColor = backgroundColor;
            }

            if (Parameters.TryGetValue("Height", out var height))
            {
                Height = Convert.ToInt32(height);
            }

            if (Parameters.TryGetValue("Width", out var width))
            {
                Width = Convert.ToInt32(width);
            }

            if (Parameters.TryGetValue("FontSize", out var fontSize))
            {
                FontSize = Convert.ToInt32(fontSize);
            }

            if (Parameters.TryGetValue("Resolution", out var resolution))
            {
                Resolution = Convert.ToInt32(resolution);
            }

            if (Parameters.TryGetValue("Main", out var main))
            {
                Main = main;
            }

            if (Parameters.TryGetValue("XLabel", out var xLabel))
            {
                XLabel = xLabel;
            }

            if (Parameters.TryGetValue("YLabel", out var yLabel))
            {
                YLabel = yLabel;
            }

            if (Parameters.TryGetValue("DataColumns", out var dataColumns))
            {
                m_DataColumns = dataColumns;
            }

            if (Parameters.TryGetValue("Outliers", out var outliers))
            {
                m_Outliers = outliers.ToUpper();
            }

            if (Parameters.TryGetValue("Color", out var color))
            {
                m_Color = color;
            }

            if (Parameters.TryGetValue("LabelScale", out var labelScale))
            {
                m_LabelScale = labelScale;
            }

            if (Parameters.TryGetValue("BoxWidth", out var boxWidth))
            {
                m_BoxWidth = boxWidth;
            }

            if (Parameters.TryGetValue("ShowCount", out var showCount))
            {
                m_ShowCount = showCount.ToUpper();
            }

            if (Parameters.TryGetValue("ShowLegend", out var showLegend))
            {
                m_ShowLegend = showLegend.ToUpper();
            }

            if (Parameters.TryGetValue("Stamp", out var stamp))
            {
                m_Stamp = stamp;
            }

            if (Parameters.TryGetValue("DoYLim", out var doYLim))
            {
                m_DoYLim = doYLim.ToUpper();
            }

            if (Parameters.TryGetValue("yMin", out var yMin))
            {
                m_yMin = yMin;
            }

            if (Parameters.TryGetValue("yMax", out var yMax))
            {
                m_yMax = yMax;
            }

            if (Directory.Exists(Model.WorkDirectory))
            {
                var s_PlotDirectory = Path.Combine(
                    Model.WorkDirectory, "Plots").Replace("\\", "/");
                if (!Directory.Exists(s_PlotDirectory))
                {
                    Directory.CreateDirectory(s_PlotDirectory);
                }

                PlotFileName =
                    Path.Combine(s_PlotDirectory,
                    Parameters[nameof(RequiredParameters.PlotFileName)]).Replace("\\", "/");
            }

            return true;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool BoxPlotFunction()
        {
            bool successful;

            var tTable = GetTemporaryTableName("tmpBoxPlot_");

            var rCmd = string.Format(
                // ReSharper disable StringLiteralTypo
                "Boxplots(x={0}, Columns={1}, " +
                "file=\"{2}\", colorByFactor={3}, colorFactorTable={4}, " +
                "colorFactorName={5}, " +
                "outliers={6}, color=\"{7}\", bkground=\"{8}\", labelscale={9}, " +
                "boxwidth={10}, showcount={11}, showlegend={12}, stamp={13}, " +
                "do.ylim={14}, ymin={15}, ymax={16}, ylabel=\"{17}\", " +
                "IMGwidth={18}, IMGheight={19}, FNTsize={20}, res={21})\n" +
                "rm({0})\n",
                // ReSharper restore StringLiteralTypo
                tTable,
                m_DataColumns,
                PlotFileName,
                m_ColorByFactor,
                m_ColumnFactorTable,
                m_FactorColumn,
                m_Outliers,
                m_Color,
                BackgroundColor,
                m_LabelScale,
                m_BoxWidth,
                m_ShowCount,
                m_ShowLegend,
                m_Stamp,
                m_DoYLim,
                m_yMin,
                m_yMax,
                YLabel,
                Width,
                Height,
                FontSize,
                Resolution
                );

            try
            {
                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while creating a " +
                    "BoxPlot:\n" + ex);
                SaveCurrentREnvironment();
                successful = false;
            }

            return successful;
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
