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
using System.Text;
using System.IO;

//using log4net;

namespace Cyclops.VisualizationModules
{
    /// <summary>
    /// This class holds the parameters used by all other Visualization classes 
    /// to create plots
    /// </summary>
    public class clsVisualizationParameterHandler
    {
        #region Variables
        //private static ILog traceLog = LogManager.GetLogger("TraceLog");
        private Dictionary<string, dynamic> d_Param = new Dictionary<string, dynamic>();

        private string
            s_AddDist = "FALSE",
            s_AddRug = "TRUE",
            s_BackgroundColor = "white",
            s_BarColor = "cornflowerblue",
            s_Bins = "100",
            s_BoxWidth = "1",
            s_Color = "cornflowerblue",
            s_ColorByFactor = "FALSE",
            s_ColumnFactorTable = "",
            s_ConsolidationFactor = "",
            s_DataColumns = "",
            s_DoYLim = "FALSE",
            s_Factor = "1",
            s_FatorTable = "",
            s_FileName = "",
            s_FixedEffect = "",
            s_FontSize = "12",
            s_HeatmapClusterFun = "hclust2",
            s_HeatmapColDendrogram = "FALSE",
            s_HeatmapColors = "c(\"green\", \"black\", \"red\")",
            s_HeatmapColorScaleDegree = "20",
            s_HeatmapColSideColors = "",
            s_HeatmapDist = "dist2",
            s_HeatmapDrawDendrogram = "both",
            s_HeatmapRmNA = "TRUE",
            s_HeatmapRowDendrogram = "TRUE",
            s_HeatmapScale = "row",
            s_HeatmapScaleMax = "1",
            s_HeatmapScaleMin = "-1",
            s_HeatmapSymm = "FALSE",
            s_HeatmapTrace = "none",
            s_Height = "1200",
            s_HistType = "standard",
            s_Horizontal = "TRUE",
            s_ImageType = "png",
            s_LabelScale = "0.8",
            s_Main = "",
            s_ModuleName = "",
            s_Outliers = "TRUE",
            s_PlotFileName = "",
            s_PointSize = "12",
            s_Resolution = "600",
            s_RowFactorTable = "",
            s_ShowCount = "TRUE",
            s_ShowLegend = "TRUE",
            s_Stamp = "NULL",
            s_TableName = "",
            s_Width = "1200",
            s_WorkDir = "",
            s_xColumn = "",
            s_xLab = "",
            s_yColumn = "",
            s_yLab = "",
            s_yMin = "NULL",
            s_yMax = "NULL";            

        private bool b_PlotDir = false, b_HasTableName = false, b_HasPlotFileName = false,
            b_HasXcolumn = false, b_HasYcolumn = false, b_AbsLogX = false, 
            b_AbsLogY = false, b_HasWorkDir = false, b_HasImageType = false,
            b_HasHistogramType = false, b_HasDataColumns = false;
        #endregion

        #region Constructors
        /// Basic constructor
        public clsVisualizationParameterHandler()
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Parameters">Dictionary of Visualization Parameters</param>
        public clsVisualizationParameterHandler(Dictionary<string, dynamic> Parameters)
        {
            d_Param = Parameters;
            GetParameters();
        }
        #endregion

        #region Properties
        public Dictionary<string, dynamic> Parameters
        {
            set { d_Param = value; }
        }
        public string TableName
        {
            get { return s_TableName; }
            set { s_TableName = value; }
        }
        public string PlotFileName
        {
            get { return s_PlotFileName; }
            set { s_PlotFileName = value; }
        }
        /// <summary>
        /// Width of box in plot, defaults to 1
        /// </summary>
        public string BoxWidth
        {
            get { return s_BoxWidth; }
            set { s_BoxWidth = value; }
        }
        /// <summary>
        /// Consolidation Factor passed in through the ATM
        /// </summary>
        public string ConsolidationFactor
        {
            get { return s_ConsolidationFactor; }
            set { s_ConsolidationFactor = value; }
        }
        /// <summary>
        /// Index for Factor, defaults to 1
        /// </summary>
        public string Factor 
        {
            get { return s_Factor; }
            set { s_Factor = value; }
        }
        /// <summary>
        /// Fixed Effect Factor passed in through the ATM
        /// </summary>
        public string FixedEffect
        {
            get { return s_FixedEffect; }
            set { s_FixedEffect = value; }
        }
        /// <summary>
        /// Color used in graph/plot, defaults to cornflowerblue
        /// </summary>
        public string Color
        {
            get { return s_Color; }
            set { s_Color = value; }
        }
        /// <summary>
        /// Whether to color by the factor info, defaults to FALSE
        /// </summary>
        public string ColorByFactor
        {
            get { return s_ColorByFactor; }
            set { s_ColorByFactor = value; }
        }
        /// <summary>
        /// Name of the Column Factor table
        /// </summary>
        public string ColumnFactorTable
        {
            get { return s_ColumnFactorTable; }
            set { s_ColumnFactorTable = value; }
        }
        /// <summary>
        /// Use y limits, defaults to FALSE
        /// </summary>
        public string DoYLim
        {
            get { return s_DoYLim; }
            set { s_DoYLim = value; }
        }
        /// <summary>
        /// Name to save the plot file to
        /// </summary>
        public string FileName
        {
            get { return s_FileName; }
            set { s_FileName = value; }
        }
        public string xColumn
        {
            get { return s_xColumn; }
            set { s_xColumn = value; }
        }
        public string yColumn
        {
            get { return s_yColumn; }
            set { s_yColumn = value; }
        }
        public string Bins
        {
            get { return s_Bins; }
            set { s_Bins = value; }
        }
        public string ImageType
        {
            get { return s_ImageType; }
            set { s_ImageType = value; }
        }
        public string Width
        {
            get { return s_Width; }
            set { s_Width = value; }
        }
        public string Height
        {
            get { return s_Height; }
            set { s_Height = value; }
        }
        public string Horizontal
        {
            get { return s_Horizontal; }
            set { s_Horizontal = value; }
        }
        public string PointSize
        {
            get { return s_PointSize; }
            set { s_PointSize = value; }
        }
        /// <summary>
        /// Scale for labels, defaults to 0.8
        /// </summary>
        public string LabelScale
        {
            get { return s_LabelScale; }
            set { s_LabelScale = value; }
        }
        public string FontSize
        {
            get { return s_FontSize; }
            set { s_FontSize = value; }
        }
        public string WorkDir
        {
            get { return s_WorkDir; }
            set { s_WorkDir = value; }
        }
        public string xLabel
        {
            get { return s_xLab; }
            set { s_xLab = value; }
        }
        public string yLabel
        {
            get { return s_yLab; }
            set { s_yLab = value; }
        }
        /// <summary>
        /// Whether to show counts, defaults to TRUE
        /// </summary>
        public string ShowCount
        {
            get { return s_ShowCount; }
            set { s_ShowCount = value; }
        }
        /// <summary>
        /// Whether to show a legend or not, defaults to TRUE
        /// </summary>
        public string ShowLegend
        {
            get { return s_ShowLegend; }
            set { s_ShowLegend = value; }
        }
        /// <summary>
        /// Whether to include outliers, defaults to TRUE
        /// </summary>
        public string Outliers
        {
            get { return s_Outliers; }
            set { s_Outliers = value; }
        }
        public string Main
        {
            get { return s_Main; }
            set { s_Main = value; }
        }
        public string BackgroundColor
        {
            get { return s_BackgroundColor; }
            set { s_BackgroundColor = value; }
        }
        public string BarColor
        {
            get { return s_BarColor; }
            set { s_BarColor = value; }
        }
        public string AddRug
        {
            get { return s_AddRug; }
            set { s_AddRug = value; }
        }
        public string AddDist
        {
            get { return s_AddDist; }
            set { s_AddDist = value; }
        }
        public string HistogramType
        {
            get { return s_HistType; }
            set { s_HistType = value; }
        }
        public string DataColumns
        {
            get { return s_DataColumns; }
            set { s_DataColumns = value; }
        }
        public string HeatmapColors
        {
            get { return s_HeatmapColors; }
            set { s_HeatmapColors = value; }
        }
        public string HeatmapScaleMin
        {
            get { return s_HeatmapScaleMin; }
            set { s_HeatmapScaleMin = value; }
        }
        public string HeatmapScaleMax
        {
            get { return s_HeatmapScaleMax; }
            set { s_HeatmapScaleMax = value; }
        }
        public string HeatmapDist
        {
            get { return s_HeatmapDist; }
            set { s_HeatmapDist = value; }
        }
        public string HeatmapClusterFunction
        {
            get { return s_HeatmapClusterFun; }
            set { s_HeatmapClusterFun = value; }
        }
        public string HeatmapRowDendrogram
        {
            get { return s_HeatmapRowDendrogram; }
            set { s_HeatmapRowDendrogram = value; }
        }
        public string HeatmapColDendrogram
        {
            get { return s_HeatmapColDendrogram; }
            set { s_HeatmapColDendrogram = value; }
        }
        public string HeatmapDrawDendrogram
        {
            get { return s_HeatmapDrawDendrogram; }
            set { s_HeatmapDrawDendrogram = value; }
        }
        public string HeatmapScale
        {
            get { return s_HeatmapScale; }
            set { s_HeatmapScale = value; }
        }
        public string HeatmapRemoveNA
        {
            get { return s_HeatmapRmNA; }
            set { s_HeatmapRmNA = value; }
        }
        public string HeatmapSymm
        {
            get { return s_HeatmapSymm; }
            set { s_HeatmapSymm = value; }
        }
        public string HeatmapTrace
        {
            get { return s_HeatmapTrace; }
            set { s_HeatmapTrace = value; }
        }
        public string HeatmapColSideColors
        {
            get { return s_HeatmapColSideColors; }
            set { s_HeatmapColSideColors = value; }
        }
        public string HeatmapColorScaleDegree
        {
            get { return s_HeatmapColorScaleDegree; }
            set { s_HeatmapColorScaleDegree = value; }
        }
        public bool PlotDirectory
        {
            get { return b_PlotDir; }
            set { b_PlotDir = value; }
        }

        public bool HasTableName
        {
            get { return b_HasTableName; }
            set { b_HasTableName = value; }
        }
        public bool HasPlotFileName
        {
            get { return b_HasPlotFileName; }
            set { b_HasPlotFileName = value; }
        }
        public bool HasXcolumn
        {
            get { return b_HasXcolumn; }
            set { b_HasXcolumn = value; }
        }
        public bool HasYcolumn
        {
            get { return b_HasYcolumn; }
            set { b_HasYcolumn = value; }
        }
        public bool HasWorkDir
        {
            get { return b_HasWorkDir; }
            set { b_HasWorkDir = value; }
        }
        public bool HasImageType
        {
            get { return b_HasImageType; }
            set { b_HasImageType = value; }
        }
        public bool HasHistogramType
        {
            get { return b_HasHistogramType; }
            set { b_HasHistogramType = value; }
        }
        public bool HasDataColumns
        {
            get { return b_HasDataColumns; }
            set { b_HasDataColumns = value; }
        }
        public bool AbsLogX
        {
            get { return b_AbsLogX; }
            set { b_AbsLogX = value; }
        }
        public bool AbsLogY
        {
            get { return b_AbsLogY; }
            set { b_AbsLogY = value; }
        }
        /// <summary>
        /// Defaults to NULL
        /// </summary>
        public string Stamp
        {
            get { return s_Stamp; }
            set { s_Stamp = value; }
        }
        /// <summary>
        /// Resolution of the image, default 600
        /// </summary>
        public string Resolution
        {
            get { return s_Resolution; }
            set { s_Resolution = value; }
        }
        /// <summary>
        /// Name of the Row Factor table
        /// </summary>
        public string RowFactorTable
        {
            get { return s_RowFactorTable; }
            set { s_RowFactorTable = value; }
        }
        /// <summary>
        /// Y minimum
        /// </summary>
        public string yMin
        {
            get { return s_yMin; }
            set { s_yMin = value; }
        }
        /// <summary>
        /// Y maximum
        /// </summary>
        public string yMax
        {
            get { return s_yMax; }
            set { s_yMax = value; }
        }
        #endregion

        #region Methods
        public void GetParameters()
        {
            if (d_Param.Count > 0)
            {
                SetValues();
            }
        }
        public void GetParameters(string ModuleName)
        {
            s_ModuleName = ModuleName;
            if (d_Param.Count > 0)
            {
                SetValues();
            }
        }
        public void GetParameters(string ModuleName,
            Dictionary<string, dynamic> Parameters)
        {
            s_ModuleName = ModuleName;
            d_Param = Parameters;
            if (d_Param.Count > 0)
            {
                SetValues();
            }
        }

        private void SetValues()
        {
            foreach(KeyValuePair<string, dynamic> kvp in d_Param)
            {
                switch (kvp.Key)
                {
                    case "tableName":
                        TableName = kvp.Value;
                        if (TableName.Length > 0)
                            HasTableName = true;
                        break;
                    case "boxWidth":
                        BoxWidth = kvp.Value;
                        break;
                    case "colorByFactor":
                        ColorByFactor = kvp.Value;
                        break;
                    case "columnFactorTable":
                        ColumnFactorTable = kvp.Value;
                        break;
                    case "Consolidation_Factor":
                        ConsolidationFactor = kvp.Value;
                        break;
                    case "doYLim":
                        DoYLim = kvp.Value;
                        break;
                    case "factor":
                        Factor = kvp.Value;
                        break;
                    case "factorTable":
                        break;
                    case "fileName":
                        FileName = kvp.Value;
                        break;
                    case "Fixed_Effect":
                        FixedEffect = kvp.Value;
                        break;
                    case "width":
                        Width = kvp.Value;
                        break;
                    case "height":
                        Height = kvp.Value;
                        break;
                    case "horizontal":
                        Horizontal = kvp.Value;
                        break;
                    case "pointsize":
                        PointSize = kvp.Value;
                        break;
                    case "xLab":
                        xLabel = kvp.Value;
                        break;
                    case "yLab":
                        yLabel = kvp.Value;
                        break;
                    case "main":
                        Main = kvp.Value;
                        break;
                    case "xColumn":
                        xColumn = kvp.Value;
                        if (xColumn.Length > 0)
                            HasXcolumn = true;
                        break;
                    case "yColumn":
                        yColumn = kvp.Value;
                        if (yColumn.Length > 0)
                            HasYcolumn = true;
                        break;
                    case "bins":
                        Bins = kvp.Value;
                        break;
                    case "image":
                        ImageType = kvp.Value;
                        if (ImageType.Length > 0)
                            HasImageType = true;
                        break;
                    case "workDir":
                        WorkDir = kvp.Value.Replace('\\', '/');
                        if (WorkDir.Length > 0)
                            HasWorkDir = true;
                        break;
                    case "plotFileName":
                        PlotFileName = kvp.Value;
                        if (PlotFileName.Length > 0)
                            HasPlotFileName = true;
                        break;
                    case "outliers": // default TRUE
                        s_Outliers = kvp.Value;
                        break;
                    case "backgroundColor":
                        BackgroundColor = kvp.Value;
                        break;
                    case "barColor":
                        BarColor = kvp.Value;
                        break;
                    case "fontSize":
                        FontSize = kvp.Value;
                        break;
                    case "hist_type":
                        HistogramType = kvp.Value;
                        if (HistogramType.Length > 0)
                            HasHistogramType = true;
                        break;
                    case "addRug":
                        AddRug = kvp.Value.ToUpper();
                        break;
                    case "addDist":
                        AddDist = kvp.Value.ToUpper();
                        break;
                    case "heatmapColors":
                        HeatmapColors = kvp.Value;
                        break;
                    case "heatmapScale":
                        HeatmapScale = kvp.Value;
                        break;
                    case "heatmapScaleMin":
                        HeatmapScaleMin = kvp.Value;
                        break;
                    case "heatmapScaleMax":
                        HeatmapScaleMax = kvp.Value;
                        break;
                    case "heatmapDist":
                        HeatmapDist = kvp.Value;
                        break;
                    case "heatmapClusterFunction":
                        HeatmapClusterFunction = kvp.Value;
                        break;
                    case "heatmapRowDengrogram":
                        HeatmapRowDendrogram = kvp.Value.ToUpper();
                        break;
                    case "heatmapColDendrogram":
                        HeatmapColDendrogram = kvp.Value.ToUpper();
                        break;
                    case "heatmapDrawDendrogram":
                        HeatmapDrawDendrogram = kvp.Value;
                        break;
                    case "heatmapRemoveNAs":
                        HeatmapRemoveNA = kvp.Value;
                        break;
                    case "heatmapSymm":
                        HeatmapSymm = kvp.Value;
                        break;
                    case "heatmapTrace":
                        HeatmapTrace = kvp.Value;
                        break;
                    case "heatmapColSideColors":
                        s_HeatmapColSideColors = kvp.Value;
                        break;
                    case "heatmapColorScaleDegree":
                        s_HeatmapColorScaleDegree = kvp.Value;
                        break;
                    case "labelScale": // default 0.8
                        s_LabelScale = kvp.Value;
                        break; 
                    case "dataColumns":
                        DataColumns = kvp.Value;
                        if (DataColumns.Length > 0)
                            HasDataColumns = true;
                        break;
                    case "plotDir":
                        string spd = kvp.Value;
                        PlotDirectory = true ? spd.ToLower().Equals("true") : false;
                        break;
                    case "absLogX":
                        string salx = kvp.Value;
                        b_AbsLogX = true ? salx.ToLower().Equals("true") : false;
                        break;
                    case "absLogY":
                        string saly = kvp.Value; 
                        b_AbsLogY = true ? saly.ToLower().Equals("true") : false;
                        break;
                    case "showCounts": // defaults to TRUE
                        ShowCount = kvp.Value;
                        break;
                    case "showLegend":  // defaults to TRUE
                        ShowLegend = kvp.Value;
                        break;
                    case "stamp": // defaults to NULL
                        Stamp = kvp.Value;
                        break;
                    case "resolution":
                        Resolution = kvp.Value;
                        break;
                    case "rowFactorTable":
                        RowFactorTable = kvp.Value;
                        break;
                    case "yMin":
                        yMin = kvp.Value;
                        break;
                    case "yMax":
                        yMax = kvp.Value;
                        break;
                }                
            }

            if (HasWorkDir & HasPlotFileName & b_PlotDir)
            {
                PlotFileName = WorkDir + "/Plots/" + Path.GetFileName(PlotFileName);
            }
            else if (HasWorkDir & HasPlotFileName)
            {
                PlotFileName = WorkDir + "/" + Path.GetFileName(PlotFileName);
            }

            if (HasTableName & HasDataColumns)
            {
                if (DataColumns.Equals("all"))
                {
                    // reformat Data.Columns
                    DataColumns = "c(1:ncol(" + TableName + "))";
                }
            }
        }

        public void ResetPlotFileName()
        {
            if (HasWorkDir & HasPlotFileName & b_PlotDir)
            {
                PlotFileName = WorkDir + "/Plots/" + Path.GetFileName(PlotFileName);
            }
            else if (HasWorkDir & HasPlotFileName)
            {
                PlotFileName = WorkDir + "/" + Path.GetFileName(PlotFileName);
            }

            if (HasTableName & HasDataColumns)
            {
                if (DataColumns.Equals("all"))
                {
                    // reformat Data.Columns
                    DataColumns = "c(1:ncol(" + TableName + "))";
                }
            }
        }
        #endregion
    }
}
