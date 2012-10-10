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

                BuildHtmlFile();
            }
        }

        /// <summary>
        /// Construct the HTML file
        /// </summary>
        private void BuildHtmlFile()
        {
            esp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                string s_CssFileName = "styles.css", s_DatasetsFileName = "Datasets.html",
                    s_QCFileName = "QC.html";
                List<clsHtmlLinkNode> l_NavBarNodes = new List<clsHtmlLinkNode>();
                
                l_NavBarNodes.Add(new clsHtmlLinkNode(
                    "Home", esp.FileName, false));
                l_NavBarNodes.Add(new clsHtmlLinkNode(
                    "Datasets", s_DatasetsFileName, false));
                l_NavBarNodes.Add(new clsHtmlLinkNode(
                    "QC Plots", s_QCFileName, false));
                                
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
                    , "Spectral_Count_Summary.png"
                    , clsSQLiteHandler.GetDataTable("SELECT * FROM T_MAC_SpecCnt_Summary",
                        Path.Combine(esp.WorkDirectory, "Results.db3"))
                    , 1, 1, 1));
                sb_QC.Append("\t\t</DIV>\n");

                // Missed Cleavages
                sb_QC.Append("\t\t<A NAME='mc'/A>\n");
                if (File.Exists(Path.Combine(esp.WorkDirectory, "Plots", "MissedCleavage_Summary.png")))
                {
                    sb_QC.Append(clsHTMLFileHandler.GetQCElement(
                        "Missed Cleavage Summary"
                        , "table_header"
                        , "MissedCleavage_Summary.png"
                        , clsSQLiteHandler.GetDataTable("SELECT * FROM T_MissedCleavageSummary",
                            Path.Combine(esp.WorkDirectory, "Results.db3"))
                        , 1, 1, 1));
                }
                if (File.Exists(Path.Combine(esp.WorkDirectory, "Plots", "MissedCleavage_Summary_10.png")))
                {
                    sb_QC.Append(clsHTMLFileHandler.GetQCElement(
                        "10% FDR Missed Cleavage Summary"
                        , "table_header"
                        , "MissedCleavage_Summary_10.png"
                        , clsGenericRCalls.GetDataTable(s_RInstance, "T_MissedCleavageSummary_10")
                        , 1, 1, 1));
                }
                if (File.Exists(Path.Combine(esp.WorkDirectory, "Plots", "MissedCleavage_Summary_5.png")))
                {
                    sb_QC.Append(clsHTMLFileHandler.GetQCElement(
                        "5% FDR Missed Cleavage Summary"
                        , "table_header"
                        , "MissedCleavage_Summary_5.png"
                        , clsGenericRCalls.GetDataTable(s_RInstance, "T_MissedCleavageSummary_5")
                        , 1, 1, 1));
                }
                if (File.Exists(Path.Combine(esp.WorkDirectory, "Plots", "MissedCleavage_Summary_1.png")))
                {
                    sb_QC.Append(clsHTMLFileHandler.GetQCElement(
                        "1% FDR Missed Cleavage Summary"
                        , "table_header"
                        , "MissedCleavage_Summary_1.png"
                        , clsGenericRCalls.GetDataTable(s_RInstance, "T_MissedCleavageSummary_1")
                        , 1, 1, 1));
                }                

                // Tryptic peptides
                sb_QC.Append("\t\t<A NAME='tp'/A>\n");

                sb_QC.Append(clsHTMLFileHandler.GetQCElement(
                    "Tryptic Peptide Summary"
                    , "table_header"
                    , "Tryptic_Summary.png"
                    , clsSQLiteHandler.GetDataTable("SELECT * FROM T_MAC_Trypticity_Summary",
                    Path.Combine(esp.WorkDirectory, "Results.db3"))
                    , 1, 1, 1));

                // Hexbin plot
                sb_QC.Append("\t\t<A NAME='hb' /A>\n");

                if (File.Exists(Path.Combine(esp.WorkDirectory, "Plots", "Filtered_MSGF_vs_PPM.png")))
                {
                    sb_QC.Append("\t\t<P ID='table_header'>MSGF vs PPM Hexbin Plot</P>\n");
                    sb_QC.Append(clsHTMLFileHandler.GetPictureCode(
                        "Filtered_MSGF_vs_PPM.png", true, "pos_left", null, null));
                }
                if (File.Exists(Path.Combine(esp.WorkDirectory, "Plots", "Filtered_10_MSGF_vs_PPM.png")))
                {
                    sb_QC.Append("\t\t<P ID='table_header'>10% FDR MSGF vs PPM Hexbin Plot</P>\n");
                    sb_QC.Append(clsHTMLFileHandler.GetPictureCode(
                        "Filtered_10_MSGF_vs_PPM.png", true, "pos_left", null, null));
                }
                if (File.Exists(Path.Combine(esp.WorkDirectory, "Plots", "Filtered_5_MSGF_vs_PPM.png")))
                {
                    sb_QC.Append("\t\t<P ID='table_header'>5% FDR MSGF vs PPM Hexbin Plot</P>\n");
                    sb_QC.Append(clsHTMLFileHandler.GetPictureCode(
                        "Filtered_5_MSGF_vs_PPM.png", true, "pos_left", null, null));
                }
                if (File.Exists(Path.Combine(esp.WorkDirectory, "Plots", "Filtered_1_MSGF_vs_PPM.png")))
                {
                    sb_QC.Append("\t\t<P ID='table_header'>1% FDR MSGF vs PPM Hexbin Plot</P>\n");
                    sb_QC.Append(clsHTMLFileHandler.GetPictureCode(
                        "Filtered_1_MSGF_vs_PPM.png", true, "pos_left", null, null));
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

                // Construct and write-out the main html summary page
                StringBuilder sb_HTML = new StringBuilder();

                l_NavBarNodes.Add(new clsHtmlLinkNode(
                    "Correlation", "ch", true));

                sb_HTML.Append(clsHTMLFileHandler.GetHtmlHeader());
                sb_HTML.Append(clsHTMLFileHandler.GetHtmlJavascriptStart());
                sb_HTML.Append(WriteHtmlScripts());
                sb_HTML.Append(clsHTMLFileHandler.GetHtmlScriptEnd());
                sb_HTML.Append(clsHTMLFileHandler.GetCSSLink(s_CssFileName));
                sb_HTML.Append(clsHTMLFileHandler.GetEndHeadStartBody());
                sb_HTML.Append(clsHTMLFileHandler.GetNavTable(l_NavBarNodes));
                //sb_HTML.Append(clsHTMLFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
                sb_HTML.Append(WriteHtmlBody(HTMLFileType.Index));

                sb_HTML.Append("\t<DIV ID='main_content'>\n");
                sb_HTML.Append("\t\t<A NAME='ch'/A>\n");
                sb_HTML.Append("\t\t<P ID='table_header'>Correlation Heatmap</P>\n");

                string s = Path.Combine(esp.WorkDirectory, "Plots", "T_SpectralCounts_CorrelationHeatmap.png");
                if (File.Exists(s))
                    sb_HTML.Append(clsHTMLFileHandler.GetPictureCode(
                        "T_SpectralCounts_CorrelationHeatmap.png", true, "pos_left", null, null));
                s = Path.Combine(esp.WorkDirectory, "Plots", "T_SpectralCounts_10_CorrelationHeatmap.png");
                if (File.Exists(s))
                    sb_HTML.Append(clsHTMLFileHandler.GetPictureCode(
                        "T_SpectralCounts_10_CorrelationHeatmap.png", true, "pos_left", null, null));
                s = Path.Combine(esp.WorkDirectory, "Plots", "T_SpectralCounts_5_CorrelationHeatmap.png");
                if (File.Exists(s)) 
                    sb_HTML.Append(clsHTMLFileHandler.GetPictureCode(
                        "T_SpectralCounts_5_CorrelationHeatmap.png", true, "pos_left", null, null));
                s = Path.Combine(esp.WorkDirectory, "Plots", "T_SpectralCounts_1_CorrelationHeatmap.png");
                if (File.Exists(s))
                    sb_HTML.Append(clsHTMLFileHandler.GetPictureCode(
                        "T_SpectralCounts_1_CorrelationHeatmap.png", true, "pos_left", null, null));

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
                    s_Body = clsHTMLFileHandler.GetDatasetTableHtml(
                        Path.Combine(esp.WorkDirectory, esp.DatabaseName), null,
                            "table_header", "left", 0, 2, 4);
                    break;
                case HTMLFileType.Index:

                    break;
            }

            return s_Body;
        }
        #endregion

    }
}
