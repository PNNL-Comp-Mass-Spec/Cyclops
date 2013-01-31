﻿/* Written by Joseph N. Brown
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
    public class clsiTRAQMainOperation : clsBaseOperationModule
    {
        #region Members
        protected string s_RInstance;
        public enum ItqTypes { Log2, MainAnovaPractice };
        private string s_OperationsDBPath = @"\\gigasax\DMS_Workflows\Cyclops\Cyclops_Operations.db3";
        private string[] s_iTRAQTableNames = new string[] {
            "T_iTRAQ_PipelineOperation", "T_iTRAQ_MainAnovaPractice" };
        private string s_iTRAQTableName = "T_iTRAQ_PipelineOperation";
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Constructors
        /// <summary>
        /// Operation performs a general iTRAQ analysis
        /// </summary>
        public clsiTRAQMainOperation()
        {
            ModuleName = "iTRAQ Pipeline Operation";
        }

        /// <summary>
        /// Operation performs a general iTRAQ analysis
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsiTRAQMainOperation(string InstanceOfR)
        {
            ModuleName = "iTRAQ Pipeline Operation";
            s_RInstance = InstanceOfR;
        }

        /// <summary>
        /// Operation performs a general iTRAQ analysis
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsiTRAQMainOperation(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            Model = TheCyclopsModel;
            ModuleName = "iTRAQ Pipeline Operation";
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties
        public string OperationsDBPath
        {
            get { return s_OperationsDBPath; }
            set { s_OperationsDBPath = value; }
        }
        public DataModules.clsBaseDataModule Root { get; set; }
        public DataModules.clsBaseDataModule CurrentNode { get; set; }
        public VisualizationModules.clsBaseVisualizationModule CurrentVixNode { get; set; }
        public ExportModules.clsBaseExportModule CurrentExportNode { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Runs operation
        /// </summary>
        public override void PerformOperation()
        {
            traceLog.Info("Running iTRAQ Operation...");

            ConstructModules();

            RunModules();
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
                string.Format("SELECT * FROM {0}", s_iTRAQTableName),
                s_OperationsDBPath);

            StepValueNode svn = GetMaximumStepValueInOperationsTable(dt_Modules);

            for (int i = svn.MinimumValue; i < svn.MaximumValue + 1; i++)
            {
                try
                {
                    string stepExp = string.Format("Step = {0}", i);
                    DataRow[] dr_Rows = dt_Modules.Select(stepExp);

                    SetModule(dr_Rows);
                }
                catch (Exception exc)
                {
                    traceLog.Error("ERROR ITQ-Operation Setting Module: (Module number: " + i + "):" + exc.ToString());
                }
            }
        }

        /// <summary>
        /// Sets the type of general ITQ analysis to perform, defaults to Log2
        /// </summary>
        /// <param name="TypeOfAnalysis"></param>
        public void SetType(ItqTypes TypeOfAnalysis)
        {
            switch (TypeOfAnalysis)
            {
                case ItqTypes.Log2:
                    s_iTRAQTableName = s_iTRAQTableNames[(int)ItqTypes.Log2];
                    break;
                case ItqTypes.MainAnovaPractice:
                    s_iTRAQTableName = s_iTRAQTableNames[(int)ItqTypes.MainAnovaPractice];
                    break;
            }
        }

        private void SetModule(DataRow[] Rows)
        {
            if (Rows.Length < 1)
            {
                // There is an error
                traceLog.Error("ERROR Label-free Main Operation: Received empty Rows #SetModule!");
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

        #region Add Data Modules
        /// <summary>
        /// Adds a Data Module to the pipeline.
        /// </summary>
        /// <param name="Rows"></param>
        private void AddDataModule(DataRow[] Rows)
        {
            string s_Module = Rows[0]["Module"].ToString();
            int i_StepNumber = Int32.Parse(Rows[0]["Step"].ToString());
            switch (s_Module)
            {
                case "Aggregate":
                    DataModules.clsAggregate aggregate = new DataModules.clsAggregate(Model, s_RInstance);
                    aggregate.StepNumber = i_StepNumber;
                    aggregate.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = aggregate;
                        CurrentNode = aggregate;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(aggregate);
                        CurrentNode = aggregate;
                        Model.NumberOfModules++;
                    }
                    break;
                case "Anova":
                    DataModules.clsANOVA anova = new DataModules.clsANOVA(Model, s_RInstance);
                    anova.StepNumber = i_StepNumber;
                    anova.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = anova;
                        CurrentNode = anova;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(anova);
                        CurrentNode = anova;
                        Model.NumberOfModules++;
                    }
                    break;
                case "CentralTendency":
                    DataModules.clsCentralTendency centralTend = new DataModules.clsCentralTendency(Model, s_RInstance);
                    centralTend.StepNumber = i_StepNumber;
                    centralTend.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = centralTend;
                        CurrentNode = centralTend;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(centralTend);
                        CurrentNode = centralTend;
                        Model.NumberOfModules++;
                    }
                    break;
                case "CleanDataAndColumnFactors":
                    DataModules.clsCleanDataAndColumnFactors cleanUp = new DataModules.clsCleanDataAndColumnFactors(Model, s_RInstance);
                    cleanUp.StepNumber = i_StepNumber;
                    cleanUp.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = cleanUp;
                        CurrentNode = cleanUp;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(cleanUp);
                        CurrentNode = cleanUp;
                        Model.NumberOfModules++;
                    }
                    break;
                case "FilterByPeptideProteinCount":
                    DataModules.clsFilterByPeptideProteinCount filterByPepProtCnt = new DataModules.clsFilterByPeptideProteinCount(Model, s_RInstance);
                    filterByPepProtCnt.StepNumber = i_StepNumber;
                    filterByPepProtCnt.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = filterByPepProtCnt;
                        CurrentNode = filterByPepProtCnt;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(filterByPepProtCnt);
                        CurrentNode = filterByPepProtCnt;
                        Model.NumberOfModules++;
                    }
                    break; 
                case "FilterByTable":
                    DataModules.clsFilterByAnotherTable filterByTable = new DataModules.clsFilterByAnotherTable(Model, s_RInstance);
                    filterByTable.StepNumber = i_StepNumber;
                    filterByTable.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = filterByTable;
                        CurrentNode = filterByTable;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(filterByTable);
                        CurrentNode = filterByTable;
                        Model.NumberOfModules++;
                    }
                    break;
                case "FoldChange":
                    DataModules.clsFoldChange fc = new DataModules.clsFoldChange(Model, s_RInstance);
                    fc.StepNumber = i_StepNumber;
                    fc.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = fc;
                        CurrentNode = fc;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(fc);
                        CurrentNode = fc;
                        Model.NumberOfModules++;
                    }
                    break;
                case "Kbase":
                    DataModules.clsKBaseFormat kbase = new DataModules.clsKBaseFormat(Model, s_RInstance);
                    kbase.StepNumber = i_StepNumber;
                    kbase.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = kbase;
                        CurrentNode = kbase;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(kbase);
                        CurrentNode = kbase;
                        Model.NumberOfModules++;
                    }
                    break;
                case "LinearRegression":
                    DataModules.clsLinearRegression linreg = new DataModules.clsLinearRegression(Model, s_RInstance);
                    linreg.StepNumber = i_StepNumber;
                    linreg.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = linreg;
                        CurrentNode = linreg;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(linreg);
                        CurrentNode = linreg;
                        Model.NumberOfModules++;
                    }
                    break;
                case "LoadRSourceFiles":
                    DataModules.clsRSourceFileModule load = new DataModules.clsRSourceFileModule(Model, s_RInstance);
                    load.StepNumber = i_StepNumber;
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
                case "LoadRWorkspace":
                    DataModules.clsLoadRWorkspaceModule source = new DataModules.clsLoadRWorkspaceModule(Model, s_RInstance);
                    source.StepNumber = i_StepNumber;
                    source.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = source;
                        CurrentNode = source;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(source);
                        CurrentNode = source;
                        Model.NumberOfModules++;
                    }
                    break;
                case "Merge":
                    DataModules.clsMerge merge = new DataModules.clsMerge(Model, s_RInstance);
                    merge.StepNumber = i_StepNumber;
                    merge.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = merge;
                        CurrentNode = merge;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(merge);
                        CurrentNode = merge;
                        Model.NumberOfModules++;
                    }
                    break;
                case "Import":
                    DataModules.clsImportDataModule import = new DataModules.clsImportDataModule(Model, s_RInstance);
                    import.StepNumber = i_StepNumber;
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
                    mca.StepNumber = i_StepNumber;
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
                    protProph.StepNumber = i_StepNumber;
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
                case "PValueAdjust":
                    DataModules.clsPValueAdjust pvaladj = new DataModules.clsPValueAdjust(Model, s_RInstance);
                    pvaladj.StepNumber = i_StepNumber;
                    pvaladj.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = pvaladj;
                        CurrentNode = pvaladj;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(pvaladj);
                        CurrentNode = pvaladj;
                        Model.NumberOfModules++;
                    }
                    break;
                case "RMD":
                    DataModules.clsRMD rmd = new DataModules.clsRMD(Model, s_RInstance);
                    rmd.StepNumber = i_StepNumber;
                    rmd.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = rmd;
                        CurrentNode = rmd;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(rmd);
                        CurrentNode = rmd;
                        Model.NumberOfModules++;
                    }
                    break;
                case "RRollup":
                    DataModules.clsRRollup rrollup = new DataModules.clsRRollup(Model, s_RInstance);
                    rrollup.StepNumber = i_StepNumber;
                    rrollup.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = rrollup;
                        CurrentNode = rrollup;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(rrollup);
                        CurrentNode = rrollup;
                        Model.NumberOfModules++;
                    }
                    break;
                case "SummarizeData":
                    DataModules.clsSummarizeData summarizeData = new DataModules.clsSummarizeData(Model, s_RInstance);
                    summarizeData.StepNumber = i_StepNumber;
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
                case "Transform":
                    DataModules.clsTransformModule transform = new DataModules.clsTransformModule(Model, s_RInstance);
                    transform.StepNumber = i_StepNumber;
                    transform.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        Root = transform;
                        CurrentNode = transform;
                        Model.NumberOfModules++;
                    }
                    else
                    {
                        CurrentNode.AddDataChild(transform);
                        CurrentNode = transform;
                        Model.NumberOfModules++;
                    }
                    break;
                case "Clean":
                    DataModules.clsCleanUpRSourceFileObjects clean = new DataModules.clsCleanUpRSourceFileObjects(Model, s_RInstance);
                    clean.StepNumber = i_StepNumber;
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
            }
        }
        #endregion

        #region Add Export Modules
        /// <summary>
        /// Adds an Export Module to the pipeline.
        /// </summary>
        /// <param name="Rows"></param>
        private void AddExportModule(DataRow[] Rows)
        {
            string s_Module = Rows[0]["Module"].ToString();
            int i_StepNumber = Int32.Parse(Rows[0]["Step"].ToString());
            switch (s_Module)
            {
                case "ExportTable":
                    ExportModules.clsExportTableModule exportTable = new ExportModules.clsExportTableModule(Model, s_RInstance);
                    exportTable.StepNumber = i_StepNumber;
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
                    save.StepNumber = i_StepNumber;
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
                case "LBF_Summary":
                    ExportModules.clsLBF_Summary_HTML lbfSummary = new ExportModules.clsLBF_Summary_HTML(Model, s_RInstance);
                    lbfSummary.StepNumber = i_StepNumber;
                    lbfSummary.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        traceLog.Error("ERROR: Trying to add LBF Summary node to Root. This operation can not be performed!");
                    }
                    else
                    {
                        CurrentNode.AddExportChild(lbfSummary);
                        Model.NumberOfModules++;
                    }
                    break;
            }
        }
        #endregion

        #region Add Visual Modules
        /// <summary>
        /// Adds a Visual Module to the pipeline.
        /// </summary>
        /// <param name="Rows"></param>
        private void AddVisualModules(DataRow[] Rows)
        {
            string s_Module = Rows[0]["Module"].ToString();
            int i_StepNumber = Int32.Parse(Rows[0]["Step"].ToString());
            switch (s_Module)
            {
                case "Histogram":
                    VisualizationModules.clsHistogram histogram = new VisualizationModules.clsHistogram(s_RInstance);
                    histogram.StepNumber = i_StepNumber;
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
                case "Heatmap":
                    VisualizationModules.clsHeatmap heatMap = new VisualizationModules.clsHeatmap(Model, s_RInstance);
                    heatMap.StepNumber = i_StepNumber;
                    heatMap.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        traceLog.Error("ERROR: Trying to add Heatmap node to Root. This operation can not be performed!");
                    }
                    else
                    {
                        CurrentNode.AddVisualChild(heatMap);
                        Model.NumberOfModules++;
                    }
                    break;
                case "Hexbin":
                    VisualizationModules.clsHexbin hexbin = new VisualizationModules.clsHexbin(Model, s_RInstance);
                    hexbin.StepNumber = i_StepNumber;
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
                    barPlot.StepNumber = i_StepNumber;
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
                case "BoxPlot":
                    VisualizationModules.clsBoxPlot box = new VisualizationModules.clsBoxPlot(Model, s_RInstance);
                    box.StepNumber = i_StepNumber;
                    box.Parameters = GetParameters(Rows);
                    if (Root == null)
                    {
                        traceLog.Error("ERROR: Trying to add Boxplot node to Root. This operation can not be performed!");
                    }
                    else
                    {
                        traceLog.Info("Adding Boxplot module from XML...");
                        CurrentNode.AddVisualChild(box);
                        Model.NumberOfModules++;
                    }
                    break;
                case "CorrelationHeatmap":
                    VisualizationModules.clsCorrelationHeatmap corrPlot = new VisualizationModules.clsCorrelationHeatmap(Model, s_RInstance);
                    corrPlot.StepNumber = i_StepNumber;
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

        /// <summary>
        /// Runs the assembled modules
        /// </summary>
        private void RunModules()
        {
            Root.PerformOperation();
        }
        #endregion
    }
}
