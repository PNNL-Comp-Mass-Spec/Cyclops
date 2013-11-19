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
    public class LBF_Summary_HTML : BaseDataModule
    {
        #region Enums
        private enum HTMLFileType { Dataset, Index };
        #endregion

        #region Members
        private string m_ModuleName = "LBF_Summary_HTML",
            m_Description = "",
            m_TypticTableSummaryName = "T_MAC_Trypticity_Summary",
            m_WorkingDirectory = "",
            m_DatabaseName = "Results.db3";

        private DataTable m_Overlap = new DataTable("LBF");

        private bool m_LR = false, m_CT = false;
        private Dictionary<string, string>
            m_FileNameVault = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// Required parameters to run LBF_Summary_HTML Module
        /// </summary>
        private enum RequiredParameters
        {
            FileName
        }
        #endregion

        #region Properties
        public Dictionary<string, string> FileNameVault
        {
            get { return m_FileNameVault; }
            set { m_FileNameVault = value; }
        }
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
        public LBF_Summary_HTML(CyclopsModel CyclopsModel,
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

                Model.LogMessage("Running LBF_Summary_HTML",
                        ModuleName, StepNumber);

                if (CheckParameters())
                    b_Successful = LBF_Summary_HTMLFunction();
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
                    Model.LogError("Required Field Missing: " + s,
                        ModuleName, StepNumber);
                    b_Successful = false;
                    return b_Successful;
                }
            }

            if (Parameters.ContainsKey("WorkDir") && b_Successful)
            {
                if (!string.IsNullOrEmpty(Parameters["WorkDir"]))
                    m_WorkingDirectory = Parameters["WorkDir"];
                else
                {
                    Model.LogError("Error in 'LBF_Summary_HTML', no 'WorkDir' supplied!",
                        ModuleName, StepNumber);
                    b_Successful = false;
                }
            }
            else if (b_Successful)
            {
                Model.LogError("Error in 'LBF_Summary_HTML', no 'WorkDir' supplied!",
                    ModuleName, StepNumber);
                b_Successful = false;
            }

            if (Parameters.ContainsKey("DatabaseName"))
            {
                if (!string.IsNullOrEmpty(Parameters["DatabaseName"]))
                    m_DatabaseName = Parameters["DatabaseName"];
            }

            return b_Successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool LBF_Summary_HTMLFunction()
        {
            bool b_Successful = true;

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
                    "constructing and writing Datasets HTML Page:\n" +
                    ex.ToString(),
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
                    "constructing and writing Summary HTML Page:\n" +
                    ex.ToString(),
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
                    "constructing and writing QC HTML Page:\n" +
                    ex.ToString(),
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
                    "constructing and writing Boxplot HTML Page:\n" +
                    ex.ToString(),
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
                    "constructing and writing Correlation Heatmap HTML Page:\n" +
                    ex.ToString(),
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
                    "constructing and writing Main HTML Page:\n" +
                    ex.ToString(),
                    ModuleName, StepNumber);
                return false;
            }
            #endregion

            return b_Successful;
        }

        private List<HtmlLinkNode> GetOriginalNavBar()
        {
            List<HtmlLinkNode> l_NavBarNodes = new List<HtmlLinkNode>();

            l_NavBarNodes.Add(new HtmlLinkNode(
                "Home", Parameters[RequiredParameters.FileName.ToString()], false));
            l_NavBarNodes.Add(new HtmlLinkNode(
                "Datasets", FileNameVault["DatasetsHtmlFileName"], false));
            l_NavBarNodes.Add(new HtmlLinkNode(
                "Summary Tables", FileNameVault["SummaryTableHtmlFileName"], false));
            l_NavBarNodes.Add(new HtmlLinkNode(
                "QC Plots", FileNameVault["QcHtmlFileName"], false));
            l_NavBarNodes.Add(new HtmlLinkNode(
                "Box Plots", FileNameVault["BoxPlotHtmlFileName"], false));
            l_NavBarNodes.Add(new HtmlLinkNode(
                "Correlation Heatmaps", FileNameVault["CorrelationHtmlFileName"], false));

            return l_NavBarNodes;
        }

        /// <summary>
        /// Sets the boolean values that indicate if normalization algrithms have been run.
        /// </summary>
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
            using (StreamWriter sw = File.AppendText(Path.Combine(m_WorkingDirectory, FileNameVault["CssFileName"])))
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
            StringBuilder sb_Scripts = new StringBuilder();

            sb_Scripts.Append(HtmlFileHandler.GetHtmlHeader());
            sb_Scripts.Append(HtmlFileHandler.GetCSSLink(FileNameVault["CssFileName"]));
            sb_Scripts.Append(HtmlFileHandler.GetHtmlJavascriptStart());
            
            sb_Scripts.Append(HtmlFileHandler.GetHtmlScriptEnd());
            sb_Scripts.Append(HtmlFileHandler.GetEndHeadStartBody());
            sb_Scripts.Append(HtmlFileHandler.GetNavTable(NavBar));
            //sb_Scripts.Append(HtmlFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));

            sb_Scripts.Append("<DIV ID='main_content'>\n");
            sb_Scripts.Append(HtmlFileHandler.GetDatasetTableHtml(
                Path.Combine(m_WorkingDirectory, m_DatabaseName), "", "table_header", "left", 1, 1, 1));
            sb_Scripts.Append("</DIV>\n");
            sb_Scripts.Append(HtmlFileHandler.GetEndBodyEndHtml());

            StreamWriter sw = new StreamWriter(Path.Combine(
                m_WorkingDirectory, FileNameVault["DatasetsHtmlFileName"]));
            sw.WriteLine(sb_Scripts);
            sw.Close();
        }

        /// <summary>
        /// Construct and write-out the Summary HTML Page
        /// </summary>
        /// <param name="NavBar">HTML Navigation Bar</param>
        private void WriteSummaryHTMLPage(List<HtmlLinkNode> NavBar)
        {
            NavBar.Add(new HtmlLinkNode("Peptide Original",
                    "pepOrig", true));
            NavBar.Add(new HtmlLinkNode("Peptide Log2",
                    "peplog2", true));
            if (m_CT)
            {
                NavBar.Add(new HtmlLinkNode("Peptide CT",
                        "pepCT", true));
            }
            if (m_LR)
            {
                NavBar.Add(new HtmlLinkNode("Peptide LR",
                        "pepLR", true));
            }
            NavBar.Add(new HtmlLinkNode("RRollup Protein",
                    "protRR", true));
            if (m_CT)
            {
                NavBar.Add(new HtmlLinkNode("RRollup CT Protein",
                        "protRRCT", true));
            }
            if (m_LR)
            {
                NavBar.Add(new HtmlLinkNode("RRollup LR Protein",
                        "protRRLR", true));
            }

            StringBuilder sb_Scripts = new StringBuilder();
            sb_Scripts.Append(HtmlFileHandler.GetHtmlHeader());
            sb_Scripts.Append(HtmlFileHandler.GetCSSLink(FileNameVault["CssFileName"]));
            sb_Scripts.Append(HtmlFileHandler.GetHtmlJavascriptStart());

            sb_Scripts.Append(HtmlFileHandler.GetHtmlScriptEnd());
            sb_Scripts.Append(HtmlFileHandler.GetEndHeadStartBody());
            sb_Scripts.Append(HtmlFileHandler.GetNavTable(NavBar));
            //sb_Scripts.Append(HtmlFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));

            sb_Scripts.Append("<DIV ID='main_content'>\n");
            sb_Scripts.Append("<A NAME='pepOrig' /A>\n");

            sb_Scripts.Append(
                HtmlFileHandler.GetSummaryTableHtml(
                    Model.RCalls.GetDataTableIncludingRowNames(
                        "Summary_T_Data$TotalSummary", "QC_Params"),
                    "Summary of Original Peptide Abundances", "table_header",
                    1, 1, 1));

            sb_Scripts.Append("<A NAME='peplog2' /A>\n");
            sb_Scripts.Append(
                HtmlFileHandler.GetSummaryTableHtml(
                    Model.RCalls.GetDataTableIncludingRowNames(
                        "Summary_Log_T_Data$TotalSummary", "QC_Params"),
                    "Summary of Log2 Peptide Abundances", "table_header",
                    1, 1, 1));

            if (m_CT)
            {
                sb_Scripts.Append("<A NAME='pepCT' /A>\n");
                sb_Scripts.Append(
                    HtmlFileHandler.GetSummaryTableHtml(
                        Model.RCalls.GetDataTableIncludingRowNames(
                            "Summary_CT_Log_T_Data$TotalSummary", "QC_Params"),
                        "Summary of Central Tendency Log2 Peptide Abundances", "table_header",
                        1, 1, 1));
            }

            if (m_LR)
            {
                sb_Scripts.Append("<A NAME='pepLR' /A>\n");
                sb_Scripts.Append(
                    HtmlFileHandler.GetSummaryTableHtml(
                        Model.RCalls.GetDataTableIncludingRowNames(
                            "Summary_LR_Log_T_Data$TotalSummary", "QC_Params"),
                        "Summary of Linear Regression Log2 Peptide Abundances", "table_header",
                        1, 1, 1));
            }

            // Proteins
            sb_Scripts.Append("<A NAME='protRR' /A>\n");
            sb_Scripts.Append(
                HtmlFileHandler.GetSummaryTableHtml(
                    Model.RCalls.GetDataTableIncludingRowNames(
                        "Summary_RR_Log_T_Data$TotalSummary", "QC_Params"),
                    "Summary of Protein Abundances from Log2 Peptides (RRollup)", "table_header",
                    1, 1, 1));

            if (m_CT)
            {
                sb_Scripts.Append("<A NAME='protRRCT' /A>\n");
                sb_Scripts.Append(
                    HtmlFileHandler.GetSummaryTableHtml(
                        Model.RCalls.GetDataTableIncludingRowNames(
                            "Summary_RR_CT_Log_T_Data$TotalSummary", "QC_Params"),
                        "Summary of Protein Abundances from Central Tendency Log2 Peptides Abundances (RRollup)", "table_header",
                        1, 1, 1));
            }

            if (m_LR)
            {
                sb_Scripts.Append("<A NAME='protRRLR' /A>\n");
                sb_Scripts.Append(
                    HtmlFileHandler.GetSummaryTableHtml(
                        Model.RCalls.GetDataTableIncludingRowNames(
                            "Summary_RR_LR_Log_T_Data$TotalSummary", "QC_Params"),
                        "Summary of Protein Abundances from Linear Regression Log2 Peptide Abundances (RRollup)", "table_header",
                        1, 1, 1));
            }

            sb_Scripts.Append("</DIV>\n");

            sb_Scripts.Append(HtmlFileHandler.GetEndBodyEndHtml());

            StreamWriter sw = new StreamWriter(Path.Combine(
                m_WorkingDirectory, FileNameVault["SummaryTableHtmlFileName"]));
            sw.Write(sb_Scripts);
            sw.Close();
        }

        /// <summary>
        /// Construct and write-out the QC HTML Page
        /// </summary>
        /// <param name="NavBar">HTML Navigation Bar</param>
        private void WriteQCHTMLPage(List<HtmlLinkNode> NavBar)
        {
            bool b_ContainsTrypticPeptideSummary =     
                Model.ModuleLoader.SQLiteDatabase.TableExists(m_TypticTableSummaryName);

            NavBar.Add(new HtmlLinkNode("LBF Summary", "sum", true));
            NavBar.Add(new HtmlLinkNode("Missed Cleavages", "mc", true));
            NavBar.Add(new HtmlLinkNode("Tryptic Peptides", "tp", true));

            StringBuilder sb_Scripts = new StringBuilder();
            sb_Scripts.Append(HtmlFileHandler.GetHtmlHeader());
            sb_Scripts.Append(HtmlFileHandler.GetHtmlJavascriptStart());
            
            sb_Scripts.Append(HtmlFileHandler.GetHtmlScriptEnd());
            sb_Scripts.Append(HtmlFileHandler.GetCSSLink(FileNameVault["CssFileName"]));
            sb_Scripts.Append(HtmlFileHandler.GetEndHeadStartBody());
            sb_Scripts.Append(HtmlFileHandler.GetNavTable(NavBar));
            //sb_Scripts.Append(HtmlFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
            sb_Scripts.Append(WriteHtmlBody(HTMLFileType.Index));

            sb_Scripts.Append("<DIV ID='main_content'>\n");
            sb_Scripts.Append("\t\t<A NAME='sum' />\n");
            sb_Scripts.Append(
                HtmlFileHandler.GetQCElement(
                    "Label-free Analysis Summary",
                    "table_header",
                    FileNameVault["LbfAnalysisSummaryFigureFileName"],
                    Model.ModuleLoader.SQLiteDatabase.GetTable(
                        FileNameVault["LbfAnalysisSummaryFigureTableName"]),
                    1, 1, 1));

            sb_Scripts.Append("\t\t<A NAME='mc'/A>\n");
            sb_Scripts.Append(
                HtmlFileHandler.GetQCElement(
                    "Missed Cleavage Summary",
                    "table_header",
                    FileNameVault["LbfMissedCleavageFigureFileName"],
                    Model.ModuleLoader.SQLiteDatabase.GetTable(
                        FileNameVault["LbfMissedCleavageFigureTableName"]),
                    1, 1, 1));

            if (b_ContainsTrypticPeptideSummary)
            {
                sb_Scripts.Append("\t\t<A NAME='tp'/A>\n");
                sb_Scripts.Append(
                    HtmlFileHandler.GetQCElement(
                        "Tryptic Peptide Summary",
                        "table_header",
                        FileNameVault["LbfTrypticSummaryFigureFileName"],
                        Model.ModuleLoader.SQLiteDatabase.GetTable(
                            FileNameVault["LbfTrypticSummaryFigureTableName"]),
                        1, 1, 1));
            }

            sb_Scripts.Append("</DIV>\n");
            sb_Scripts.Append(HtmlFileHandler.GetEndBodyEndHtml());
                        
            StreamWriter sw = new StreamWriter(Path.Combine(
                m_WorkingDirectory, FileNameVault["QcHtmlFileName"]));
            sw.WriteLine(sb_Scripts);
            sw.Close();
        }

        /// <summary>
        /// Construct and write-out the Main HTML Page
        /// </summary>
        /// <param name="NavBar">HTML Navigation Bar</param>
        private void WriteMainHTMLPage(List<HtmlLinkNode> NavBar)
        {
            StringBuilder sb_Scripts = new StringBuilder();
            sb_Scripts.Append(HtmlFileHandler.GetHtmlHeader());
            sb_Scripts.Append(HtmlFileHandler.GetHtmlJavascriptStart());

            sb_Scripts.Append(HtmlFileHandler.GetHtmlScriptEnd());
            sb_Scripts.Append(HtmlFileHandler.GetCSSLink(FileNameVault["CssFileName"]));
            sb_Scripts.Append(HtmlFileHandler.GetEndHeadStartBody());
            sb_Scripts.Append(HtmlFileHandler.GetNavTable(NavBar));
            //sb_Scripts.Append(HtmlFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
            sb_Scripts.Append(WriteHtmlBody(HTMLFileType.Index));


            sb_Scripts.Append("<DIV ID='main_content'>\n");

            // Add to the index page here...

            sb_Scripts.Append("</DIV>\n");

            sb_Scripts.Append(HtmlFileHandler.GetEndBodyEndHtml());


            StreamWriter sw = new StreamWriter(Path.Combine(
                m_WorkingDirectory, FileNameVault["MainFileName"]));
            sw.WriteLine(sb_Scripts);
            sw.Close();
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

            StringBuilder sb_Scripts = new StringBuilder();

            sb_Scripts.Append(HtmlFileHandler.GetHtmlHeader());
            sb_Scripts.Append(HtmlFileHandler.GetHtmlJavascriptStart());
            
            sb_Scripts.Append(HtmlFileHandler.GetHtmlScriptEnd());
            sb_Scripts.Append(HtmlFileHandler.GetCSSLink(FileNameVault["CssFileName"]));
            sb_Scripts.Append(HtmlFileHandler.GetEndHeadStartBody());
            sb_Scripts.Append(HtmlFileHandler.GetNavTable(NavBar));
            //sb_Scripts.Append(HtmlFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
            sb_Scripts.Append(WriteHtmlBody(HTMLFileType.Index));

            sb_Scripts.Append("<DIV ID='main_content'>\n");
            sb_Scripts.Append("\t\t<A NAME='log2bp'/A>\n");
            sb_Scripts.Append("\t\t<DIV>\n");
            sb_Scripts.Append("\t\t<P ID='table_header'>Peptide Log2 Box Plot</P>\n");
            sb_Scripts.Append(HtmlFileHandler.GetPictureCode(
                FileNameVault["LbfBoxplotFigureFileName"],
                true, "pos_left", null, null) + "\n");
            sb_Scripts.Append("\t\t</DIV>\n");
            if (m_LR)
            {
                sb_Scripts.Append("\t\t<A NAME='lrlog2bp'/A>\n");
                sb_Scripts.Append("\t\t<DIV>\n");
                sb_Scripts.Append("\t\t<P ID='table_header'>Peptide Linear Regression Log2 Box Plot</P>\n");
                sb_Scripts.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["LbfBoxplotLRFigureFileName"],
                    true, "pos_left", null, null) + "\n");
                sb_Scripts.Append("\t\t</DIV>\n");
            }
            if (m_CT)
            {
                sb_Scripts.Append("\t\t<A NAME='ctlog2bp'/A>\n");
                sb_Scripts.Append("\t\t<DIV>\n");
                sb_Scripts.Append("\t\t<P ID='table_header'>Peptide Central Tendency Log2 Box Plot</P>\n");
                sb_Scripts.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["LbfBoxplotCTFigureFileName"],
                    true, "pos_left", null, null) + "\n");
                sb_Scripts.Append("\t\t</DIV>\n");
            }

            sb_Scripts.Append("\t\t<A NAME='protbp'/A>\n");
            sb_Scripts.Append("\t\t<DIV>\n");
            sb_Scripts.Append("\t\t<P ID='table_header'>Protein Log2 Box Plot</P>\n");
            sb_Scripts.Append(HtmlFileHandler.GetPictureCode(
                FileNameVault["LbfBoxplotRrFigureFileName"],
                true, "pos_left", null, null) + "\n");
            sb_Scripts.Append("\t\t</DIV>\n");

            if (m_LR)
            {
                sb_Scripts.Append("\t\t<A NAME='protlrbp'/A>\n");
                sb_Scripts.Append("\t\t<DIV>\n");
                sb_Scripts.Append("\t\t<P ID='table_header'>Protein Linear Regression Log2 Box Plot</P>\n");
                sb_Scripts.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["LbfBoxplotRrLRFigureFileName"],
                    true, "pos_left", null, null) + "\n");
                sb_Scripts.Append("\t\t</DIV>\n");
            }
            if (m_CT)
            {
                sb_Scripts.Append("\t\t<A NAME='protctbp'/A>\n");
                sb_Scripts.Append("\t\t<DIV>\n");
                sb_Scripts.Append("\t\t<P ID='table_header'>Protein Central Tendency Log2 Box Plot</P>\n");
                sb_Scripts.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["LbfBoxplotRrCTFigureFileName"],
                    true, "pos_left", null, null) + "\n");
                sb_Scripts.Append("\t\t</DIV>\n");
            }

            sb_Scripts.Append("</DIV>\n");

            sb_Scripts.Append(HtmlFileHandler.GetEndBodyEndHtml());

            StreamWriter sw = new StreamWriter(Path.Combine(
                m_WorkingDirectory, FileNameVault["BoxPlotHtmlFileName"]));
            sw.WriteLine(sb_Scripts);
            sw.Close();
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

            StringBuilder sb_Scripts = new StringBuilder();
            sb_Scripts.Append(HtmlFileHandler.GetHtmlHeader());
            sb_Scripts.Append(HtmlFileHandler.GetHtmlJavascriptStart());
            
            sb_Scripts.Append(HtmlFileHandler.GetHtmlScriptEnd());
            sb_Scripts.Append(HtmlFileHandler.GetCSSLink(FileNameVault["CssFileName"]));
            sb_Scripts.Append(HtmlFileHandler.GetEndHeadStartBody());
            sb_Scripts.Append(HtmlFileHandler.GetNavTable(NavBar));
            //sb_Scripts.Append(HtmlFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
            sb_Scripts.Append(WriteHtmlBody(HTMLFileType.Index));

            sb_Scripts.Append("<DIV ID='main_content'>\n");
            sb_Scripts.Append("\t\t<A NAME='log2ch'/A>\n");
            sb_Scripts.Append("\t\t<DIV>\n");
            sb_Scripts.Append("\t\t<P ID='table_header'>Peptide Log2 Correlation Heatmap</P>\n");
            sb_Scripts.Append(HtmlFileHandler.GetPictureCode(
                FileNameVault["LbfCorrelationHeatmapFigureFileName"],
                true, "pos_left", null, null) + "\n");
            sb_Scripts.Append("\t\t</DIV>\n");
            if (m_LR)
            {
                sb_Scripts.Append("\t\t<A NAME='lrlog2ch'/A>\n");
                sb_Scripts.Append("\t\t<DIV>\n");
                sb_Scripts.Append("\t\t<P ID='table_header'>Peptide Linear Regression Log2 Correlation Heatmap</P>\n");
                sb_Scripts.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["LbfCorrelationHeatmapLRFigureFileName"],
                    true, "pos_left", null, null) + "\n");
                sb_Scripts.Append("\t\t</DIV>\n");
            }
            if (m_CT)
            {
                sb_Scripts.Append("\t\t<A NAME='ctlog2ch'/A>\n");
                sb_Scripts.Append("\t\t<DIV>\n");
                sb_Scripts.Append("\t\t<P ID='table_header'>Peptide Central Tendency Log2 Correlation Heatmap</P>\n");
                sb_Scripts.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["LbfCorrelationHeatmapCTFigureFileName"],
                    true, "pos_left", null, null) + "\n");
                sb_Scripts.Append("\t\t</DIV>\n");
            }

            sb_Scripts.Append("\t\t<A NAME='protch'/A>\n");
            sb_Scripts.Append("\t\t<DIV>\n");
            sb_Scripts.Append("\t\t<P ID='table_header'>Protein Log2 Correlation Heatmap</P>\n");
            sb_Scripts.Append(HtmlFileHandler.GetPictureCode(
                FileNameVault["LbfCorrelationHeatmapRrFigureFileName"],
                true, "pos_left", null, null) + "\n");
            sb_Scripts.Append("\t\t</DIV>\n");

            if (m_LR)
            {
                sb_Scripts.Append("\t\t<A NAME='protlrch'/A>\n");
                sb_Scripts.Append("\t\t<DIV>\n");
                sb_Scripts.Append("\t\t<P ID='table_header'>Protein Linear Regression Log2 Correlation Heatmap</P>\n");
                sb_Scripts.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["LbfCorrelationHeatmapRrLRFigureFileName"],
                    true, "pos_left", null, null) + "\n");
                sb_Scripts.Append("\t\t</DIV>\n");
            }
            if (m_CT)
            {
                sb_Scripts.Append("\t\t<A NAME='protctch'/A>\n");
                sb_Scripts.Append("\t\t<DIV>\n");
                sb_Scripts.Append("\t\t<P ID='table_header'>Protein Central Tendency Log2 Correlation Heatmap</P>\n");
                sb_Scripts.Append(HtmlFileHandler.GetPictureCode(
                    FileNameVault["LbfCorrelationHeatmapRrCTFigureFileName"],
                    true, "pos_left", null, null) + "\n");
                sb_Scripts.Append("\t\t</DIV>\n");
            }

            sb_Scripts.Append("</DIV>\n");

            sb_Scripts.Append(HtmlFileHandler.GetEndBodyEndHtml());
            
            StreamWriter sw = new StreamWriter(Path.Combine(
                m_WorkingDirectory, FileNameVault["CorrelationHtmlFileName"]));
            sw.WriteLine(sb_Scripts);
            sw.Close();
        }

        private string WriteHtmlBody(HTMLFileType TheHTMLFileType)
        {
            string s_Body = "";
            switch (TheHTMLFileType)
            {
                case HTMLFileType.Dataset:
                    s_Body = HtmlFileHandler.GetDatasetTableHtml(
                        Path.Combine(m_WorkingDirectory, m_DatabaseName), null,
                            "table_header", "center", 0, 2, 4);
                    break;
                case HTMLFileType.Index:

                    break;
            }

            return s_Body;
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
