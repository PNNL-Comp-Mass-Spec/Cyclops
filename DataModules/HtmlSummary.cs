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
    public class HtmlSummary : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "HtmlSummary",
            m_LineDelimiter = "\n",
            m_Tab = "\t",
            m_DatabaseFileName = "Results.db3";
        /// <summary>
        /// Required parameters to run Aggregate
        /// </summary>
        private enum RequiredParameters
        { FileName
        }
        private bool m_DatabaseFound = false;
        private enum HTMLFileType { Dataset, Index };
        private PNNLOmics.Databases.SQLiteHandler sql = new PNNLOmics.Databases.SQLiteHandler();
        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an HtmlSummary Module
        /// </summary>
        public HtmlSummary()
        {
            ModuleName = m_ModuleName;
        }

        /// <summary>
        /// HtmlSummary module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public HtmlSummary(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
        }

        /// <summary>
        /// HtmlSummary module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public HtmlSummary(CyclopsModel CyclopsModel,
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

                if (CheckParameters())
                    Model.PipelineCurrentlySuccessful = HtmlSummaryFunction();

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
        public bool HtmlSummaryFunction()
        {
            bool b_Successful = true;

            string s_CssFileName = "styles.css", s_DatasetsFileName = "Datasets.html",
                    s_QCFileName = "QC.html";
            List<HtmlLinkNode> l_NavBarNodes = new List<HtmlLinkNode>();

            l_NavBarNodes.Add(new HtmlLinkNode(
                "Home", 
                Parameters[RequiredParameters.FileName.ToString()],
                false));
            l_NavBarNodes.Add(new HtmlLinkNode(
                "Datasets", s_DatasetsFileName, false));
            l_NavBarNodes.Add(new HtmlLinkNode(
                "QC Plots", s_QCFileName, false));

            using (StreamWriter sw_Css = File.AppendText(Path.Combine(
                Model.WorkDirectory, s_CssFileName)))
            {
                sw_Css.WriteLine(HtmlFileHandler.GetCSS(HtmlFileHandler.CssStyle.NavBar, 160));
                sw_Css.WriteLine(HtmlFileHandler.GetCSS(HtmlFileHandler.CssStyle.LeftIndent, 160));
                sw_Css.WriteLine(HtmlFileHandler.GetCSS(HtmlFileHandler.CssStyle.Th, 160));
            }

            // Construct and write-out the Datasets Page
            StringBuilder sb_Datasets = new StringBuilder();

            sb_Datasets.Append(HtmlFileHandler.GetHtmlHeader());
            sb_Datasets.Append(HtmlFileHandler.GetCSSLink(s_CssFileName));
            sb_Datasets.Append(HtmlFileHandler.GetHtmlJavascriptStart());
            sb_Datasets.Append(WriteHtmlScripts());
            sb_Datasets.Append(HtmlFileHandler.GetHtmlScriptEnd());
            sb_Datasets.Append(HtmlFileHandler.GetEndHeadStartBody());
            sb_Datasets.Append(HtmlFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
            sb_Datasets.Append("\t\t<DIV ID='DatasetTable' style='position: absolute; left:200px; top:100px;'>\n");
            sb_Datasets.Append(WriteHtmlBody(HTMLFileType.Dataset));
            sb_Datasets.Append("\t\t</DIV>\n");
            sb_Datasets.Append(HtmlFileHandler.GetEndBodyEndHtml());

            StreamWriter sw_Datasets = new StreamWriter(Path.Combine(
                Model.WorkDirectory, 
                s_DatasetsFileName));
            sw_Datasets.Write(sb_Datasets);
            sw_Datasets.Close();
            sb_Datasets.Clear();

            // Construct and write-out the QC Page
            StringBuilder sb_QC = new StringBuilder();

            HtmlLinkNode node_MissedCleavages = new HtmlLinkNode("Missed Cleavages", "mc", true);
            l_NavBarNodes.Add(node_MissedCleavages);
            HtmlLinkNode node_TrypticPeptides = new HtmlLinkNode("Tryptic Peptides", "tp", true);
            l_NavBarNodes.Add(node_TrypticPeptides);

            sb_QC.Append(HtmlFileHandler.GetHtmlHeader());
            sb_QC.Append(HtmlFileHandler.GetHtmlJavascriptStart());
            sb_QC.Append(WriteHtmlScripts());
            sb_QC.Append(HtmlFileHandler.GetHtmlScriptEnd());
            sb_QC.Append(HtmlFileHandler.GetCSSLink(s_CssFileName));
            sb_QC.Append(HtmlFileHandler.GetEndHeadStartBody());
            sb_QC.Append(HtmlFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
            sb_QC.Append(WriteHtmlBody(HTMLFileType.Index));
            sb_QC.Append("\t\t<H2 class='pos_left'>Spectral Count Summary</H2>\n");
            sb_QC.Append(HtmlFileHandler.GetPictureCode(
                "Spectral_Count_Summary.png", true, "pos_left", null, null));
            sb_QC.Append("\t\t<DIV ID='SpectralCount' style='position: absolute; left:700px; top:100px;'>\n");
            sb_QC.Append(HtmlFileHandler.GetTableHtml(
                sql.GetTable("T_MAC_SpecCnt_Summary"), null, null, null, null));
            sb_QC.Append("\t\t</DIV>\n");

            sb_QC.Append("\t\t<A NAME='mc'/A>\n");
            sb_QC.Append("\t\t<H2 class='pos_left'>Missed Cleavage Summary</H2>\n");
            sb_QC.Append(HtmlFileHandler.GetPictureCode(
                "MissedCleavage_Summary.png", true, "pos_left", null, null));
            sb_QC.Append("\t\t<DIV ID='MissedCleavage' style='position: absolute; left:700px; top:560px;'>\n");
            sb_QC.Append(HtmlFileHandler.GetTableHtml(
                sql.GetTable("T_MissedCleavageSummary"), null, null, null, null));
            sb_QC.Append("\t\t</DIV>\n");

            sb_QC.Append("\t\t<A NAME='tp'/A>\n");
            sb_QC.Append("\t\t<H2 class='pos_left'>Tryptic Peptide Summary</H2>\n");
            sb_QC.Append(HtmlFileHandler.GetPictureCode(
                "Tryptic_Summary.png", true, "pos_left", null, null));
            sb_QC.Append("\t\t<DIV ID='TrypticCoverage' style='position: absolute; left:700px; top:1040px;'>\n");
            sb_QC.Append(HtmlFileHandler.GetTableHtml(
                sql.GetTable("T_MAC_Trypticity_Summary"), null, null, null, null));
            sb_QC.Append("\t\t</DIV>\n");

            StreamWriter sw_QC = new StreamWriter(Path.Combine(Model.WorkDirectory,
                s_QCFileName));
            sw_QC.WriteLine(sb_QC);
            sw_QC.Close();

            l_NavBarNodes.Remove(node_MissedCleavages);
            l_NavBarNodes.Remove(node_TrypticPeptides);

            // Construct and write-out the main html summary page
            StringBuilder sb_HTML = new StringBuilder();

            l_NavBarNodes.Add(new HtmlLinkNode(
                "Correlation", "ch", true));

            sb_HTML.Append(HtmlFileHandler.GetHtmlHeader());
            sb_HTML.Append(HtmlFileHandler.GetHtmlJavascriptStart());
            sb_HTML.Append(WriteHtmlScripts());
            sb_HTML.Append(HtmlFileHandler.GetHtmlScriptEnd());
            sb_HTML.Append(HtmlFileHandler.GetCSSLink(s_CssFileName));
            sb_HTML.Append(HtmlFileHandler.GetEndHeadStartBody());
            sb_HTML.Append(HtmlFileHandler.GetNavBar(l_NavBarNodes, "LEFT"));
            sb_HTML.Append(WriteHtmlBody(HTMLFileType.Index));
            sb_HTML.Append("\t\t<A NAME='ch'/A>\n");
            sb_HTML.Append("\t\t<H2 class='pos_left'>Correlation Heatmap</H2>\n");
            sb_HTML.Append(HtmlFileHandler.GetPictureCode(
                "T_SpectralCounts_CorrelationHeatmap.png", true, "pos_left", null, null));

            sb_HTML.Append(HtmlFileHandler.GetEndBodyEndHtml());


            // TODO : Write the html out to the file
            StreamWriter sw = new StreamWriter(Path.Combine(
                Model.WorkDirectory, 
                Parameters[RequiredParameters.FileName.ToString()]));
            sw.Write(sb_HTML);
            sw.Close();

            return b_Successful;
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
                    s_Body = HtmlFileHandler.GetDatasetTableHtml(
                        Path.Combine(Model.WorkDirectory, m_DatabaseFileName), null,
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
        #endregion
    }
}
