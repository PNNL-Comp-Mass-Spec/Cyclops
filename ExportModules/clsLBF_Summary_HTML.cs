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

namespace Cyclops.ExportModules
{
    public class clsLBF_Summary_HTML : clsBaseExportModule
    {
        #region Members
        private ExportModules.cslExportParameterHandler esp =
            new ExportModules.cslExportParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        private string s_LineDelimiter = "\n";
        private string s_Tab = "\t";
        private DataTable dt_Overlap = new DataTable("LBF");

        private string s_RInstance;
        #endregion

        #region Constructors
        /// <summary>
        /// Exports an HTML file that summarizes LBF Data Analysis
        /// </summary>
        public clsLBF_Summary_HTML()
        {
            ModuleName = "LBF Quality Control Module";
        }
        /// <summary>
        /// Exports an HTML file that summarizes LBF Data Analysis
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsLBF_Summary_HTML(string InstanceOfR)
        {
            ModuleName = "LBF Quality Control Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Exports an HTML file that summarizes LBF Data Analysis
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsLBF_Summary_HTML(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "LBF Quality Control Module";
            Model = TheCyclopsModel;
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties
        
        #endregion

        #region Methods
        /// <summary>
        /// Determine is all the necessary parameters are being passed to the object
        /// </summary>
        /// <returns>Returns true import module can proceed</returns>
        public bool CheckPassedParameters()
        {
            bool b_2Param = true;

            // NECESSARY PARAMETERS
            if (esp.FileName.Equals(""))
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR LBF Summary HTML: 'fileName' was not found in the passed parameters");
                b_2Param = false;
            }
            if (!esp.HasWorkDirectory)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR LBF Summary HTML: 'workDir' was not found in the passed parameters");
                b_2Param = false;
            }

            return b_2Param;
        }

        /// <summary>
        /// Runs module
        /// </summary>
        public override void PerformOperation()
        {
            traceLog.Info("Preparing to write LBF Summary HTML File...");

            BuildHtmlFile();
        }


        /// <summary>
        /// Construct the HTML file
        /// </summary>
        private void BuildHtmlFile()
        {
            esp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                // Builds the HTML file in StringBuilder
                StringBuilder sb_HTML = new StringBuilder();
                sb_HTML.Append(WriteHtmlHeader());
                sb_HTML.Append(WriteHtmlScripts());
                sb_HTML.Append(WriteEndHead());
                sb_HTML.Append(WriteHtmlBody());
                sb_HTML.Append(WriteEndHtml());


                // TODO : Write the html out to the file
                StreamWriter sw = new StreamWriter(Path.Combine(esp.WorkDirectory, esp.FileName));
                sw.Write(sb_HTML);
                sw.Close();

                WriteCSSFile();
            }
        }

        private string WriteHtmlHeader()
        {
            string s_HTML = "<HTML>" + s_LineDelimiter + s_Tab + "<HEAD>" + s_LineDelimiter;
            s_HTML += s_Tab + s_Tab + "<TITLE>Label-free Quantitative " +
                "Proteome Analysis Summary</TITLE>" + s_LineDelimiter;
            s_HTML += s_Tab + s_Tab + "<LINK rel='stylesheet' href='lbfstyle.css'>" +
                s_LineDelimiter;
            return s_HTML;
        }

        private void WriteCSSFile()
        {
            StreamWriter sw_CSS = new StreamWriter(Path.Combine(esp.WorkDirectory, "lbfstyle.css"));

            StringBuilder sb_CSS = new StringBuilder();
            sb_CSS.Append("ul#list-nav {" + s_LineDelimiter +
                s_Tab + "list-style:none;" + s_LineDelimiter +
                s_Tab + "margin:20px;" + s_LineDelimiter +
                s_Tab + "padding:0;" + s_LineDelimiter +
                s_Tab + "width:600px;" + s_LineDelimiter +
                s_Tab + "position:fixed;" + s_LineDelimiter +
                s_Tab + "left:50%;" + s_LineDelimiter +
                s_Tab + "margin-left:-300px;" + s_LineDelimiter +
                "}" + s_LineDelimiter + s_LineDelimiter);

            sb_CSS.Append("ul#list-nav li {" + s_LineDelimiter +
                s_Tab + "display:inline;" + s_LineDelimiter +
                s_Tab + "font-weight:bold;" + s_LineDelimiter +
                "}" + s_LineDelimiter + s_LineDelimiter);

            sb_CSS.Append("ul#list-nav li a {" + s_LineDelimiter +
                s_Tab + "text-decoration:none;" + s_LineDelimiter +
                s_Tab + "padding:5px 0;" + s_LineDelimiter +
                s_Tab + "width:180px;" + s_LineDelimiter +
                s_Tab + "background:#485e49;" + s_LineDelimiter +
                s_Tab + "color:#eee;" + s_LineDelimiter +
                s_Tab + "float:left;" + s_LineDelimiter +
                s_Tab + "text-align:center;" + s_LineDelimiter +
                s_Tab + "border-left:1px solid #fff;" + s_LineDelimiter +
                "}" + s_LineDelimiter + s_LineDelimiter);

            sb_CSS.Append("ul#list-nav li a:hover {" + s_LineDelimiter +
                s_Tab + "background:#a2b3a1;" + s_LineDelimiter +
                s_Tab + "color:#000" + s_LineDelimiter +
                "}" + s_LineDelimiter);

            sw_CSS.WriteLine(sb_CSS);
            sw_CSS.Close();
        }

        private StringBuilder WriteHtmlScripts()
        {
            StringBuilder sb_ReturnScripts = new StringBuilder();
            sb_ReturnScripts.Append(s_Tab + "<SCRIPT type='application/javascript'>" + s_LineDelimiter);

            sb_ReturnScripts.Append("</SCRIPT>" + s_LineDelimiter);
            return sb_ReturnScripts;
        }

        private string WriteEndHead()
        {
            string s_Head = s_Tab + "</HEAD>" + s_LineDelimiter;
            return s_Head;
        }

        private string WriteHtmlBody()
        {
            string s_Body = s_Tab + "<BODY>" + s_LineDelimiter;
            s_Body += s_Tab + "<A NAME='datasets'></A>";

            // Write out navigation bar
            s_Body += s_Tab + "<CENTER>" + s_LineDelimiter;
            s_Body += s_Tab + s_Tab + "<NAV>" + s_LineDelimiter;
            s_Body += s_Tab + s_Tab + s_Tab + "<UL ID='list-nav'>" + s_LineDelimiter;
            s_Body += s_Tab + s_Tab + s_Tab + s_Tab +
                "<LI><A HREF='#datasets'>Datasets</A></LI>" + s_LineDelimiter;
            s_Body += s_Tab + s_Tab + s_Tab + s_Tab + 
                "<LI><A HREF='#PepSummary'>Filtering Summary</A></LI>" + s_LineDelimiter;
            s_Body += s_Tab + s_Tab + s_Tab + s_Tab +
                "<LI><A HREF='#TranBox'>Transformation Boxplots</A></LI>" + s_LineDelimiter;            
            s_Body += s_Tab + s_Tab + s_Tab + "</UL>" + s_LineDelimiter;
            s_Body += s_Tab + s_Tab + "</NAV>" + s_LineDelimiter;
            s_Body += s_Tab + "</CENTER><BR><BR>" + s_LineDelimiter + s_LineDelimiter;


            // Write LBF DATASET INFORMATION
            s_Body += s_Tab + s_Tab + "<H2>Datasets used in Label-Free Quantitative Analysis:</H2>" + s_LineDelimiter;
            s_Body += s_Tab + s_Tab + "<TABLE><TR><TD>" + s_LineDelimiter;

            DataTable dt_Datasets = GetDatasetInfo();
            s_Body += WriteTable(dt_Datasets, "left", 0, 2, 4);

            s_Body += s_Tab + s_Tab + "</TD>";

            DataTable dt_Legend = GetLegendInfo();
            if (dt_Legend != null)
            {
                s_Body += "<TD valign='bottom'>" + s_LineDelimiter + s_Tab + s_Tab +
                    "<H2><A NAME='PepSummary'>Bar Plot Legend</A></H2>" + s_LineDelimiter + 
                    WriteTable(dt_Legend, "right", 0, 2, 4);
            }

            s_Body += s_Tab + s_Tab + "</TR>" + s_LineDelimiter;

            s_Body += s_Tab + s_Tab + "<TR><TD>" + s_LineDelimiter;
            s_Body += string.Format(s_Tab + s_Tab + 
                "<A HREF='Plots/{0}'>" + 
                "<IMG src='Plots/{0}' width='400' height='400' /></A>",
                "qc_Summary.png");
            s_Body += s_Tab + s_Tab + "</TD><TD>" + s_LineDelimiter;
            s_Body += string.Format(s_Tab + s_Tab + 
                "<A HREF='Plots/{0}'>" +
                "<IMG src='Plots/{0}' width='400' height='400' /></A>",
                "log_qc_Summary.png");
            s_Body += s_Tab + s_Tab + "</TD></TR>" + s_LineDelimiter;


            if (clsGenericRCalls.ContainsObject(s_RInstance, "T_MAC_LBF_Summary"))
            {
                DataTable dt_Summary = clsGenericRCalls.GetDataTable(s_RInstance, "T_MAC_LBF_Summary");
                s_Body += s_Tab + s_Tab + "<TR><TD><H2>Summary</H2>" + s_LineDelimiter;
                s_Body += WriteTable(dt_Summary, "left", 0, 2, 4) + s_Tab + s_Tab + "</TD></TR>";
            }

            s_Body += s_Tab + s_Tab + "<TR><TD>" + s_LineDelimiter;
            s_Body += s_Tab + s_Tab + "<B>Log2 Transformed</B></TD><TD><B>CT Log2 Transformed</B></TD>" +
                "</TR>" + s_LineDelimiter;
            s_Body += s_Tab + s_Tab + string.Format("<TR><TD>" +
                "<A NAME='TranBox' HREF='Plots/{0}'>" +
                "<IMG src='Plots/{0}' width='400' height='400' " +
                "/></A></TD><TD>" +
                "<A HREF='Plots/{1}'>" +
                "<IMG src='Plots/{1}' width='400' height='400' /></A>",
                "Boxplot_Log_T_Data.png",
                "Boxplot_CT_Log_T_Data.png");
            s_Body += "</TD></TR>" + s_LineDelimiter;


            string s_LR_BoxplotFile = "Plots/Boxplot_LR_Log_T_Data.png";
            if (File.Exists(s_LR_BoxplotFile))
            {
                s_Body += s_Tab + s_Tab + string.Format("<TR><TD>" +
                    "<B>LR Log2 Transformed<B>" +
                    "<A HREF='{0}'>" +
                    "<IMG src='{0}' width='400' height='400' /></A>" +
                    "</TD>",
                    s_LR_BoxplotFile);
                s_Body += "</TR>" + s_LineDelimiter;
            }           

            s_Body += s_Tab + s_Tab + "</TABLE>" + s_LineDelimiter + s_LineDelimiter;

            s_Body += s_Tab + "</BODY>" + s_LineDelimiter;
            return s_Body;
        }

        private string WriteEndHtml()
        {
            string s_End = "</HTML>";
            return s_End;
        }

        /// <summary>
        /// Generates the html code to display a table on the webpage
        /// </summary>
        /// <param name="Table">Table to display</param>
        /// <param name="alignment">How to align the table on the page, eg. left, center, right</param>
        /// <param name="border">Size of border</param>
        /// <param name="TabSpaces">Number of tab spaces before the table tag</param>
        /// <returns></returns>
        private string WriteTable(DataTable Table, string alignment, int border, int TabSpaces, int CellPadding)
        {
            string s_TableTabs = "", s_RowTabs = "";

            for (int i = 0; i < TabSpaces; i++)
            {
                s_TableTabs += s_Tab;
                s_RowTabs += s_Tab;
            }
            s_RowTabs += s_Tab;

            string s_Table = string.Format(s_TableTabs + "<TABLE align='{0}' border='{1}' " +
                "CELLPADDING={2} RULES=GROUPS FRAME=BOX>" +
                s_LineDelimiter,
                alignment,
                border,
                CellPadding);

            s_Table += s_RowTabs + "<THEAD>" + s_LineDelimiter +
                s_RowTabs + "<TR>";

            // Write the table headers
            foreach (DataColumn c in Table.Columns)
            {
                s_Table += "<TH><B><P align='center'>" + c.ColumnName + "</P></B></TH>";
            }
            s_Table += "</TR>" + s_LineDelimiter + s_RowTabs + "</THEAD>" + s_LineDelimiter;

            int i_Row = 0;
            foreach (DataRow r in Table.Rows)
            {
                i_Row++;
                if (i_Row % 2 == 0)
                    s_Table += s_RowTabs + "<TR bgcolor='lightgrey'>";
                else
                    s_Table += s_RowTabs + "<TR>";
                for (int c = 0; c < Table.Columns.Count; c++)
                {
                    s_Table += "<TD><P align='center'>" + r[c].ToString() + "</P></TD>";
                }
                s_Table += "</TR>" + s_LineDelimiter;
            }

            s_Table += s_TableTabs + "</TABLE>" + s_LineDelimiter + s_LineDelimiter;
            return s_Table;
        }

        /// <summary>
        /// Grabs the dataset information table from the SQLite database, and returns it in a DataTable
        /// </summary>
        /// <returns>LBF Dataset Summary Information</returns>
        private DataTable GetDatasetInfo()
        {
            string s_Command = "Select Alias, Dataset, Dataset_ID FROM t_factors ORDER BY Alias";

            DataTable dt_Return = clsSQLiteHandler.GetDataTable(
                s_Command,
                Path.Combine(esp.WorkDirectory, esp.InputFileName));

            return dt_Return;
        }

        /// <summary>
        /// From the 'legend' information supplied in the workflow xml file, constructs a table
        /// that describes symbols used in graphs for the html summary page.
        /// Rows are delineated by '|' and columns by ';'.
        /// </summary>
        /// <returns>Legend information</returns>
        private DataTable GetLegendInfo()
        {
            DataTable dt_Legend = new DataTable();

            if (esp.Legend.Equals("")) 
                return null;

            string[] s_Rows = esp.Legend.Split('|');

            for (int i = 0; i < s_Rows.Length; i++)
            {
                if (i == 0) // column headers
                {
                    string[] s_Headers = s_Rows[i].Split(';');
                    foreach (string h in s_Headers)
                    {
                        DataColumn dc = new DataColumn(h);
                        dt_Legend.Columns.Add(dc);
                    }
                }
                else
                {
                    string[] s_CurrentRow = s_Rows[i].Split(';');
                    dt_Legend.Rows.Add(s_CurrentRow);
                }
            }

            return dt_Legend;
        }

        /// <summary>
        /// Modifies the Summary table by adding a column for percent of original
        /// </summary>
        /// <param name="dt_Summary">Summary DataTable</param>
        /// <param name="LastRowForPercent">Last Row to calculate percent, all after this one are 'NA'</param>
        /// <returns></returns>
        private DataTable ModifySummaryTable(DataTable dt_Summary, int LastRowForPercent)
        {
            DataTable dt_Return = new DataTable("ModifiedSummaryTable");

            foreach (DataColumn dc in dt_Summary.Columns)
            {
                dt_Return.Columns.Add(new DataColumn(dc.ColumnName));
            }
            DataColumn dc_Percent = new DataColumn("PercentOfOriginal");
            dt_Return.Columns.Add(dc_Percent);

            return dt_Return;
        }
        #endregion
    }
}
