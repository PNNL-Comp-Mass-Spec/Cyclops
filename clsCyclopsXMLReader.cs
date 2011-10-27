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

using RDotNet;

namespace Cyclops
{
    /// <summary>
    /// Reads a Cyclops XML Workflow and assemble the modules
    /// </summary>
    public class clsCyclopsXMLReader : clsBaseDataModule
    {        
        private XmlNodeReader reader;
        private enum ModuleType {Data, Visual, Export};
        private Dictionary<string, string> d_CyclopsParameters = new Dictionary<string, string>();

        #region Constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        public clsCyclopsXMLReader()
        {
        }
        /// <summary>
        /// Constructor that requires the parameters to run Cyclops Pipeline
        /// </summary>
        /// <param name="ParametersForCyclops">Parameters to run a Cyclops Pipeline</param>
        public clsCyclopsXMLReader(Dictionary<string, string> ParametersForCyclops)
        {
            d_CyclopsParameters = ParametersForCyclops;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Read the XML workflow and assemble the modules
        /// </summary>
        /// <param name="XML_FileName">Path to the XML Workflow file</param>
        /// <param name="InstanceOfR">Name of the instance of R workspace</param>
        /// <returns>the root node module</returns>
        public clsBaseDataModule ReadXML_Workflow(string XML_FileName, string InstanceOfR)
        {
            ModuleType currentModuleType = ModuleType.Data;

            clsBaseDataModule root = null;                     // keeps track of the root of the tree
            clsBaseDataModule currentNode = root;              // pointer to keep track of where you are in the tree as you add nodes
            clsBaseVisualizationModule currentVizNode = null;  // current pointer for visualization modules - important for adding parameters
            clsBaseExportModule currentExportNode = null;      // current pointer for export modules - important for adding parameters

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(XML_FileName);
                reader = new XmlNodeReader(doc);
                
                while (reader.Read())
                {
                    switch(reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            switch (reader.Name)
                            {
                                case "module":
                                    currentModuleType = ModuleType.Data;
                                    string modid = reader.GetAttribute("id");

                                            switch (modid)
                                            {
                                                case "Import":
                                                    clsImportDataModule import = new clsImportDataModule(InstanceOfR);
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
                                                case "Transform":
                                                    clsTransformModule transform = new clsTransformModule(InstanceOfR);
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
                                                case "LoadRSourceFiles":
                                                    clsRSourceFileModule source = new clsRSourceFileModule(InstanceOfR);
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
                                                    clsLoadRWorkspaceModule load = new clsLoadRWorkspaceModule(InstanceOfR);
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
                                                case "Aggregate":
                                                    clsAggregate aggregate = new clsAggregate(InstanceOfR);
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
                                                case "RMD":
                                                    clsRMD rmd = new clsRMD(InstanceOfR);
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
                                                
                                            }
                                    break;
                                
                                case "visual":
                                    currentModuleType = ModuleType.Visual;
                                    modid = reader.GetAttribute("id");
                                    switch (modid)
                                    {
                                        case "Hexbin":
                                            clsHexbin hexbin = new clsHexbin(InstanceOfR);
                                            currentVizNode = hexbin;
                                            if (root == null)
                                            {
                                                Console.WriteLine("Error: trying to add a Hexbin Module without a root!");
                                            }
                                            else
                                            {
                                                currentNode.AddVisualChild(hexbin);
                                            }
                                            break;
                                    }
                                    break;

                                case "export":
                                    currentModuleType = ModuleType.Export;
                                    modid = reader.GetAttribute("id");

                                    switch (modid)
                                    {
                                        case "Save":
                                            clsSaveEnvironment se_Node = new clsSaveEnvironment(InstanceOfR);
                                            currentExportNode = se_Node;
                                            if (root == null)
                                            {
                                                Console.WriteLine("Error: trying to add a Save Module without a root!");
                                            }
                                            else
                                            {
                                                currentNode.AddExportChild(se_Node);
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
                                            else // if the key is already in the dictionary -> replace the value
                                            {
                                                d_Parameters[reader.GetAttribute("key").ToString()] =
                                                    reader.GetAttribute("value");
                                            }
                                        }
                                    }

                                    /// Now, add the passed in Cyclops Parameters to the current dictionary
                                    /// of parameters (derived from the XML workflow)
                                    foreach(KeyValuePair<string, string> kvp in d_CyclopsParameters)
                                    {
                                        d_Parameters.Add(kvp.Key, kvp.Value);
                                    }

                                    if (currentModuleType == ModuleType.Data)
                                        currentNode.Parameters = d_Parameters;
                                    else if (currentModuleType == ModuleType.Visual)
                                        currentVizNode.Parameters = d_Parameters;
                                    else if (currentModuleType == ModuleType.Export)
                                        currentExportNode.Parameters = d_Parameters;
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
