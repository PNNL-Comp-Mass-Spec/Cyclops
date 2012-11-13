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

using RDotNet;
using log4net;

namespace Cyclops.ExportModules
{
    public class clsSCO_Summary_HTML : clsBaseExportModule
    {
        #region Members
        private ExportModules.cslExportParameterHandler esp =
            new ExportModules.cslExportParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        private string s_LineDelimiter = "\n";
        private string s_Tab = "\t";
        private enum HTMLFileType { Dataset, Index };
        private Dictionary<string, string>
            d_FileNameVault = new Dictionary<string, string>();

        private string s_RInstance;
        #endregion

        #region Constructors
        /// <summary>
        /// Exports a Spectral Count HTML summary file of the pipeline and workflow
        /// </summary>
        public clsSCO_Summary_HTML()
        {
            ModuleName = "Sco HTML Summary Module";
        }
        /// <summary>
        /// Exports a Spectral Count  HTML summary file of the pipeline and workflow
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsSCO_Summary_HTML(string InstanceOfR)
        {
            ModuleName = "Sco HTML Summary Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Exports a Spectral Count  HTML summary file of the pipeline and workflow
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsSCO_Summary_HTML(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Sco HTML Summary Module";
            Model = TheCyclopsModel;            
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties
        public Dictionary<string, string> FileNameVault
        {
            get { return d_FileNameVault; }
            set { d_FileNameVault = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Determine is all the necessary parameters are being passed to the object
        /// </summary>
        /// <returns>Returns true import module can proceed</returns>
        public bool CheckPassedParameters()
        {
            bool b_2Param = true;

            // NECESSARY PARAMETERS
            if (!esp.HasFileName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR Sco HTML SUMMARY FILE: 'fileName' was not found in the passed parameters");
                b_2Param = false;
            }
            if (!esp.HasWorkDirectory)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR Sco HTML SUMMARY FILE: 'workDir' was not found in the passed parameters");
                b_2Param = false;
            }

            return b_2Param;
        }

        /// <summary>
        /// Runs module
        /// </summary>
        public override void PerformOperation()
        {
            if (Model.SuccessRunningPipeline)
            {
                Model.IncrementStep(ModuleName);

                traceLog.Info("Preparing Spectral Count HTML summary file...");

                AddDefaultValues2FileNameVault();

                BuildHtmlFile();
            }
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
        /// Construct the HTML file
        /// </summary>
        private void BuildHtmlFile()
        {
            esp.GetParameters(ModuleName, Parameters);
            
            if (CheckPassedParameters())
            {
                string s_CssFileName = FileNameVault["CssFileName"],
                    s_DatasetsFileName = FileNameVault["DatasetsHtmlFileName"],
                    s_QCFileName = FileNameVault["QcHtmlFileName"],
                    s_CorrFileName = FileNameVault["CorrelationHtmlFileName"],
                    s_HeatmapsFileName = FileNameVault["HeatmapsFileName"];

                List<clsHtmlLinkNode> l_NavBarNodes = new List<clsHtmlLinkNode>();
                
                l_NavBarNodes.Add(new clsHtmlLinkNode(
                    "Home", esp.FileName, false));
                l_NavBarNodes.Add(new clsHtmlLinkNode(
                    "Datasets", s_DatasetsFileName, false));
                l_NavBarNodes.Add(new clsHtmlLinkNode(
                    "QC Plots", s_QCFileName, false));
                l_NavBarNodes.Add(new clsHtmlLinkNode(
                    "Correlation Heatmaps", s_CorrFileName, false));
                l_NavBarNodes.Add(new clsHtmlLinkNode(
                    "Heatmaps", s_HeatmapsFileName, false));
                                
                using (StreamWriter sw_Css = File.AppendText(Path.Combine(esp.WorkDirectory, s_CssFileName)))
                {
                    sw_Css.WriteLine(clsHTMLFileHandler.GetCSS(clsHTMLFileHandler.CssStyle.NavBar, 160));
                    sw_Css.WriteLine(clsHTMLFileHandler.GetCSS(clsHTMLFileHandler.CssStyle.LeftIndent, 160));
                    sw_Css.WriteLine(clsHTMLFileHandler.GetCSS(clsHTMLFileHandler.CssStyle.Th, 160));
                }

                // Construct and write-out the Datasets Page
                StringBuilder sb_Datasets = new StringBuilder();

                sb_Datasets.Append(clsHTMLFileHandler.GetHtmlHeader());
                sb_Datasets.Append(clsHTMLFileHandler.GetCSSLink(s_CssFileName));
                sb_Datasets.Append(clsHTMLFileHandler.GetHtmlJavascriptStart());
                sb_Datasets.Append(WriteHtmlScripts());
                sb_Datasets.Append(clsHTMLFileHandler.GetHtmlScriptEnd());
                sb_Datasets.Append(clsHTMLFileHandler.GetEndHeadStartBody());
                sb_Datasets.Append(clsHTMLFileHandler.GetNavTable(l_NavBarNodes));
                //sb_Datasets.Append(clsHTMLFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
                sb_Datasets.Append("\t\t<DIV ID='main_content'>\n");
                sb_Datasets.Append(WriteHtmlBody(HTMLFileType.Dataset));
                sb_Datasets.Append("\t\t</DIV>\n");
                sb_Datasets.Append(clsHTMLFileHandler.GetEndBodyEndHtml());

                StreamWriter sw_Datasets = new StreamWriter(Path.Combine(esp.WorkDirectory,
                    s_DatasetsFileName));
                sw_Datasets.Write(sb_Datasets);
                sw_Datasets.Close();
                sb_Datasets.Clear();

                // Construct and write-out the QC Page
                StringBuilder sb_QC = new StringBuilder();

                clsHtmlLinkNode node_ScoSummary = new clsHtmlLinkNode("Summary", "sum", true);
                l_NavBarNodes.Add(node_ScoSummary);
                clsHtmlLinkNode node_MissedCleavages = new clsHtmlLinkNode("Missed Cleavages", "mc", true);
                l_NavBarNodes.Add(node_MissedCleavages);
                clsHtmlLinkNode node_TrypticPeptides = new clsHtmlLinkNode("Tryptic Peptides", "tp", true);
                l_NavBarNodes.Add(node_TrypticPeptides);
                clsHtmlLinkNode node_Hexbin = new clsHtmlLinkNode("Hexbin", "hb", true);
                l_NavBarNodes.Add(node_Hexbin);

                sb_QC.Append(clsHTMLFileHandler.GetHtmlHeader());
                sb_QC.Append(clsHTMLFileHandler.GetHtmlJavascriptStart());
                sb_QC.Append(WriteHtmlScripts());
                sb_QC.Append(clsHTMLFileHandler.GetHtmlScriptEnd());
                sb_QC.Append(clsHTMLFileHandler.GetCSSLink(s_CssFileName));
                sb_QC.Append(clsHTMLFileHandler.GetEndHeadStartBody());
                sb_QC.Append(clsHTMLFileHandler.GetNavTable(l_NavBarNodes));
                //sb_QC.Append(clsHTMLFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
                sb_QC.Append(WriteHtmlBody(HTMLFileType.Index));

                sb_QC.Append("\t<DIV ID='main_content'>\n");

                // Spectral Count Summary
                sb_QC.Append("\t\t<A NAME='sum' />\n");
                sb_QC.Append("\t\t<DIV ID='SpectralCount'>\n");    
                sb_QC.Append(clsHTMLFileHandler.GetQCElement("Spectral Count Summary"
                    , "table_header"
                    , FileNameVault["SpectralCountSummaryFigureFileName"]
                    , clsSQLiteHandler.GetDataTable("SELECT * FROM T_MAC_SpecCnt_Summary",
                        Path.Combine(esp.WorkDirectory, "Results.db3"))
                    , 1, 1, 1));
                sb_QC.Append("\t\t</DIV>\n");

                // Missed Cleavages
                sb_QC.Append("\t\t<A NAME='mc'/A>\n");
                if (File.Exists(Path.Combine(esp.WorkDirectory, "Plots",
                    FileNameVault["MissedCleavageSummaryFigureFileName"])))
                {
                    sb_QC.Append(clsHTMLFileHandler.GetQCElement(
                        "Missed Cleavage Summary"
                        , "table_header"
                        , FileNameVault["MissedCleavageSummaryFigureFileName"]
                        , clsSQLiteHandler.GetDataTable("SELECT * FROM T_MissedCleavageSummary",
                            Path.Combine(esp.WorkDirectory, "Results.db3"))
                        , 1, 1, 1));
                }
                if (File.Exists(Path.Combine(esp.WorkDirectory, "Plots",
                    FileNameVault["MissedCleavageSummary10percentFdrFigureFileName"])))
                {
                    sb_QC.Append(clsHTMLFileHandler.GetQCElement(
                        "10% FDR Missed Cleavage Summary"
                        , "table_header"
                        , FileNameVault["MissedCleavageSummary10percentFdrFigureFileName"]
                        , clsGenericRCalls.GetDataTable(s_RInstance, "T_MissedCleavageSummary_10")
                        , 1, 1, 1));
                }
                if (File.Exists(Path.Combine(esp.WorkDirectory, "Plots",
                    FileNameVault["MissedCleavageSummary05percentFdrFigureFileName"])))
                {
                    sb_QC.Append(clsHTMLFileHandler.GetQCElement(
                        "5% FDR Missed Cleavage Summary"
                        , "table_header"
                        , FileNameVault["MissedCleavageSummary05percentFdrFigureFileName"]
                        , clsGenericRCalls.GetDataTable(s_RInstance, "T_MissedCleavageSummary_5")
                        , 1, 1, 1));
                }
                if (File.Exists(Path.Combine(esp.WorkDirectory, "Plots",
                    FileNameVault["MissedCleavageSummary01percentFdrFigureFileName"])))
                {
                    sb_QC.Append(clsHTMLFileHandler.GetQCElement(
                        "1% FDR Missed Cleavage Summary"
                        , "table_header"
                        , FileNameVault["MissedCleavageSummary01percentFdrFigureFileName"]
                        , clsGenericRCalls.GetDataTable(s_RInstance, "T_MissedCleavageSummary_1")
                        , 1, 1, 1));
                }                

                // Tryptic peptides
                sb_QC.Append("\t\t<A NAME='tp'/A>\n");

                sb_QC.Append(clsHTMLFileHandler.GetQCElement(
                    "Tryptic Peptide Summary"
                    , "table_header"
                    , FileNameVault["TrypticPeptideSummaryFigureFileName"]
                    , clsSQLiteHandler.GetDataTable("SELECT * FROM T_MAC_Trypticity_Summary",
                    Path.Combine(esp.WorkDirectory, "Results.db3"))
                    , 1, 1, 1));

                // Hexbin plot
                sb_QC.Append("\t\t<A NAME='hb' /A>\n");

                if (File.Exists(Path.Combine(esp.WorkDirectory, "Plots", "Filtered_MSGF_vs_PPM.png")))
                {
                    sb_QC.Append("\t\t<P ID='table_header'>MSGF vs PPM Hexbin Plot</P>\n");
                    sb_QC.Append(clsHTMLFileHandler.GetPictureCode(
                        FileNameVault["FilteredMsgfPpmHexbinFigureFileName"], 
                        true, "pos_left", null, null));
                }
                if (File.Exists(Path.Combine(esp.WorkDirectory, "Plots", "Filtered_10_MSGF_vs_PPM.png")))
                {
                    sb_QC.Append("\t\t<P ID='table_header'>10% FDR MSGF vs PPM Hexbin Plot</P>\n");
                    sb_QC.Append(clsHTMLFileHandler.GetPictureCode(
                        FileNameVault["FilteredMsgfPpmHexbin10percentFdrFigureFileName"], 
                        true, "pos_left", null, null));
                }
                if (File.Exists(Path.Combine(esp.WorkDirectory, "Plots", "Filtered_5_MSGF_vs_PPM.png")))
                {
                    sb_QC.Append("\t\t<P ID='table_header'>5% FDR MSGF vs PPM Hexbin Plot</P>\n");
                    sb_QC.Append(clsHTMLFileHandler.GetPictureCode(
                        FileNameVault["FilteredMsgfPpmHexbin05percentFdrFigureFileName"], 
                        true, "pos_left", null, null));
                }
                if (File.Exists(Path.Combine(esp.WorkDirectory, "Plots", "Filtered_1_MSGF_vs_PPM.png")))
                {
                    sb_QC.Append("\t\t<P ID='table_header'>1% FDR MSGF vs PPM Hexbin Plot</P>\n");
                    sb_QC.Append(clsHTMLFileHandler.GetPictureCode(
                        FileNameVault["FilteredMsgfPpmHexbin01percentFdrFigureFileName"], 
                        true, "pos_left", null, null));
                }
                

                sb_QC.Append("\t</DIV>\n");
                sb_QC.Append(clsHTMLFileHandler.GetEndBodyEndHtml());

                StreamWriter sw_QC = new StreamWriter(Path.Combine(esp.WorkDirectory,
                    s_QCFileName));
                sw_QC.WriteLine(sb_QC);
                sw_QC.Close();

                l_NavBarNodes.Remove(node_ScoSummary);
                l_NavBarNodes.Remove(node_MissedCleavages);
                l_NavBarNodes.Remove(node_TrypticPeptides);
                l_NavBarNodes.Remove(node_Hexbin);


                StringBuilder sb_Corr = new StringBuilder();

                clsHtmlLinkNode hln_Corr = new clsHtmlLinkNode(
                    "Correlation", "ch", true);
                l_NavBarNodes.Add(hln_Corr);

                sb_Corr.Append(clsHTMLFileHandler.GetHtmlHeader());
                sb_Corr.Append(clsHTMLFileHandler.GetHtmlJavascriptStart());
                sb_Corr.Append(WriteHtmlScripts());
                sb_Corr.Append(clsHTMLFileHandler.GetHtmlScriptEnd());
                sb_Corr.Append(clsHTMLFileHandler.GetCSSLink(s_CssFileName));
                sb_Corr.Append(clsHTMLFileHandler.GetEndHeadStartBody());
                sb_Corr.Append(clsHTMLFileHandler.GetNavTable(l_NavBarNodes));
                //sb_HTML.Append(clsHTMLFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
                sb_Corr.Append(WriteHtmlBody(HTMLFileType.Index));

                sb_Corr.Append("\t<DIV ID='main_content'>\n");
                sb_Corr.Append("\t\t<A NAME='ch'/A>\n");
                sb_Corr.Append("\t\t<P ID='table_header'>Correlation Heatmap</P>\n");

                string s = Path.Combine(esp.WorkDirectory, "Plots", 
                    FileNameVault["SpectralCountsCorrelationHeatmapFigureFileName"]);
                if (File.Exists(s))
                    sb_Corr.Append(clsHTMLFileHandler.GetPictureCode(
                        FileNameVault["SpectralCountsCorrelationHeatmapFigureFileName"], 
                        true, "pos_left", null, null));
                s = Path.Combine(esp.WorkDirectory, "Plots",
                    FileNameVault["SpectralCountsCorrelationHeatmap10percentFdrFigureFileName"]);
                if (File.Exists(s))
                    sb_Corr.Append(clsHTMLFileHandler.GetPictureCode(
                        FileNameVault["SpectralCountsCorrelationHeatmap10percentFdrFigureFileName"], 
                        true, "pos_left", null, null));
                s = Path.Combine(esp.WorkDirectory, "Plots",
                    FileNameVault["SpectralCountsCorrelationHeatmap05percentFdrFigureFileName"]);
                if (File.Exists(s))
                    sb_Corr.Append(clsHTMLFileHandler.GetPictureCode(
                        FileNameVault["SpectralCountsCorrelationHeatmap05percentFdrFigureFileName"], 
                        true, "pos_left", null, null));
                s = Path.Combine(esp.WorkDirectory, "Plots", 
                    FileNameVault["SpectralCountsCorrelationHeatmap01percentFdrFigureFileName"]);
                if (File.Exists(s))
                    sb_Corr.Append(clsHTMLFileHandler.GetPictureCode(
                        FileNameVault["SpectralCountsCorrelationHeatmap01percentFdrFigureFileName"], 
                        true, "pos_left", null, null));

                sb_Corr.Append("\t</DIV>\n");
                sb_Corr.Append(clsHTMLFileHandler.GetEndBodyEndHtml());


                // TODO : Write the html out to the file
                StreamWriter sw_Corr = new StreamWriter(Path.Combine(esp.WorkDirectory, s_CorrFileName));
                sw_Corr.Write(sb_Corr);
                sw_Corr.Close();
                l_NavBarNodes.Remove(hln_Corr);


                StringBuilder sb_Heatmaps = new StringBuilder();
                sb_Heatmaps.Append(clsHTMLFileHandler.GetHtmlHeader());
                sb_Heatmaps.Append(clsHTMLFileHandler.GetHtmlJavascriptStart());
                sb_Heatmaps.Append(WriteHtmlScripts());
                sb_Heatmaps.Append(clsHTMLFileHandler.GetHtmlScriptEnd());
                sb_Heatmaps.Append(clsHTMLFileHandler.GetCSSLink(s_CssFileName));
                sb_Heatmaps.Append(clsHTMLFileHandler.GetEndHeadStartBody());
                sb_Heatmaps.Append(clsHTMLFileHandler.GetNavTable(l_NavBarNodes));
                sb_Heatmaps.Append(WriteHtmlBody(HTMLFileType.Index));

                sb_Heatmaps.Append("\t<DIV ID='main_content'>\n");
                

                if (File.Exists(
                    Path.Combine(esp.WorkDirectory, "Plots", 
                    FileNameVault["BBM_Heatmap_01_FigureFileName"])))
                {
                    sb_Heatmaps.Append("\t\t<A NAME='ch'/A>\n");
                    sb_Heatmaps.Append("\t\t<P ID='table_header'>Proteins of Significant Difference (P-value < 0.01)</P>\n");

                    sb_Heatmaps.Append(clsHTMLFileHandler.GetPictureCode(
                        FileNameVault["BBM_Heatmap_01_FigureFileName"],
                        true, "pos_left", null, null));
                }

                if (File.Exists(
                    Path.Combine(esp.WorkDirectory, "Plots",
                    FileNameVault["BBM_Heatmap_05_FigureFileName"])))
                {
                    sb_Heatmaps.Append("\t\t<A NAME='ch'/A>\n");
                    sb_Heatmaps.Append("\t\t<P ID='table_header'>Proteins of Significant Difference (P-value < 0.05)</P>\n");

                    sb_Heatmaps.Append(clsHTMLFileHandler.GetPictureCode(
                        FileNameVault["BBM_Heatmap_05_FigureFileName"],
                        true, "pos_left", null, null));
                }

                sb_Heatmaps.Append("\t</DIV>\n");
                sb_Heatmaps.Append(clsHTMLFileHandler.GetEndBodyEndHtml());

                StreamWriter sw_Heatmaps = new StreamWriter(Path.Combine(esp.WorkDirectory, s_HeatmapsFileName));
                sw_Heatmaps.Write(sb_Heatmaps);
                sw_Heatmaps.Close();

                // Construct and write-out the main html summary page
                StringBuilder sb_HTML = new StringBuilder();                

                sb_HTML.Append(clsHTMLFileHandler.GetHtmlHeader());
                sb_HTML.Append(clsHTMLFileHandler.GetHtmlJavascriptStart());
                sb_HTML.Append(WriteHtmlScripts());
                sb_HTML.Append(clsHTMLFileHandler.GetHtmlScriptEnd());
                sb_HTML.Append(clsHTMLFileHandler.GetCSSLink(s_CssFileName));
                sb_HTML.Append(clsHTMLFileHandler.GetEndHeadStartBody());
                sb_HTML.Append(clsHTMLFileHandler.GetNavTable(l_NavBarNodes));
                sb_HTML.Append(WriteHtmlBody(HTMLFileType.Index));

                sb_HTML.Append("\t<DIV ID='main_content'>\n");                
                //sb_HTML.Append("\t\t<P ID='table_header'>Analysis Summary Statistics</P>\n");

                sb_HTML.Append("\t\t" +
                    clsHTMLFileHandler.GetSummaryTableHtml(
                    GetSpectralCountSummary(),
                    "Spectral Count Analysis Summary",
                    "table_header",
                    0, 2, 4) + "\n\n" +
                    clsHTMLFileHandler.GetSummaryTableHtml(
                    GetSpectralCountStatSummary(),
                    "Spectral Count Beta-Binomial Model Statistics Summary",
                    "table_header",
                    0, 2, 4));
                
                sb_HTML.Append("\t</DIV>\n");
                sb_HTML.Append(clsHTMLFileHandler.GetEndBodyEndHtml());


                // TODO : Write the html out to the file
                StreamWriter sw = new StreamWriter(Path.Combine(esp.WorkDirectory, esp.FileName));
                sw.Write(sb_HTML);
                sw.Close();
            }
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
                    if (File.Exists(Path.Combine(esp.WorkDirectory, esp.DatabaseName)))
                    {
                        s_Body = clsHTMLFileHandler.GetDatasetTableHtml(
                            Path.Combine(esp.WorkDirectory, esp.DatabaseName), null,
                                "table_header", "left", 0, 2, 4);
                    }
                    else
                    {
                        traceLog.Error(string.Format(
                            "ERROR in clsSCO_Summary_HTML: " +
                            "File: {0} does not exist",
                            Path.Combine(esp.WorkDirectory, esp.DatabaseName)));
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

            if (clsGenericRCalls.ContainsObject(s_RInstance,
                FileNameVault["RowMetadataTable10FDR"]))
            {
                if (clsGenericRCalls.TableContainsColumn(s_RInstance,
                    FileNameVault["RowMetadataTable10FDR"],
                    FileNameVault["RowMetadataTablePeptideColumnName"]))
                {
                    // 10% FDR Summary
                    List<string> s_UnqPep = clsGenericRCalls.GetCharacterVector(
                        s_RInstance, string.Format(
                        "length(unique({0}[,'{1}']))",
                        FileNameVault["RowMetadataTable10FDR"],
                        FileNameVault["RowMetadataTablePeptideColumnName"]));
                    List<string> s_UnqProt = clsGenericRCalls.GetCharacterVector(
                        s_RInstance, string.Format(
                        "length(unique({0}[,'{1}']))",
                        FileNameVault["RowMetadataTable10FDR"],
                        FileNameVault["RowMetadataTableProteinColumnName"]));

                    DataRow dr_Fdr10 = dt_Return.NewRow();
                    dr_Fdr10["FDR"] = 10;
                    dr_Fdr10["Unique Peptides"] = s_UnqPep[0];
                    dr_Fdr10["Unique Proteins"] = s_UnqProt[0];
                    dt_Return.Rows.Add(dr_Fdr10);
                }
            }

            if (clsGenericRCalls.ContainsObject(s_RInstance,
                FileNameVault["RowMetadataTable05FDR"]))
            {
                if (clsGenericRCalls.TableContainsColumn(s_RInstance,
                    FileNameVault["RowMetadataTable05FDR"],
                    FileNameVault["RowMetadataTablePeptideColumnName"]))
                {
                    // 5% FDR Summary
                    List<string> s_UnqPep = clsGenericRCalls.GetCharacterVector(
                        s_RInstance, string.Format(
                        "length(unique({0}[,'{1}']))",
                        FileNameVault["RowMetadataTable05FDR"],
                        FileNameVault["RowMetadataTablePeptideColumnName"]));
                    List<string> s_UnqProt = clsGenericRCalls.GetCharacterVector(
                        s_RInstance, string.Format(
                        "length(unique({0}[,'{1}']))",
                        FileNameVault["RowMetadataTable05FDR"],
                        FileNameVault["RowMetadataTableProteinColumnName"]));

                    DataRow dr_Fdr05 = dt_Return.NewRow();
                    dr_Fdr05["FDR"] = 5;
                    dr_Fdr05["Unique Peptides"] = s_UnqPep[0];
                    dr_Fdr05["Unique Proteins"] = s_UnqProt[0];
                    dt_Return.Rows.Add(dr_Fdr05);
                }
            }

            if (clsGenericRCalls.ContainsObject(s_RInstance,
                FileNameVault["RowMetadataTable01FDR"]))
            {
                if (clsGenericRCalls.TableContainsColumn(s_RInstance,
                    FileNameVault["RowMetadataTable01FDR"],
                    FileNameVault["RowMetadataTablePeptideColumnName"]))
                {
                    // 1% FDR Summary
                    List<string> s_UnqPep = clsGenericRCalls.GetCharacterVector(
                        s_RInstance, string.Format(
                        "length(unique({0}[,'{1}']))",
                        FileNameVault["RowMetadataTable01FDR"],
                        FileNameVault["RowMetadataTablePeptideColumnName"]));
                    List<string> s_UnqProt = clsGenericRCalls.GetCharacterVector(
                        s_RInstance, string.Format(
                        "length(unique({0}[,'{1}']))",
                        FileNameVault["RowMetadataTable01FDR"],
                        FileNameVault["RowMetadataTableProteinColumnName"]));

                    DataRow dr_Fdr01 = dt_Return.NewRow();
                    dr_Fdr01["FDR"] = 1;
                    dr_Fdr01["Unique Peptides"] = s_UnqPep[0];
                    dr_Fdr01["Unique Proteins"] = s_UnqProt[0];
                    dt_Return.Rows.Add(dr_Fdr01);
                }
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

            
            if (clsGenericRCalls.ContainsObject(
                s_RInstance, FileNameVault["BbmResultsFdr01"]))
            {
                if (clsGenericRCalls.TableContainsColumn(
                    s_RInstance, FileNameVault["BbmResultsFdr01"],
                    FileNameVault["BBM_Pvals"]))
                {
                    // P-value 0.05
                    DataRow dr05 = dt_Return.NewRow();
                    double pVal = 0.05;

                    int i_Prot = clsGenericRCalls.GetNumberOfRowsInTable(
                        s_RInstance, FileNameVault["BbmResultsFdr01"],
                        FileNameVault["BBM_Pvals"],
                        null, pVal.ToString());
                    
                    dr05["P-value"] = "< " + pVal;
                    dr05["Proteins"] = i_Prot;
                    dt_Return.Rows.Add(dr05);



                    // P-value 0.01
                    DataRow dr01 = dt_Return.NewRow();
                    pVal = 0.01;

                    i_Prot = clsGenericRCalls.GetNumberOfRowsInTable(
                        s_RInstance, FileNameVault["BbmResultsFdr01"],
                        FileNameVault["BBM_Pvals"],
                        null, pVal.ToString());

                    dr01["P-value"] = "< " + pVal;
                    dr01["Proteins"] = i_Prot;

                    dt_Return.Rows.Add(dr01);
                }
            }

            return dt_Return;
        }
        #endregion

    }
}
