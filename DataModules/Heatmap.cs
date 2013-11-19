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
    public class Heatmap : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "Heatmap",
            m_Description = "",
            m_PValueThreshold = "0.01",
            m_ClusterRows = "FALSE",
            m_ClusterColumns = "FALSE",
            m_RemoveNA = "TRUE",
            m_NumberOfTopMostAbundant = "200",
            m_PlotFileType = "png",
            m_Distance = "NULL",
            m_HClust = "NULL",
            m_Scale = "c('row')",
            m_ColorPalette = "c('green', 'black', 'red')",
            m_ColorDegree = "20",
            m_MinColorScale = "-1",
            m_MaxColorScale = "1",
            m_NaColor = "gray";

        private bool m_ZeroFill = false,
            m_ShowRowNames = false;

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
        public Heatmap(CyclopsModel CyclopsModel,
            Dictionary<string, string> ExportParameters)
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
            bool b_Successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running Heatmap",
                        ModuleName, StepNumber);

                if (CheckParameters())
                    b_Successful = HeatmapFunction();
            }

            return b_Successful;
        }

        /// <summary>
        /// Retrieves a dictionary of all parameters used by the module, 
        /// and the corresponding default values
        /// </summary>
        /// <returns>Parameters used by module</returns>
        public override Dictionary<string, string> GetParametersTemplate()
        {
            Dictionary<string, string> d_Parameters = new Dictionary<string, string>();

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                d_Parameters.Add(s, "");
            }

            return d_Parameters;
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

            if (b_Successful &&
                !Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.TableName.ToString()]))
            {
                Model.LogWarning("Error Checking Parameters in Heatmap:\n" +
                    "The input table, " +
                    Parameters[RequiredParameters.TableName.ToString()] +
                    ", does not exist in the R work environment!",
                    ModuleName, StepNumber);
                b_Successful = false;
            }

            if (b_Successful &&
                Parameters[RequiredParameters.Mode.ToString()].ToUpper().Equals("FILTERPVALS"))
            {
                foreach (string s in Enum.GetNames(typeof(FilteredRequiredParameters)))
                {
                    if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                    {
                        Model.LogWarning("Required Field Missing: " + s,
                            ModuleName, StepNumber);
                        b_Successful = false;
                        return b_Successful;
                    }
                }
                
                if (Parameters.ContainsKey("PValue"))
                {
                    if (!string.IsNullOrEmpty(Parameters["PValue"]))
                        m_PValueThreshold = Parameters["PValue"];
                }

                if (b_Successful &&
                    !Model.RCalls.ContainsObject(
                    Parameters[FilteredRequiredParameters.SignificanceTable.ToString()]))
                {
                    Model.LogWarning("Error Checking Parameters in Heatmap:\n" +
                        "Filter P-values was selected, but the significance table, " +
                        Parameters[FilteredRequiredParameters.SignificanceTable.ToString()] +
                        ", does not exist in the R work environment!",
                        ModuleName, StepNumber);
                    b_Successful = false;
                }

                if (b_Successful &&
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
                    b_Successful = false;
                }
            }

            if (b_Successful &&
                Parameters.ContainsKey("HeatmapClusterRows"))
            {
                if (!string.IsNullOrEmpty(Parameters["HeatmapClusterRows"]))
                    m_ClusterRows = Parameters["HeatmapClusterRows"].ToUpper();
            }

            if (b_Successful &&
                Parameters.ContainsKey("HeatmapClusterColumns"))
            {
                if (!string.IsNullOrEmpty(Parameters["HeatmapClusterColumns"]))
                    m_ClusterColumns = Parameters["HeatmapClusterColumns"].ToUpper();
            }

            if (b_Successful &&
                Parameters.ContainsKey("ZeroReplacement"))
            {
                if (!string.IsNullOrEmpty(Parameters["ZeroReplacement"]))
                    m_ZeroFill = Convert.ToBoolean(Parameters["ZeroReplacement"]);
            }

            if (b_Successful &&
                Parameters.ContainsKey("Distance"))
            {
                if (!string.IsNullOrEmpty(Parameters["Distance"]))
                    m_Distance = Parameters["Distance"];
            }

            if (b_Successful &&
                Parameters.ContainsKey("HClust"))
            {
                if (!string.IsNullOrEmpty(Parameters["HClust"]))
                    m_HClust = Parameters["HClust"];
            }

            if (b_Successful &&
                Parameters.ContainsKey("ColorPalette"))
            {
                if (!string.IsNullOrEmpty(Parameters["ColorPalette"]))
                    m_ColorPalette = Parameters["ColorPalette"];
            }

            if (b_Successful &&
                Parameters.ContainsKey("ColorDegree"))
            {
                if (!string.IsNullOrEmpty(Parameters["ColorDegree"]))
                    m_ColorDegree = Parameters["ColorDegree"];
            }

            if (b_Successful &&
                Parameters.ContainsKey("MinColorScale"))
            {
                if (!string.IsNullOrEmpty(Parameters["MinColorScale"]))
                    m_MinColorScale = Parameters["MinColorScale"];
            }

            if (b_Successful &&
                Parameters.ContainsKey("MaxColorScale"))
            {
                if (!string.IsNullOrEmpty(Parameters["MaxColorScale"]))
                    m_MaxColorScale = Parameters["MaxColorScale"];
            }

            if (b_Successful &&
                Parameters.ContainsKey("PlotFileType"))
            {
                if (!string.IsNullOrEmpty(Parameters["PlotFileType"]))
                    m_PlotFileType = Parameters["PlotFileType"];
            }

            if (b_Successful &&
                Parameters.ContainsKey("NA.Color"))
            {
                if (!string.IsNullOrEmpty(Parameters["NA.Color"]))
                    m_NaColor = Parameters["NA.Color"];
            }

            if (b_Successful &&
                Parameters.ContainsKey("ShowRowNames"))
            {
                if (!string.IsNullOrEmpty(Parameters["ShowRowNames"]))
                    m_ShowRowNames = Convert.ToBoolean(Parameters["ShowRowNames"]);
            }

            return b_Successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool HeatmapFunction()
        {
            bool b_Successful = true;

            switch (Parameters[RequiredParameters.Mode.ToString()].ToLower())
            {
                case "standard":
                    b_Successful = CreateHeatmap();
                    break;
                case "filterpvals":
                    b_Successful = FilterSignificanceTableForPValues();

                    if (b_Successful)
                        b_Successful = CreateHeatmap();
                    break;
            }

            return b_Successful;
        }

        public bool FilterSignificanceTableForPValues()
        {
            bool b_Successful = true;

            string s_TmpFilterTable = GetTemporaryTableName("TmpFilterPval_");

            Dictionary<string, string> d_FilterTableParam = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);
            d_FilterTableParam.Add("InputTableName", 
                Parameters[FilteredRequiredParameters.SignificanceTable.ToString()]);
            d_FilterTableParam.Add("NewTableName", s_TmpFilterTable);
            d_FilterTableParam.Add("ColumnName", 
                Parameters[FilteredRequiredParameters.PValueColumn.ToString()]);
            d_FilterTableParam.Add("Operation", "<=");
            d_FilterTableParam.Add("Value", m_PValueThreshold);

            FilterTable ft = new FilterTable(Model, d_FilterTableParam);
            ft.StepNumber = StepNumber;
            b_Successful = ft.PerformOperation();

            if (b_Successful)
            {
                Dictionary<string, string> d_MergeParam = new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);
                d_MergeParam.Add("NewTableName", s_TmpFilterTable);
                d_MergeParam.Add("XTable", s_TmpFilterTable);
                d_MergeParam.Add("YTable",
                    Parameters[RequiredParameters.TableName.ToString()]);
                d_MergeParam.Add("XLink", "row.names");
                d_MergeParam.Add("YLink", "row.names");
                d_MergeParam.Add("AllX", "TRUE");
                d_MergeParam.Add("AllY", "FALSE");

                Merge m = new Merge(Model, d_MergeParam);
                m.StepNumber = StepNumber;
                b_Successful = m.PerformOperation();
            }
            else
            {
                Model.LogError("Error running FilterTable within the Heatmap Module!",
                    ModuleName, StepNumber);
                return false;
            }

            string Command = string.Format(
                    "rownames({0}) <- {0}[,1]\n" +
                    "{0} <- {0}[,-1]\n",
                    s_TmpFilterTable);

            Parameters[RequiredParameters.TableName.ToString()] = s_TmpFilterTable;

            try
            {
                b_Successful = Model.RCalls.Run(Command,
                    ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while filtering " +
                    "significance table for p-value threshold: " + ex.Message,
                    ModuleName, StepNumber);
                b_Successful = false;
            }

            return b_Successful;
        }

        private bool GetTopMostAbundantProteins()
        {
            bool b_Successful = true;

            string s_TmpMostAbundant = GetTemporaryTableName("TmpMostAbundant_");

            Dictionary<string, string> d_Param = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);
            d_Param.Add("InputTableName", Parameters[RequiredParameters.TableName.ToString()]);
            d_Param.Add("NewTableName", s_TmpMostAbundant);
            d_Param.Add("NumberOfMostAbundant", m_NumberOfTopMostAbundant);

            TopMostAbundant tma = new TopMostAbundant(Model, d_Param);
            tma.StepNumber = StepNumber;
            b_Successful = tma.PerformOperation();
            
            Parameters[RequiredParameters.TableName.ToString()] = s_TmpMostAbundant;

            return b_Successful;
        }

        public bool CreateHeatmap()
        {
            bool b_Successful = true;

            CheckForPlotsDirectory();

            string Command = "",                
                s_FileName = Path.Combine(Model.WorkDirectory,
                    "Plots", Parameters[RequiredParameters.PlotFileName.ToString()]),
                s_MatrixFileName = "hm_" + Path.GetFileNameWithoutExtension(s_FileName);

            s_FileName = s_FileName.Replace('\\', '/');

            switch (m_PlotFileType.ToLower())
            {
                case "png":
                    Command += string.Format(
                        "CairoPNG(filename='{0}')\n",
                        s_FileName);
                    break;
                case "svg":
                    Command += string.Format(
                        "svg(filename='{0}')\n",
                        s_FileName);
                    break;
                case "ps":
                    Command += string.Format(
                        "cairo_ps(filename='{0}')\n",
                        s_FileName);
                    break;
            }  
          
            /// Setup heatmap.2 parameters
            Command += string.Format(
                "col <- colorRampPalette({0})\n" +
                "cmap <- col({1})\n" +
                "colscale <- seq({2}, {3}, length={1}+1)\n"
                , m_ColorPalette
                , m_ColorDegree
                , m_MinColorScale
                , m_MaxColorScale);

            Command += string.Format(
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
                , s_MatrixFileName
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


            Command += "dev.off()\n";

            try
            {
                b_Successful = Model.RCalls.Run(Command,
                    ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while performing Heatmap:\n" +
                    ex.ToString());
                SaveCurrentREnvironment();
                b_Successful = false;
            }

            if (!b_Successful)
                SaveCurrentREnvironment();

            return b_Successful;
        }

        public string GetDendrogram()
        {
            if (m_ClusterColumns.ToUpper().StartsWith("T") &&
                m_ClusterRows.ToUpper().StartsWith("T"))
                return "c('both')";
            else if (m_ClusterRows.ToUpper().StartsWith("T"))
                return "c('row')";
            else if (m_ClusterColumns.ToUpper().StartsWith("T"))
                return "c('column')";
            else
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
