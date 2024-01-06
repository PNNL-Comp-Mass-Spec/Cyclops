﻿/* Written by Joseph N. Brown
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
    public class Heatmap : BaseDataModule
    {
        // Ignore Spelling: Dendrogram, Heatmap

        private readonly string m_ModuleName = "Heatmap";
        private readonly string m_Description = "";
        private string m_PValueThreshold = "0.01";
        private string m_ClusterRows = "FALSE";
        private string m_ClusterColumns = "FALSE";
        private readonly string m_RemoveNA = "TRUE";
        private readonly string m_NumberOfTopMostAbundant = "200";
        private string m_PlotFileType = "png";
        private string m_Distance = "NULL";
        private string m_HClust = "NULL";
        private readonly string m_Scale = "c('row')";
        private string m_ColorPalette = "c('green', 'black', 'red')";
        private string m_ColorDegree = "20";
        private string m_MinColorScale = "-1";
        private string m_MaxColorScale = "1";
        private string m_NaColor = "gray";

        private bool m_ShowRowNames;

        /// <summary>
        /// Required parameters to run Heatmap Module
        /// </summary>
        private enum RequiredParameters
        {
            TableName, Mode, PlotFileName
        }

        private enum FilteredRequiredParameters
        {
            SignificanceTable, PValueColumn
        }

        /// <summary>
        /// Generic constructor creating an Heatmap Module
        /// </summary>
        public Heatmap()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// Heatmap module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public Heatmap(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// Heatmap module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public Heatmap(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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

                Model.LogMessage("Running Heatmap", ModuleName, StepNumber);

                if (CheckParameters())
                {
                    successful = HeatmapFunction();
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
                Model.LogWarning("Error Checking Parameters in Heatmap:\n" +
                    "The input table, " +
                    Parameters[nameof(RequiredParameters.TableName)] +
                    ", does not exist in the R work environment!",
                    ModuleName, StepNumber);
                successful = false;
            }

            if (successful &&
                Parameters[nameof(RequiredParameters.Mode)].Equals("FilterPvals", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var s in Enum.GetNames(typeof(FilteredRequiredParameters)))
                {
                    if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                    {
                        Model.LogWarning("Required Field Missing: " + s, ModuleName, StepNumber);
                        return false;
                    }
                }

                if (Parameters.ContainsKey("PValue") && !string.IsNullOrEmpty(Parameters["PValue"]))
                {
                    m_PValueThreshold = Parameters["PValue"];
                }

                if (!Model.RCalls.ContainsObject(Parameters[nameof(FilteredRequiredParameters.SignificanceTable)]))
                {
                    Model.LogWarning("Error Checking Parameters in Heatmap:\n" +
                        "Filter P-values was selected, but the significance table, " +
                        Parameters[nameof(FilteredRequiredParameters.SignificanceTable)] +
                        ", does not exist in the R work environment!",
                        ModuleName, StepNumber);
                    successful = false;
                }

                if (successful &&
                    !Model.RCalls.TableContainsColumn(
                    Parameters[nameof(FilteredRequiredParameters.SignificanceTable)],
                    Parameters[nameof(FilteredRequiredParameters.PValueColumn)]))
                {
                    Model.LogWarning("Error Checking Parameters in Heatmap:\n" +
                        "Filter P-values was selected, but the significance table, " +
                        Parameters[nameof(FilteredRequiredParameters.SignificanceTable)] +
                        ", does not the specified Significance Column, " +
                        Parameters[nameof(FilteredRequiredParameters.PValueColumn)] +
                        "!",
                        ModuleName, StepNumber);
                    successful = false;
                }
            }

            if (successful &&
                Parameters.ContainsKey("HeatmapClusterRows") && !string.IsNullOrEmpty(Parameters["HeatmapClusterRows"]))
            {
                m_ClusterRows = Parameters["HeatmapClusterRows"].ToUpper();
            }

            if (successful &&
                Parameters.ContainsKey("HeatmapClusterColumns") && !string.IsNullOrEmpty(Parameters["HeatmapClusterColumns"]))
            {
                m_ClusterColumns = Parameters["HeatmapClusterColumns"].ToUpper();
            }

            /*
            if (successful &&
                Parameters.ContainsKey("ZeroReplacement"))
            {
                if (!string.IsNullOrEmpty(Parameters["ZeroReplacement"]))
                    m_ZeroFill = Convert.ToBoolean(Parameters["ZeroReplacement"]);
            }
            */

            if (successful &&
                Parameters.ContainsKey("Distance") && !string.IsNullOrEmpty(Parameters["Distance"]))
            {
                m_Distance = Parameters["Distance"];
            }

            if (successful &&
                Parameters.ContainsKey("HClust") && !string.IsNullOrEmpty(Parameters["HClust"]))
            {
                m_HClust = Parameters["HClust"];
            }

            if (successful &&
                Parameters.ContainsKey("ColorPalette") && !string.IsNullOrEmpty(Parameters["ColorPalette"]))
            {
                m_ColorPalette = Parameters["ColorPalette"];
            }

            if (successful &&
                Parameters.ContainsKey("ColorDegree") && !string.IsNullOrEmpty(Parameters["ColorDegree"]))
            {
                m_ColorDegree = Parameters["ColorDegree"];
            }

            if (successful &&
                Parameters.ContainsKey("MinColorScale") && !string.IsNullOrEmpty(Parameters["MinColorScale"]))
            {
                m_MinColorScale = Parameters["MinColorScale"];
            }

            if (successful &&
                Parameters.ContainsKey("MaxColorScale") && !string.IsNullOrEmpty(Parameters["MaxColorScale"]))
            {
                m_MaxColorScale = Parameters["MaxColorScale"];
            }

            if (successful &&
                Parameters.ContainsKey("PlotFileType") && !string.IsNullOrEmpty(Parameters["PlotFileType"]))
            {
                m_PlotFileType = Parameters["PlotFileType"];
            }

            if (successful &&
                Parameters.ContainsKey("NA.Color") && !string.IsNullOrEmpty(Parameters["NA.Color"]))
            {
                m_NaColor = Parameters["NA.Color"];
            }

            if (successful &&
                Parameters.ContainsKey("ShowRowNames") && !string.IsNullOrEmpty(Parameters["ShowRowNames"]))
            {
                m_ShowRowNames = Convert.ToBoolean(Parameters["ShowRowNames"]);
            }

            return successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool HeatmapFunction()
        {
            var successful = true;

            var nodeName = Parameters[nameof(RequiredParameters.Mode)];

            if (string.Equals(nodeName, "standard", StringComparison.OrdinalIgnoreCase))
            {
                successful = CreateHeatmap();
            }
            else if (string.Equals(nodeName, "FilterPvals", StringComparison.OrdinalIgnoreCase))
            {
                successful = FilterSignificanceTableForPValues();

                if (successful)
                {
                    successful = CreateHeatmap();
                }
            }

            return successful;
        }

        public bool FilterSignificanceTableForPValues()
        {
            var tFilterTable = GetTemporaryTableName("TmpFilterPval_");

            var filterTableParam = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"InputTableName", Parameters[nameof(FilteredRequiredParameters.SignificanceTable)]},
                {"NewTableName", tFilterTable},
                {"ColumnName", Parameters[nameof(FilteredRequiredParameters.PValueColumn)]},
                {"Operation", "<="},
                {"Value", m_PValueThreshold}
            };

            var ft = new FilterTable(Model, filterTableParam) {
                StepNumber = StepNumber
            };

            var successful = ft.PerformOperation();

            if (successful)
            {
                var mergeParam = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    {"NewTableName", tFilterTable},
                    {"XTable", tFilterTable},
                    {"YTable", Parameters[nameof(RequiredParameters.TableName)]},
                    {"XLink", "row.names"},
                    {"YLink", "row.names"},
                    {"AllX", "TRUE"},
                    {"AllY", "FALSE"}
                };

                var m = new Merge(Model, mergeParam) {
                    StepNumber = StepNumber
                };
                m.PerformOperation();
            }
            else
            {
                Model.LogError("Error running FilterTable within the Heatmap Module!", ModuleName, StepNumber);
                return false;
            }

            var rCmd = string.Format(
                    "rownames({0}) <- {0}[,1]\n" +
                    "{0} <- {0}[,-1]\n",
                    tFilterTable);

            Parameters[nameof(RequiredParameters.TableName)] = tFilterTable;

            try
            {
                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while filtering " +
                    "significance table for p-value threshold: " + ex.Message,
                    ModuleName, StepNumber);
                successful = false;
            }

            return successful;
        }

        [Obsolete("Unused")]
        private bool GetTopMostAbundantProteins()
        {
            var tMostAbundant = GetTemporaryTableName("TmpMostAbundant_");

            var paramDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"InputTableName", Parameters[nameof(RequiredParameters.TableName)]},
                {"NewTableName", tMostAbundant},
                {"NumberOfMostAbundant", m_NumberOfTopMostAbundant}
            };

            var tma = new TopMostAbundant(Model, paramDictionary) {
                StepNumber = StepNumber
            };
            var successful = tma.PerformOperation();

            Parameters[nameof(RequiredParameters.TableName)] = tMostAbundant;

            return successful;
        }

        public bool CreateHeatmap()
        {
            bool successful;

            CheckForPlotsDirectory();

            var rCmd = "";
            var plotFileName = Parameters[nameof(RequiredParameters.PlotFileName)];
            var plotFilePath = Path.Combine(Model.WorkDirectory, "Plots", plotFileName);
            var matrixFileName = "hm_" + Path.GetFileNameWithoutExtension(plotFileName);

            var plotFilePathForR = GenericRCalls.ConvertToRCompatiblePath(plotFilePath);

            switch (m_PlotFileType.ToLower())
            {
                case "png":
                    rCmd += string.Format("CairoPNG(filename='{0}')\n", plotFilePathForR);
                    break;
                case "svg":
                    rCmd += string.Format("svg(filename='{0}')\n", plotFilePathForR);
                    break;
                case "ps":
                    rCmd += string.Format("cairo_ps(filename='{0}')\n", plotFilePathForR);
                    break;
                default:
                    Model.LogError("Unsupported plot type: " + m_PlotFileType,
                                   ModuleName, StepNumber);
                    return false;
            }

            // Setup heatmap.2 parameters
            rCmd += string.Format(
                "col <- colorRampPalette({0})\n" +
                "cmap <- col({1})\n" +
                "colscale <- seq({2}, {3}, length={1}+1)\n"
                , m_ColorPalette
                , m_ColorDegree
                , m_MinColorScale
                , m_MaxColorScale);

            rCmd += string.Format(
                "{0} <- heatmap.2(" +
                "x=data.matrix({1}), " +
                "Rowv={2}, " +
                "Colv={3}, " +
                "dist={4}, " +
                "hclust={5}, " +
                "dendrogram={6}, " +
                "na.rm={7}, " +
                "col=cmap, " +
                "scale={8}, " +
                "breaks=colscale, " +
                "trace=c('none'), " +
                "na.color='{9}', " +
                "{10}" +
                "main=paste('{11}', '\n', nrow({1}), 'Proteins'))\n" +
                "{0} <- jnb_GetHeatmapMatrix({0})\n"
                , matrixFileName
                , Parameters[nameof(RequiredParameters.TableName)]
                , m_ClusterRows
                , m_ClusterColumns
                , m_Distance
                , m_HClust
                , GetDendrogram()
                , m_RemoveNA
                , m_Scale
                , m_NaColor
                , m_ShowRowNames ? "" : "labRow=rep('', nrow(" +
                    Parameters[nameof(RequiredParameters.TableName)] +
                    ")), "
                , Main
            );

            rCmd += "dev.off()\n";

            try
            {
                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while performing Heatmap:\n" + ex);
                SaveCurrentREnvironment();
                successful = false;
            }

            if (!successful)
            {
                SaveCurrentREnvironment();
            }

            return successful;
        }

        public string GetDendrogram()
        {
            if (m_ClusterColumns.StartsWith("T", StringComparison.OrdinalIgnoreCase) &&
                m_ClusterRows.StartsWith("T", StringComparison.OrdinalIgnoreCase))
            {
                return "c('both')";
            }

            if (m_ClusterRows.StartsWith("T", StringComparison.OrdinalIgnoreCase))
            {
                return "c('row')";
            }

            if (m_ClusterColumns.StartsWith("T", StringComparison.OrdinalIgnoreCase))
            {
                return "c('column')";
            }

            return "c('none')";
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
