/* Written by Joseph N. Brown
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
    public class Heatmap : BaseDataModule
    {
        #region Members

        private string m_ModuleName = "Heatmap";
        private string m_Description = "";
        private string m_PValueThreshold = "0.01";
        private string m_ClusterRows = "FALSE";
        private string m_ClusterColumns = "FALSE";
        private string m_RemoveNA = "TRUE";
        private string m_NumberOfTopMostAbundant = "200";
        private string m_PlotFileType = "png";
        private string m_Distance = "NULL";
        private string m_HClust = "NULL";
        private string m_Scale = "c('row')";
        private string m_ColorPalette = "c('green', 'black', 'red')";
        private string m_ColorDegree = "20";
        private string m_MinColorScale = "-1";
        private string m_MaxColorScale = "1";
        private string m_NaColor = "gray";

        private bool m_ZeroFill = false;
        private bool m_ShowRowNames = false;

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
        #endregion

        #region Properties

        #endregion

        #region Constructors
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
        #endregion

        #region Methods
        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override bool PerformOperation()
        {
            bool successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running Heatmap", ModuleName, StepNumber);

                if (CheckParameters())
                    successful = HeatmapFunction();
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
            Dictionary<string, string> paramDictionary = new Dictionary<string, string>();

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
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
            bool successful = true;

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogWarning("Required Field Missing: " + s, ModuleName, StepNumber);
                    successful = false;
                    return successful;
                }
            }

            if (successful &&
                !Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.TableName.ToString()]))
            {
                Model.LogWarning("Error Checking Parameters in Heatmap:\n" +
                    "The input table, " +
                    Parameters[RequiredParameters.TableName.ToString()] +
                    ", does not exist in the R work environment!",
                    ModuleName, StepNumber);
                successful = false;
            }

            if (successful &&
                Parameters[RequiredParameters.Mode.ToString()].ToUpper().Equals("FILTERPVALS"))
            {
                foreach (string s in Enum.GetNames(typeof(FilteredRequiredParameters)))
                {
                    if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                    {
                        Model.LogWarning("Required Field Missing: " + s, ModuleName, StepNumber);
                        successful = false;
                        return successful;
                    }
                }

                if (Parameters.ContainsKey("PValue"))
                {
                    if (!string.IsNullOrEmpty(Parameters["PValue"]))
                        m_PValueThreshold = Parameters["PValue"];
                }

                if (successful &&
                    !Model.RCalls.ContainsObject(
                    Parameters[FilteredRequiredParameters.SignificanceTable.ToString()]))
                {
                    Model.LogWarning("Error Checking Parameters in Heatmap:\n" +
                        "Filter P-values was selected, but the significance table, " +
                        Parameters[FilteredRequiredParameters.SignificanceTable.ToString()] +
                        ", does not exist in the R work environment!",
                        ModuleName, StepNumber);
                    successful = false;
                }

                if (successful &&
                    !Model.RCalls.TableContainsColumn(
                    Parameters[FilteredRequiredParameters.SignificanceTable.ToString()],
                    Parameters[FilteredRequiredParameters.PValueColumn.ToString()]))
                {
                    Model.LogWarning("Error Checking Parameters in Heatmap:\n" +
                        "Filter P-values was selected, but the significance table, " +
                        Parameters[FilteredRequiredParameters.SignificanceTable.ToString()] +
                        ", does not the specified Significance Column, " +
                        Parameters[FilteredRequiredParameters.PValueColumn.ToString()] +
                        "!",
                        ModuleName, StepNumber);
                    successful = false;
                }
            }

            if (successful &&
                Parameters.ContainsKey("HeatmapClusterRows"))
            {
                if (!string.IsNullOrEmpty(Parameters["HeatmapClusterRows"]))
                    m_ClusterRows = Parameters["HeatmapClusterRows"].ToUpper();
            }

            if (successful &&
                Parameters.ContainsKey("HeatmapClusterColumns"))
            {
                if (!string.IsNullOrEmpty(Parameters["HeatmapClusterColumns"]))
                    m_ClusterColumns = Parameters["HeatmapClusterColumns"].ToUpper();
            }

            if (successful &&
                Parameters.ContainsKey("ZeroReplacement"))
            {
                if (!string.IsNullOrEmpty(Parameters["ZeroReplacement"]))
                    m_ZeroFill = Convert.ToBoolean(Parameters["ZeroReplacement"]);
            }

            if (successful &&
                Parameters.ContainsKey("Distance"))
            {
                if (!string.IsNullOrEmpty(Parameters["Distance"]))
                    m_Distance = Parameters["Distance"];
            }

            if (successful &&
                Parameters.ContainsKey("HClust"))
            {
                if (!string.IsNullOrEmpty(Parameters["HClust"]))
                    m_HClust = Parameters["HClust"];
            }

            if (successful &&
                Parameters.ContainsKey("ColorPalette"))
            {
                if (!string.IsNullOrEmpty(Parameters["ColorPalette"]))
                    m_ColorPalette = Parameters["ColorPalette"];
            }

            if (successful &&
                Parameters.ContainsKey("ColorDegree"))
            {
                if (!string.IsNullOrEmpty(Parameters["ColorDegree"]))
                    m_ColorDegree = Parameters["ColorDegree"];
            }

            if (successful &&
                Parameters.ContainsKey("MinColorScale"))
            {
                if (!string.IsNullOrEmpty(Parameters["MinColorScale"]))
                    m_MinColorScale = Parameters["MinColorScale"];
            }

            if (successful &&
                Parameters.ContainsKey("MaxColorScale"))
            {
                if (!string.IsNullOrEmpty(Parameters["MaxColorScale"]))
                    m_MaxColorScale = Parameters["MaxColorScale"];
            }

            if (successful &&
                Parameters.ContainsKey("PlotFileType"))
            {
                if (!string.IsNullOrEmpty(Parameters["PlotFileType"]))
                    m_PlotFileType = Parameters["PlotFileType"];
            }

            if (successful &&
                Parameters.ContainsKey("NA.Color"))
            {
                if (!string.IsNullOrEmpty(Parameters["NA.Color"]))
                    m_NaColor = Parameters["NA.Color"];
            }

            if (successful &&
                Parameters.ContainsKey("ShowRowNames"))
            {
                if (!string.IsNullOrEmpty(Parameters["ShowRowNames"]))
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
            bool successful = true;

            switch (Parameters[RequiredParameters.Mode.ToString()].ToLower())
            {
                case "standard":
                    successful = CreateHeatmap();
                    break;
                case "filterpvals":
                    successful = FilterSignificanceTableForPValues();

                    if (successful)
                        successful = CreateHeatmap();
                    break;
            }

            return successful;
        }

        public bool FilterSignificanceTableForPValues()
        {
            bool successful = true;

            string tFilterTable = GetTemporaryTableName("TmpFilterPval_");

            Dictionary<string, string> filterTableParam = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"InputTableName", Parameters[FilteredRequiredParameters.SignificanceTable.ToString()]},
                {"NewTableName", tFilterTable},
                {"ColumnName", Parameters[FilteredRequiredParameters.PValueColumn.ToString()]},
                {"Operation", "<="},
                {"Value", m_PValueThreshold}
            };

            FilterTable ft = new FilterTable(Model, filterTableParam);
            ft.StepNumber = StepNumber;
            successful = ft.PerformOperation();

            if (successful)
            {
                Dictionary<string, string> mergeParam = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    {"NewTableName", tFilterTable},
                    {"XTable", tFilterTable},
                    {"YTable", Parameters[RequiredParameters.TableName.ToString()]},
                    {"XLink", "row.names"},
                    {"YLink", "row.names"},
                    {"AllX", "TRUE"},
                    {"AllY", "FALSE"}
                };

                Merge m = new Merge(Model, mergeParam);
                m.StepNumber = StepNumber;
                successful = m.PerformOperation();
            }
            else
            {
                Model.LogError("Error running FilterTable within the Heatmap Module!", ModuleName, StepNumber);
                return false;
            }

            string rCmd = string.Format(
                    "rownames({0}) <- {0}[,1]\n" +
                    "{0} <- {0}[,-1]\n",
                    tFilterTable);

            Parameters[RequiredParameters.TableName.ToString()] = tFilterTable;

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

        private bool GetTopMostAbundantProteins()
        {
            bool successful = true;

            string tMostAbundant = GetTemporaryTableName("TmpMostAbundant_");

            Dictionary<string, string> paramDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            paramDictionary.Add("InputTableName", Parameters[RequiredParameters.TableName.ToString()]);
            paramDictionary.Add("NewTableName", tMostAbundant);
            paramDictionary.Add("NumberOfMostAbundant", m_NumberOfTopMostAbundant);

            TopMostAbundant tma = new TopMostAbundant(Model, paramDictionary);
            tma.StepNumber = StepNumber;
            successful = tma.PerformOperation();

            Parameters[RequiredParameters.TableName.ToString()] = tMostAbundant;

            return successful;
        }

        public bool CreateHeatmap()
        {
            bool successful = true;

            CheckForPlotsDirectory();

            string rCmd = "";
            var plotFileName = Parameters[RequiredParameters.PlotFileName.ToString()];
            var plotfilePath = Path.Combine(Model.WorkDirectory, "Plots", plotFileName);
            var matrixFileName = "hm_" + Path.GetFileNameWithoutExtension(plotFileName);

            var plotFilePathForR = GenericRCalls.ConvertToRCompatiblePath(plotfilePath);

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
                , Parameters[RequiredParameters.TableName.ToString()]
                , m_ClusterRows
                , m_ClusterColumns
                , m_Distance
                , m_HClust
                , GetDendrogram()
                , m_RemoveNA
                , m_Scale
                , m_NaColor
                , m_ShowRowNames ? "" : "labRow=rep('', nrow(" +
                    Parameters[RequiredParameters.TableName.ToString()] +
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
                Model.LogError("Exception encountered while performing Heatmap:\n" +
                    ex.ToString());
                SaveCurrentREnvironment();
                successful = false;
            }

            if (!successful)
                SaveCurrentREnvironment();

            return successful;
        }

        public string GetDendrogram()
        {
            if (m_ClusterColumns.ToUpper().StartsWith("T") &&
                m_ClusterRows.ToUpper().StartsWith("T"))
                return "c('both')";

            if (m_ClusterRows.ToUpper().StartsWith("T"))
                return "c('row')";

            if (m_ClusterColumns.ToUpper().StartsWith("T"))
                return "c('column')";

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
        #endregion
    }
}
