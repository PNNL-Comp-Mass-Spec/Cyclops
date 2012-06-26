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

namespace Cyclops.Operations
{
    /// <summary>
    /// Provides an operation pipeline for conducting a general spectral count analysis
    /// </summary>
    public class clsSpectralCountMainOperation : clsBaseOperationModule
    {
        #region Members
        protected string s_RInstance;
        private string s_OperationsDBPath = @"\\gigasax\DMS_Workflows\Cyclops\Cyclops_Operations.db3";
        private string s_SpectralCountTableName = "T_SpectralCountPipelineOperation";
        public enum ScoTypes { Standard, Iterator };
        private string[] s_SpectralCountTableNames = new string[] {
            "T_SpectralCountPipelineOperation",
            "T_SpectralCountIteratorPipelineOperation"
        };
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Constructors
        /// <summary>
        /// Operation performs a general spectral count analysis
        /// </summary>
        public clsSpectralCountMainOperation()
        {
            ModuleName = "Spectral Count Pipeline Operation";
        }

        /// <summary>
        /// Operation performs a general spectral count analysis
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsSpectralCountMainOperation(string InstanceOfR)
        {
            ModuleName = "Spectral Count Pipeline Operation";
            s_RInstance = InstanceOfR;
        }

        /// <summary>
        /// Operation performs a general spectral count analysis
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsSpectralCountMainOperation(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            Model = TheCyclopsModel;
            ModuleName = "Spectral Count Pipeline Operation";            
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties
        public string OperationsDBPath
        {
            get { return s_OperationsDBPath; }
            set { s_OperationsDBPath = value; }
        }
        public DataModules.clsBaseDataModule Root {get; set;}
        public DataModules.clsBaseDataModule CurrentNode {get;set;}
        public VisualizationModules.clsBaseVisualizationModule CurrentVixNode {get;set;}
        public ExportModules.clsBaseExportModule CurrentExportNode {get;set;}
        #endregion

        #region Methods
        /// <summary>
        /// Runs operation
        /// </summary>
        public override void PerformOperation()
        {
            traceLog.Info("Running Spectral Count Operation...");

            ConstructModules();

            RunModules();
        }

        public void SetTypes(ScoTypes TypeOfAnalysis)
        {
            switch (TypeOfAnalysis)
            {
                case ScoTypes.Standard:
                    s_SpectralCountTableName = s_SpectralCountTableNames[(int)ScoTypes.Standard];
                    break;
                case ScoTypes.Iterator:
                    s_SpectralCountTableName = s_SpectralCountTableNames[(int)ScoTypes.Iterator];
                    break;
            }
        }

        /// <summary>
        /// Retrieves the table from the Operations database and assembles the Modules 
        /// into a pipeline.
        /// </summary>
        private void ConstructModules()
        {
            if (Parameters.ContainsKey("OperationsDatabaseDirectory"))
                s_OperationsDBPath = Parameters["OperationsDatabaseDirectory"];

            DataTable dt_Modules = clsSQLiteHandler.GetDataTable(
                string.Format("SELECT * FROM {0}", s_SpectralCountTableName),
                s_OperationsDBPath);
                        
            StepValueNode svn = GetMaximumStepValueInOperationsTable(dt_Modules);

            for (int i = svn.MinimumValue; i < svn.MaximumValue + 1; i++)
            {
                string stepExp = string.Format("Step = {0}", i);
                DataRow[] dr_Rows = dt_Modules.Select(stepExp);

                SetModule(dr_Rows);
            }
        }

        private void RunModules()
        {
            Root.PerformOperation();
        }

        private void SetModule(DataRow[] Rows)
        {
            if (Rows.Length < 1)
            {
                // There is an error
                traceLog.Error("ERROR Spectral Count Main Operation: Received empty Rows #SetModule!");
                return;
            }

            int i_ModuleType = Convert.ToInt32(Rows[0]["ModuleType"]);
            switch (i_ModuleType)
            {
                case 1:
                    AddDataModule(Rows);
                    break;
                case 2:
                    AddExportModule(Rows);
                    break;
                case 3:
                    AddVisualModules(Rows);
                    break;
            }
        }

        
        /// <summary>
        /// Adds a Data Module to the pipeline.
        /// </summary>
        /// <param name="Rows"></param>
        private void AddDataModule(DataRow[] Rows)
        {
            string s_Module = Rows[0]["Module"].ToString();
            switch (s_Module)
            {
                case "LoadRSourceFiles":
                    DataModules.clsRSourceFileModule load = new DataModules.clsRSourceFileModule(Model, s_RInstance);
                    load.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = load;
                        CurrentNode = Root;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(load);
                        CurrentNode = load;
                        Model.NumberOfModules++;
                    }
                    break;
                case "Import":
                    DataModules.clsImportDataModule import = new DataModules.clsImportDataModule(Model, s_RInstance);
                    import.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = import;
                        CurrentNode = Root;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(import);
                        CurrentNode = import;
                        Model.NumberOfModules++;
                    }
                    break;
                case "MissedCleavageSummary":
                    DataModules.clsMissedCleavageAssessor mca = new DataModules.clsMissedCleavageAssessor(Model, s_RInstance);
                    mca.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = mca;
                        CurrentNode = Root;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(mca);
                        CurrentNode = mca;
                        Model.NumberOfModules++;
                    }
                    break;
                case "ProteinProphet":
                    DataModules.clsProteinProphet protProph = new DataModules.clsProteinProphet(Model, s_RInstance);
                    protProph.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = protProph;
                        CurrentNode = Root;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(protProph);
                        CurrentNode = protProph;
                        Model.NumberOfModules++;
                    }
                    break;
                case "BetaBinomial":
                    DataModules.clsBetaBinomialModelModule betaB = new DataModules.clsBetaBinomialModelModule(Model, s_RInstance);
                    betaB.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = betaB;
                        CurrentNode = Root;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(betaB);
                        CurrentNode = betaB;
                        Model.NumberOfModules++;
                    }
                    break;
                case "QuasiTel":
                    DataModules.clsQuasiTel quasi = new DataModules.clsQuasiTel(Model, s_RInstance);
                    quasi.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = quasi;
                        CurrentNode = Root;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(quasi);
                        CurrentNode = quasi;
                        Model.NumberOfModules++;
                    }
                    break;
                case "Clean":
                    DataModules.clsCleanUpRSourceFileObjects clean = new DataModules.clsCleanUpRSourceFileObjects(Model, s_RInstance);
                    clean.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = clean;
                        CurrentNode = Root;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(clean);
                        CurrentNode = clean;
                        Model.NumberOfModules++;
                    }
                    break;
                case "SummarizeData":
                    DataModules.clsSummarizeData summarizeData = new DataModules.clsSummarizeData(Model, s_RInstance);
                    summarizeData.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = summarizeData;
                        CurrentNode = summarizeData;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(summarizeData);
                        CurrentNode = summarizeData;
                    }
                    Model.NumberOfModules++;
                    break;
            }
        }

        private void AddParameters(ref DataModules.clsLoadRWorkspaceModule load, DataRow[] Rows)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds an Export Module to the pipeline.
        /// </summary>
        /// <param name="Rows"></param>
        private void AddExportModule(DataRow[] Rows)
        {
            string s_Module = Rows[0]["Module"].ToString();
            switch (s_Module)
            {
                case "ExportTable":
                    ExportModules.clsExportTableModule exportTable = new ExportModules.clsExportTableModule(Model, s_RInstance);
                    exportTable.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        traceLog.Error("ERROR: Trying to add Export Table node to Root. This operation can not be performed!");
                    }
                    else
                    {
                        CurrentNode.AddExportChild(exportTable);
                        Model.NumberOfModules++;
                    }
                    break;
                case "Save":
                    ExportModules.clsSaveEnvironment save = new ExportModules.clsSaveEnvironment(Model, s_RInstance);
                    save.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        traceLog.Error("ERROR: Trying to add Save node to Root. This operation can not be performed!");
                    }
                    else
                    {
                        CurrentNode.AddExportChild(save);
                        Model.NumberOfModules++;
                    }
                    break;
                case "Sco_HTML_Summary":
                    ExportModules.clsSCO_Summary_HTML sco_HTML = new ExportModules.clsSCO_Summary_HTML(Model, s_RInstance);
                    sco_HTML.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        traceLog.Error("ERROR: Trying to add Spectral Count HTML Summary node to Root. This operation can not be performed!");
                    }
                    else
                    {
                        CurrentNode.AddExportChild(sco_HTML);
                        Model.NumberOfModules++;
                    }
                    break;                    
            }
        }

        /// <summary>
        /// Adds a Visual Module to the pipeline.
        /// </summary>
        /// <param name="Rows"></param>
        private void AddVisualModules(DataRow[] Rows)
        {
            string s_Module = Rows[0]["Module"].ToString();
            switch (s_Module)
            {
                case "Histogram":
                    VisualizationModules.clsHistogram histogram = new VisualizationModules.clsHistogram(s_RInstance);
                    histogram.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        traceLog.Error("ERROR: Trying to add Histogram node to Root. This operation can not be performed!");
                    }
                    else
                    {
                        CurrentNode.AddVisualChild(histogram);
                        Model.NumberOfModules++;
                    }
                    break;
                case "Hexbin":
                    VisualizationModules.clsHexbin hexbin = new VisualizationModules.clsHexbin(Model, s_RInstance);
                    hexbin.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        traceLog.Error("ERROR: Trying to add Hexbin node to Root. This operation can not be performed!");
                    }
                    else
                    {
                        CurrentNode.AddVisualChild(hexbin);
                        Model.NumberOfModules++;
                    }
                    break;
                case "BarPlot":
                    VisualizationModules.clsBarPlot barPlot = new VisualizationModules.clsBarPlot(Model, s_RInstance);
                    barPlot.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        traceLog.Error("ERROR: Trying to add BarPlot node to Root. This operation can not be performed!");
                    }
                    else
                    {
                        CurrentNode.AddVisualChild(barPlot);
                        Model.NumberOfModules++;
                    }
                    break;
                case "CorrelationHeatmap":
                    VisualizationModules.clsCorrelationHeatmap corrPlot = new VisualizationModules.clsCorrelationHeatmap(Model, s_RInstance);
                    corrPlot.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        traceLog.Error("ERROR: Trying to add Correlation Heatmap node to Root. This operation can not be performed!");
                    }
                    else
                    {
                        CurrentNode.AddVisualChild(corrPlot);
                        Model.NumberOfModules++;
                    }
                    break;
            }
        }
        #endregion
    }
}
