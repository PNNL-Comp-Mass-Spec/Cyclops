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
        private string s_OperationsDBPath = @"\\gigasax\DMS_Workflows\Cyclops\Cyclops_Operations.db3";
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
            switch (s_Module)
            {
                case "Aggregate":
                    DataModules.clsAggregate aggregate = new DataModules.clsAggregate(Model, s_RInstance);
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
                case "FilterByTable":
                    DataModules.clsFilterByAnotherTable filterByTable = new DataModules.clsFilterByAnotherTable(Model, s_RInstance);
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
                case "LinearRegression":
                    DataModules.clsLinearRegression linreg = new DataModules.clsLinearRegression(Model, s_RInstance);
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
                case "PValueAdjust":
                    DataModules.clsPValueAdjust pvaladj = new DataModules.clsPValueAdjust(Model, s_RInstance);
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
                case "LBF_Summary":
                    ExportModules.clsLBF_Summary_HTML lbfSummary = new ExportModules.clsLBF_Summary_HTML(Model, s_RInstance);
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
                case "BoxPlot":
                    VisualizationModules.clsBoxPlot box = new VisualizationModules.clsBoxPlot(Model, s_RInstance);
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