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
    public class clsLBF_Summary_HTML : clsBaseExportModule
    {
        #region Members
        private ExportModules.cslExportParameterHandler esp =
            new ExportModules.cslExportParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        private string s_LineDelimiter = "\n";
        private string s_Tab = "\t";
        private DataTable dt_Overlap = new DataTable("LBF");
        private enum HTMLFileType { Dataset, Index };
        private string s_RInstance;

        // Table names
        private string s_TypticTableSummaryName = "T_MAC_Trypticity_Summary";
        private bool b_LR = false, b_CT = false;
        #endregion

        #region Constructors
        /// <summary>
        /// Exports an HTML file that summarizes LBF Data Analysis
        /// </summary>
        public clsLBF_Summary_HTML()
        {
            ModuleName = "LBF Quality Control Module";            
        }
        /// <summary>
        /// Exports an HTML file that summarizes LBF Data Analysis
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsLBF_Summary_HTML(string InstanceOfR)
        {
            ModuleName = "LBF Quality Control Module";
            s_RInstance = InstanceOfR;
            
        }
        /// <summary>
        /// Exports an HTML file that summarizes LBF Data Analysis
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsLBF_Summary_HTML(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "LBF Quality Control Module";
            Model = TheCyclopsModel;
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties
        
        #endregion

        #region Methods
        /// <summary>
        /// Sets the boolean values that indicate if normalization algrithms have been run.
        /// </summary>
        private void SetLRandCTflags()
        {
            b_LR = clsGenericRCalls.ContainsObject(s_RInstance, "LR_Log_T_Data");
            b_CT = clsGenericRCalls.ContainsObject(s_RInstance, "CT_Log_T_Data");
        }

        /// <summary>
        /// Determine is all the necessary parameters are being passed to the object
        /// </summary>
        /// <returns>Returns true import module can proceed</returns>
        public bool CheckPassedParameters()
        {
            bool b_2Param = true;

            // NECESSARY PARAMETERS
            if (esp.FileName.Equals(""))
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR LBF Summary HTML: 'fileName' was not found in the passed parameters");
                b_2Param = false;
            }
            if (!esp.HasWorkDirectory)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR LBF Summary HTML: 'workDir' was not found in the passed parameters");
                b_2Param = false;
            }

            return b_2Param;
        }

        /// <summary>
        /// Runs module
        /// </summary>
        public override void PerformOperation()
        {
            traceLog.Info("Preparing to write LBF Summary HTML File...");

            SetLRandCTflags();

            BuildHtmlFile();
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
                    s_SummaryTableFileName = "SummaryTables.html",
                    s_QCFileName = "QC.html", s_BoxPlotFileName = "BoxPlots.html", 
                    s_CorrelationHeatmaps = "CorrHeatmaps.html";
                List<clsHtmlLinkNode> l_NavBarNodes = new List<clsHtmlLinkNode>();

                l_NavBarNodes.Add(new clsHtmlLinkNode(
                    "Home", esp.FileName, false));
                l_NavBarNodes.Add(new clsHtmlLinkNode(
                    "Datasets", s_DatasetsFileName, false));
                l_NavBarNodes.Add(new clsHtmlLinkNode(
                    "Summary Tables", s_SummaryTableFileName, false));
                l_NavBarNodes.Add(new clsHtmlLinkNode(
                    "QC Plots", s_QCFileName, false));
                l_NavBarNodes.Add(new clsHtmlLinkNode(
                    "Box Plots", s_BoxPlotFileName, false));
                l_NavBarNodes.Add(new clsHtmlLinkNode(
                    "Correlation Heatmaps", s_CorrelationHeatmaps, false));

                using (StreamWriter sw_Css = File.AppendText(Path.Combine(esp.WorkDirectory, s_CssFileName)))
                {
                    sw_Css.WriteLine(clsHTMLFileHandler.GetCSS(clsHTMLFileHandler.CssStyle.NavBar, 250));
                    sw_Css.WriteLine(clsHTMLFileHandler.GetCSS(clsHTMLFileHandler.CssStyle.LeftIndent, 250));
                    sw_Css.WriteLine(clsHTMLFileHandler.GetCSS(clsHTMLFileHandler.CssStyle.Th, 250));
                }

                ///
                /// Construct and write-out the Datasets Page
                /// 
                #region Datasets HTML Page
                StringBuilder sb_Datasets = new StringBuilder();

                sb_Datasets.Append(clsHTMLFileHandler.GetHtmlHeader());
                sb_Datasets.Append(clsHTMLFileHandler.GetCSSLink(s_CssFileName));
                sb_Datasets.Append(clsHTMLFileHandler.GetHtmlJavascriptStart());
                sb_Datasets.Append(WriteHtmlScripts());
                sb_Datasets.Append(clsHTMLFileHandler.GetHtmlScriptEnd());
                sb_Datasets.Append(clsHTMLFileHandler.GetEndHeadStartBody());
                sb_Datasets.Append(clsHTMLFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
                //sb_Datasets.Append("\t\t<H2 class='pos_left' style='top:-40px'>Datasets used in the Analysis</H2>n");
                sb_Datasets.Append("\t\t<DIV ID='DatasetTable' style='position: absolute; left:200px; top:50px;'>\n");
                sb_Datasets.Append(clsHTMLFileHandler.GetDatasetTableHtml(
                    Path.Combine(esp.WorkDirectory, esp.DatabaseName), "", "left", 1, 1, 1));
                //sb_Datasets.Append(WriteHtmlBody(HTMLFileType.Dataset));
                sb_Datasets.Append("\t\t</DIV>\n");
                sb_Datasets.Append(clsHTMLFileHandler.GetEndBodyEndHtml());

                StreamWriter sw_Datasets = new StreamWriter(Path.Combine(esp.WorkDirectory,
                    s_DatasetsFileName));
                sw_Datasets.Write(sb_Datasets);
                sw_Datasets.Close();
                sb_Datasets.Clear();
                #endregion

                ///
                /// Construct Summary Tables HTML Page
                /// 
                #region Summary HTML Page
                int i_HeaderTop = -40, i_TableTop = 100,
                    i_HeaderIncrement = 203, i_TableIncrement = 250;
                StringBuilder sb_SummaryTables = new StringBuilder();

                clsHtmlLinkNode ln_SummaryOriginal = new clsHtmlLinkNode("Peptide Original",
                    "pepOrig", true);
                clsHtmlLinkNode ln_SummaryLog2Pep = new clsHtmlLinkNode("Peptide Log2",
                    "peplog2", true);
                clsHtmlLinkNode ln_SummaryCTPep = new clsHtmlLinkNode("Peptide CT",
                    "pepCT", true);
                clsHtmlLinkNode ln_SummaryLRPep = new clsHtmlLinkNode("Peptide LR",
                    "pepLR", true);
                clsHtmlLinkNode ln_SummaryRRProt = new clsHtmlLinkNode("RRollup Protein",
                    "protRR", true);
                clsHtmlLinkNode ln_SummaryRRCTProt = new clsHtmlLinkNode("RRollup CT Protein",
                    "protRRCT", true);
                clsHtmlLinkNode ln_SummaryRRLRProt = new clsHtmlLinkNode("RRollup LR Protein",
                    "protRRLR", true);

                l_NavBarNodes.Add(ln_SummaryOriginal);
                l_NavBarNodes.Add(ln_SummaryLog2Pep);
                if (b_CT)
                {                    
                    l_NavBarNodes.Add(ln_SummaryCTPep);
                }
                if (b_LR)
                {                    
                    l_NavBarNodes.Add(ln_SummaryLRPep);
                }
                
                l_NavBarNodes.Add(ln_SummaryRRProt);
                if (b_CT)
                {                    
                    l_NavBarNodes.Add(ln_SummaryRRCTProt);
                }
                if (b_LR)
                {                    
                    l_NavBarNodes.Add(ln_SummaryRRLRProt);
                }
                
                sb_SummaryTables.Append(clsHTMLFileHandler.GetHtmlHeader());
                sb_SummaryTables.Append(clsHTMLFileHandler.GetCSSLink(s_CssFileName));
                sb_SummaryTables.Append(clsHTMLFileHandler.GetHtmlJavascriptStart());
                sb_SummaryTables.Append(WriteHtmlScripts());
                sb_SummaryTables.Append(clsHTMLFileHandler.GetHtmlScriptEnd());
                sb_SummaryTables.Append(clsHTMLFileHandler.GetEndHeadStartBody());
                sb_SummaryTables.Append(clsHTMLFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));

                sb_SummaryTables.Append(string.Format("\t\t<H2 class='pos_left' style='top:{0}px'>" +
                    "<A NAME='pepOrig' /A>" +
                    "Summary of Original Peptide Abundances</H2>\n",
                    i_HeaderTop));
                sb_SummaryTables.Append(string.Format("\t\t<DIV ID='DatasetTable' style='position: absolute; left:200px; top:{0}px;'>\n",
                    i_TableTop));
                i_HeaderTop = i_HeaderTop + i_HeaderIncrement;
                i_TableTop = i_TableTop + i_TableIncrement;
                DataTable dt_OrigTotalSummary = clsGenericRCalls.GetDataTableIncludingRownames(s_RInstance, "Summary_T_Data$TotalSummary", "QC Params");
                sb_SummaryTables.Append(WriteHtmlBody(HTMLFileType.Index));
                sb_SummaryTables.Append(clsHTMLFileHandler.GetTableHtml(dt_OrigTotalSummary, "left", null, null, null));
                sb_SummaryTables.Append("\t\t</DIV>\n");
                
                sb_SummaryTables.Append(string.Format("\t\t<H2 class='pos_left' style='top:{0}px'>" + 
                    "<A NAME='peplog2' /A>Summary of Log2 Peptide Abundances</H2>\n",
                    i_HeaderTop));                
                sb_SummaryTables.Append(string.Format("\t\t<DIV ID='DatasetTable' style='position: absolute; left:200px; top:{0}px;'>\n",
                    i_TableTop));
                i_HeaderTop = i_HeaderTop + i_HeaderIncrement;
                i_TableTop = i_TableTop + i_TableIncrement;
                DataTable dt_TotalSummary = clsGenericRCalls.GetDataTableIncludingRownames(s_RInstance, "Summary_Log_T_Data$TotalSummary", "QC Params");
                sb_SummaryTables.Append(WriteHtmlBody(HTMLFileType.Index));
                sb_SummaryTables.Append(clsHTMLFileHandler.GetTableHtml(dt_TotalSummary, "left", null, null, null));
                sb_SummaryTables.Append("\t\t</DIV>\n");

                if (b_CT)
                {
                    sb_SummaryTables.Append(string.Format("\t\t<H2 class='pos_left' style='top:{0}px'>" + 
                        "<A NAME='pepCT' /A>Summary of Central Tendency Log2 Peptide Abundances</H2>\n",
                        i_HeaderTop));                    
                    sb_SummaryTables.Append(string.Format("\t\t<DIV ID='DatasetTable' style='position: absolute; left:200px; top:{0}px;'>\n",
                        i_TableTop));
                    i_HeaderTop = i_HeaderTop + i_HeaderIncrement;
                    i_TableTop = i_TableTop + i_TableIncrement;
                    DataTable dt_CTLog2Summary = clsGenericRCalls.GetDataTableIncludingRownames(s_RInstance, "Summary_CT_Log_T_Data$TotalSummary", "QC Params");
                    sb_SummaryTables.Append(WriteHtmlBody(HTMLFileType.Index));
                    sb_SummaryTables.Append(clsHTMLFileHandler.GetTableHtml(dt_CTLog2Summary, "left", null, null, null));
                    sb_SummaryTables.Append("\t\t</DIV>\n");
                }

                if (b_LR)
                {
                    sb_SummaryTables.Append(string.Format("\t\t<H2 class='pos_left' style='top:{0}px'>" +
                        "<A NAME='pepLR' /A>Summary of Linear Regression Log2 Peptide Abundances</H2>\n",
                        i_HeaderTop));                    
                    sb_SummaryTables.Append(string.Format("\t\t<DIV ID='DatasetTable' style='position: absolute; left:200px; top:{0}px;'>\n",
                        i_TableTop));
                    i_HeaderTop = i_HeaderTop + i_HeaderIncrement;
                    i_TableTop = i_TableTop + i_TableIncrement;
                    DataTable dt_LRLog2Summary = clsGenericRCalls.GetDataTableIncludingRownames(s_RInstance, "Summary_LR_Log_T_Data$TotalSummary", "QC Params");
                    sb_SummaryTables.Append(WriteHtmlBody(HTMLFileType.Index));
                    sb_SummaryTables.Append(clsHTMLFileHandler.GetTableHtml(dt_LRLog2Summary, "left", null, null, null));
                    sb_SummaryTables.Append("\t\t</DIV>\n");
                }

                // Proteins
                sb_SummaryTables.Append(string.Format("\t\t<H2 class='pos_left' style='top:{0}px'>" +
                    "<A NAME='protRR' /A>Summary of Protein Abundances from Log2 Peptides (RRollup)</H2>\n",
                    i_HeaderTop));                
                sb_SummaryTables.Append(string.Format("\t\t<DIV ID='DatasetTable' style='position: absolute; left:200px; top:{0}px;'>\n",
                    i_TableTop));
                i_HeaderTop = i_HeaderTop + i_HeaderIncrement;
                i_TableTop = i_TableTop + i_TableIncrement;
                DataTable dt_ProteinTotalSummary = clsGenericRCalls.GetDataTableIncludingRownames(s_RInstance, "Summary_RR_Log_T_Data$TotalSummary", "QC Params");
                sb_SummaryTables.Append(WriteHtmlBody(HTMLFileType.Index));
                sb_SummaryTables.Append(clsHTMLFileHandler.GetTableHtml(dt_ProteinTotalSummary, "left", null, null, null));
                sb_SummaryTables.Append("\t\t</DIV>\n");

                if (b_CT)
                {
                    sb_SummaryTables.Append(string.Format("\t\t<H2 class='pos_left' style='top:{0}px'>" +
                        "<A NAME='protRRCT' /A>Summary of Protein Abundances from Central Tendency Log2 Peptides (RRollup)</H2>\n",
                        i_HeaderTop));                    
                    sb_SummaryTables.Append(string.Format("\t\t<DIV ID='DatasetTable' style='position: absolute; left:200px; top:{0}px;'>\n",
                        i_TableTop));
                    i_HeaderTop = i_HeaderTop + i_HeaderIncrement;
                    i_TableTop = i_TableTop + i_TableIncrement;
                    DataTable dt_ProteinCTTotalSummary = clsGenericRCalls.GetDataTableIncludingRownames(s_RInstance, "Summary_RR_CT_Log_T_Data$TotalSummary", "QC Params");
                    sb_SummaryTables.Append(WriteHtmlBody(HTMLFileType.Index));
                    sb_SummaryTables.Append(clsHTMLFileHandler.GetTableHtml(dt_ProteinCTTotalSummary, "left", null, null, null));
                    sb_SummaryTables.Append("\t\t</DIV>\n");
                }

                if (b_LR)
                {
                    sb_SummaryTables.Append(string.Format("\t\t<H2 class='pos_left' style='top:{0}px'>" +
                        "<A NAME='protRRLR' /A>Summary of Protein Abundances from Linear Regression Log2 Peptides (RRollup)</H2>\n",
                        i_HeaderTop));                    
                    sb_SummaryTables.Append(string.Format("\t\t<DIV ID='DatasetTable' style='position: absolute; left:200px; top:{0}px;'>\n",
                        i_TableTop));
                    i_HeaderTop = i_HeaderTop + i_HeaderIncrement;
                    i_TableTop = i_TableTop + i_TableIncrement;
                    DataTable dt_ProteinLRTotalSummary = clsGenericRCalls.GetDataTableIncludingRownames(s_RInstance, "Summary_RR_LR_Log_T_Data$TotalSummary", "QC Params");
                    sb_SummaryTables.Append(WriteHtmlBody(HTMLFileType.Index));
                    sb_SummaryTables.Append(clsHTMLFileHandler.GetTableHtml(dt_ProteinLRTotalSummary, "left", null, null, null));
                    sb_SummaryTables.Append("\t\t</DIV>\n");
                }

                sb_SummaryTables.Append(clsHTMLFileHandler.GetEndBodyEndHtml());

                StreamWriter sw_SummaryTables = new StreamWriter(Path.Combine(esp.WorkDirectory,
                    s_SummaryTableFileName));
                sw_SummaryTables.Write(sb_SummaryTables);
                sw_SummaryTables.Close();
                sb_SummaryTables.Clear();

                l_NavBarNodes.Remove(ln_SummaryOriginal);
                l_NavBarNodes.Remove(ln_SummaryLog2Pep);
                l_NavBarNodes.Remove(ln_SummaryCTPep);
                l_NavBarNodes.Remove(ln_SummaryLRPep);
                l_NavBarNodes.Remove(ln_SummaryRRProt);
                l_NavBarNodes.Remove(ln_SummaryRRCTProt);
                l_NavBarNodes.Remove(ln_SummaryRRLRProt);
                #endregion

                ///
                /// Construct and write-out the QC Page
                /// 
                #region QC HTML Page
                StringBuilder sb_QC = new StringBuilder();

                bool b_ContainsTrypticPeptideSummary = 
                    clsSQLiteHandler.TableExists(s_TypticTableSummaryName, 
                    Path.Combine(esp.WorkDirectory, "Results.db3"));

                clsHtmlLinkNode node_LBFSummary = new clsHtmlLinkNode("LBF Summary", "sum", true);
                l_NavBarNodes.Add(node_LBFSummary);
                clsHtmlLinkNode node_MissedCleavages = new clsHtmlLinkNode("Missed Cleavages", "mc", true);
                l_NavBarNodes.Add(node_MissedCleavages);

                clsHtmlLinkNode node_TrypticPeptides = new clsHtmlLinkNode("Tryptic Peptides", "tp", true);
                if (b_ContainsTrypticPeptideSummary)
                {                    
                    l_NavBarNodes.Add(node_TrypticPeptides);
                }

                sb_QC.Append(clsHTMLFileHandler.GetHtmlHeader());
                sb_QC.Append(clsHTMLFileHandler.GetHtmlJavascriptStart());
                sb_QC.Append(WriteHtmlScripts());
                sb_QC.Append(clsHTMLFileHandler.GetHtmlScriptEnd());
                sb_QC.Append(clsHTMLFileHandler.GetCSSLink(s_CssFileName));
                sb_QC.Append(clsHTMLFileHandler.GetEndHeadStartBody());
                sb_QC.Append(clsHTMLFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
                sb_QC.Append(WriteHtmlBody(HTMLFileType.Index));
                sb_QC.Append("\t\t<A NAME='sum' />\n");
                sb_QC.Append("\t\t<H2 class='pos_left'>Label-free Analysis Summary</H2>\n");
                sb_QC.Append(clsHTMLFileHandler.GetPictureCode(
                    "LBF_Analysis_Summary.png", true, "pos_left", null, null));
                sb_QC.Append("\t\t<DIV ID='SpectralCount' style='position: absolute; left:700px; top:100px;'>\n");
                sb_QC.Append(clsHTMLFileHandler.GetTableHtml(
                    clsSQLiteHandler.GetDataTable("SELECT * FROM T_MAC_MassTagID_Summary",
                    Path.Combine(esp.WorkDirectory, "Results.db3")), null, null, null, null));
                sb_QC.Append("\t\t</DIV>\n");

                sb_QC.Append("\t\t<A NAME='mc'/A>\n");
                sb_QC.Append("\t\t<H2 class='pos_left'>Missed Cleavage Summary</H2>\n");
                sb_QC.Append(clsHTMLFileHandler.GetPictureCode(
                    "MissedCleavage_Summary.png", true, "pos_left", null, null));
                sb_QC.Append("\t\t<DIV ID='MissedCleavage' style='position: absolute; left:700px; top:560px;'>\n");
                sb_QC.Append(clsHTMLFileHandler.GetTableHtml(
                    clsSQLiteHandler.GetDataTable("SELECT * FROM T_MissedCleavageSummary",
                    Path.Combine(esp.WorkDirectory, "Results.db3")), null, null, null, null));
                sb_QC.Append("\t\t</DIV>\n");

                if (b_ContainsTrypticPeptideSummary)
                {
                    sb_QC.Append("\t\t<A NAME='tp'/A>\n");
                    sb_QC.Append("\t\t<H2 class='pos_left'>Tryptic Peptide Summary</H2>\n");
                    sb_QC.Append(clsHTMLFileHandler.GetPictureCode(
                        "Tryptic_Summary.png", true, "pos_left", null, null));
                    sb_QC.Append("\t\t<DIV ID='TrypticCoverage' style='position: absolute; left:700px; top:1040px;'>\n");
                    sb_QC.Append(clsHTMLFileHandler.GetTableHtml(
                        clsSQLiteHandler.GetDataTable("SELECT * FROM T_MAC_Trypticity_Summary",
                        Path.Combine(esp.WorkDirectory, "Results.db3")), null, null, null, null));
                    sb_QC.Append("\t\t</DIV>\n");
                }

                sb_QC.Append(clsHTMLFileHandler.GetEndBodyEndHtml());                

                StreamWriter sw_QC = new StreamWriter(Path.Combine(esp.WorkDirectory,
                    s_QCFileName));
                sw_QC.WriteLine(sb_QC);
                sw_QC.Close();

                l_NavBarNodes.Remove(node_LBFSummary);
                l_NavBarNodes.Remove(node_MissedCleavages);
                if (b_ContainsTrypticPeptideSummary)
                {
                    l_NavBarNodes.Remove(node_TrypticPeptides);
                }
                #endregion

                ///
                /// Construct and write-out the main html summary page
                /// 
                StringBuilder sb_HTML = new StringBuilder();
                                

                sb_HTML.Append(clsHTMLFileHandler.GetHtmlHeader());
                sb_HTML.Append(clsHTMLFileHandler.GetHtmlJavascriptStart());
                sb_HTML.Append(WriteHtmlScripts());
                sb_HTML.Append(clsHTMLFileHandler.GetHtmlScriptEnd());
                sb_HTML.Append(clsHTMLFileHandler.GetCSSLink(s_CssFileName));
                sb_HTML.Append(clsHTMLFileHandler.GetEndHeadStartBody());
                sb_HTML.Append(clsHTMLFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
                sb_HTML.Append(WriteHtmlBody(HTMLFileType.Index));
                

                sb_HTML.Append(clsHTMLFileHandler.GetEndBodyEndHtml());


                // TODO : Write the html out to the file
                StreamWriter sw = new StreamWriter(Path.Combine(esp.WorkDirectory, esp.FileName));
                sw.Write(sb_HTML);
                sw.Close();
                sb_HTML.Clear();


                StringBuilder sb_BoxPlots = new StringBuilder();

                bool b_LRBoxplot = File.Exists(Path.Combine(esp.WorkDirectory, "Plots", "Boxplot_LR_Log_T_Data.png")),
                    b_CTBoxplot = File.Exists(Path.Combine(esp.WorkDirectory, "Plots", "Boxplot_CT_Log_T_Data.png"));

                clsHtmlLinkNode m_PepLog2BP = new clsHtmlLinkNode("Pep Log2 Boxplot", "log2bp", true);
                clsHtmlLinkNode m_PepLRBP = new clsHtmlLinkNode("Pep LR Boxplot", "lrlog2bp", true);
                clsHtmlLinkNode m_PepCTBP = new clsHtmlLinkNode("Pep CT Boxplot", "ctlog2bp", true);
                clsHtmlLinkNode m_ProtLog2BP = new clsHtmlLinkNode("Prot Log2 Boxplot", "protbp", true);
                clsHtmlLinkNode m_ProtLRBP = new clsHtmlLinkNode("Prot LR Boxplot", "protlrbp", true);
                clsHtmlLinkNode m_ProtCTBP = new clsHtmlLinkNode("Prot CT Boxplot", "protctbp", true);

                l_NavBarNodes.Add(m_PepLog2BP);
                if (b_LRBoxplot)
                    l_NavBarNodes.Add(m_PepLRBP);
                if (b_CTBoxplot)
                    l_NavBarNodes.Add(m_PepCTBP);
                l_NavBarNodes.Add(m_ProtLog2BP);
                if (b_LRBoxplot)
                    l_NavBarNodes.Add(m_ProtLRBP);
                if (b_CTBoxplot)
                    l_NavBarNodes.Add(m_ProtCTBP);

                sb_BoxPlots.Append(clsHTMLFileHandler.GetHtmlHeader());
                sb_BoxPlots.Append(clsHTMLFileHandler.GetHtmlJavascriptStart());
                sb_BoxPlots.Append(WriteHtmlScripts());
                sb_BoxPlots.Append(clsHTMLFileHandler.GetHtmlScriptEnd());
                sb_BoxPlots.Append(clsHTMLFileHandler.GetCSSLink(s_CssFileName));
                sb_BoxPlots.Append(clsHTMLFileHandler.GetEndHeadStartBody());
                sb_BoxPlots.Append(clsHTMLFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
                sb_BoxPlots.Append(WriteHtmlBody(HTMLFileType.Index));

                sb_BoxPlots.Append("\t\t<A NAME='log2bp'/A>\n");
                sb_BoxPlots.Append("\t\t<H2 class='pos_left'>Peptide Log2 Box Plot</H2>\n");
                sb_BoxPlots.Append(clsHTMLFileHandler.GetPictureCode(
                    "Boxplot_Log_T_Data.png", true, "pos_left", null, null));
                if (b_LRBoxplot)
                {
                    sb_BoxPlots.Append("\t\t<A NAME='lrlog2bp'/A>\n");
                    sb_BoxPlots.Append("\t\t<H2 class='pos_left'>Peptide Linear Regression Log2 Box Plot</H2>\n");
                    sb_BoxPlots.Append(clsHTMLFileHandler.GetPictureCode(
                        "Boxplot_LR_Log_T_Data.png", true, "pos_left", null, null));
                }
                if (b_CTBoxplot)
                {
                    sb_BoxPlots.Append("\t\t<A NAME='ctlog2bp'/A>\n");
                    sb_BoxPlots.Append("\t\t<H2 class='pos_left'>Peptide Central Tendency Log2 Box Plot</H2>\n");
                    sb_BoxPlots.Append(clsHTMLFileHandler.GetPictureCode(
                        "Boxplot_CT_Log_T_Data.png", true, "pos_left", null, null));
                }

                sb_BoxPlots.Append("\t\t<A NAME='protbp'/A>\n");
                sb_BoxPlots.Append("\t\t<H2 class='pos_left'>Protein Log2 Box Plot</H2>\n");
                sb_BoxPlots.Append(clsHTMLFileHandler.GetPictureCode(
                    "Boxplot_RR_Log_T_Data.png", true, "pos_left", null, null));

                if (b_LRBoxplot)
                {
                    sb_BoxPlots.Append("\t\t<A NAME='protlrbp'/A>\n");
                    sb_BoxPlots.Append("\t\t<H2 class='pos_left'>Protein Linear Regression Log2 Box Plot</H2>\n");
                    sb_BoxPlots.Append(clsHTMLFileHandler.GetPictureCode(
                        "Boxplot_RR_LR_Log_T_Data.png", true, "pos_left", null, null));
                }
                if (b_CTBoxplot)
                {
                    sb_BoxPlots.Append("\t\t<A NAME='protctbp'/A>\n");
                    sb_BoxPlots.Append("\t\t<H2 class='pos_left'>Protein Central Tendency Log2 Box Plot</H2>\n");
                    sb_BoxPlots.Append(clsHTMLFileHandler.GetPictureCode(
                        "Boxplot_RR_CT_Log_T_Data.png", true, "pos_left", null, null));
                }

                sb_BoxPlots.Append(clsHTMLFileHandler.GetEndBodyEndHtml());

                StreamWriter sw_BoxPlots = new StreamWriter(Path.Combine(esp.WorkDirectory,
                    s_BoxPlotFileName));
                sw_BoxPlots.WriteLine(sb_BoxPlots);
                sw_BoxPlots.Close();
                sb_BoxPlots.Clear();

                l_NavBarNodes.Remove(m_PepLog2BP);
                l_NavBarNodes.Remove(m_PepLRBP);
                l_NavBarNodes.Remove(m_PepCTBP);
                l_NavBarNodes.Remove(m_ProtLog2BP);
                l_NavBarNodes.Remove(m_ProtLRBP);
                l_NavBarNodes.Remove(m_ProtCTBP);


                StringBuilder sb_CorrHeatmaps = new StringBuilder();

                b_LRBoxplot = File.Exists(Path.Combine(esp.WorkDirectory, "Plots", "LR_Log_T_Data_CorrelationHeatmap.png"));
                b_CTBoxplot = File.Exists(Path.Combine(esp.WorkDirectory, "Plots", "CT_Log_T_Data_CorrelationHeatmap.png"));

                clsHtmlLinkNode m_PepLog2Corr = new clsHtmlLinkNode("Pep Log2 Corr", "log2ch", true);
                clsHtmlLinkNode m_PepLRCorr = new clsHtmlLinkNode("Pep LR Corr", "lrlog2ch", true);
                clsHtmlLinkNode m_PepCTCorr = new clsHtmlLinkNode("Pep CT Corr", "ctlog2ch", true);
                clsHtmlLinkNode m_ProtLog2Corr = new clsHtmlLinkNode("Prot Log2 Corr", "protch", true);
                clsHtmlLinkNode m_ProtLRCorr = new clsHtmlLinkNode("Prot LR Corr", "protlrch", true);
                clsHtmlLinkNode m_ProtCTCorr = new clsHtmlLinkNode("Prot CT Corr", "protctch", true);
                l_NavBarNodes.Add(m_PepLog2Corr);

                if (b_LRBoxplot)                    
                    l_NavBarNodes.Add(m_PepLRCorr);
                if (b_CTBoxplot)
                    l_NavBarNodes.Add(m_PepCTCorr);

                l_NavBarNodes.Add(m_ProtLog2Corr);
                if (b_LRBoxplot)
                    l_NavBarNodes.Add(m_ProtLRCorr);
                if (b_CTBoxplot)
                    l_NavBarNodes.Add(m_ProtCTCorr);

                sb_CorrHeatmaps.Append(clsHTMLFileHandler.GetHtmlHeader());
                sb_CorrHeatmaps.Append(clsHTMLFileHandler.GetHtmlJavascriptStart());
                sb_CorrHeatmaps.Append(WriteHtmlScripts());
                sb_CorrHeatmaps.Append(clsHTMLFileHandler.GetHtmlScriptEnd());
                sb_CorrHeatmaps.Append(clsHTMLFileHandler.GetCSSLink(s_CssFileName));
                sb_CorrHeatmaps.Append(clsHTMLFileHandler.GetEndHeadStartBody());
                sb_CorrHeatmaps.Append(clsHTMLFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
                sb_CorrHeatmaps.Append(WriteHtmlBody(HTMLFileType.Index));

                sb_CorrHeatmaps.Append("\t\t<A NAME='log2ch'/A>\n");
                sb_CorrHeatmaps.Append("\t\t<H2 class='pos_left'>Peptide Log2 Correlation Heatmap</H2>\n");
                sb_CorrHeatmaps.Append(clsHTMLFileHandler.GetPictureCode(
                    "Log_T_Data_CorrelationHeatmap.png", true, "pos_left", null, null));
                if (b_LRBoxplot)
                {
                    sb_CorrHeatmaps.Append("\t\t<A NAME='lrlog2ch'/A>\n");
                    sb_CorrHeatmaps.Append("\t\t<H2 class='pos_left'>Peptide Linear Regression Log2 Correlation Heatmap</H2>\n");
                    sb_CorrHeatmaps.Append(clsHTMLFileHandler.GetPictureCode(
                        "LR_Log_T_Data_CorrelationHeatmap.png", true, "pos_left", null, null));
                }
                if (b_CTBoxplot)
                {
                    sb_CorrHeatmaps.Append("\t\t<A NAME='ctlog2ch'/A>\n");
                    sb_CorrHeatmaps.Append("\t\t<H2 class='pos_left'>Peptide Central Tendency Log2 Correlation Heatmap</H2>\n");
                    sb_CorrHeatmaps.Append(clsHTMLFileHandler.GetPictureCode(
                        "CT_Log_T_Data_CorrelationHeatmap.png", true, "pos_left", null, null));
                }

                sb_CorrHeatmaps.Append("\t\t<A NAME='protch'/A>\n");
                sb_CorrHeatmaps.Append("\t\t<H2 class='pos_left'>Protein Log2 Correlation Heatmap</H2>\n");
                sb_CorrHeatmaps.Append(clsHTMLFileHandler.GetPictureCode(
                    "RR_Log_T_Data_CorrelationHeatmap.png", true, "pos_left", null, null));

                if (b_LRBoxplot)
                {
                    sb_CorrHeatmaps.Append("\t\t<A NAME='protlrch'/A>\n");
                    sb_CorrHeatmaps.Append("\t\t<H2 class='pos_left'>Protein Linear Regression Log2 Correlation Heatmap</H2>\n");
                    sb_CorrHeatmaps.Append(clsHTMLFileHandler.GetPictureCode(
                        "RR_LR_Log_T_Data_CorrelationHeatmap.png", true, "pos_left", null, null));
                }
                if (b_CTBoxplot)
                {
                    sb_CorrHeatmaps.Append("\t\t<A NAME='protctch'/A>\n");
                    sb_CorrHeatmaps.Append("\t\t<H2 class='pos_left'>Protein Central Tendency Log2 Correlation Heatmap</H2>\n");
                    sb_CorrHeatmaps.Append(clsHTMLFileHandler.GetPictureCode(
                        "RR_CT_Log_T_Data_CorrelationHeatmap.png", true, "pos_left", null, null));
                }

                sb_BoxPlots.Append(clsHTMLFileHandler.GetEndBodyEndHtml());

                StreamWriter sw_CorrHeatmaps = new StreamWriter(Path.Combine(esp.WorkDirectory,
                    s_CorrelationHeatmaps));
                sw_CorrHeatmaps.WriteLine(sb_CorrHeatmaps);
                sw_CorrHeatmaps.Close();
                sb_CorrHeatmaps.Clear();

                l_NavBarNodes.Remove(m_PepLog2Corr);
                l_NavBarNodes.Remove(m_PepLRCorr);
                l_NavBarNodes.Remove(m_PepCTCorr);
                l_NavBarNodes.Remove(m_ProtLog2Corr);
                l_NavBarNodes.Remove(m_ProtLRCorr);
                l_NavBarNodes.Remove(m_ProtCTCorr);
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
                            "center", 0, 2, 4);
                    break;
                case HTMLFileType.Index:

                    break;
            }

            return s_Body;
        }




        //private string WriteHtmlHeader()
        //{
        //    string s_HTML = "<HTML>" + s_LineDelimiter + s_Tab + "<HEAD>" + s_LineDelimiter;
        //    s_HTML += s_Tab + s_Tab + "<TITLE>Label-free Quantitative " +
        //        "Proteome Analysis Summary</TITLE>" + s_LineDelimiter;
        //    s_HTML += s_Tab + s_Tab + "<LINK rel='stylesheet' href='lbfstyle.css'>" +
        //        s_LineDelimiter;
        //    return s_HTML;
        //}

        //private void WriteCSSFile()
        //{
        //    StreamWriter sw_CSS = new StreamWriter(Path.Combine(esp.WorkDirectory, "lbfstyle.css"));

        //    StringBuilder sb_CSS = new StringBuilder();
        //    sb_CSS.Append("ul#list-nav {" + s_LineDelimiter +
        //        s_Tab + "list-style:none;" + s_LineDelimiter +
        //        s_Tab + "margin:20px;" + s_LineDelimiter +
        //        s_Tab + "padding:0;" + s_LineDelimiter +
        //        s_Tab + "width:600px;" + s_LineDelimiter +
        //        s_Tab + "position:fixed;" + s_LineDelimiter +
        //        s_Tab + "left:50%;" + s_LineDelimiter +
        //        s_Tab + "margin-left:-300px;" + s_LineDelimiter +
        //        "}" + s_LineDelimiter + s_LineDelimiter);

        //    sb_CSS.Append("ul#list-nav li {" + s_LineDelimiter +
        //        s_Tab + "display:inline;" + s_LineDelimiter +
        //        s_Tab + "font-weight:bold;" + s_LineDelimiter +
        //        "}" + s_LineDelimiter + s_LineDelimiter);

        //    sb_CSS.Append("ul#list-nav li a {" + s_LineDelimiter +
        //        s_Tab + "text-decoration:none;" + s_LineDelimiter +
        //        s_Tab + "padding:5px 0;" + s_LineDelimiter +
        //        s_Tab + "width:180px;" + s_LineDelimiter +
        //        s_Tab + "background:#485e49;" + s_LineDelimiter +
        //        s_Tab + "color:#eee;" + s_LineDelimiter +
        //        s_Tab + "float:left;" + s_LineDelimiter +
        //        s_Tab + "text-align:center;" + s_LineDelimiter +
        //        s_Tab + "border-left:1px solid #fff;" + s_LineDelimiter +
        //        "}" + s_LineDelimiter + s_LineDelimiter);

        //    sb_CSS.Append("ul#list-nav li a:hover {" + s_LineDelimiter +
        //        s_Tab + "background:#a2b3a1;" + s_LineDelimiter +
        //        s_Tab + "color:#000" + s_LineDelimiter +
        //        "}" + s_LineDelimiter);

        //    sw_CSS.WriteLine(sb_CSS);
        //    sw_CSS.Close();
        //}

        //private StringBuilder WriteHtmlScripts()
        //{
        //    StringBuilder sb_ReturnScripts = new StringBuilder();
        //    sb_ReturnScripts.Append(s_Tab + "<SCRIPT type='application/javascript'>" + s_LineDelimiter);

        //    sb_ReturnScripts.Append("</SCRIPT>" + s_LineDelimiter);
        //    return sb_ReturnScripts;
        //}

        //private string WriteEndHead()
        //{
        //    string s_Head = s_Tab + "</HEAD>" + s_LineDelimiter;
        //    return s_Head;
        //}

        //private string WriteHtmlBody()
        //{
        //    string s_Body = s_Tab + "<BODY>" + s_LineDelimiter;
        //    s_Body += s_Tab + "<A NAME='datasets'></A>";

        //    // Write out navigation bar
        //    s_Body += s_Tab + "<CENTER>" + s_LineDelimiter;
        //    s_Body += s_Tab + s_Tab + "<NAV>" + s_LineDelimiter;
        //    s_Body += s_Tab + s_Tab + s_Tab + "<UL ID='list-nav'>" + s_LineDelimiter;
        //    s_Body += s_Tab + s_Tab + s_Tab + s_Tab +
        //        "<LI><A HREF='#datasets'>Datasets</A></LI>" + s_LineDelimiter;
        //    s_Body += s_Tab + s_Tab + s_Tab + s_Tab + 
        //        "<LI><A HREF='#PepSummary'>Filtering Summary</A></LI>" + s_LineDelimiter;
        //    s_Body += s_Tab + s_Tab + s_Tab + s_Tab +
        //        "<LI><A HREF='#TranBox'>Transformation Boxplots</A></LI>" + s_LineDelimiter;            
        //    s_Body += s_Tab + s_Tab + s_Tab + "</UL>" + s_LineDelimiter;
        //    s_Body += s_Tab + s_Tab + "</NAV>" + s_LineDelimiter;
        //    s_Body += s_Tab + "</CENTER><BR><BR>" + s_LineDelimiter + s_LineDelimiter;


        //    // Write LBF DATASET INFORMATION
        //    s_Body += s_Tab + s_Tab + "<H2>Datasets used in Label-Free Quantitative Analysis:</H2>" + s_LineDelimiter;
        //    s_Body += s_Tab + s_Tab + "<TABLE><TR><TD>" + s_LineDelimiter;

        //    DataTable dt_Datasets = GetDatasetInfo();
        //    s_Body += WriteTable(dt_Datasets, "left", 0, 2, 4);

        //    s_Body += s_Tab + s_Tab + "</TD>";

        //    DataTable dt_Legend = GetLegendInfo();
        //    if (dt_Legend != null)
        //    {
        //        s_Body += "<TD valign='bottom'>" + s_LineDelimiter + s_Tab + s_Tab +
        //            "<H2><A NAME='PepSummary'>Bar Plot Legend</A></H2>" + s_LineDelimiter + 
        //            WriteTable(dt_Legend, "right", 0, 2, 4);
        //    }

        //    s_Body += s_Tab + s_Tab + "</TR>" + s_LineDelimiter;

        //    s_Body += s_Tab + s_Tab + "<TR><TD>" + s_LineDelimiter;
        //    s_Body += string.Format(s_Tab + s_Tab + 
        //        "<A HREF='Plots/{0}'>" + 
        //        "<IMG src='Plots/{0}' width='400' height='400' /></A>",
        //        "qc_Summary.png");
        //    s_Body += s_Tab + s_Tab + "</TD><TD>" + s_LineDelimiter;
        //    s_Body += string.Format(s_Tab + s_Tab + 
        //        "<A HREF='Plots/{0}'>" +
        //        "<IMG src='Plots/{0}' width='400' height='400' /></A>",
        //        "log_qc_Summary.png");
        //    s_Body += s_Tab + s_Tab + "</TD></TR>" + s_LineDelimiter;


        //    if (clsGenericRCalls.ContainsObject(s_RInstance, "T_MAC_LBF_Summary"))
        //    {
        //        DataTable dt_Summary = clsGenericRCalls.GetDataTable(s_RInstance, "T_MAC_LBF_Summary");
        //        s_Body += s_Tab + s_Tab + "<TR><TD><H2>Summary</H2>" + s_LineDelimiter;
        //        s_Body += WriteTable(dt_Summary, "left", 0, 2, 4) + s_Tab + s_Tab + "</TD></TR>";
        //    }

        //    s_Body += s_Tab + s_Tab + "<TR><TD>" + s_LineDelimiter;
        //    s_Body += s_Tab + s_Tab + "<B>Log2 Transformed</B></TD><TD><B>CT Log2 Transformed</B></TD>" +
        //        "</TR>" + s_LineDelimiter;
        //    s_Body += s_Tab + s_Tab + string.Format("<TR><TD>" +
        //        "<A NAME='TranBox' HREF='Plots/{0}'>" +
        //        "<IMG src='Plots/{0}' width='400' height='400' " +
        //        "/></A></TD><TD>" +
        //        "<A HREF='Plots/{1}'>" +
        //        "<IMG src='Plots/{1}' width='400' height='400' /></A>",
        //        "Boxplot_Log_T_Data.png",
        //        "Boxplot_CT_Log_T_Data.png");
        //    s_Body += "</TD></TR>" + s_LineDelimiter;


        //    string s_LR_BoxplotFile = "Plots/Boxplot_LR_Log_T_Data.png";
        //    if (File.Exists(s_LR_BoxplotFile))
        //    {
        //        s_Body += s_Tab + s_Tab + string.Format("<TR><TD>" +
        //            "<B>LR Log2 Transformed<B>" +
        //            "<A HREF='{0}'>" +
        //            "<IMG src='{0}' width='400' height='400' /></A>" +
        //            "</TD>",
        //            s_LR_BoxplotFile);
        //        s_Body += "</TR>" + s_LineDelimiter;
        //    }           

        //    s_Body += s_Tab + s_Tab + "</TABLE>" + s_LineDelimiter + s_LineDelimiter;

        //    s_Body += s_Tab + "</BODY>" + s_LineDelimiter;
        //    return s_Body;
        //}

        //private string WriteEndHtml()
        //{
        //    string s_End = "</HTML>";
        //    return s_End;
        //}

        /// <summary>
        /// Generates the html code to display a table on the webpage
        /// </summary>
        /// <param name="Table">Table to display</param>
        /// <param name="alignment">How to align the table on the page, eg. left, center, right</param>
        /// <param name="border">Size of border</param>
        /// <param name="TabSpaces">Number of tab spaces before the table tag</param>
        /// <returns></returns>
        //private string WriteTable(DataTable Table, string alignment, int border, int TabSpaces, int CellPadding)
        //{
        //    string s_TableTabs = "", s_RowTabs = "";

        //    for (int i = 0; i < TabSpaces; i++)
        //    {
        //        s_TableTabs += s_Tab;
        //        s_RowTabs += s_Tab;
        //    }
        //    s_RowTabs += s_Tab;

        //    string s_Table = string.Format(s_TableTabs + "<TABLE align='{0}' border='{1}' " +
        //        "CELLPADDING={2} RULES=GROUPS FRAME=BOX>" +
        //        s_LineDelimiter,
        //        alignment,
        //        border,
        //        CellPadding);

        //    s_Table += s_RowTabs + "<THEAD>" + s_LineDelimiter +
        //        s_RowTabs + "<TR>";

        //    // Write the table headers
        //    foreach (DataColumn c in Table.Columns)
        //    {
        //        s_Table += "<TH><B><P align='center'>" + c.ColumnName + "</P></B></TH>";
        //    }
        //    s_Table += "</TR>" + s_LineDelimiter + s_RowTabs + "</THEAD>" + s_LineDelimiter;

        //    int i_Row = 0;
        //    foreach (DataRow r in Table.Rows)
        //    {
        //        i_Row++;
        //        if (i_Row % 2 == 0)
        //            s_Table += s_RowTabs + "<TR bgcolor='lightgrey'>";
        //        else
        //            s_Table += s_RowTabs + "<TR>";
        //        for (int c = 0; c < Table.Columns.Count; c++)
        //        {
        //            s_Table += "<TD><P align='center'>" + r[c].ToString() + "</P></TD>";
        //        }
        //        s_Table += "</TR>" + s_LineDelimiter;
        //    }

        //    s_Table += s_TableTabs + "</TABLE>" + s_LineDelimiter + s_LineDelimiter;
        //    return s_Table;
        //}

        /// <summary>
        /// Grabs the dataset information table from the SQLite database, and returns it in a DataTable
        /// </summary>
        /// <returns>LBF Dataset Summary Information</returns>
        //private DataTable GetDatasetInfo()
        //{
        //    string s_Command = "Select Alias, Dataset, Dataset_ID FROM t_factors ORDER BY Alias";

        //    DataTable dt_Return = clsSQLiteHandler.GetDataTable(
        //        s_Command,
        //        Path.Combine(esp.WorkDirectory, esp.InputFileName));

        //    return dt_Return;
        //}

        /// <summary>
        /// From the 'legend' information supplied in the workflow xml file, constructs a table
        /// that describes symbols used in graphs for the html summary page.
        /// Rows are delineated by '|' and columns by ';'.
        /// </summary>
        /// <returns>Legend information</returns>
        //private DataTable GetLegendInfo()
        //{
        //    DataTable dt_Legend = new DataTable();

        //    if (esp.Legend.Equals("")) 
        //        return null;

        //    string[] s_Rows = esp.Legend.Split('|');

        //    for (int i = 0; i < s_Rows.Length; i++)
        //    {
        //        if (i == 0) // column headers
        //        {
        //            string[] s_Headers = s_Rows[i].Split(';');
        //            foreach (string h in s_Headers)
        //            {
        //                DataColumn dc = new DataColumn(h);
        //                dt_Legend.Columns.Add(dc);
        //            }
        //        }
        //        else
        //        {
        //            string[] s_CurrentRow = s_Rows[i].Split(';');
        //            dt_Legend.Rows.Add(s_CurrentRow);
        //        }
        //    }

        //    return dt_Legend;
        //}

        /// <summary>
        /// Modifies the Summary table by adding a column for percent of original
        /// </summary>
        /// <param name="dt_Summary">Summary DataTable</param>
        /// <param name="LastRowForPercent">Last Row to calculate percent, all after this one are 'NA'</param>
        ///// <returns></returns>
        //private DataTable ModifySummaryTable(DataTable dt_Summary, int LastRowForPercent)
        //{
        //    DataTable dt_Return = new DataTable("ModifiedSummaryTable");

        //    foreach (DataColumn dc in dt_Summary.Columns)
        //    {
        //        dt_Return.Columns.Add(new DataColumn(dc.ColumnName));
        //    }
        //    DataColumn dc_Percent = new DataColumn("PercentOfOriginal");
        //    dt_Return.Columns.Add(dc_Percent);

        //    return dt_Return;
        //}
        #endregion
    }
}
