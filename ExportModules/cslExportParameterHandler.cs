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

namespace Cyclops.ExportModules
{
    /// <summary>
    /// This class holds the parameters used by all Export classes 
    /// </summary>
    public class cslExportParameterHandler
    {
        #region Members
        //private static ILog traceLog = LogManager.GetLogger("TraceLog");
        private Dictionary<string, dynamic> d_Param = new Dictionary<string, dynamic>();

        private string
            s_CanvasHeight="600",
            s_DatabaseName="Results.db3",
            s_FileName = "",
            s_FontSize="20",
            s_HeaderFontSize="40",   
            s_HeatmapFileName="",
            s_Height = "700",
            s_IncludeHeatmap = "TRUE",
            s_IncludeRowNames = "true",            
            s_Margin="30",
            s_ModuleName = "", 
            s_NewTableName = "",
            s_OverlapHeight="50",
            s_RectHeight="50",
            s_RownamesColumnHeader = "", 
            s_SeparatingCharacter = "",
            s_Source = "", 
            s_TableName = "", 
            s_Width = "850",
            s_WorkDir = "";

        private bool b_HasSource = false, b_HasTableName = false,
            b_HasFileName = false, b_HasWorkDir = false, b_HasNewTableName = false;
        #endregion

        #region Constructors
        /// Basic constructor
        public cslExportParameterHandler()
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Parameters">Dictionary of Visualization Parameters</param>
        public cslExportParameterHandler(Dictionary<string, dynamic> Parameters)
        {
            d_Param = Parameters;
            GetParameters();
        }
        #endregion

        #region Properties
        public string CanvasHeight
        {
            get { return s_CanvasHeight; }
            set { s_CanvasHeight = value; }
        }
        public string DatabaseName
        {
            get { return s_DatabaseName; }
            set { s_DatabaseName = value; }
        }
        public string FileName
        {
            get { return s_FileName; }
            set { s_FileName = value; }
        }
        public string FontSize
        {
            get { return s_FontSize; }
            set { s_FontSize = value; }
        }
        public bool HasFileName
        {
            get
            {
                return FileName.Length > 0 ? true : false;
            }
        }
        public bool HasNewTableName
        {
            get
            {
                return NewTableName.Length > 0 ? true : false;
            }
        }
        public bool HasSource
        {
            get
            {
                return Source.Length > 0 ? true : false;
            }
        }

        public bool HasTableName
        {
            get
            {
                return TableName.Length > 0 ? true : false;
            }
        }
        public bool HasWorkDirectory
        {
            get
            {
                return WorkDirectory.Length > 0 ? true : false;
            }
        }
        public string HeaderFontSize
        {
            get { return s_HeaderFontSize; }
            set { s_HeaderFontSize = value; }
        }
        public string HeatmapFileName
        {
            get { return s_HeatmapFileName; }
            set { s_HeatmapFileName = value; }
        }
        public string Height
        {
            get { return s_Height; }
            set { s_Height = value; }
        }
        public bool IncludeHeatmap
        {
            get { return s_IncludeHeatmap.ToUpper().Equals("TRUE") ? true : false; }
        }
        public bool IncludeRowNames
        {
            get
            {
                return s_IncludeRowNames.ToLower().Equals("true") ? true : false;
            }
        }
        public string Margin
        {
            get { return s_Margin; }
            set { s_Margin = value; }
        }
        public string NewTableName
        {
            get { return s_NewTableName; }
            set { s_NewTableName = value; }
        }
        public string OverlapHeight
        {
            get { return s_OverlapHeight; }
            set { s_OverlapHeight = value; }
        }
        public string RectangleHeight
        {
            get { return s_RectHeight; }
            set { s_RectHeight = value; }
        }
        public string RownamesColumnHeader
        {
            get
            {
                return s_RownamesColumnHeader;
            }
            set
            {
                s_RownamesColumnHeader = value;
            }
        }
        public string SeparatingCharacter
        {
            get
            {
                return s_SeparatingCharacter;
            }
            set
            {
                s_SeparatingCharacter = value;
            }
        }
        /// <summary>
        /// Describes where the data is coming from, e.g. SQLite, CSV, TXT, etc.
        /// </summary>
        public string Source
        {
            get { return s_Source; }
            set { s_Source = value; }
        }

        public string TableName
        {
            get { return s_TableName; }
            set { s_TableName = value; }
        }
        public string Width
        {
            get { return s_Width; }
            set { s_Width = value; }
        }
        public string WorkDirectory
        {
            get { return s_WorkDir; }
            set { s_WorkDir = value; }
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
            foreach (KeyValuePair<string, dynamic> kvp in d_Param)
            {
                switch (kvp.Key)
                {
                    case "canvasHeight":
                        CanvasHeight = kvp.Value;
                        break;
                    case "databaseName":
                        DatabaseName = kvp.Value;
                        break;
                    case "fileName":
                        FileName = kvp.Value;
                        if (FileName.Length > 0)
                            b_HasFileName = true;
                        break;
                    case "fontSize":
                        FontSize = kvp.Value;
                        break;
                    case "headerFontSize":
                        HeaderFontSize = kvp.Value;
                        break;
                    case "heatmapFileName":
                        HeatmapFileName = kvp.Value;
                        break;
                    case "height":
                        Height = kvp.Value;
                        break;
                    case "includeHeatmap":
                        s_IncludeRowNames = kvp.Value;
                        break;
                    case "includeRowNames":
                        s_IncludeRowNames = kvp.Value;
                        break;
                    case "margin":
                        Margin = kvp.Value;
                        break;
                    case "newTableName":
                        NewTableName = kvp.Value;
                        if (NewTableName.Length > 0)
                            b_HasNewTableName = true;
                        break;
                    case "overlapHeight":
                        OverlapHeight = kvp.Value;
                        break;
                    case "rectHeight":
                        RectangleHeight = kvp.Value;
                        break;
                    case "rowNamesColumnHeader":
                        RownamesColumnHeader = kvp.Value;
                        break;
                    case "separatingCharacter":
                        SeparatingCharacter = kvp.Value;
                        break;
                    case "source":
                        Source = kvp.Value;
                        if (Source.Length > 0)
                            b_HasSource = true;
                        break;
                    case "tableName":
                        TableName = kvp.Value;
                        if (TableName.Length > 0)
                            b_HasTableName = true;
                        break;
                    case "width":
                        Width = kvp.Value;
                        break;
                    case "workDir":
                        WorkDirectory = kvp.Value.Replace('\\', '/');
                        if (WorkDirectory.Length > 0)
                            b_HasWorkDir = true;
                        break;
                }
            }
        }

        #endregion
    }
}
