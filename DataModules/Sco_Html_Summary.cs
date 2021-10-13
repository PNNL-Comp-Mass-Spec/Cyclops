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
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;

namespace Cyclops.DataModules
{
    public class Sco_Html_Summary : BaseDataModule
    {
        /// <summary>
        /// Required parameters to run Aggregate
        /// </summary>
        private enum RequiredParameters
        { }

        private enum HTMLFileType { Dataset, Index };

        private readonly string m_ModuleName = "Sco_HTML_Summary";
        private readonly string m_Description = "";

        private bool m_DatabaseFound;
        private readonly SQLiteHandler m_SQLiteReader = new SQLiteHandler();

        public Dictionary<string, string> FileNameVault { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Generic constructor creating an Sco_Html_Summary Module
        /// </summary>
        public Sco_Html_Summary()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// Sco_Html_Summary module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public Sco_Html_Summary(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// Sco_Html_Summary module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public Sco_Html_Summary(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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

                AddDefaultValues2FileNameVault();

                if (CheckParameters())
                {
                    successful = Sco_Html_SummaryFunction();
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

            if (Parameters.ContainsKey("FileName") && !string.IsNullOrEmpty(Parameters["FileName"]))
            {
                FileNameVault["MainFileName"] = Parameters["FileName"];
            }

            if (Parameters.ContainsKey("DatabaseFileName"))
            {
                var databaseFile = Parameters["DatabaseFileName"];
                if (File.Exists(databaseFile))
                {
                    m_SQLiteReader.DatabaseFileName = databaseFile;
                    m_DatabaseFound = true;
                }
                else if (File.Exists(Path.Combine(Model.WorkDirectory, databaseFile)))
                {
                    m_SQLiteReader.DatabaseFileName = Path.Combine(Model.WorkDirectory, databaseFile);
                    m_DatabaseFound = true;
                }
            }
            else
            {
                if (File.Exists(Path.Combine(Model.WorkDirectory, "Results.db3")))
                {
                    m_SQLiteReader.DatabaseFileName = Path.Combine(Model.WorkDirectory, "Results.db3");
                    Parameters.Add("DatabaseFileName", m_SQLiteReader.DatabaseFileName);
                    m_DatabaseFound = true;
                }
            }

            if (!m_DatabaseFound)
            {
                Model.LogError("Unable to establish successful database connection!", ModuleName, StepNumber);
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool Sco_Html_SummaryFunction()
        {
            WriteCssFile();

            try
            {
                WriteDatasetsPage(GetOriginalNavBar());
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while constructing the Datasets HTML Summary Page: " + ex,
                    ModuleName, StepNumber);
                return false;
            }

            try
            {
                WriteQCHTMLPage(GetOriginalNavBar());
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while constructing the Quality Control HTML Summary Page: " + ex,
                    ModuleName, StepNumber);
                return false;
            }

            try
            {
                WriteCorrelationHeatmapHTMLPage(GetOriginalNavBar());
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while constructing the Correlation HTML Summary Page: " + ex,
                    ModuleName, StepNumber);
                return false;
            }

            try
            {
                WriteHeatmapsPage(GetOriginalNavBar());
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while constructing the Heatmaps HTML Summary Page: " + ex,
                    ModuleName, StepNumber);
                return false;
            }

            try
            {
                WriteMainHTMLPage(GetOriginalNavBar());
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while constructing the Main HTML Summary Page: " + ex,
                    ModuleName, StepNumber);
                return false;
            }

            return true;
        }

        private List<HtmlLinkNode> GetOriginalNavBar()
        {
            var navBarNodes = new List<HtmlLinkNode>
            {
                new HtmlLinkNode("Home", FileNameVault["MainFileName"], false),
                new HtmlLinkNode("Datasets", FileNameVault["DatasetsHtmlFileName"], false),
                new HtmlLinkNode("QC Plots", FileNameVault["QcHtmlFileName"], false),
                new HtmlLinkNode("Correlation Heatmaps", FileNameVault["CorrelationHtmlFileName"], false),
                new HtmlLinkNode("Protein Heatmaps", FileNameVault["HeatmapsFileName"], false)
            };

            return navBarNodes;
        }

        /// <summary>
        /// Adds default values to the FileNameVault library
        /// </summary>
        private void AddDefaultValues2FileNameVault()
        {
            FileNameVault.Add("CssFileName", "styles.css");
            FileNameVault.Add("MainFileName", "index.html");
            FileNameVault.Add("DatasetsHtmlFileName", "Datasets.html");
            FileNameVault.Add("QcHtmlFileName", "QC.html");
            FileNameVault.Add("CorrelationHtmlFileName", "Correlations.html");
            FileNameVault.Add("HeatmapsFileName", "Heatmaps.html");
            FileNameVault.Add("SpectralCountSummaryFigureFileName", "Spectral_Count_Summary.png");
            FileNameVault.Add("MissedCleavageSummaryFigureFileName", "MissedCleavage_Summary.png");
            FileNameVault.Add("MissedCleavageSummary10percentFdrFigureFileName", "MissedCleavage_Summary_10.png");
            FileNameVault.Add("MissedCleavageSummary05percentFdrFigureFileName", "MissedCleavage_Summary_5.png");
            FileNameVault.Add("MissedCleavageSummary01percentFdrFigureFileName", "MissedCleavage_Summary_1.png");
            FileNameVault.Add("TrypticPeptideSummaryFigureFileName", "Tryptic_Summary.png");
            FileNameVault.Add("FilteredMsgfPpmHexbinFigureFileName", "Filtered_MSGF_vs_PPM.png");
            FileNameVault.Add("FilteredMsgfPpmHexbin10percentFdrFigureFileName", "Filtered_10_MSGF_vs_PPM.png");
            FileNameVault.Add("FilteredMsgfPpmHexbin05percentFdrFigureFileName", "Filtered_5_MSGF_vs_PPM.png");
            FileNameVault.Add("FilteredMsgfPpmHexbin01percentFdrFigureFileName", "Filtered_1_MSGF_vs_PPM.png");

            FileNameVault.Add("SpectralCountsCorrelationHeatmapFigureFileName", "T_SpectralCounts_CorrelationHeatmap.png");
            FileNameVault.Add("SpectralCountsCorrelationHeatmap10percentFdrFigureFileName", "T_SpectralCounts_10_CorrelationHeatmap.png");
            FileNameVault.Add("SpectralCountsCorrelationHeatmap05percentFdrFigureFileName", "T_SpectralCounts_5_CorrelationHeatmap.png");
            FileNameVault.Add("SpectralCountsCorrelationHeatmap01percentFdrFigureFileName", "T_SpectralCounts_1_CorrelationHeatmap.png");

            FileNameVault.Add("RowMetadataTablePeptideColumnName", "Peptide");
            FileNameVault.Add("RowMetadataTableProteinColumnName", "Protein");
            FileNameVault.Add("RowMetadataTableAltProteinColumnName", "Reference");
            FileNameVault.Add("RowMetadataTable10FDR", "T_RowMetadata_10");
            FileNameVault.Add("RowMetadataTable05FDR", "T_RowMetadata_5");
            FileNameVault.Add("RowMetadataTable01FDR", "T_RowMetadata_1");

            FileNameVault.Add("BBM_Pvals", "BBM_Pvals");
            FileNameVault.Add("BBM_AdjPvals", "BBM_AdjPvals");
            FileNameVault.Add("BbmResultsFdr01", "BBM_QuasiTel_Results_FDR01");
            FileNameVault.Add("AggBbmResultsFdr01", "Agg_BBM_QuasiTel_Results_FDR01");

            FileNameVault.Add("BBM_Heatmap_01_FigureFileName", "BBM_Heatmap_LT01.png");
            FileNameVault.Add("BBM_Heatmap_05_FigureFileName", "BBM_Heatmap_LT05.png");
        }

        /// <summary>
        /// Writes out the CSS file to the working directory
        /// </summary>
        private void WriteCssFile()
        {
            using (var cssWriter = File.AppendText(Path.Combine(Model.WorkDirectory, FileNameVault["CssFileName"])))
            {
                cssWriter.WriteLine(HtmlFileHandler.GetCSS(HtmlFileHandler.CssStyle.NavBar, 160));
                cssWriter.WriteLine(HtmlFileHandler.GetCSS(HtmlFileHandler.CssStyle.LeftIndent, 160));
                cssWriter.WriteLine(HtmlFileHandler.GetCSS(HtmlFileHandler.CssStyle.Th, 160));
            }
        }

        private void WriteDatasetsPage(List<HtmlLinkNode> NavBar)
        {
            // Construct and write-out the Datasets Page
            var datasetHtml = new StringBuilder();

            datasetHtml.Append(HtmlFileHandler.GetHtmlHeader());
            datasetHtml.Append(HtmlFileHandler.GetCSSLink(FileNameVault["CssFileName"]));
            datasetHtml.Append(HtmlFileHandler.GetHtmlJavascriptStart());
            datasetHtml.Append(WriteHtmlScripts());
            datasetHtml.Append(HtmlFileHandler.GetHtmlScriptEnd());
            datasetHtml.Append(HtmlFileHandler.GetEndHeadStartBody());
            datasetHtml.Append(HtmlFileHandler.GetNavTable(NavBar));
            //datasetHtml.Append(HtmlFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
            datasetHtml.Append("\t\t<DIV ID='main_content'>\n");
            datasetHtml.Append(WriteHtmlBody(HTMLFileType.Dataset));
            datasetHtml.Append("\t\t</DIV>\n");
            datasetHtml.Append(HtmlFileHandler.GetEndBodyEndHtml());

            var datasetWriter = new StreamWriter(Path.Combine(Model.WorkDirectory, FileNameVault["DatasetsHtmlFileName"]));
            datasetWriter.Write(datasetHtml);
            datasetWriter.Close();
        }

        /// <summary>
        /// Construct and write-out the QC HTML Page
        /// </summary>
        /// <param name="NavBar">HTML Navigation Bar</param>
        private void WriteQCHTMLPage(List<HtmlLinkNode> NavBar)
        {
            // Construct and write-out the QC Page
            var qcHtml = new StringBuilder();

            NavBar.Add(new HtmlLinkNode("Summary", "sum", true));
            NavBar.Add(new HtmlLinkNode("Missed Cleavages", "mc", true));
            NavBar.Add(new HtmlLinkNode("Tryptic Peptides", "tp", true));
            NavBar.Add(new HtmlLinkNode("Hexbin", "hb", true));

            qcHtml.Append(HtmlFileHandler.GetHtmlHeader());
            qcHtml.Append(HtmlFileHandler.GetHtmlJavascriptStart());
            qcHtml.Append(WriteHtmlScripts());
            qcHtml.Append(HtmlFileHandler.GetHtmlScriptEnd());
            qcHtml.Append(HtmlFileHandler.GetCSSLink(FileNameVault["CssFileName"]));
            qcHtml.Append(HtmlFileHandler.GetEndHeadStartBody());
            qcHtml.Append(HtmlFileHandler.GetNavTable(NavBar));
            //qcHtml.Append(HtmlFileHandler.GetNavBar(navBarNodes, "LEFT"));
            qcHtml.Append(WriteHtmlBody(HTMLFileType.Index));

            qcHtml.Append("\t<DIV ID='main_content'>\n");

            // Spectral Count Summary
            qcHtml.Append("\t\t<A NAME='sum' />\n");
            qcHtml.Append("\t\t<DIV ID='SpectralCount'>\n");
            qcHtml.Append(HtmlFileHandler.GetQCElement("Spectral Count Summary"
                , "table_header"
                , FileNameVault["SpectralCountSummaryFigureFileName"]
                , m_SQLiteReader.GetTable("T_MAC_SpecCnt_Summary")
                , 1, 1, 1));
            qcHtml.Append("\t\t</DIV>\n");

            // Missed Cleavages
            qcHtml.Append("\t\t<A NAME='mc'/A>\n");
            if (File.Exists(Path.Combine(Model.WorkDirectory, "Plots",
                FileNameVault["MissedCleavageSummaryFigureFileName"])))
            {
                qcHtml.Append(HtmlFileHandler.GetQCElement(
                    "Missed Cleavage Summary"
                    , "table_header"
                    , FileNameVault["MissedCleavageSummaryFigureFileName"]
                    , m_SQLiteReader.GetTable("T_MissedCleavageSummary")
                    , 1, 1, 1));
            }
            if (File.Exists(Path.Combine(Model.WorkDirectory, "Plots",
                FileNameVault["MissedCleavageSummary10percentFdrFigureFileName"])))
            {
                qcHtml.Append(HtmlFileHandler.GetQCElement(
                    "10% FDR Missed Cleavage Summary"
                    , "table_header"
                    , FileNameVault["MissedCleavageSummary10percentFdrFigureFileName"]
                    , Model.RCalls.GetDataTable("T_MissedCleavageSummary_10", false)
                    , 1, 1, 1));
            }
            if (File.Exists(Path.Combine(Model.WorkDirectory, "Plots",
                FileNameVault["MissedCleavageSummary05percentFdrFigureFileName"])))
            {
                qcHtml.Append(HtmlFileHandler.GetQCElement(
                    "5% FDR Missed Cleavage Summary"
                    , "table_header"
                    , FileNameVault["MissedCleavageSummary05percentFdrFigureFileName"]
                    , Model.RCalls.GetDataTable("T_MissedCleavageSummary_5", false)
                    , 1, 1, 1));
            }
            if (File.Exists(Path.Combine(Model.WorkDirectory, "Plots",
                FileNameVault["MissedCleavageSummary01percentFdrFigureFileName"])))
            {
                qcHtml.Append(HtmlFileHandler.GetQCElement(
                    "1% FDR Missed Cleavage Summary"
                    , "table_header"
                    , FileNameVault["MissedCleavageSummary01percentFdrFigureFileName"]
                    , Model.RCalls.GetDataTable("T_MissedCleavageSummary_1", false)
                    , 1, 1, 1));
            }

            // Tryptic peptides
            qcHtml.Append("\t\t<A NAME='tp'/A>\n");

            qcHtml.Append(HtmlFileHandler.GetQCElement(
                "Tryptic Peptide Summary"
                , "table_header"
                , FileNameVault["TrypticPeptideSummaryFigureFileName"]
                , m_SQLiteReader.GetTable("T_MAC_Trypticity_Summary")
                , 1, 1, 1));

            // Hexbin plot
            qcHtml.Append("\t\t<A NAME='hb' /A>\n");

            if (File.Exists(Path.Combine(Model.WorkDirectory, "Plots", "Filtered_MSGF_vs_PPM.png")))
            {
                qcHtml.Append("\t\t<P ID='table_header'>MSGF vs PPM Hexbin Plot</P>\n");
                qcHtml.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["FilteredMsgfPpmHexbinFigureFileName"],
                    true, "pos_left", null, null));
            }
            if (File.Exists(Path.Combine(Model.WorkDirectory, "Plots", "Filtered_10_MSGF_vs_PPM.png")))
            {
                qcHtml.Append("\t\t<P ID='table_header'>10% FDR MSGF vs PPM Hexbin Plot</P>\n");
                qcHtml.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["FilteredMsgfPpmHexbin10percentFdrFigureFileName"],
                    true, "pos_left", null, null));
            }
            if (File.Exists(Path.Combine(Model.WorkDirectory, "Plots", "Filtered_5_MSGF_vs_PPM.png")))
            {
                qcHtml.Append("\t\t<P ID='table_header'>5% FDR MSGF vs PPM Hexbin Plot</P>\n");
                qcHtml.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["FilteredMsgfPpmHexbin05percentFdrFigureFileName"],
                    true, "pos_left", null, null));
            }
            if (File.Exists(Path.Combine(Model.WorkDirectory, "Plots", "Filtered_1_MSGF_vs_PPM.png")))
            {
                qcHtml.Append("\t\t<P ID='table_header'>1% FDR MSGF vs PPM Hexbin Plot</P>\n");
                qcHtml.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["FilteredMsgfPpmHexbin01percentFdrFigureFileName"],
                    true, "pos_left", null, null));
            }

            qcHtml.Append("\t</DIV>\n");
            qcHtml.Append(HtmlFileHandler.GetEndBodyEndHtml());

            var qcWriter = new StreamWriter(Path.Combine(Model.WorkDirectory, FileNameVault["QcHtmlFileName"]));
            qcWriter.WriteLine(qcHtml);
            qcWriter.Close();
        }

        /// <summary>
        /// Construct and write-out the Correlation Heatmap HTML Page
        /// </summary>
        /// <param name="NavBar">HTML Navigation Bar</param>
        private void WriteCorrelationHeatmapHTMLPage(List<HtmlLinkNode> NavBar)
        {
            var heatmapHtml = new StringBuilder();

            NavBar.Add(new HtmlLinkNode("Correlation", "ch", true));

            heatmapHtml.Append(HtmlFileHandler.GetHtmlHeader());
            heatmapHtml.Append(HtmlFileHandler.GetHtmlJavascriptStart());
            heatmapHtml.Append(WriteHtmlScripts());
            heatmapHtml.Append(HtmlFileHandler.GetHtmlScriptEnd());
            heatmapHtml.Append(HtmlFileHandler.GetCSSLink(FileNameVault["CssFileName"]));
            heatmapHtml.Append(HtmlFileHandler.GetEndHeadStartBody());
            heatmapHtml.Append(HtmlFileHandler.GetNavTable(NavBar));
            //heatmapHtml.Append(HtmlFileHandler.GetNavBar(navBarNodes, "LEFT"));
            heatmapHtml.Append(WriteHtmlBody(HTMLFileType.Index));

            heatmapHtml.Append("\t<DIV ID='main_content'>\n");
            heatmapHtml.Append("\t\t<A NAME='ch'/A>\n");
            heatmapHtml.Append("\t\t<P ID='table_header'>Correlation Heatmap</P>\n");

            var correlationHeatMapFile = Path.Combine(Model.WorkDirectory, "Plots", FileNameVault["SpectralCountsCorrelationHeatmapFigureFileName"]);
            if (File.Exists(correlationHeatMapFile))
            {
                heatmapHtml.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["SpectralCountsCorrelationHeatmapFigureFileName"],
                    true, "pos_left", null, null));
            }

            var heatMap10PercentFdr = Path.Combine(Model.WorkDirectory, "Plots",
                FileNameVault["SpectralCountsCorrelationHeatmap10percentFdrFigureFileName"]);
            if (File.Exists(heatMap10PercentFdr))
            {
                heatmapHtml.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["SpectralCountsCorrelationHeatmap10percentFdrFigureFileName"],
                    true, "pos_left", null, null));
            }

            var heatMap05PercentFdr = Path.Combine(Model.WorkDirectory, "Plots",
                FileNameVault["SpectralCountsCorrelationHeatmap05percentFdrFigureFileName"]);
            if (File.Exists(heatMap05PercentFdr))
            {
                heatmapHtml.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["SpectralCountsCorrelationHeatmap05percentFdrFigureFileName"],
                    true, "pos_left", null, null));
            }

            var heatMap01PercentFdr = Path.Combine(Model.WorkDirectory, "Plots",
                FileNameVault["SpectralCountsCorrelationHeatmap01percentFdrFigureFileName"]);
            if (File.Exists(heatMap01PercentFdr))
            {
                heatmapHtml.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["SpectralCountsCorrelationHeatmap01percentFdrFigureFileName"],
                    true, "pos_left", null, null));
            }

            heatmapHtml.Append("\t</DIV>\n");
            heatmapHtml.Append(HtmlFileHandler.GetEndBodyEndHtml());

            var heatmapWriter = new StreamWriter(Path.Combine(Model.WorkDirectory, FileNameVault["CorrelationHtmlFileName"]));
            heatmapWriter.Write(heatmapHtml);
            heatmapWriter.Close();
        }

        private void WriteHeatmapsPage(List<HtmlLinkNode> NavBar)
        {
            var heatmapsHtml = new StringBuilder();
            heatmapsHtml.Append(HtmlFileHandler.GetHtmlHeader());
            heatmapsHtml.Append(HtmlFileHandler.GetHtmlJavascriptStart());
            heatmapsHtml.Append(WriteHtmlScripts());
            heatmapsHtml.Append(HtmlFileHandler.GetHtmlScriptEnd());
            heatmapsHtml.Append(HtmlFileHandler.GetCSSLink(FileNameVault["CssFileName"]));
            heatmapsHtml.Append(HtmlFileHandler.GetEndHeadStartBody());
            heatmapsHtml.Append(HtmlFileHandler.GetNavTable(NavBar));
            heatmapsHtml.Append(WriteHtmlBody(HTMLFileType.Index));

            heatmapsHtml.Append("\t<DIV ID='main_content'>\n");

            if (File.Exists(
                Path.Combine(Model.WorkDirectory, "Plots",
                FileNameVault["BBM_Heatmap_01_FigureFileName"])))
            {
                heatmapsHtml.Append("\t\t<A NAME='ch'/A>\n");
                heatmapsHtml.Append("\t\t<P ID='table_header'>Proteins of Significant Difference (P-value < 0.01)</P>\n");

                heatmapsHtml.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["BBM_Heatmap_01_FigureFileName"],
                    true, "pos_left", null, null));
            }

            if (File.Exists(
                Path.Combine(Model.WorkDirectory, "Plots",
                FileNameVault["BBM_Heatmap_05_FigureFileName"])))
            {
                heatmapsHtml.Append("\t\t<A NAME='ch'/A>\n");
                heatmapsHtml.Append("\t\t<P ID='table_header'>Proteins of Significant Difference (P-value < 0.05)</P>\n");

                heatmapsHtml.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["BBM_Heatmap_05_FigureFileName"],
                    true, "pos_left", null, null));
            }

            heatmapsHtml.Append("\t</DIV>\n");
            heatmapsHtml.Append(HtmlFileHandler.GetEndBodyEndHtml());

            var heatmapsWriter = new StreamWriter(Path.Combine(Model.WorkDirectory, FileNameVault["HeatmapsFileName"]));
            heatmapsWriter.Write(heatmapsHtml);
            heatmapsWriter.Close();
        }

        /// <summary>
        /// Construct and write-out the Main HTML Page
        /// </summary>
        /// <param name="NavBar">HTML Navigation Bar</param>
        private void WriteMainHTMLPage(List<HtmlLinkNode> NavBar)
        {
            // Construct and write-out the main html summary page
            var mainHtml = new StringBuilder();

            mainHtml.Append(HtmlFileHandler.GetHtmlHeader());
            mainHtml.Append(HtmlFileHandler.GetHtmlJavascriptStart());
            mainHtml.Append(WriteHtmlScripts());
            mainHtml.Append(HtmlFileHandler.GetHtmlScriptEnd());
            mainHtml.Append(HtmlFileHandler.GetCSSLink(FileNameVault["CssFileName"]));
            mainHtml.Append(HtmlFileHandler.GetEndHeadStartBody());
            mainHtml.Append(HtmlFileHandler.GetNavTable(NavBar));
            mainHtml.Append(WriteHtmlBody(HTMLFileType.Index));

            mainHtml.Append("\t<DIV ID='main_content'>\n");
            //mainHtml.Append("\t\t<P ID='table_header'>Analysis Summary Statistics</P>\n");

            var specCntSummary = GetSpectralCountSummary();
            var specCntStatSummary = GetSpectralCountStatSummary();

            mainHtml.Append("\t\t" +
                HtmlFileHandler.GetSummaryTableHtml(
                specCntSummary,
                "Spectral Count Analysis Summary",
                "table_header",
                0, 2, 4) + "\n\n" +
                HtmlFileHandler.GetSummaryTableHtml(
                specCntStatSummary,
                "Spectral Count Beta-Binomial Model Statistics Summary",
                "table_header",
                0, 2, 4));

            mainHtml.Append("\t</DIV>\n");
            mainHtml.Append(HtmlFileHandler.GetEndBodyEndHtml());

            var htmlWriter = new StreamWriter(Path.Combine(Model.WorkDirectory, FileNameVault["MainFileName"]));
            htmlWriter.Write(mainHtml);
            htmlWriter.Close();
        }

        private StringBuilder WriteHtmlScripts()
        {
            var scriptData = new StringBuilder();
            // TODO: Build Script

            return scriptData;
        }

        private string WriteHtmlBody(HTMLFileType TheHTMLFileType)
        {
            var body = "";
            switch (TheHTMLFileType)
            {
                case HTMLFileType.Dataset:
                    if (m_DatabaseFound)
                    {
                        body = HtmlFileHandler.GetDatasetTableHtml(
                            Path.Combine(Model.WorkDirectory,
                                Parameters["DatabaseFileName"]), null,
                                "table_header", "left", 0, 2, 4);
                    }
                    else
                    {
                        Model.LogError(string.Format(
                            "ERROR in Sco_Html_Summary: " +
                            "File: {0} does not exist",
                            Path.Combine(Model.WorkDirectory,
                            Parameters["DatabaseFileName"])),
                        ModuleName, StepNumber);
                    }
                    break;
                case HTMLFileType.Index:

                    break;
            }

            return body;
        }

        private DataTable GetSpectralCountSummary()
        {
            var outTable = new DataTable();

            var fdrDataColumn = new DataColumn("FDR");
            outTable.Columns.Add(fdrDataColumn);

            var peptidesDataColumn = new DataColumn("Unique Peptides");
            outTable.Columns.Add(peptidesDataColumn);

            var proteinsDataColumn = new DataColumn("Unique Proteins");
            outTable.Columns.Add(proteinsDataColumn);

            try
            {
                if (Model.RCalls.ContainsObject(FileNameVault["RowMetadataTable10FDR"]))
                {
                    var unqPep = new List<string>();
                    var unqProt = new List<string>();

                    if (Model.RCalls.TableContainsColumn(
                        FileNameVault["RowMetadataTable10FDR"],
                        FileNameVault["RowMetadataTablePeptideColumnName"]))
                    {
                        // 10% FDR Summary
                        unqPep = Model.RCalls.GetCharacterVector(
                            string.Format(
                            "length(unique({0}[,'{1}']))",
                            FileNameVault["RowMetadataTable10FDR"],
                            FileNameVault["RowMetadataTablePeptideColumnName"]));
                    }
                    if (Model.RCalls.TableContainsColumn(
                        FileNameVault["RowMetadataTable10FDR"],
                        FileNameVault["RowMetadataTableProteinColumnName"]))
                    {
                        unqProt = Model.RCalls.GetCharacterVector(
                            string.Format(
                            "length(unique({0}[,'{1}']))",
                            FileNameVault["RowMetadataTable10FDR"],
                            FileNameVault["RowMetadataTableProteinColumnName"]));
                    }
                    else if (Model.RCalls.TableContainsColumn(
                        FileNameVault["RowMetadataTable10FDR"],
                        FileNameVault["RowMetadataTableAltProteinColumnName"]))
                    {
                        unqProt = Model.RCalls.GetCharacterVector(
                            string.Format(
                            "length(unique({0}[,'{1}']))",
                            FileNameVault["RowMetadataTable10FDR"],
                            FileNameVault["RowMetadataTableAltProteinColumnName"]));
                    }

                    var fdrRow = outTable.NewRow();
                    fdrRow["FDR"] = 10;
                    fdrRow["Unique Peptides"] = unqPep[0];
                    fdrRow["Unique Proteins"] = unqProt[0];
                    outTable.Rows.Add(fdrRow);
                }

                if (Model.RCalls.ContainsObject(FileNameVault["RowMetadataTable05FDR"]))
                {
                    var unqPep = new List<string>();
                    var unqProt = new List<string>();
                    // Peptides
                    if (Model.RCalls.TableContainsColumn(
                        FileNameVault["RowMetadataTable05FDR"],
                        FileNameVault["RowMetadataTablePeptideColumnName"]))
                    {
                        // 5% FDR Summary
                        unqPep = Model.RCalls.GetCharacterVector(
                            string.Format(
                            "length(unique({0}[,'{1}']))",
                            FileNameVault["RowMetadataTable05FDR"],
                            FileNameVault["RowMetadataTablePeptideColumnName"]));
                    }
                    // Proteins
                    if (Model.RCalls.TableContainsColumn(
                        FileNameVault["RowMetadataTable05FDR"],
                        FileNameVault["RowMetadataTableProteinColumnName"]))
                    {
                        unqProt = Model.RCalls.GetCharacterVector(
                                string.Format(
                                "length(unique({0}[,'{1}']))",
                                FileNameVault["RowMetadataTable05FDR"],
                                FileNameVault["RowMetadataTableProteinColumnName"]));
                    }
                    else if (Model.RCalls.TableContainsColumn(
                        FileNameVault["RowMetadataTable05FDR"],
                        FileNameVault["RowMetadataTableAltProteinColumnName"]))
                    {
                        unqProt = Model.RCalls.GetCharacterVector(
                                string.Format(
                                "length(unique({0}[,'{1}']))",
                                FileNameVault["RowMetadataTable05FDR"],
                                FileNameVault["RowMetadataTableAltProteinColumnName"]));
                    }

                    var fdrRow = outTable.NewRow();
                    fdrRow["FDR"] = 5;
                    fdrRow["Unique Peptides"] = unqPep[0];
                    fdrRow["Unique Proteins"] = unqProt[0];
                    outTable.Rows.Add(fdrRow);
                }

                if (Model.RCalls.ContainsObject(FileNameVault["RowMetadataTable01FDR"]))
                {
                    var unqPep = new List<string>();
                    var unqProt = new List<string>();
                    // Peptides
                    if (Model.RCalls.TableContainsColumn(
                        FileNameVault["RowMetadataTable01FDR"],
                        FileNameVault["RowMetadataTablePeptideColumnName"]))
                    {
                        // 1% FDR Summary
                        unqPep = Model.RCalls.GetCharacterVector(
                            string.Format(
                            "length(unique({0}[,'{1}']))",
                            FileNameVault["RowMetadataTable01FDR"],
                            FileNameVault["RowMetadataTablePeptideColumnName"]));
                    }
                    // Proteins
                    if (Model.RCalls.TableContainsColumn(
                        FileNameVault["RowMetadataTable01FDR"],
                        FileNameVault["RowMetadataTableProteinColumnName"]))
                    {
                        unqProt = Model.RCalls.GetCharacterVector(
                            string.Format(
                            "length(unique({0}[,'{1}']))",
                            FileNameVault["RowMetadataTable01FDR"],
                            FileNameVault["RowMetadataTableProteinColumnName"]));
                    }
                    else if (Model.RCalls.TableContainsColumn(
                        FileNameVault["RowMetadataTable01FDR"],
                        FileNameVault["RowMetadataTableAltProteinColumnName"]))
                    {
                        unqProt = Model.RCalls.GetCharacterVector(
                            string.Format(
                            "length(unique({0}[,'{1}']))",
                            FileNameVault["RowMetadataTable01FDR"],
                            FileNameVault["RowMetadataTableAltProteinColumnName"]));
                    }

                    var fdrRow = outTable.NewRow();
                    fdrRow["FDR"] = 1;
                    fdrRow["Unique Peptides"] = unqPep[0];
                    fdrRow["Unique Proteins"] = unqProt[0];
                    outTable.Rows.Add(fdrRow);
                }
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered within " +
                    "GetSpectralCountSummary(): " + ex,
                    ModuleName, StepNumber);
                return null;
            }

            return outTable;
        }

        private DataTable GetSpectralCountStatSummary()
        {
            var outTable = new DataTable();

            var fdrDataColumn = new DataColumn("P-value");
            outTable.Columns.Add(fdrDataColumn);

            var proteinsDataColumn = new DataColumn("Proteins");
            outTable.Columns.Add(proteinsDataColumn);

            try
            {
                if (Model.RCalls.ContainsObject(FileNameVault["BbmResultsFdr01"]) &&
                    Model.RCalls.TableContainsColumn(
                        FileNameVault["BbmResultsFdr01"],
                        FileNameVault["BBM_Pvals"]))
                {
                    // P-value 0.05
                    var dataRow05 = outTable.NewRow();
                    var pVal = 0.05;

                    var protCount = Model.RCalls.GetNumberOfRowsInTable(
                        FileNameVault["BbmResultsFdr01"],
                        FileNameVault["BBM_Pvals"],
                        null, pVal.ToString(CultureInfo.InvariantCulture));

                    dataRow05["P-value"] = "< " + pVal;
                    dataRow05["Proteins"] = protCount;
                    outTable.Rows.Add(dataRow05);

                    // P-value 0.01
                    var dataRow01 = outTable.NewRow();
                    pVal = 0.01;

                    protCount = Model.RCalls.GetNumberOfRowsInTable(
                        FileNameVault["BbmResultsFdr01"],
                        FileNameVault["BBM_Pvals"],
                        null, pVal.ToString(CultureInfo.InvariantCulture));

                    dataRow01["P-value"] = "< " + pVal;
                    dataRow01["Proteins"] = protCount;

                    outTable.Rows.Add(dataRow01);
                }
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered within " +
                    "GetSpectralCountStatSummary(): " + ex,
                    ModuleName, StepNumber);
                return null;
            }

            return outTable;
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
