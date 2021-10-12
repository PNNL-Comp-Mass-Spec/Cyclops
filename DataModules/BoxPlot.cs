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
    public class BoxPlot : BaseDataModule
    {
        #region Members
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

        #endregion

        #region Properties
        public string PlotFileName { get; set; }
        #endregion

        #region Constructors
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

                Model.LogMessage("Running BoxPlot", ModuleName, StepNumber);

                if (CheckParameters())
                    successful = BoxPlotFunction();
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

            #region Handling Factor Information
            if (Parameters.ContainsKey("ColorByFactor"))
                m_ColorByFactor = Parameters["ColorByFactor"].ToUpper();

            if (Parameters.ContainsKey("ColumnFactorTable"))
            {
                if (Model.RCalls.ContainsObject(
                    Parameters["ColumnFactorTable"]))
                {
                    m_ColumnFactorTable = Parameters["ColumnFactorTable"];
                }
            }

            if (Parameters.ContainsKey("FactorColumn"))
            {
                if (Model.RCalls.TableContainsColumn(
                    Parameters["ColumnFactorTable"],
                    Parameters["FactorColumn"]))
                {
                    m_FactorColumn = Parameters["FactorColumn"];
                }
            }

            if (!string.IsNullOrEmpty(m_ColumnFactorTable) &&
                !string.IsNullOrEmpty(m_FactorColumn) &&
                m_ColorByFactor.Equals("FALSE"))
            {
                m_ColorByFactor = "TRUE";
            }
            #endregion

            #region General Plot Parameters
            if (Parameters.ContainsKey("BackgroundColor"))
                BackgroundColor = Parameters["BackgroundColor"];
            if (Parameters.ContainsKey("Height"))
                Height = Convert.ToInt32(Parameters["Height"]);
            if (Parameters.ContainsKey("Width"))
                Width = Convert.ToInt32(Parameters["Width"]);
            if (Parameters.ContainsKey("FontSize"))
                FontSize = Convert.ToInt32(Parameters["FontSize"]);
            if (Parameters.ContainsKey("Resolution"))
                Resolution = Convert.ToInt32(Parameters["Resolution"]);
            if (Parameters.ContainsKey("Main"))
                Main = Parameters["Main"];
            if (Parameters.ContainsKey("XLabel"))
                XLabel = Parameters["XLabel"];
            if (Parameters.ContainsKey("YLabel"))
                YLabel = Parameters["YLabel"];
            #endregion

            #region Plot-specific Parameters
            if (Parameters.ContainsKey("DataColumns"))
                m_DataColumns = Parameters["DataColumns"];
            if (Parameters.ContainsKey("Outliers"))
                m_Outliers = Parameters["Outliers"].ToUpper();
            if (Parameters.ContainsKey("Color"))
                m_Color = Parameters["Color"];
            if (Parameters.ContainsKey("LabelScale"))
                m_LabelScale = Parameters["LabelScale"];
            if (Parameters.ContainsKey("BoxWidth"))
                m_BoxWidth = Parameters["BoxWidth"];
            if (Parameters.ContainsKey("ShowCount"))
                m_ShowCount = Parameters["ShowCount"].ToUpper();
            if (Parameters.ContainsKey("ShowLegend"))
                m_ShowLegend = Parameters["ShowLegend"].ToUpper();
            if (Parameters.ContainsKey("Stamp"))
                m_Stamp = Parameters["Stamp"];
            if (Parameters.ContainsKey("DoYLim"))
                m_DoYLim = Parameters["DoYLim"].ToUpper();
            if (Parameters.ContainsKey("yMin"))
                m_yMin = Parameters["yMin"];
            if (Parameters.ContainsKey("yMax"))
                m_yMax = Parameters["yMax"];
            #endregion

            if (Directory.Exists(Model.WorkDirectory))
            {
                var s_PlotDirectory = Path.Combine(
                    Model.WorkDirectory, "Plots").Replace("\\", "/");
                if (!Directory.Exists(s_PlotDirectory))
                    Directory.CreateDirectory(s_PlotDirectory);
                PlotFileName =
                    Path.Combine(s_PlotDirectory,
                    Parameters[RequiredParameters.PlotFileName.ToString()]).Replace("\\", "/");
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
        #endregion
    }
}
