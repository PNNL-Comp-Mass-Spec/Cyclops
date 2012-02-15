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
        private enum ModuleType {Data, Visual, Export};
        private Dictionary<string, string> d_CyclopsParameters = new Dictionary<string, string>();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
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
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(aggregate);
                                                currentNode = aggregate;
                                            }
                                            break;
                                        case "BetaBinomial":
                                            DataModules.clsBetaBinomialModelModule bbm = new DataModules.clsBetaBinomialModelModule(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = bbm;
                                                currentNode = bbm;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(bbm);
                                                currentNode = bbm;
                                            }
                                            break;
                                        case "CentralTendency":
                                            DataModules.clsCentralTendency centralTend = new DataModules.clsCentralTendency(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = centralTend;
                                                currentNode = centralTend;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(centralTend);
                                                currentNode = centralTend;
                                            }
                                            break;
                                        case "Clean":
                                            DataModules.clsCleanUpRSourceFileObjects clean = new DataModules.clsCleanUpRSourceFileObjects(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = clean;
                                                currentNode = clean;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(clean);
                                                currentNode = clean;
                                            }
                                            break;
                                        case "CombineSpectralCountResults":
                                            DataModules.clsCombineSpectralCountResultFiles combine =
                                                new DataModules.clsCombineSpectralCountResultFiles(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = combine;
                                                currentNode = combine;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(combine);
                                                currentNode = combine;
                                            }
                                            break;
                                        case "DataTableManipulation":
                                            DataModules.clsDataTableManipulation dtm = new DataModules.clsDataTableManipulation(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = dtm;
                                                currentNode = dtm;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(dtm);
                                                currentNode = dtm;
                                            }
                                            break;
                                        case "FilterByTable":
                                            DataModules.clsFilterByAnotherTable filterByTable = new DataModules.clsFilterByAnotherTable(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = filterByTable;
                                                currentNode = filterByTable;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(filterByTable);
                                                currentNode = filterByTable;
                                            }
                                            break;
                                        case "FoldChange":
                                            DataModules.clsFoldChange fc = new DataModules.clsFoldChange(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = fc;
                                                currentNode = fc;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(fc);
                                                currentNode = fc;
                                            }
                                            break;
                                        case "Import":
                                            DataModules.clsImportDataModule import = new DataModules.clsImportDataModule(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = import;
                                                currentNode = import;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(import);
                                                currentNode = import;
                                            }
                                            break;
                                        case "LinearRegression":
                                            DataModules.clsLinearRegression linreg = new DataModules.clsLinearRegression(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = linreg;
                                                currentNode = linreg;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(linreg);
                                                currentNode = linreg;
                                            }
                                            break;  
                                        case "LoadRSourceFiles":
                                            DataModules.clsRSourceFileModule source = new DataModules.clsRSourceFileModule(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = source;
                                                currentNode = source;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(source);
                                                currentNode = source;
                                            }
                                            break;
                                        case "LoadRWorkspace":
                                            DataModules.clsLoadRWorkspaceModule load = new DataModules.clsLoadRWorkspaceModule(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = load;
                                                currentNode = load;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(load);
                                                currentNode = load;
                                            }
                                            break;
                                        case "NormalizeSpectralCounts":
                                            DataModules.clsNormalizingSpectralCounts norm = new DataModules.clsNormalizingSpectralCounts(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = norm;
                                                currentNode = norm;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(norm);
                                                currentNode = norm;
                                            }
                                            break;
                                        case "RMD":
                                            DataModules.clsRMD rmd = new DataModules.clsRMD(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = rmd;
                                                currentNode = rmd;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(rmd);
                                                currentNode = rmd;
                                            }
                                            break;
                                        case "RRollup":
                                            DataModules.clsRRollup rrollup = new DataModules.clsRRollup(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = rrollup;
                                                currentNode = rrollup;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(rrollup);
                                                currentNode = rrollup;
                                            }
                                            break;    
                                        case "ProteinProphet":
                                            DataModules.clsProteinProphet pp = new DataModules.clsProteinProphet(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = pp;
                                                currentNode = pp;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(pp);
                                                currentNode = pp;
                                            }
                                            break;
                                        
                                        case "Test":
                                            clsMyTestingModule test = new clsMyTestingModule(InstanceOfR);
                                            if (root == null)
                                            {
                                                root = test;
                                                currentNode = test;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(test);
                                                currentNode = test;
                                            }
                                            break;
                                        case "Transform":
                                            DataModules.clsTransformModule transform = new DataModules.clsTransformModule(Model, InstanceOfR);
                                            if (root == null)
                                            {
                                                root = transform;
                                                currentNode = transform;
                                            }
                                            else
                                            {
                                                currentNode.AddDataChild(transform);
                                                currentNode = transform;
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
                                            }
                                            break;
                                        case "BoxPlot":
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
                                        case "Save":
                                            ExportModules.clsSaveEnvironment se_Node = new ExportModules.clsSaveEnvironment(Model, InstanceOfR);
                                            currentExportNode = se_Node;
                                            if (root == null)
                                            {
                                                traceLog.Error("ERROR reading XML, " + se_Node.ModuleName + ", trying to add a Save Module without a root!");
                                                Console.WriteLine("Error: trying to add a Save Module without a root!");
                                            }
                                            else
                                            {
                                                currentNode.AddExportChild(se_Node);
                                            }
                                            break;
                                        case "ExportTable":
                                            ExportModules.clsExportTableModule tab = new ExportModules.clsExportTableModule(Model, InstanceOfR);
                                            currentExportNode = tab;
                                            if (root == null)
                                            {
                                                traceLog.Error("ERROR reading XML, " + tab.ModuleName + ", trying to add a Save Module without a root!");
                                                Console.WriteLine("Error: trying to add a ExportTable Module without a root!");
                                            }
                                            else
                                            {
                                                currentNode.AddExportChild(tab);
                                            }
                                            break;
                                        case "QC_Fractions":
                                            ExportModules.clsQC_Fraction_HTML qc = new ExportModules.clsQC_Fraction_HTML(Model, InstanceOfR);
                                            currentExportNode = qc;
                                            if (root == null)
                                            {
                                                traceLog.Error("ERROR reading XML, " + qc.ModuleName + ", trying to add a Save Module without a root!");
                                                Console.WriteLine("Error: trying to add a ExportTable Module without a root!");
                                            }
                                            else
                                            {
                                                currentNode.AddExportChild(qc);
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

            return root;
        }
        #endregion
    }
}
