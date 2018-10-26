/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/
 * -----------------------------------------------------
 *
 * Licensed under the 2-Clause BSD License; you may not use this
 * file except in compliance with the License.  You may obtain
 * a copy of the License at https://opensource.org/licenses/BSD-2-Clause
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cyclops.DataModules
{
    public class LBF_Summary_HTML : BaseDataModule
    {
        #region Enums
        private enum HTMLFileType { Dataset, Index };
        #endregion

        #region Members
        private readonly string m_ModuleName = "LBF_Summary_HTML";
        private readonly string m_Description = "";
        private readonly string m_TrypticTableSummaryName = "T_MAC_Trypticity_Summary";
        private string m_WorkingDirectory = "";
        private string m_DatabaseName = "Results.db3";

        // private DataTable m_Overlap = new DataTable("LBF");

        private bool m_LR;
        private bool m_CT;

        /// <summary>
        /// Required parameters to run LBF_Summary_HTML Module
        /// </summary>
        private enum RequiredParameters
        {
            FileName
        }

        #endregion

        #region Properties
        public Dictionary<string, string> FileNameVault { get; set; } = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an LBF_Summary_HTML Module
        /// </summary>
        public LBF_Summary_HTML()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// LBF_Summary_HTML module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public LBF_Summary_HTML(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// LBF_Summary_HTML module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public LBF_Summary_HTML(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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

                Model.LogMessage("Running LBF_Summary_HTML", ModuleName, StepNumber);

                if (CheckParameters())
                    successful = LBF_Summary_HTMLFunction();
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
                    Model.LogError("Required Field Missing: " + s, ModuleName, StepNumber);
                    return false;
                }
            }

            if (Parameters.ContainsKey("WorkDir"))
            {
                if (!string.IsNullOrEmpty(Parameters["WorkDir"]))
                    m_WorkingDirectory = Parameters["WorkDir"];
                else
                {
                    Model.LogError("Error in 'LBF_Summary_HTML', no 'WorkDir' supplied!", ModuleName, StepNumber);
                    successful = false;
                }
            }
            else
            {
                Model.LogError("Error in 'LBF_Summary_HTML', no 'WorkDir' supplied!", ModuleName, StepNumber);
                successful = false;
            }

            if (Parameters.ContainsKey("DatabaseName"))
            {
                if (!string.IsNullOrEmpty(Parameters["DatabaseName"]))
                    m_DatabaseName = Parameters["DatabaseName"];
            }

            return successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool LBF_Summary_HTMLFunction()
        {
            AddDefaultValues2FileNameVault();

            WriteCssFile();

            #region Datasets Page
            try
            {
                WriteDatasetsPage(GetOriginalNavBar());
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while " +
                    "constructing and writing Datasets HTML Page:\n" + ex,
                    ModuleName, StepNumber);
                return false;
            }
            #endregion

            #region Summary HTML Page
            try
            {
                WriteSummaryHTMLPage(GetOriginalNavBar());
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while " +
                    "constructing and writing Summary HTML Page:\n" + ex,
                    ModuleName, StepNumber);
                return false;
            }
            #endregion

            #region QC Page
            try
            {
                WriteQCHTMLPage(GetOriginalNavBar());
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while " +
                    "constructing and writing QC HTML Page:\n" + ex,
                    ModuleName, StepNumber);
                return false;
            }
            #endregion

            #region BoxPlots Page
            try
            {
                WriteBoxPlotHTMLPage(GetOriginalNavBar());
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while " +
                    "constructing and writing Boxplot HTML Page:\n" + ex,
                    ModuleName, StepNumber);
                return false;
            }
            #endregion

            #region Correlation Heatmaps Page
            try
            {
                WriteCorrelationHeatmapHTMLPage(GetOriginalNavBar());
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while " +
                    "constructing and writing Correlation Heatmap HTML Page:\n" + ex,
                    ModuleName, StepNumber);
                return false;
            }
            #endregion

            #region Main HTML Page
            try
            {
                WriteMainHTMLPage(GetOriginalNavBar());
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while " +
                    "constructing and writing Main HTML Page:\n" + ex,
                    ModuleName, StepNumber);
                return false;
            }
            #endregion

            return true;
        }

        private List<HtmlLinkNode> GetOriginalNavBar()
        {
            var navBarNodes = new List<HtmlLinkNode>
            {
                new HtmlLinkNode(
                    "Home", Parameters[RequiredParameters.FileName.ToString()], false),
                new HtmlLinkNode(
                    "Datasets", FileNameVault["DatasetsHtmlFileName"], false),
                new HtmlLinkNode(
                    "Summary Tables", FileNameVault["SummaryTableHtmlFileName"], false),
                new HtmlLinkNode(
                    "QC Plots", FileNameVault["QcHtmlFileName"], false),
                new HtmlLinkNode(
                    "Box Plots", FileNameVault["BoxPlotHtmlFileName"], false),
                new HtmlLinkNode(
                    "Correlation Heatmaps", FileNameVault["CorrelationHtmlFileName"], false)
            };


            return navBarNodes;
        }

        /// <summary>
        /// Sets the boolean values that indicate if normalization algorithms have been run.
        /// </summary>
        [Obsolete("Unused")]
        private void SetLRandCTflags()
        {
            m_LR = Model.RCalls.ContainsObject("LR_Log_T_Data");
            m_CT = Model.RCalls.ContainsObject("CT_Log_T_Data");
        }

        /// <summary>
        /// Adds default values to the FileNameVault library
        /// </summary>
        private void AddDefaultValues2FileNameVault()
        {
            FileNameVault.Add("CssFileName", "styles.css");
            FileNameVault.Add("MainFileName", "index.html");
            FileNameVault.Add("DatasetsHtmlFileName", "Datasets.html");
            FileNameVault.Add("SummaryTableHtmlFileName", "SummaryTables.html");
            FileNameVault.Add("QcHtmlFileName", "QC.html");
            FileNameVault.Add("BoxPlotHtmlFileName", "BoxPlots.html");
            FileNameVault.Add("CorrelationHtmlFileName", "Correlations.html");

            FileNameVault.Add("LbfAnalysisSummaryFigureFileName", "LBF_Analysis_Summary.png");
            FileNameVault.Add("LbfAnalysisSummaryFigureTableName", "T_MAC_MassTagID_Summary");
            FileNameVault.Add("LbfMissedCleavageFigureFileName", "MissedCleavage_Summary.png");
            FileNameVault.Add("LbfMissedCleavageFigureTableName", "T_MissedCleavageSummary");
            FileNameVault.Add("LbfTrypticSummaryFigureFileName", "Tryptic_Summary.png");
            FileNameVault.Add("LbfTrypticSummaryFigureTableName", "T_MAC_Trypticity_Summary");

            FileNameVault.Add("LbfBoxplotFigureFileName", "Boxplot_Log_T_Data.png");
            FileNameVault.Add("LbfBoxplotLRFigureFileName", "Boxplot_LR_Log_T_Data.png");
            FileNameVault.Add("LbfBoxplotCTFigureFileName", "Boxplot_CT_Log_T_Data.png");

            FileNameVault.Add("LbfBoxplotRrFigureFileName", "Boxplot_RR_Log_T_Data.png");
            FileNameVault.Add("LbfBoxplotRrLRFigureFileName", "Boxplot_RR_LR_Log_T_Data.png");
            FileNameVault.Add("LbfBoxplotRrCTFigureFileName", "Boxplot_RR_CT_Log_T_Data.png");

            FileNameVault.Add("LbfCorrelationHeatmapFigureFileName", "Log_T_Data_CorrelationHeatmap.png");
            FileNameVault.Add("LbfCorrelationHeatmapLRFigureFileName", "LR_Log_T_Data_CorrelationHeatmap.png");
            FileNameVault.Add("LbfCorrelationHeatmapCTFigureFileName", "CT_Log_T_Data_CorrelationHeatmap.png");

            FileNameVault.Add("LbfCorrelationHeatmapRrFigureFileName", "RR_Log_T_Data_CorrelationHeatmap.png");
            FileNameVault.Add("LbfCorrelationHeatmapRrLRFigureFileName", "RR_LR_Log_T_Data_CorrelationHeatmap.png");
            FileNameVault.Add("LbfCorrelationHeatmapRrCTFigureFileName", "RR_CT_Log_T_Data_CorrelationHeatmap.png");
        }

        /// <summary>
        /// Writes out the CSS file to the working directory
        /// </summary>
        private void WriteCssFile()
        {
            using (var sw = File.AppendText(Path.Combine(m_WorkingDirectory, FileNameVault["CssFileName"])))
            {
                sw.WriteLine(HtmlFileHandler.GetCSS(HtmlFileHandler.CssStyle.NavBar, 250));
                sw.WriteLine(HtmlFileHandler.GetCSS(HtmlFileHandler.CssStyle.LeftIndent, 250));
                sw.WriteLine(HtmlFileHandler.GetCSS(HtmlFileHandler.CssStyle.Th, 250));
            }
        }

        /// <summary>
        /// Construct and write-out the Datasets Page
        /// </summary>
        /// <param name="NavBar">HTML Navigation Bar</param>
        private void WriteDatasetsPage(List<HtmlLinkNode> NavBar)
        {
            var scriptHtml = new StringBuilder();

            scriptHtml.Append(HtmlFileHandler.GetHtmlHeader());
            scriptHtml.Append(HtmlFileHandler.GetCSSLink(FileNameVault["CssFileName"]));
            scriptHtml.Append(HtmlFileHandler.GetHtmlJavascriptStart());

            scriptHtml.Append(HtmlFileHandler.GetHtmlScriptEnd());
            scriptHtml.Append(HtmlFileHandler.GetEndHeadStartBody());
            scriptHtml.Append(HtmlFileHandler.GetNavTable(NavBar));
            //scriptHtml.Append(HtmlFileHandler.GetNavBar(navBarNodes, "LEFT"));

            scriptHtml.Append("<DIV ID='main_content'>\n");
            scriptHtml.Append(HtmlFileHandler.GetDatasetTableHtml(
                Path.Combine(m_WorkingDirectory, m_DatabaseName), "", "table_header", "left", 1, 1, 1));
            scriptHtml.Append("</DIV>\n");
            scriptHtml.Append(HtmlFileHandler.GetEndBodyEndHtml());

            var htmlWriter = new StreamWriter(Path.Combine(m_WorkingDirectory, FileNameVault["DatasetsHtmlFileName"]));
            htmlWriter.WriteLine(scriptHtml);
            htmlWriter.Close();
        }

        /// <summary>
        /// Construct and write-out the Summary HTML Page
        /// </summary>
        /// <param name="NavBar">HTML Navigation Bar</param>
        private void WriteSummaryHTMLPage(List<HtmlLinkNode> NavBar)
        {
            NavBar.Add(new HtmlLinkNode("Peptide Original", "pepOrig", true));
            NavBar.Add(new HtmlLinkNode("Peptide Log2", "peplog2", true));
            if (m_CT)
            {
                NavBar.Add(new HtmlLinkNode("Peptide CT", "pepCT", true));
            }
            if (m_LR)
            {
                NavBar.Add(new HtmlLinkNode("Peptide LR", "pepLR", true));
            }
            NavBar.Add(new HtmlLinkNode("RRollup Protein", "protRR", true));
            if (m_CT)
            {
                NavBar.Add(new HtmlLinkNode("RRollup CT Protein", "protRRCT", true));
            }
            if (m_LR)
            {
                NavBar.Add(new HtmlLinkNode("RRollup LR Protein", "protRRLR", true));
            }

            var summaryHtml = new StringBuilder();
            summaryHtml.Append(HtmlFileHandler.GetHtmlHeader());
            summaryHtml.Append(HtmlFileHandler.GetCSSLink(FileNameVault["CssFileName"]));
            summaryHtml.Append(HtmlFileHandler.GetHtmlJavascriptStart());

            summaryHtml.Append(HtmlFileHandler.GetHtmlScriptEnd());
            summaryHtml.Append(HtmlFileHandler.GetEndHeadStartBody());
            summaryHtml.Append(HtmlFileHandler.GetNavTable(NavBar));
            //summaryHtml.Append(HtmlFileHandler.GetNavBar(navBarNodes, "LEFT"));

            summaryHtml.Append("<DIV ID='main_content'>\n");
            summaryHtml.Append("<A NAME='pepOrig' /A>\n");

            summaryHtml.Append(
                HtmlFileHandler.GetSummaryTableHtml(
                    Model.RCalls.GetDataTableIncludingRowNames(
                        "Summary_T_Data$TotalSummary", "QC_Params"),
                    "Summary of Original Peptide Abundances", "table_header",
                    1, 1, 1));

            summaryHtml.Append("<A NAME='peplog2' /A>\n");
            summaryHtml.Append(
                HtmlFileHandler.GetSummaryTableHtml(
                    Model.RCalls.GetDataTableIncludingRowNames(
                        "Summary_Log_T_Data$TotalSummary", "QC_Params"),
                    "Summary of Log2 Peptide Abundances", "table_header",
                    1, 1, 1));

            if (m_CT)
            {
                summaryHtml.Append("<A NAME='pepCT' /A>\n");
                summaryHtml.Append(
                    HtmlFileHandler.GetSummaryTableHtml(
                        Model.RCalls.GetDataTableIncludingRowNames(
                            "Summary_CT_Log_T_Data$TotalSummary", "QC_Params"),
                        "Summary of Central Tendency Log2 Peptide Abundances", "table_header",
                        1, 1, 1));
            }

            if (m_LR)
            {
                summaryHtml.Append("<A NAME='pepLR' /A>\n");
                summaryHtml.Append(
                    HtmlFileHandler.GetSummaryTableHtml(
                        Model.RCalls.GetDataTableIncludingRowNames(
                            "Summary_LR_Log_T_Data$TotalSummary", "QC_Params"),
                        "Summary of Linear Regression Log2 Peptide Abundances", "table_header",
                        1, 1, 1));
            }

            // Proteins
            summaryHtml.Append("<A NAME='protRR' /A>\n");
            summaryHtml.Append(
                HtmlFileHandler.GetSummaryTableHtml(
                    Model.RCalls.GetDataTableIncludingRowNames(
                        "Summary_RR_Log_T_Data$TotalSummary", "QC_Params"),
                    "Summary of Protein Abundances from Log2 Peptides (RRollup)", "table_header",
                    1, 1, 1));

            if (m_CT)
            {
                summaryHtml.Append("<A NAME='protRRCT' /A>\n");
                summaryHtml.Append(
                    HtmlFileHandler.GetSummaryTableHtml(
                        Model.RCalls.GetDataTableIncludingRowNames(
                            "Summary_RR_CT_Log_T_Data$TotalSummary", "QC_Params"),
                        "Summary of Protein Abundances from Central Tendency Log2 Peptides Abundances (RRollup)", "table_header",
                        1, 1, 1));
            }

            if (m_LR)
            {
                summaryHtml.Append("<A NAME='protRRLR' /A>\n");
                summaryHtml.Append(
                    HtmlFileHandler.GetSummaryTableHtml(
                        Model.RCalls.GetDataTableIncludingRowNames(
                            "Summary_RR_LR_Log_T_Data$TotalSummary", "QC_Params"),
                        "Summary of Protein Abundances from Linear Regression Log2 Peptide Abundances (RRollup)", "table_header",
                        1, 1, 1));
            }

            summaryHtml.Append("</DIV>\n");

            summaryHtml.Append(HtmlFileHandler.GetEndBodyEndHtml());

            var summaryWriter = new StreamWriter(Path.Combine(m_WorkingDirectory, FileNameVault["SummaryTableHtmlFileName"]));
            summaryWriter.Write(summaryHtml);
            summaryWriter.Close();
        }

        /// <summary>
        /// Construct and write-out the QC HTML Page
        /// </summary>
        /// <param name="NavBar">HTML Navigation Bar</param>
        private void WriteQCHTMLPage(List<HtmlLinkNode> NavBar)
        {
            var containsTrypticPeptideSummary =
                Model.ModuleLoader.SQLiteDatabase.TableExists(m_TrypticTableSummaryName);

            NavBar.Add(new HtmlLinkNode("LBF Summary", "sum", true));
            NavBar.Add(new HtmlLinkNode("Missed Cleavages", "mc", true));
            NavBar.Add(new HtmlLinkNode("Tryptic Peptides", "tp", true));

            var qcHtml = new StringBuilder();
            qcHtml.Append(HtmlFileHandler.GetHtmlHeader());
            qcHtml.Append(HtmlFileHandler.GetHtmlJavascriptStart());

            qcHtml.Append(HtmlFileHandler.GetHtmlScriptEnd());
            qcHtml.Append(HtmlFileHandler.GetCSSLink(FileNameVault["CssFileName"]));
            qcHtml.Append(HtmlFileHandler.GetEndHeadStartBody());
            qcHtml.Append(HtmlFileHandler.GetNavTable(NavBar));
            //qcHtml.Append(HtmlFileHandler.GetNavBar(navBarNodes, "LEFT"));
            qcHtml.Append(WriteHtmlBody(HTMLFileType.Index));

            qcHtml.Append("<DIV ID='main_content'>\n");
            qcHtml.Append("\t\t<A NAME='sum' />\n");
            qcHtml.Append(
                HtmlFileHandler.GetQCElement(
                    "Label-free Analysis Summary",
                    "table_header",
                    FileNameVault["LbfAnalysisSummaryFigureFileName"],
                    Model.ModuleLoader.SQLiteDatabase.GetTable(
                        FileNameVault["LbfAnalysisSummaryFigureTableName"]),
                    1, 1, 1));

            qcHtml.Append("\t\t<A NAME='mc'/A>\n");
            qcHtml.Append(
                HtmlFileHandler.GetQCElement(
                    "Missed Cleavage Summary",
                    "table_header",
                    FileNameVault["LbfMissedCleavageFigureFileName"],
                    Model.ModuleLoader.SQLiteDatabase.GetTable(
                        FileNameVault["LbfMissedCleavageFigureTableName"]),
                    1, 1, 1));

            if (containsTrypticPeptideSummary)
            {
                qcHtml.Append("\t\t<A NAME='tp'/A>\n");
                qcHtml.Append(
                    HtmlFileHandler.GetQCElement(
                        "Tryptic Peptide Summary",
                        "table_header",
                        FileNameVault["LbfTrypticSummaryFigureFileName"],
                        Model.ModuleLoader.SQLiteDatabase.GetTable(
                            FileNameVault["LbfTrypticSummaryFigureTableName"]),
                        1, 1, 1));
            }

            qcHtml.Append("</DIV>\n");
            qcHtml.Append(HtmlFileHandler.GetEndBodyEndHtml());

            var qcHtmlWriter = new StreamWriter(Path.Combine(m_WorkingDirectory, FileNameVault["QcHtmlFileName"]));
            qcHtmlWriter.WriteLine(qcHtml);
            qcHtmlWriter.Close();
        }

        /// <summary>
        /// Construct and write-out the Main HTML Page
        /// </summary>
        /// <param name="NavBar">HTML Navigation Bar</param>
        private void WriteMainHTMLPage(List<HtmlLinkNode> NavBar)
        {
            var mainHtml = new StringBuilder();
            mainHtml.Append(HtmlFileHandler.GetHtmlHeader());
            mainHtml.Append(HtmlFileHandler.GetHtmlJavascriptStart());

            mainHtml.Append(HtmlFileHandler.GetHtmlScriptEnd());
            mainHtml.Append(HtmlFileHandler.GetCSSLink(FileNameVault["CssFileName"]));
            mainHtml.Append(HtmlFileHandler.GetEndHeadStartBody());
            mainHtml.Append(HtmlFileHandler.GetNavTable(NavBar));
            //mainHtml.Append(HtmlFileHandler.GetNavBar(navBarNodes, "LEFT"));
            mainHtml.Append(WriteHtmlBody(HTMLFileType.Index));

            mainHtml.Append("<DIV ID='main_content'>\n");

            // Add to the index page here...

            mainHtml.Append("</DIV>\n");

            mainHtml.Append(HtmlFileHandler.GetEndBodyEndHtml());

            var htmlWriter = new StreamWriter(Path.Combine(m_WorkingDirectory, FileNameVault["MainFileName"]));
            htmlWriter.WriteLine(mainHtml);
            htmlWriter.Close();
        }

        /// <summary>
        /// Construct and write-out the BoxPlot HTML Page
        /// </summary>
        /// <param name="NavBar">HTML Navigation Bar</param>
        private void WriteBoxPlotHTMLPage(List<HtmlLinkNode> NavBar)
        {
            NavBar.Add(new HtmlLinkNode("Pep Log2 Boxplot", "log2bp", true));
            if (m_LR)
            {
                NavBar.Add(new HtmlLinkNode("Pep LR Boxplot", "lrlog2bp", true));
            }
            if (m_CT)
            {
                NavBar.Add(new HtmlLinkNode("Pep CT Boxplot", "ctlog2bp", true));
            }
            NavBar.Add(new HtmlLinkNode("Prot Log2 Boxplot", "protbp", true));
            if (m_LR)
            {
                NavBar.Add(new HtmlLinkNode("Prot LR Boxplot", "protlrbp", true));
            }
            if (m_CT)
            {
                NavBar.Add(new HtmlLinkNode("Prot CT Boxplot", "protctbp", true));
            }

            var boxPlotHtml = new StringBuilder();

            boxPlotHtml.Append(HtmlFileHandler.GetHtmlHeader());
            boxPlotHtml.Append(HtmlFileHandler.GetHtmlJavascriptStart());

            boxPlotHtml.Append(HtmlFileHandler.GetHtmlScriptEnd());
            boxPlotHtml.Append(HtmlFileHandler.GetCSSLink(FileNameVault["CssFileName"]));
            boxPlotHtml.Append(HtmlFileHandler.GetEndHeadStartBody());
            boxPlotHtml.Append(HtmlFileHandler.GetNavTable(NavBar));
            //boxPlotHtml.Append(HtmlFileHandler.GetNavBar(navBarNodes, "LEFT"));
            boxPlotHtml.Append(WriteHtmlBody(HTMLFileType.Index));

            boxPlotHtml.Append("<DIV ID='main_content'>\n");
            boxPlotHtml.Append("\t\t<A NAME='log2bp'/A>\n");
            boxPlotHtml.Append("\t\t<DIV>\n");
            boxPlotHtml.Append("\t\t<P ID='table_header'>Peptide Log2 Box Plot</P>\n");
            boxPlotHtml.Append(HtmlFileHandler.GetPictureCode(
                FileNameVault["LbfBoxplotFigureFileName"],
                true, "pos_left", null, null) + "\n");
            boxPlotHtml.Append("\t\t</DIV>\n");
            if (m_LR)
            {
                boxPlotHtml.Append("\t\t<A NAME='lrlog2bp'/A>\n");
                boxPlotHtml.Append("\t\t<DIV>\n");
                boxPlotHtml.Append("\t\t<P ID='table_header'>Peptide Linear Regression Log2 Box Plot</P>\n");
                boxPlotHtml.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["LbfBoxplotLRFigureFileName"],
                    true, "pos_left", null, null) + "\n");
                boxPlotHtml.Append("\t\t</DIV>\n");
            }
            if (m_CT)
            {
                boxPlotHtml.Append("\t\t<A NAME='ctlog2bp'/A>\n");
                boxPlotHtml.Append("\t\t<DIV>\n");
                boxPlotHtml.Append("\t\t<P ID='table_header'>Peptide Central Tendency Log2 Box Plot</P>\n");
                boxPlotHtml.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["LbfBoxplotCTFigureFileName"],
                    true, "pos_left", null, null) + "\n");
                boxPlotHtml.Append("\t\t</DIV>\n");
            }

            boxPlotHtml.Append("\t\t<A NAME='protbp'/A>\n");
            boxPlotHtml.Append("\t\t<DIV>\n");
            boxPlotHtml.Append("\t\t<P ID='table_header'>Protein Log2 Box Plot</P>\n");
            boxPlotHtml.Append(HtmlFileHandler.GetPictureCode(
                FileNameVault["LbfBoxplotRrFigureFileName"],
                true, "pos_left", null, null) + "\n");
            boxPlotHtml.Append("\t\t</DIV>\n");

            if (m_LR)
            {
                boxPlotHtml.Append("\t\t<A NAME='protlrbp'/A>\n");
                boxPlotHtml.Append("\t\t<DIV>\n");
                boxPlotHtml.Append("\t\t<P ID='table_header'>Protein Linear Regression Log2 Box Plot</P>\n");
                boxPlotHtml.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["LbfBoxplotRrLRFigureFileName"],
                    true, "pos_left", null, null) + "\n");
                boxPlotHtml.Append("\t\t</DIV>\n");
            }
            if (m_CT)
            {
                boxPlotHtml.Append("\t\t<A NAME='protctbp'/A>\n");
                boxPlotHtml.Append("\t\t<DIV>\n");
                boxPlotHtml.Append("\t\t<P ID='table_header'>Protein Central Tendency Log2 Box Plot</P>\n");
                boxPlotHtml.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["LbfBoxplotRrCTFigureFileName"],
                    true, "pos_left", null, null) + "\n");
                boxPlotHtml.Append("\t\t</DIV>\n");
            }

            boxPlotHtml.Append("</DIV>\n");

            boxPlotHtml.Append(HtmlFileHandler.GetEndBodyEndHtml());

            var boxPlotWriter = new StreamWriter(Path.Combine(m_WorkingDirectory, FileNameVault["BoxPlotHtmlFileName"]));
            boxPlotWriter.WriteLine(boxPlotHtml);
            boxPlotWriter.Close();
        }

        /// <summary>
        /// Construct and write-out the Correlation Heatmap HTML Page
        /// </summary>
        /// <param name="NavBar">HTML Navigation Bar</param>
        private void WriteCorrelationHeatmapHTMLPage(List<HtmlLinkNode> NavBar)
        {
            NavBar.Add(new HtmlLinkNode("Pep Log2 Corr", "log2ch", true));
            if (m_LR)
            {
                NavBar.Add(new HtmlLinkNode("Pep LR Corr", "lrlog2ch", true));
            }
            if (m_CT)
            {
                NavBar.Add(new HtmlLinkNode("Pep CT Corr", "ctlog2ch", true));
            }
            NavBar.Add(new HtmlLinkNode("Prot Log2 Corr", "protch", true));
            if (m_LR)
            {
                NavBar.Add(new HtmlLinkNode("Prot LR Corr", "protlrch", true));
            }
            if (m_CT)
            {
                NavBar.Add(new HtmlLinkNode("Prot CT Corr", "protctch", true));
            }

            var heatmapHtml = new StringBuilder();
            heatmapHtml.Append(HtmlFileHandler.GetHtmlHeader());
            heatmapHtml.Append(HtmlFileHandler.GetHtmlJavascriptStart());

            heatmapHtml.Append(HtmlFileHandler.GetHtmlScriptEnd());
            heatmapHtml.Append(HtmlFileHandler.GetCSSLink(FileNameVault["CssFileName"]));
            heatmapHtml.Append(HtmlFileHandler.GetEndHeadStartBody());
            heatmapHtml.Append(HtmlFileHandler.GetNavTable(NavBar));
            //heatmapHtml.Append(HtmlFileHandler.GetNavBar(navBarNodes, "LEFT"));
            heatmapHtml.Append(WriteHtmlBody(HTMLFileType.Index));

            heatmapHtml.Append("<DIV ID='main_content'>\n");
            heatmapHtml.Append("\t\t<A NAME='log2ch'/A>\n");
            heatmapHtml.Append("\t\t<DIV>\n");
            heatmapHtml.Append("\t\t<P ID='table_header'>Peptide Log2 Correlation Heatmap</P>\n");
            heatmapHtml.Append(HtmlFileHandler.GetPictureCode(
                FileNameVault["LbfCorrelationHeatmapFigureFileName"],
                true, "pos_left", null, null) + "\n");
            heatmapHtml.Append("\t\t</DIV>\n");
            if (m_LR)
            {
                heatmapHtml.Append("\t\t<A NAME='lrlog2ch'/A>\n");
                heatmapHtml.Append("\t\t<DIV>\n");
                heatmapHtml.Append("\t\t<P ID='table_header'>Peptide Linear Regression Log2 Correlation Heatmap</P>\n");
                heatmapHtml.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["LbfCorrelationHeatmapLRFigureFileName"],
                    true, "pos_left", null, null) + "\n");
                heatmapHtml.Append("\t\t</DIV>\n");
            }
            if (m_CT)
            {
                heatmapHtml.Append("\t\t<A NAME='ctlog2ch'/A>\n");
                heatmapHtml.Append("\t\t<DIV>\n");
                heatmapHtml.Append("\t\t<P ID='table_header'>Peptide Central Tendency Log2 Correlation Heatmap</P>\n");
                heatmapHtml.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["LbfCorrelationHeatmapCTFigureFileName"],
                    true, "pos_left", null, null) + "\n");
                heatmapHtml.Append("\t\t</DIV>\n");
            }

            heatmapHtml.Append("\t\t<A NAME='protch'/A>\n");
            heatmapHtml.Append("\t\t<DIV>\n");
            heatmapHtml.Append("\t\t<P ID='table_header'>Protein Log2 Correlation Heatmap</P>\n");
            heatmapHtml.Append(HtmlFileHandler.GetPictureCode(
                FileNameVault["LbfCorrelationHeatmapRrFigureFileName"],
                true, "pos_left", null, null) + "\n");
            heatmapHtml.Append("\t\t</DIV>\n");

            if (m_LR)
            {
                heatmapHtml.Append("\t\t<A NAME='protlrch'/A>\n");
                heatmapHtml.Append("\t\t<DIV>\n");
                heatmapHtml.Append("\t\t<P ID='table_header'>Protein Linear Regression Log2 Correlation Heatmap</P>\n");
                heatmapHtml.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["LbfCorrelationHeatmapRrLRFigureFileName"],
                    true, "pos_left", null, null) + "\n");
                heatmapHtml.Append("\t\t</DIV>\n");
            }
            if (m_CT)
            {
                heatmapHtml.Append("\t\t<A NAME='protctch'/A>\n");
                heatmapHtml.Append("\t\t<DIV>\n");
                heatmapHtml.Append("\t\t<P ID='table_header'>Protein Central Tendency Log2 Correlation Heatmap</P>\n");
                heatmapHtml.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["LbfCorrelationHeatmapRrCTFigureFileName"],
                    true, "pos_left", null, null) + "\n");
                heatmapHtml.Append("\t\t</DIV>\n");
            }

            heatmapHtml.Append("</DIV>\n");

            heatmapHtml.Append(HtmlFileHandler.GetEndBodyEndHtml());

            var heatmapWriter = new StreamWriter(Path.Combine(m_WorkingDirectory, FileNameVault["CorrelationHtmlFileName"]));
            heatmapWriter.WriteLine(heatmapHtml);
            heatmapWriter.Close();
        }

        private string WriteHtmlBody(HTMLFileType htmlFileType)
        {
            var body = "";
            switch (htmlFileType)
            {
                case HTMLFileType.Dataset:
                    body = HtmlFileHandler.GetDatasetTableHtml(
                        Path.Combine(m_WorkingDirectory, m_DatabaseName), null,
                            "table_header", "center", 0, 2, 4);
                    break;
                case HTMLFileType.Index:

                    break;
            }

            return body;
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
