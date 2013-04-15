﻿/* Written by Joseph N. Brown
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
    public class Sco_Html_Summary : BaseDataModule
    {
        #region Enums
        /// <summary>
        /// Required parameters to run Aggregate
        /// </summary>
        private enum RequiredParameters
        {
            FileName
        }
        private enum HTMLFileType { Dataset, Index };
        #endregion

        #region Members
        private string m_ModuleName = "Sco_HTML_Summary",
            m_DatabaseFileName = "Results.db3";
        private Dictionary<string, string>
            d_FileNameVault = new Dictionary<string, string>();
        private bool m_DatabaseFound = false;
        private PNNLOmics.Databases.SQLiteHandler sql = new PNNLOmics.Databases.SQLiteHandler();
        #endregion

        #region Properties
        public Dictionary<string, string> FileNameVault
        {
            get { return d_FileNameVault; }
            set { d_FileNameVault = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an Sco_Html_Summary Module
        /// </summary>
        public Sco_Html_Summary()
        {
            ModuleName = m_ModuleName;
        }

        /// <summary>
        /// Sco_Html_Summary module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public Sco_Html_Summary(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
        }

        /// <summary>
        /// Sco_Html_Summary module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public Sco_Html_Summary(CyclopsModel CyclopsModel,
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

                AddDefaultValues2FileNameVault();

                if (CheckParameters())
                    Model.PipelineCurrentlySuccessful = Sco_Html_SummaryFunction();

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

            if (Parameters.ContainsKey("DatabaseFileName"))
            {
                if (File.Exists(Parameters["DatabaseFileName"]))
                {
                    m_DatabaseFileName = Parameters["DatabaseFileName"];
                    sql.DatabaseFileName = Parameters["DatabaseFileName"];
                    m_DatabaseFound = true;
                }
                else if (File.Exists(Path.Combine(Model.WorkDirectory,
                    Parameters["DatabaseFileName"])))
                {
                    m_DatabaseFileName = Parameters["DatabaseFileName"];
                    sql.DatabaseFileName = Path.Combine(Model.WorkDirectory,
                    Parameters["DatabaseFileName"]);
                    m_DatabaseFound = true;
                }
            }
            else
            {
                if (File.Exists(Path.Combine(Model.WorkDirectory,
                    "Results.db3")))
                {
                    sql.DatabaseFileName = Path.Combine(Model.WorkDirectory,
                    "Results.db3");
                    Parameters.Add("DatabaseFileName",
                        sql.DatabaseFileName);
                    m_DatabaseFound = true;
                }
            }

            if (!m_DatabaseFound)
            {
                Model.LogError("Unable to establish successful database connection!",
                    ModuleName, StepNumber);
                b_Successful = false;
            }

            return b_Successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool Sco_Html_SummaryFunction()
        {
            bool b_Successful = true;

            string s_CssFileName = FileNameVault["CssFileName"],
                    s_DatasetsFileName = FileNameVault["DatasetsHtmlFileName"],
                    s_QCFileName = FileNameVault["QcHtmlFileName"],
                    s_CorrFileName = FileNameVault["CorrelationHtmlFileName"],
                    s_HeatmapsFileName = FileNameVault["HeatmapsFileName"];

            List<HtmlLinkNode> l_NavBarNodes = new List<HtmlLinkNode>();

            l_NavBarNodes.Add(new HtmlLinkNode(
                "Home", Parameters[RequiredParameters.FileName.ToString()], false));
            l_NavBarNodes.Add(new HtmlLinkNode(
                "Datasets", s_DatasetsFileName, false));
            l_NavBarNodes.Add(new HtmlLinkNode(
                "QC Plots", s_QCFileName, false));
            l_NavBarNodes.Add(new HtmlLinkNode(
                "Correlation Heatmaps", s_CorrFileName, false));
            l_NavBarNodes.Add(new HtmlLinkNode(
                "Heatmaps", s_HeatmapsFileName, false));

            using (StreamWriter sw_Css = File.AppendText(Path.Combine(Model.WorkDirectory, s_CssFileName)))
            {
                sw_Css.WriteLine(HtmlFileHandler.GetCSS(HtmlFileHandler.CssStyle.NavBar, 160));
                sw_Css.WriteLine(HtmlFileHandler.GetCSS(HtmlFileHandler.CssStyle.LeftIndent, 160));
                sw_Css.WriteLine(HtmlFileHandler.GetCSS(HtmlFileHandler.CssStyle.Th, 160));
            }

            try
            {
                // Construct and write-out the Datasets Page
                StringBuilder sb_Datasets = new StringBuilder();

                sb_Datasets.Append(HtmlFileHandler.GetHtmlHeader());
                sb_Datasets.Append(HtmlFileHandler.GetCSSLink(s_CssFileName));
                sb_Datasets.Append(HtmlFileHandler.GetHtmlJavascriptStart());
                sb_Datasets.Append(WriteHtmlScripts());
                sb_Datasets.Append(HtmlFileHandler.GetHtmlScriptEnd());
                sb_Datasets.Append(HtmlFileHandler.GetEndHeadStartBody());
                sb_Datasets.Append(HtmlFileHandler.GetNavTable(l_NavBarNodes));
                //sb_Datasets.Append(HtmlFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
                sb_Datasets.Append("\t\t<DIV ID='main_content'>\n");
                sb_Datasets.Append(WriteHtmlBody(HTMLFileType.Dataset));
                sb_Datasets.Append("\t\t</DIV>\n");
                sb_Datasets.Append(HtmlFileHandler.GetEndBodyEndHtml());

                StreamWriter sw_Datasets = new StreamWriter(Path.Combine(Model.WorkDirectory,
                    s_DatasetsFileName));
                sw_Datasets.Write(sb_Datasets);
                sw_Datasets.Close();
                sb_Datasets.Clear();
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while constructing " +
                    "the Datasets HTML Summary Page: " + exc.ToString(),
                    ModuleName, StepNumber);
                return false;
            }

            try
            {
                // Construct and write-out the QC Page
                StringBuilder sb_QC = new StringBuilder();

                HtmlLinkNode node_ScoSummary = new HtmlLinkNode("Summary", "sum", true);
                l_NavBarNodes.Add(node_ScoSummary);
                HtmlLinkNode node_MissedCleavages = new HtmlLinkNode("Missed Cleavages", "mc", true);
                l_NavBarNodes.Add(node_MissedCleavages);
                HtmlLinkNode node_TrypticPeptides = new HtmlLinkNode("Tryptic Peptides", "tp", true);
                l_NavBarNodes.Add(node_TrypticPeptides);
                HtmlLinkNode node_Hexbin = new HtmlLinkNode("Hexbin", "hb", true);
                l_NavBarNodes.Add(node_Hexbin);

                sb_QC.Append(HtmlFileHandler.GetHtmlHeader());
                sb_QC.Append(HtmlFileHandler.GetHtmlJavascriptStart());
                sb_QC.Append(WriteHtmlScripts());
                sb_QC.Append(HtmlFileHandler.GetHtmlScriptEnd());
                sb_QC.Append(HtmlFileHandler.GetCSSLink(s_CssFileName));
                sb_QC.Append(HtmlFileHandler.GetEndHeadStartBody());
                sb_QC.Append(HtmlFileHandler.GetNavTable(l_NavBarNodes));
                //sb_QC.Append(HtmlFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
                sb_QC.Append(WriteHtmlBody(HTMLFileType.Index));

                sb_QC.Append("\t<DIV ID='main_content'>\n");

                // Spectral Count Summary
                sb_QC.Append("\t\t<A NAME='sum' />\n");
                sb_QC.Append("\t\t<DIV ID='SpectralCount'>\n");
                sb_QC.Append(HtmlFileHandler.GetQCElement("Spectral Count Summary"
                    , "table_header"
                    , FileNameVault["SpectralCountSummaryFigureFileName"]
                    , sql.GetTable("T_MAC_SpecCnt_Summary")
                    , 1, 1, 1));
                sb_QC.Append("\t\t</DIV>\n");

                // Missed Cleavages
                sb_QC.Append("\t\t<A NAME='mc'/A>\n");
                if (File.Exists(Path.Combine(Model.WorkDirectory, "Plots",
                    FileNameVault["MissedCleavageSummaryFigureFileName"])))
                {
                    sb_QC.Append(HtmlFileHandler.GetQCElement(
                        "Missed Cleavage Summary"
                        , "table_header"
                        , FileNameVault["MissedCleavageSummaryFigureFileName"]
                        , sql.GetTable("T_MissedCleavageSummary")
                        , 1, 1, 1));
                }
                if (File.Exists(Path.Combine(Model.WorkDirectory, "Plots",
                    FileNameVault["MissedCleavageSummary10percentFdrFigureFileName"])))
                {
                    sb_QC.Append(HtmlFileHandler.GetQCElement(
                        "10% FDR Missed Cleavage Summary"
                        , "table_header"
                        , FileNameVault["MissedCleavageSummary10percentFdrFigureFileName"]
                        , Model.RCalls.GetDataTable("T_MissedCleavageSummary_10", false)
                        , 1, 1, 1));
                }
                if (File.Exists(Path.Combine(Model.WorkDirectory, "Plots",
                    FileNameVault["MissedCleavageSummary05percentFdrFigureFileName"])))
                {
                    sb_QC.Append(HtmlFileHandler.GetQCElement(
                        "5% FDR Missed Cleavage Summary"
                        , "table_header"
                        , FileNameVault["MissedCleavageSummary05percentFdrFigureFileName"]
                        , Model.RCalls.GetDataTable("T_MissedCleavageSummary_5", false)
                        , 1, 1, 1));
                }
                if (File.Exists(Path.Combine(Model.WorkDirectory, "Plots",
                    FileNameVault["MissedCleavageSummary01percentFdrFigureFileName"])))
                {
                    sb_QC.Append(HtmlFileHandler.GetQCElement(
                        "1% FDR Missed Cleavage Summary"
                        , "table_header"
                        , FileNameVault["MissedCleavageSummary01percentFdrFigureFileName"]
                        , Model.RCalls.GetDataTable("T_MissedCleavageSummary_1", false)
                        , 1, 1, 1));
                }

                // Tryptic peptides
                sb_QC.Append("\t\t<A NAME='tp'/A>\n");

                sb_QC.Append(HtmlFileHandler.GetQCElement(
                    "Tryptic Peptide Summary"
                    , "table_header"
                    , FileNameVault["TrypticPeptideSummaryFigureFileName"]
                    , sql.GetTable("T_MAC_Trypticity_Summary")
                    , 1, 1, 1));

                // Hexbin plot
                sb_QC.Append("\t\t<A NAME='hb' /A>\n");

                if (File.Exists(Path.Combine(Model.WorkDirectory, "Plots", "Filtered_MSGF_vs_PPM.png")))
                {
                    sb_QC.Append("\t\t<P ID='table_header'>MSGF vs PPM Hexbin Plot</P>\n");
                    sb_QC.Append(HtmlFileHandler.GetPictureCode(
                        FileNameVault["FilteredMsgfPpmHexbinFigureFileName"],
                        true, "pos_left", null, null));
                }
                if (File.Exists(Path.Combine(Model.WorkDirectory, "Plots", "Filtered_10_MSGF_vs_PPM.png")))
                {
                    sb_QC.Append("\t\t<P ID='table_header'>10% FDR MSGF vs PPM Hexbin Plot</P>\n");
                    sb_QC.Append(HtmlFileHandler.GetPictureCode(
                        FileNameVault["FilteredMsgfPpmHexbin10percentFdrFigureFileName"],
                        true, "pos_left", null, null));
                }
                if (File.Exists(Path.Combine(Model.WorkDirectory, "Plots", "Filtered_5_MSGF_vs_PPM.png")))
                {
                    sb_QC.Append("\t\t<P ID='table_header'>5% FDR MSGF vs PPM Hexbin Plot</P>\n");
                    sb_QC.Append(HtmlFileHandler.GetPictureCode(
                        FileNameVault["FilteredMsgfPpmHexbin05percentFdrFigureFileName"],
                        true, "pos_left", null, null));
                }
                if (File.Exists(Path.Combine(Model.WorkDirectory, "Plots", "Filtered_1_MSGF_vs_PPM.png")))
                {
                    sb_QC.Append("\t\t<P ID='table_header'>1% FDR MSGF vs PPM Hexbin Plot</P>\n");
                    sb_QC.Append(HtmlFileHandler.GetPictureCode(
                        FileNameVault["FilteredMsgfPpmHexbin01percentFdrFigureFileName"],
                        true, "pos_left", null, null));
                }


                sb_QC.Append("\t</DIV>\n");
                sb_QC.Append(HtmlFileHandler.GetEndBodyEndHtml());

                StreamWriter sw_QC = new StreamWriter(Path.Combine(Model.WorkDirectory,
                    s_QCFileName));
                sw_QC.WriteLine(sb_QC);
                sw_QC.Close();

                l_NavBarNodes.Remove(node_ScoSummary);
                l_NavBarNodes.Remove(node_MissedCleavages);
                l_NavBarNodes.Remove(node_TrypticPeptides);
                l_NavBarNodes.Remove(node_Hexbin);
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while constructing " +
                    "the Quality Control HTML Summary Page: " + exc.ToString(),
                    ModuleName, StepNumber);
                return false;
            }

            try
            {
                StringBuilder sb_Corr = new StringBuilder();

                HtmlLinkNode hln_Corr = new HtmlLinkNode(
                    "Correlation", "ch", true);
                l_NavBarNodes.Add(hln_Corr);

                sb_Corr.Append(HtmlFileHandler.GetHtmlHeader());
                sb_Corr.Append(HtmlFileHandler.GetHtmlJavascriptStart());
                sb_Corr.Append(WriteHtmlScripts());
                sb_Corr.Append(HtmlFileHandler.GetHtmlScriptEnd());
                sb_Corr.Append(HtmlFileHandler.GetCSSLink(s_CssFileName));
                sb_Corr.Append(HtmlFileHandler.GetEndHeadStartBody());
                sb_Corr.Append(HtmlFileHandler.GetNavTable(l_NavBarNodes));
                //sb_HTML.Append(HtmlFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
                sb_Corr.Append(WriteHtmlBody(HTMLFileType.Index));

                sb_Corr.Append("\t<DIV ID='main_content'>\n");
                sb_Corr.Append("\t\t<A NAME='ch'/A>\n");
                sb_Corr.Append("\t\t<P ID='table_header'>Correlation Heatmap</P>\n");

                string s = Path.Combine(Model.WorkDirectory, "Plots",
                    FileNameVault["SpectralCountsCorrelationHeatmapFigureFileName"]);
                if (File.Exists(s))
                    sb_Corr.Append(HtmlFileHandler.GetPictureCode(
                        FileNameVault["SpectralCountsCorrelationHeatmapFigureFileName"],
                        true, "pos_left", null, null));
                s = Path.Combine(Model.WorkDirectory, "Plots",
                    FileNameVault["SpectralCountsCorrelationHeatmap10percentFdrFigureFileName"]);
                if (File.Exists(s))
                    sb_Corr.Append(HtmlFileHandler.GetPictureCode(
                        FileNameVault["SpectralCountsCorrelationHeatmap10percentFdrFigureFileName"],
                        true, "pos_left", null, null));
                s = Path.Combine(Model.WorkDirectory, "Plots",
                    FileNameVault["SpectralCountsCorrelationHeatmap05percentFdrFigureFileName"]);
                if (File.Exists(s))
                    sb_Corr.Append(HtmlFileHandler.GetPictureCode(
                        FileNameVault["SpectralCountsCorrelationHeatmap05percentFdrFigureFileName"],
                        true, "pos_left", null, null));
                s = Path.Combine(Model.WorkDirectory, "Plots",
                    FileNameVault["SpectralCountsCorrelationHeatmap01percentFdrFigureFileName"]);
                if (File.Exists(s))
                    sb_Corr.Append(HtmlFileHandler.GetPictureCode(
                        FileNameVault["SpectralCountsCorrelationHeatmap01percentFdrFigureFileName"],
                        true, "pos_left", null, null));

                sb_Corr.Append("\t</DIV>\n");
                sb_Corr.Append(HtmlFileHandler.GetEndBodyEndHtml());


                // TODO : Write the html out to the file
                StreamWriter sw_Corr = new StreamWriter(Path.Combine(Model.WorkDirectory, s_CorrFileName));
                sw_Corr.Write(sb_Corr);
                sw_Corr.Close();
                l_NavBarNodes.Remove(hln_Corr);
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while constructing " +
                    "the Correlation HTML Summary Page: " + exc.ToString(),
                    ModuleName, StepNumber);
                return false;
            }

            try
            {
                StringBuilder sb_Heatmaps = new StringBuilder();
                sb_Heatmaps.Append(HtmlFileHandler.GetHtmlHeader());
                sb_Heatmaps.Append(HtmlFileHandler.GetHtmlJavascriptStart());
                sb_Heatmaps.Append(WriteHtmlScripts());
                sb_Heatmaps.Append(HtmlFileHandler.GetHtmlScriptEnd());
                sb_Heatmaps.Append(HtmlFileHandler.GetCSSLink(s_CssFileName));
                sb_Heatmaps.Append(HtmlFileHandler.GetEndHeadStartBody());
                sb_Heatmaps.Append(HtmlFileHandler.GetNavTable(l_NavBarNodes));
                sb_Heatmaps.Append(WriteHtmlBody(HTMLFileType.Index));

                sb_Heatmaps.Append("\t<DIV ID='main_content'>\n");


                if (File.Exists(
                    Path.Combine(Model.WorkDirectory, "Plots",
                    FileNameVault["BBM_Heatmap_01_FigureFileName"])))
                {
                    sb_Heatmaps.Append("\t\t<A NAME='ch'/A>\n");
                    sb_Heatmaps.Append("\t\t<P ID='table_header'>Proteins of Significant Difference (P-value < 0.01)</P>\n");

                    sb_Heatmaps.Append(HtmlFileHandler.GetPictureCode(
                        FileNameVault["BBM_Heatmap_01_FigureFileName"],
                        true, "pos_left", null, null));
                }

                if (File.Exists(
                    Path.Combine(Model.WorkDirectory, "Plots",
                    FileNameVault["BBM_Heatmap_05_FigureFileName"])))
                {
                    sb_Heatmaps.Append("\t\t<A NAME='ch'/A>\n");
                    sb_Heatmaps.Append("\t\t<P ID='table_header'>Proteins of Significant Difference (P-value < 0.05)</P>\n");

                    sb_Heatmaps.Append(HtmlFileHandler.GetPictureCode(
                        FileNameVault["BBM_Heatmap_05_FigureFileName"],
                        true, "pos_left", null, null));
                }

                sb_Heatmaps.Append("\t</DIV>\n");
                sb_Heatmaps.Append(HtmlFileHandler.GetEndBodyEndHtml());

                StreamWriter sw_Heatmaps = new StreamWriter(Path.Combine(Model.WorkDirectory, s_HeatmapsFileName));
                sw_Heatmaps.Write(sb_Heatmaps);
                sw_Heatmaps.Close();
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while constructing " +
                    "the Heatmaps HTML Summary Page: " + exc.ToString(),
                    ModuleName, StepNumber);
                return false;
            }

            try
            {
                // Construct and write-out the main html summary page
                StringBuilder sb_HTML = new StringBuilder();

                sb_HTML.Append(HtmlFileHandler.GetHtmlHeader());
                sb_HTML.Append(HtmlFileHandler.GetHtmlJavascriptStart());
                sb_HTML.Append(WriteHtmlScripts());
                sb_HTML.Append(HtmlFileHandler.GetHtmlScriptEnd());
                sb_HTML.Append(HtmlFileHandler.GetCSSLink(s_CssFileName));
                sb_HTML.Append(HtmlFileHandler.GetEndHeadStartBody());
                sb_HTML.Append(HtmlFileHandler.GetNavTable(l_NavBarNodes));
                sb_HTML.Append(WriteHtmlBody(HTMLFileType.Index));

                sb_HTML.Append("\t<DIV ID='main_content'>\n");
                //sb_HTML.Append("\t\t<P ID='table_header'>Analysis Summary Statistics</P>\n");

                DataTable dt_SpecCntSummary = GetSpectralCountSummary();
                DataTable dt_SpecCntStatSummary = GetSpectralCountStatSummary();

                sb_HTML.Append("\t\t" +
                    HtmlFileHandler.GetSummaryTableHtml(
                    dt_SpecCntSummary,
                    "Spectral Count Analysis Summary",
                    "table_header",
                    0, 2, 4) + "\n\n" +
                    HtmlFileHandler.GetSummaryTableHtml(
                    dt_SpecCntStatSummary,
                    "Spectral Count Beta-Binomial Model Statistics Summary",
                    "table_header",
                    0, 2, 4));

                sb_HTML.Append("\t</DIV>\n");
                sb_HTML.Append(HtmlFileHandler.GetEndBodyEndHtml());


                // TODO : Write the html out to the file
                StreamWriter sw = new StreamWriter(Path.Combine(Model.WorkDirectory,
                    Parameters[RequiredParameters.FileName.ToString()]));
                sw.Write(sb_HTML);
                sw.Close();
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while constructing " +
                    "the Main HTML Summary Page: " + exc.ToString(),
                    ModuleName, StepNumber);
                return false;
            }

            return b_Successful;
        }


        /// <summary>
        /// Adds default values to the FileNameVault library
        /// </summary>
        private void AddDefaultValues2FileNameVault()
        {
            FileNameVault.Add("CssFileName", "styles.css");
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

        private StringBuilder WriteHtmlScripts()
        {
            StringBuilder sb_ReturnScripts = new StringBuilder();
            // TODO: Build Script

            return sb_ReturnScripts;
        }

        private string WriteHtmlBody(HTMLFileType TheHTMLFileType)
        {
            string s_Body = "";
            switch (TheHTMLFileType)
            {
                case HTMLFileType.Dataset:
                    if (m_DatabaseFound)
                    {
                        s_Body = HtmlFileHandler.GetDatasetTableHtml(
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

            return s_Body;
        }

        private DataTable GetSpectralCountSummary()
        {
            DataTable dt_Return = new DataTable();

            DataColumn dc_FDR = new DataColumn("FDR");
            dt_Return.Columns.Add(dc_FDR);
            DataColumn dc_Peptides = new DataColumn("Unique Peptides");
            dt_Return.Columns.Add(dc_Peptides);
            DataColumn dc_Proteins = new DataColumn("Unique Proteins");
            dt_Return.Columns.Add(dc_Proteins);

            try
            {
                if (Model.RCalls.ContainsObject(
                    FileNameVault["RowMetadataTable10FDR"]))
                {
                    List<string> s_UnqPep = new List<string>();
                    List<string> s_UnqProt = new List<string>();

                    if (Model.RCalls.TableContainsColumn(
                        FileNameVault["RowMetadataTable10FDR"],
                        FileNameVault["RowMetadataTablePeptideColumnName"]))
                    {
                        // 10% FDR Summary
                        s_UnqPep = Model.RCalls.GetCharacterVector(
                            string.Format(
                            "length(unique({0}[,'{1}']))",
                            FileNameVault["RowMetadataTable10FDR"],
                            FileNameVault["RowMetadataTablePeptideColumnName"]));
                    }
                    if (Model.RCalls.TableContainsColumn(
                        FileNameVault["RowMetadataTable10FDR"],
                        FileNameVault["RowMetadataTableProteinColumnName"]))
                    {
                        s_UnqProt = Model.RCalls.GetCharacterVector(
                            string.Format(
                            "length(unique({0}[,'{1}']))",
                            FileNameVault["RowMetadataTable10FDR"],
                            FileNameVault["RowMetadataTableProteinColumnName"]));
                    }
                    else if (Model.RCalls.TableContainsColumn(
                        FileNameVault["RowMetadataTable10FDR"],
                        FileNameVault["RowMetadataTableAltProteinColumnName"]))
                    {
                        s_UnqProt = Model.RCalls.GetCharacterVector(
                            string.Format(
                            "length(unique({0}[,'{1}']))",
                            FileNameVault["RowMetadataTable10FDR"],
                            FileNameVault["RowMetadataTableAltProteinColumnName"]));
                    }

                    DataRow dr_Fdr10 = dt_Return.NewRow();
                    dr_Fdr10["FDR"] = 10;
                    dr_Fdr10["Unique Peptides"] = s_UnqPep[0];
                    dr_Fdr10["Unique Proteins"] = s_UnqProt[0];
                    dt_Return.Rows.Add(dr_Fdr10);
                }

                if (Model.RCalls.ContainsObject(
                    FileNameVault["RowMetadataTable05FDR"]))
                {
                    List<string> s_UnqPep = new List<string>();
                    List<string> s_UnqProt = new List<string>();
                    // Peptides
                    if (Model.RCalls.TableContainsColumn(
                        FileNameVault["RowMetadataTable05FDR"],
                        FileNameVault["RowMetadataTablePeptideColumnName"]))
                    {
                        // 5% FDR Summary
                        s_UnqPep = Model.RCalls.GetCharacterVector(
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
                        s_UnqProt = Model.RCalls.GetCharacterVector(
                                string.Format(
                                "length(unique({0}[,'{1}']))",
                                FileNameVault["RowMetadataTable05FDR"],
                                FileNameVault["RowMetadataTableProteinColumnName"]));
                    }
                    else if (Model.RCalls.TableContainsColumn(
                        FileNameVault["RowMetadataTable05FDR"],
                        FileNameVault["RowMetadataTableAltProteinColumnName"]))
                    {
                        s_UnqProt = Model.RCalls.GetCharacterVector(
                                string.Format(
                                "length(unique({0}[,'{1}']))",
                                FileNameVault["RowMetadataTable05FDR"],
                                FileNameVault["RowMetadataTableAltProteinColumnName"]));
                    }

                    DataRow dr_Fdr05 = dt_Return.NewRow();
                    dr_Fdr05["FDR"] = 5;
                    dr_Fdr05["Unique Peptides"] = s_UnqPep[0];
                    dr_Fdr05["Unique Proteins"] = s_UnqProt[0];
                    dt_Return.Rows.Add(dr_Fdr05);
                }

                if (Model.RCalls.ContainsObject(
                    FileNameVault["RowMetadataTable01FDR"]))
                {
                    List<string> s_UnqPep = new List<string>();
                    List<string> s_UnqProt = new List<string>();
                    // Peptides
                    if (Model.RCalls.TableContainsColumn(
                        FileNameVault["RowMetadataTable01FDR"],
                        FileNameVault["RowMetadataTablePeptideColumnName"]))
                    {
                        // 1% FDR Summary
                        s_UnqPep = Model.RCalls.GetCharacterVector(
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
                        s_UnqProt = Model.RCalls.GetCharacterVector(
                            string.Format(
                            "length(unique({0}[,'{1}']))",
                            FileNameVault["RowMetadataTable01FDR"],
                            FileNameVault["RowMetadataTableProteinColumnName"]));
                    }
                    else if (Model.RCalls.TableContainsColumn(
                        FileNameVault["RowMetadataTable01FDR"],
                        FileNameVault["RowMetadataTableAltProteinColumnName"]))
                    {
                        s_UnqProt = Model.RCalls.GetCharacterVector(
                            string.Format(
                            "length(unique({0}[,'{1}']))",
                            FileNameVault["RowMetadataTable01FDR"],
                            FileNameVault["RowMetadataTableAltProteinColumnName"]));
                    }

                    DataRow dr_Fdr01 = dt_Return.NewRow();
                    dr_Fdr01["FDR"] = 1;
                    dr_Fdr01["Unique Peptides"] = s_UnqPep[0];
                    dr_Fdr01["Unique Proteins"] = s_UnqProt[0];
                    dt_Return.Rows.Add(dr_Fdr01);
                }
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered within " +
                    "GetSpectralCountSummary(): " + exc.ToString(),
                    ModuleName, StepNumber);
                return null;
            }

            return dt_Return;
        }

        private DataTable GetSpectralCountStatSummary()
        {
            DataTable dt_Return = new DataTable();

            DataColumn dc_FDR = new DataColumn("P-value");
            dt_Return.Columns.Add(dc_FDR);
            DataColumn dc_Proteins = new DataColumn("Proteins");
            dt_Return.Columns.Add(dc_Proteins);

            try
            {
                if (Model.RCalls.ContainsObject(
                    FileNameVault["BbmResultsFdr01"]))
                {
                    if (Model.RCalls.TableContainsColumn(
                        FileNameVault["BbmResultsFdr01"],
                        FileNameVault["BBM_Pvals"]))
                    {
                        // P-value 0.05
                        DataRow dr05 = dt_Return.NewRow();
                        double pVal = 0.05;

                        int i_Prot = Model.RCalls.GetNumberOfRowsInTable(
                            FileNameVault["BbmResultsFdr01"],
                            FileNameVault["BBM_Pvals"],
                            null, pVal.ToString());

                        dr05["P-value"] = "< " + pVal;
                        dr05["Proteins"] = i_Prot;
                        dt_Return.Rows.Add(dr05);



                        // P-value 0.01
                        DataRow dr01 = dt_Return.NewRow();
                        pVal = 0.01;

                        i_Prot = Model.RCalls.GetNumberOfRowsInTable(
                            FileNameVault["BbmResultsFdr01"],
                            FileNameVault["BBM_Pvals"],
                            null, pVal.ToString());

                        dr01["P-value"] = "< " + pVal;
                        dr01["Proteins"] = i_Prot;

                        dt_Return.Rows.Add(dr01);
                    }
                }
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered within " +
                    "GetSpectralCountStatSummary(): " + exc.ToString(),
                    ModuleName, StepNumber);
                return null;
            }

            return dt_Return;
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
