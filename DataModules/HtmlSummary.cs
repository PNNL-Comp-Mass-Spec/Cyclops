/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://www.pnnl.gov/integrative-omics
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
    public class HtmlSummary : BaseDataModule
    {
        // Ignore Spelling: Heatmap, Tryptic

        private const string m_ModuleName = "HtmlSummary";

        private const string m_Description = "";

        private string m_DatabaseFileName = "Results.db3";

        /// <summary>
        /// Required parameters to run Aggregate
        /// </summary>
        private enum RequiredParameters
        {
            FileName
        }

        private bool m_DatabaseFound;
        private enum HTMLFileType { Dataset, Index };
        private readonly SQLiteHandler m_SQLiteReader = new SQLiteHandler();

        /// <summary>
        /// Generic constructor creating an HtmlSummary Module
        /// </summary>
        public HtmlSummary()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// HtmlSummary module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public HtmlSummary(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// HtmlSummary module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public HtmlSummary(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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

                if (CheckParameters())
                {
                    successful = HtmlSummaryFunction();
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

            if (Parameters.ContainsKey("DatabaseFileName"))
            {
                if (File.Exists(Parameters["DatabaseFileName"]))
                {
                    m_DatabaseFileName = Parameters["DatabaseFileName"];
                    m_SQLiteReader.DatabaseFileName = Parameters["DatabaseFileName"];
                    m_DatabaseFound = true;
                }
                else if (File.Exists(Path.Combine(Model.WorkDirectory, Parameters["DatabaseFileName"])))
                {
                    m_DatabaseFileName = Parameters["DatabaseFileName"];
                    m_SQLiteReader.DatabaseFileName = Path.Combine(Model.WorkDirectory, Parameters["DatabaseFileName"]);
                    m_DatabaseFound = true;
                }
            }
            else
            {
                if (File.Exists(Path.Combine(Model.WorkDirectory, "Results.db3")))
                {
                    m_SQLiteReader.DatabaseFileName = Path.Combine(Model.WorkDirectory, "Results.db3");
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
        public bool HtmlSummaryFunction()
        {
            var cssFileName = "styles.css";
            var datasetsFileName = "Datasets.html";
            var qcFileName = "QC.html";

            var navBarNodes = new List<HtmlLinkNode>
            {
                new HtmlLinkNode(
                    "Home", Parameters[nameof(RequiredParameters.FileName)], false),
                new HtmlLinkNode(
                    "Datasets", datasetsFileName, false),
                new HtmlLinkNode(
                    "QC Plots", qcFileName, false)
            };

            using (var cssWriter = File.AppendText(Path.Combine(
                Model.WorkDirectory, cssFileName)))
            {
                cssWriter.WriteLine(HtmlFileHandler.GetCSS(HtmlFileHandler.CssStyle.NavBar, 160));
                cssWriter.WriteLine(HtmlFileHandler.GetCSS(HtmlFileHandler.CssStyle.LeftIndent, 160));
                cssWriter.WriteLine(HtmlFileHandler.GetCSS(HtmlFileHandler.CssStyle.Th, 160));
            }

            // Construct and write-out the Datasets Page
            var datasetHtml = new StringBuilder();

            datasetHtml.Append(HtmlFileHandler.GetHtmlHeader());
            datasetHtml.Append(HtmlFileHandler.GetCSSLink(cssFileName));
            datasetHtml.Append(HtmlFileHandler.GetHtmlJavascriptStart());
            datasetHtml.Append(WriteHtmlScripts());
            datasetHtml.Append(HtmlFileHandler.GetHtmlScriptEnd());
            datasetHtml.Append(HtmlFileHandler.GetEndHeadStartBody());
            datasetHtml.Append(HtmlFileHandler.GetNavBar(navBarNodes, "LEFT"));
            datasetHtml.Append("\t\t<DIV ID='DatasetTable' style='position: absolute; left:200px; top:100px;'>\n");
            datasetHtml.Append(WriteHtmlBody(HTMLFileType.Dataset));
            datasetHtml.Append("\t\t</DIV>\n");
            datasetHtml.Append(HtmlFileHandler.GetEndBodyEndHtml());

            var datasetHtmlWriter = new StreamWriter(Path.Combine(Model.WorkDirectory, datasetsFileName));
            datasetHtmlWriter.Write(datasetHtml);
            datasetHtmlWriter.Close();
            datasetHtml.Clear();

            // Construct and write-out the QC Page
            var qcHtml = new StringBuilder();

            var missedCleavages = new HtmlLinkNode("Missed Cleavages", "mc", true);
            navBarNodes.Add(missedCleavages);
            var trypticPeptides = new HtmlLinkNode("Tryptic Peptides", "tp", true);
            navBarNodes.Add(trypticPeptides);

            qcHtml.Append(HtmlFileHandler.GetHtmlHeader());
            qcHtml.Append(HtmlFileHandler.GetHtmlJavascriptStart());
            qcHtml.Append(WriteHtmlScripts());
            qcHtml.Append(HtmlFileHandler.GetHtmlScriptEnd());
            qcHtml.Append(HtmlFileHandler.GetCSSLink(cssFileName));
            qcHtml.Append(HtmlFileHandler.GetEndHeadStartBody());
            qcHtml.Append(HtmlFileHandler.GetNavBar(navBarNodes, "LEFT"));
            qcHtml.Append(WriteHtmlBody(HTMLFileType.Index));
            qcHtml.Append("\t\t<H2 class='pos_left'>Spectral Count Summary</H2>\n");
            qcHtml.Append(HtmlFileHandler.GetPictureCode(
                "Spectral_Count_Summary.png", true, "pos_left", null, null));
            qcHtml.Append("\t\t<DIV ID='SpectralCount' style='position: absolute; left:700px; top:100px;'>\n");
            qcHtml.Append(HtmlFileHandler.GetTableHtml(
                              m_SQLiteReader.GetTable("T_MAC_SpecCnt_Summary"), null, null, null, null));
            qcHtml.Append("\t\t</DIV>\n");

            qcHtml.Append("\t\t<A NAME='mc'/A>\n");
            qcHtml.Append("\t\t<H2 class='pos_left'>Missed Cleavage Summary</H2>\n");
            qcHtml.Append(HtmlFileHandler.GetPictureCode(
                "MissedCleavage_Summary.png", true, "pos_left", null, null));
            qcHtml.Append("\t\t<DIV ID='MissedCleavage' style='position: absolute; left:700px; top:560px;'>\n");
            qcHtml.Append(HtmlFileHandler.GetTableHtml(
                              m_SQLiteReader.GetTable("T_MissedCleavageSummary"), null, null, null, null));
            qcHtml.Append("\t\t</DIV>\n");

            qcHtml.Append("\t\t<A NAME='tp'/A>\n");
            qcHtml.Append("\t\t<H2 class='pos_left'>Tryptic Peptide Summary</H2>\n");
            qcHtml.Append(HtmlFileHandler.GetPictureCode(
                "Tryptic_Summary.png", true, "pos_left", null, null));
            qcHtml.Append("\t\t<DIV ID='TrypticCoverage' style='position: absolute; left:700px; top:1040px;'>\n");
            qcHtml.Append(HtmlFileHandler.GetTableHtml(
                              m_SQLiteReader.GetTable("T_MAC_Trypticity_Summary"), null, null, null, null));
            qcHtml.Append("\t\t</DIV>\n");

            var qcWriter = new StreamWriter(Path.Combine(Model.WorkDirectory, qcFileName));
            qcWriter.WriteLine(qcHtml);
            qcWriter.Close();

            navBarNodes.Remove(missedCleavages);
            navBarNodes.Remove(trypticPeptides);

            // Construct and write-out the main HTML summary page
            var summaryHtml = new StringBuilder();

            navBarNodes.Add(new HtmlLinkNode("Correlation", "ch", true));

            summaryHtml.Append(HtmlFileHandler.GetHtmlHeader());
            summaryHtml.Append(HtmlFileHandler.GetHtmlJavascriptStart());
            summaryHtml.Append(WriteHtmlScripts());
            summaryHtml.Append(HtmlFileHandler.GetHtmlScriptEnd());
            summaryHtml.Append(HtmlFileHandler.GetCSSLink(cssFileName));
            summaryHtml.Append(HtmlFileHandler.GetEndHeadStartBody());
            summaryHtml.Append(HtmlFileHandler.GetNavBar(navBarNodes, "LEFT"));
            summaryHtml.Append(WriteHtmlBody(HTMLFileType.Index));
            summaryHtml.Append("\t\t<A NAME='ch'/A>\n");
            summaryHtml.Append("\t\t<H2 class='pos_left'>Correlation Heatmap</H2>\n");
            summaryHtml.Append(HtmlFileHandler.GetPictureCode("T_SpectralCounts_CorrelationHeatmap.png", true, "pos_left", null, null));

            summaryHtml.Append(HtmlFileHandler.GetEndBodyEndHtml());

            var summaryWriter = new StreamWriter(Path.Combine(Model.WorkDirectory, Parameters[nameof(RequiredParameters.FileName)]));
            summaryWriter.Write(summaryHtml);
            summaryWriter.Close();

            return true;
        }

        private StringBuilder WriteHtmlScripts()
        {
            var returnScripts = new StringBuilder();
            // TODO: Build Script

            return returnScripts;
        }

        private string WriteHtmlBody(HTMLFileType TheHTMLFileType)
        {
            var body = "";
            switch (TheHTMLFileType)
            {
                case HTMLFileType.Dataset:
                    body = HtmlFileHandler.GetDatasetTableHtml(
                        Path.Combine(Model.WorkDirectory, m_DatabaseFileName), null, "table_header", "center", 0, 2, 4);
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
    }
}
