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

//using log4net;

namespace Cyclops.VisualizationModules
{
    /// <summary>
    /// This class holds the parameters used by all other Visualization classes 
    /// to create plots
    /// </summary>
    public class clsVisualizationGlobalParameters
    {
        //private static ILog traceLog = LogManager.GetLogger("TraceLog");
        private Dictionary<string, dynamic> d_Param = new Dictionary<string, dynamic>();
        
        private string s_ModuleName = "",
            s_TableName = "", s_xColumn = "", s_yColumn = "", s_Bins = "100",
            s_ImageType = "png", s_PlotFileName = "", s_Width = "480", s_Height = "480",
            s_Horizontal = "TRUE", s_PointSize = "12", s_WorkDir = "", s_xLab = "",
            s_yLab = "", s_Main = "", s_BackgroundColor = "white", s_AddRug = "TRUE",
            s_AddDist = "FALSE", s_FontSize = "12", s_HistType = "standard",
            s_BarColor = "89c6ff", s_AbsLogX = "", s_AbsLogY = "";

        private bool b_PlotDir = false;

        #region Constructors
        public clsVisualizationGlobalParameters()
        {
        }

        public clsVisualizationGlobalParameters(Dictionary<string, dynamic> Parameters)
        {
            d_Param = Parameters;
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
        public string AbsLogX
        {
            get { return s_AbsLogX; }
            set { s_AbsLogX = value; }
        }
        public string AbsLogY
        {
            get { return s_AbsLogY; }
            set { s_AbsLogY = value; }
        }
        public string HistogramType
        {
            get { return s_HistType; }
            set { s_HistType = value; }
        }
        public bool PlotDirectory
        {
            get { return b_PlotDir; }
            set { b_PlotDir = value; }
        }
        #endregion

        #region Methods
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
                        break;
                    case "yColumn":
                        yColumn = kvp.Value;
                        break;
                    case "bins":
                        Bins = kvp.Value;
                        break;
                    case "image":
                        ImageType = kvp.Value;
                        break;
                    case "workDir":
                        WorkDir = kvp.Value;
                        break;
                    case "plotFileName":
                        PlotFileName = kvp.Value;
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
                        break;
                    case "plotDir":
                        PlotDirectory = true ? kvp.Value.Equals("true") : false;
                        break;
                }
            }
        }
        #endregion
    }
}
