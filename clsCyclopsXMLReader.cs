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
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

using log4net;
using RDotNet;

namespace Cyclops
{
    /// <summary>
    /// Reads a Cyclops XML Workflow and assemble the modules
    /// </summary>
    public class clsCyclopsXMLReader : DataModules.clsBaseDataModule
    {
        #region Variables
        private XmlNodeReader reader;
        private enum ModuleType {Data, Visual, Export, Operation};
        private Dictionary<string, string> d_CyclopsParameters = new Dictionary<string, string>();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        private int i_ModuleCounter = 0;
        #endregion

        #region Constructors
        /// <summary>
        /// Reads the Cyclops XML workflow, and assembles the modules in the specified order and
        /// configuration
        /// </summary>
        public clsCyclopsXMLReader()
        {
        }
        /// <summary>
        /// Reads the Cyclops XML workflow, and assembles the modules in the specified order and
        /// configuration
        /// </summary>
        /// <param name="ParametersForCyclops">Parameters to run a Cyclops Pipeline</param>
        public clsCyclopsXMLReader(Dictionary<string, string> ParametersForCyclops)
        {
            d_CyclopsParameters = ParametersForCyclops;
        }

        /// <summary>
        /// Reads the Cyclops XML workflow, and assembles the modules in the specified order and
        /// configuration
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="ParametersForCyclops">Parameters to run a Cyclops Pipeline</param>
        public clsCyclopsXMLReader(clsCyclopsModel TheCyclopsModel,
            Dictionary<string, string> ParametersForCyclops)
        {
            d_CyclopsParameters = ParametersForCyclops;
            Model = TheCyclopsModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Read the XML workflow and assemble the modules
        /// </summary>
        /// <param name="XML_FileName">Path to the XML Workflow file</param>
        /// <param name="InstanceOfR">Name of the instance of R workspace</param>
        /// <returns>the root node module</returns>
        public DataModules.clsBaseDataModule ReadXML_Workflow(string XML_FileName, string InstanceOfR)
        {
            if (!File.Exists(XML_FileName))
            {
                traceLog.Error("Cyclops did not find XML workflow: " + XML_FileName);
                return null;
            }
            else
            {
                traceLog.Info("Reading XML workflow: " + Path.GetFileName(XML_FileName));
            }

            ModuleType currentModuleType = ModuleType.Data;

            DataModules.clsBaseDataModule root = null;                     // keeps track of the root of the tree
            DataModules.clsBaseDataModule currentNode = root;              // pointer to keep track of where you are in the tree as you add nodes
            VisualizationModules.clsBaseVisualizationModule currentVizNode = null;  // current pointer for visualization modules - important for adding parameters
            ExportModules.clsBaseExportModule currentExportNode = null;      // current pointer for export modules - important for adding parameters

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(XML_FileName);
                reader = new XmlNodeReader(doc);

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            switch (reader.Name)
                            {
                                case "operation":
                                    currentModuleType = ModuleType.Operation;
                                    string opid = reader.GetAttribute("id");
                                    traceLog.Info("READING XML OPERATION: " + opid);
                                    switch (opid)
                                    {
                                        case "SpectralCountOperation":
                                            Operations.clsSpectralCountMainOperation specOp = new Operations.clsSpectralCountMainOperation(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = specOp;
                                                currentNode = specOp;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(specOp);
                                                currentNode = specOp;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "LabelFreeOperation":
                                            Operations.clsLabelFreeMainOperation lbfOp = new Operations.clsLabelFreeMainOperation(Model, InstanceOfR);
                                            #region Set Type of Label-free Analysis
                                            if (reader.GetAttribute("type").Equals("Log2"))
                                            {
                                                lbfOp.SetType(Operations.clsLabelFreeMainOperation.LbfTypes.Log2);
                                            }
                                            else if (reader.GetAttribute("type").Equals("Log2LR"))
                                            {
                                                lbfOp.SetType(Operations.clsLabelFreeMainOperation.LbfTypes.Log2LR);
                                            }
                                            else if (reader.GetAttribute("type").Equals("Log2CT"))
                                            {
                                                lbfOp.SetType(Operations.clsLabelFreeMainOperation.LbfTypes.Log2CT);
                                            }
                                            else if (reader.GetAttribute("type").Equals("Log2All"))
                                            {
                                                lbfOp.SetType(Operations.clsLabelFreeMainOperation.LbfTypes.Log2All);
                                            }
                                            #endregion

                                            if (root == null)
                                            {
                                                root = lbfOp;
                                                currentNode = lbfOp;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(lbfOp);
                                                currentNode = lbfOp;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                    }
                                    break;
                                case "module":
                                    currentModuleType = ModuleType.Data;
                                    string modid = reader.GetAttribute("id");
                                    traceLog.Info("READING XML MODULE: " + modid);
                                    switch (modid)
                                    {
                                        case "Aggregate":
                                            DataModules.clsAggregate aggregate = new DataModules.clsAggregate(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = aggregate;
                                                currentNode = aggregate;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(aggregate);
                                                currentNode = aggregate;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "Anova":
                                            DataModules.clsANOVA anova = new DataModules.clsANOVA(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = anova;
                                                currentNode = anova;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(anova);
                                                currentNode = anova;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "BetaBinomial":
                                            DataModules.clsBetaBinomialModelModule bbm = new DataModules.clsBetaBinomialModelModule(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = bbm;
                                                currentNode = bbm;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(bbm);
                                                currentNode = bbm;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "CentralTendency":
                                            DataModules.clsCentralTendency centralTend = new DataModules.clsCentralTendency(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = centralTend;
                                                currentNode = centralTend;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(centralTend);
                                                currentNode = centralTend;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "Clean":
                                            DataModules.clsCleanUpRSourceFileObjects clean = new DataModules.clsCleanUpRSourceFileObjects(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = clean;
                                                currentNode = clean;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(clean);
                                                currentNode = clean;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "CombineSpectralCountResults":
                                            DataModules.clsCombineSpectralCountResultFiles combine =
                                                new DataModules.clsCombineSpectralCountResultFiles(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = combine;
                                                currentNode = combine;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(combine);
                                                currentNode = combine;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "DataTableManipulation":
                                            DataModules.clsDataTableManipulation dtm = new DataModules.clsDataTableManipulation(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = dtm;
                                                currentNode = dtm;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(dtm);
                                                currentNode = dtm;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "FilterByTable":
                                            DataModules.clsFilterByAnotherTable filterByTable = new DataModules.clsFilterByAnotherTable(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = filterByTable;
                                                currentNode = filterByTable;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(filterByTable);
                                                currentNode = filterByTable;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "FoldChange":
                                            DataModules.clsFoldChange fc = new DataModules.clsFoldChange(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = fc;
                                                currentNode = fc;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(fc);
                                                currentNode = fc;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "Import":
                                            DataModules.clsImportDataModule import = new DataModules.clsImportDataModule(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = import;
                                                currentNode = import;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(import);
                                                currentNode = import;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "LinearRegression":
                                            DataModules.clsLinearRegression linreg = new DataModules.clsLinearRegression(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = linreg;
                                                currentNode = linreg;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(linreg);
                                                currentNode = linreg;
                                                i_ModuleCounter++;
                                            }
                                            break;  
                                        case "LoadRSourceFiles":
                                            DataModules.clsRSourceFileModule source = new DataModules.clsRSourceFileModule(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = source;
                                                currentNode = source;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(source);
                                                currentNode = source;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "LoadRWorkspace":
                                            DataModules.clsLoadRWorkspaceModule load = new DataModules.clsLoadRWorkspaceModule(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = load;
                                                currentNode = load;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(load);
                                                currentNode = load;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "MissedCleavageSummary":
                                            DataModules.clsMissedCleavageAssessor missedCleavages = new DataModules.clsMissedCleavageAssessor(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = missedCleavages;
                                                currentNode = missedCleavages;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(missedCleavages);
                                                currentNode = missedCleavages;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "NormalizeSpectralCounts":
                                            DataModules.clsNormalizingSpectralCounts norm = new DataModules.clsNormalizingSpectralCounts(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = norm;
                                                currentNode = norm;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(norm);
                                                currentNode = norm;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "ProteinProphet":
                                            DataModules.clsProteinProphet pp = new DataModules.clsProteinProphet(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = pp;
                                                currentNode = pp;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(pp);
                                                currentNode = pp;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "PValueAdjust":
                                            DataModules.clsPValueAdjust pvaladj = new DataModules.clsPValueAdjust(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = pvaladj;
                                                currentNode = pvaladj;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(pvaladj);
                                                currentNode = pvaladj;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "QC_Peptide_Overlap":
                                            DataModules.clsQCPeptideOverlap pepOverlap = new DataModules.clsQCPeptideOverlap(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = pepOverlap;
                                                currentNode = pepOverlap;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(pepOverlap);
                                                currentNode = pepOverlap;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "QuasiTel":
                                            DataModules.clsQuasiTel quasitel = new DataModules.clsQuasiTel(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = quasitel;
                                                currentNode = quasitel;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(quasitel);
                                                currentNode = quasitel;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "RMD":
                                            DataModules.clsRMD rmd = new DataModules.clsRMD(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = rmd;
                                                currentNode = rmd;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(rmd);
                                                currentNode = rmd;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "Root":
                                            DataModules.clsRoot main = new DataModules.clsRoot(Model);
                                            if (root == null)
                                            {
                                                root = main;
                                                currentNode = main;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                traceLog.Warn("WARNING: Adding an empty root node in the middle of the pipeline");
                                                currentNode.AddDataChild(main);
                                                currentNode = main;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "RRollup":
                                            DataModules.clsRRollup rrollup = new DataModules.clsRRollup(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = rrollup;
                                                currentNode = rrollup;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(rrollup);
                                                currentNode = rrollup;
                                                i_ModuleCounter++;
                                            }
                                            break; 
                                        case "SummarizeData":
                                            DataModules.clsSummarizeData summarizeData = new DataModules.clsSummarizeData(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = summarizeData;
                                                currentNode = summarizeData;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(summarizeData);
                                                currentNode = summarizeData;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "SummaryTableInsert":
                                            DataModules.clsSQLiteSummaryTableGenerator summaryInserter = new DataModules.clsSQLiteSummaryTableGenerator(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = summaryInserter;
                                                currentNode = summaryInserter;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(summaryInserter);
                                                currentNode = summaryInserter;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "Test":
                                            clsMyTestingModule test = new clsMyTestingModule(InstanceOfR);
                                            if (root == null)
                                            {
                                                root = test;
                                                currentNode = test;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(test);
                                                currentNode = test;
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "Transform":
                                            DataModules.clsTransformModule transform = new DataModules.clsTransformModule(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = transform;
                                                currentNode = transform;
                                                i_ModuleCounter++;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(transform);
                                                currentNode = transform;
                                                i_ModuleCounter++;
                                            }
                                            break;

                                    }
                                    break;

                                case "visual":
                                    currentModuleType = ModuleType.Visual;
                                    modid = reader.GetAttribute("id");
                                    traceLog.Info("READING XML VISUAL: " + modid);
                                    switch (modid)
                                    {
                                        case "BarPlot":
                                            VisualizationModules.clsBarPlot bar = new VisualizationModules.clsBarPlot(Model, InstanceOfR);
                                            currentVizNode = bar;
                                            if (root == null)
                                            {
                                                traceLog.Error("Error: trying to add a Barplot Module without a root!");
                                            }
                                            else
                                            {
                                                traceLog.Info("Adding Barplot module from XML...");
                                                currentNode.AddVisualChild(bar);
                                                i_ModuleCounter++;
                                            }
                                            break;case "BoxPlot":
                                            VisualizationModules.clsBoxPlot box = new VisualizationModules.clsBoxPlot(Model, InstanceOfR);
                                            currentVizNode = box;
                                            if (root == null)
                                            {
                                                traceLog.Error("Error: trying to add a Boxplot Module without a root!");
                                            }
                                            else
                                            {
                                                traceLog.Info("Adding Boxplot module from XML...");
                                                currentNode.AddVisualChild(box);
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        
                                        case "CorrelationHeatmap":
                                            VisualizationModules.clsCorrelationHeatmap corrHeat = new VisualizationModules.clsCorrelationHeatmap(Model, InstanceOfR);
                                            currentVizNode = corrHeat;
                                            if (root == null)
                                            {
                                                traceLog.Error("Error: trying to add a Correlation Heatmap Module without a root!");
                                            }
                                            else
                                            {
                                                traceLog.Info("Adding Correlation Heatmap module from XML...");
                                                currentNode.AddVisualChild(corrHeat);
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "Heatmap":
                                            VisualizationModules.clsHeatmap heat = new VisualizationModules.clsHeatmap(InstanceOfR);
                                            currentVizNode = heat;
                                            if (root == null)
                                            {
                                                traceLog.Error("Error: trying to add a Heatmap Module without a root!");
                                            }
                                            else
                                            {
                                                traceLog.Info("Adding Heatmap module from XML...");
                                                currentNode.AddVisualChild(heat);
                                                i_ModuleCounter++;
                                            }
                                            break;  
                                        case "Hexbin":
                                            VisualizationModules.clsHexbin hexbin = new VisualizationModules.clsHexbin(InstanceOfR);
                                            currentVizNode = hexbin;
                                            if (root == null)
                                            {
                                                traceLog.Error("Error: trying to add a Hexbin Module without a root!");
                                            }
                                            else
                                            {
                                                traceLog.Info("Adding Hexbin module from XML...");
                                                currentNode.AddVisualChild(hexbin);
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "Histogram":
                                            VisualizationModules.clsHistogram hist = new VisualizationModules.clsHistogram(InstanceOfR);
                                            currentVizNode = hist;
                                            if (root == null)
                                            {
                                                traceLog.Error("Error: trying to add a Histogram Module without a root!");
                                            }
                                            else
                                            {
                                                traceLog.Info("Adding Histogram module from XML...");
                                                currentNode.AddVisualChild(hist);
                                                i_ModuleCounter++;
                                            }
                                            break;   
                                        case "QC_Fraction_Heatmap":
                                            VisualizationModules.clsQCFractionHeatmap fractionHeat = new VisualizationModules.clsQCFractionHeatmap(Model, InstanceOfR);
                                            currentVizNode = fractionHeat;
                                            if (root == null)
                                            {
                                                traceLog.Error("Error: trying to add a QC Fraction Heatmap Module without a root!");
                                            }
                                            else
                                            {
                                                traceLog.Info("Adding QC Fraction Heatmap module from XML...");
                                                currentNode.AddVisualChild(fractionHeat);
                                                i_ModuleCounter++;
                                            }
                                            break;
                                    }
                                    break;

                                case "export":
                                    currentModuleType = ModuleType.Export;
                                    modid = reader.GetAttribute("id");
                                    traceLog.Info("READING XML EXPORT: " + modid);
                                    switch (modid)
                                    {
                                        case "ExportTable":
                                            ExportModules.clsExportTableModule tab = new ExportModules.clsExportTableModule(Model, InstanceOfR);
                                            currentExportNode = tab;
                                            if (root == null)
                                            {
                                                traceLog.Error("ERROR reading XML, " + tab.ModuleName + ", trying to add an Export Module without a root!");
                                                Console.WriteLine("Error: trying to add a ExportTable Module without a root!");
                                            }
                                            else
                                            {
                                                currentNode.AddExportChild(tab);
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "HTML_Summary":
                                            ExportModules.clsHTMLSummary generalHtml_Summary = new ExportModules.clsHTMLSummary(Model, InstanceOfR);
                                            currentExportNode = generalHtml_Summary;
                                            if (root == null)
                                            {
                                                traceLog.Error("ERROR reading XML, " + generalHtml_Summary.ModuleName + ", trying to add an Export Module without a root!");
                                                Console.WriteLine("Error: trying to add a General HTML Summary Module without a root!");
                                            }
                                            else
                                            {
                                                currentNode.AddExportChild(generalHtml_Summary);
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "LBF_Summary":
                                            ExportModules.clsLBF_Summary_HTML lbf_Summary = new ExportModules.clsLBF_Summary_HTML(Model, InstanceOfR);
                                            currentExportNode = lbf_Summary;
                                            if (root == null)
                                            {
                                                traceLog.Error("ERROR reading XML, " + lbf_Summary.ModuleName + ", trying to add an Export Module without a root!");
                                                Console.WriteLine("Error: trying to add a ExportTable Module without a root!");
                                            }
                                            else
                                            {
                                                currentNode.AddExportChild(lbf_Summary);
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "QC_Fractions":
                                            ExportModules.clsQC_Fraction_HTML qc = new ExportModules.clsQC_Fraction_HTML(Model, InstanceOfR);
                                            currentExportNode = qc;
                                            if (root == null)
                                            {
                                                traceLog.Error("ERROR reading XML, " + qc.ModuleName + ", trying to add an Export without a root!");
                                                Console.WriteLine("Error: trying to add a ExportTable Module without a root!");
                                            }
                                            else
                                            {
                                                currentNode.AddExportChild(qc);
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "Save":
                                            ExportModules.clsSaveEnvironment se_Node = new ExportModules.clsSaveEnvironment(Model, InstanceOfR);
                                            currentExportNode = se_Node;
                                            if (root == null)
                                            {
                                                traceLog.Error("ERROR reading XML, " + se_Node.ModuleName + ", trying to add an Export Module without a root!");
                                                Console.WriteLine("Error: trying to add a Save Module without a root!");
                                            }
                                            else
                                            {
                                                currentNode.AddExportChild(se_Node);
                                                i_ModuleCounter++;
                                            }
                                            break;
                                        case "Sco_HTML_Summary":
                                            ExportModules.clsSCO_Summary_HTML sco_Html_Summary = new ExportModules.clsSCO_Summary_HTML(Model, InstanceOfR);
                                            currentExportNode = sco_Html_Summary;
                                            if (root == null)
                                            {
                                                traceLog.Error("ERROR reading XML, " + sco_Html_Summary.ModuleName + ", trying to add an Export Module without a root!");
                                                Console.WriteLine("Error: trying to add a Save Module without a root!");
                                            }
                                            else
                                            {
                                                currentNode.AddExportChild(sco_Html_Summary);
                                                i_ModuleCounter++;
                                            }
                                            break;
                                    }
                                    break;

                                case "parameters":
                                    Dictionary<string, object> d_Parameters = new Dictionary<string, object>();
                                    while (reader.NodeType != XmlNodeType.EndElement | !reader.Name.Equals("parameters"))
                                    {
                                        if (reader.EOF)
                                            break;
                                        else
                                            reader.Read();

                                        if (reader.NodeType == XmlNodeType.Element & reader.Name.Equals("parameterName"))
                                        {
                                            if (!d_Parameters.ContainsKey(reader.GetAttribute("key").ToString()))
                                            {
                                                d_Parameters.Add(reader.GetAttribute("key").ToString(),
                                                reader.GetAttribute("value"));
                                            }
                                            else // if the key is already in the dictionary -> add the value as List<string>
                                            {
                                                string myKey = reader.GetAttribute("key").ToString();
                                                dynamic d = d_Parameters[myKey];
                                                if (d.GetType() == typeof(String))
                                                {
                                                    List<string> l = new List<string>();
                                                    l.Add(d);
                                                    l.Add(reader.GetAttribute("value"));
                                                    d_Parameters[myKey] = l;
                                                }
                                                else if (d.GetType() == typeof(List<string>))
                                                {
                                                    List<string> l = (List<string>)d_Parameters[myKey];
                                                    l.Add(reader.GetAttribute("value"));
                                                    d_Parameters[myKey] = l;
                                                }
                                                else // otherwise, just replace the value
                                                {
                                                    d_Parameters[reader.GetAttribute("key").ToString()] =
                                                        reader.GetAttribute("value");
                                                }
                                            }
                                        }
                                    }

                                    /// Now, add the passed in Cyclops Parameters to the current dictionary
                                    /// of parameters (derived from the XML workflow)
                                    foreach (KeyValuePair<string, string> kvp in d_CyclopsParameters)
                                    {
                                        d_Parameters.Add(kvp.Key, kvp.Value);
                                    }

                                    if (currentModuleType == ModuleType.Data)
                                    {
                                        currentNode.Parameters = d_Parameters;
                                        traceLog.Info(currentNode.GetDescription());
                                    }
                                    else if (currentModuleType == ModuleType.Visual)
                                    {
                                        currentVizNode.Parameters = d_Parameters;
                                        traceLog.Info(currentVizNode.GetDescription());
                                    }
                                    else if (currentModuleType == ModuleType.Export)
                                    {
                                        currentExportNode.Parameters = d_Parameters;
                                        traceLog.Info(currentExportNode.GetDescription());
                                    }
                                    break;
                            }
                            break;




                        case XmlNodeType.EndElement:
                            switch (reader.Name)
                            {
                                case "module":
                                    if (currentNode.HasParent())
                                        currentNode = currentNode.GetParent();

                                    break;
                            }
                            break;
                    }
                }
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR Reading XML Workflow: " + exc.ToString());
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

            Model.NumberOfModules = i_ModuleCounter; // report the total number of modules
            return root;
        }
        #endregion
    }
}
