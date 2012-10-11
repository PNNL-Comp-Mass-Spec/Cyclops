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
using System.Data;

using RDotNet;
using log4net;

namespace Cyclops.ExportModules
{
    /// <summary>
    /// Required Parameters:
    /// tableName:  name of the table to use
    /// fileName:   name of the html file name to save the QC plot
    /// 
    /// canvasHeight:   height of the canvas element in html file
    /// rectHeight:     height of the rectangles
    /// overlapHeight:  related to rectangle height
    /// margin:         margin around the canvas element in the html
    /// headerFontSize: font size for "Fractions" at top of html
    /// fontSize:       font size for text in html file
    /// 
    /// includeHeatmap: true or false, whether to include the heatmap in the html file
    /// </summary>
    public class clsQC_Fraction_HTML : clsBaseExportModule
    {
        #region Members
        private ExportModules.cslExportParameterHandler esp =
            new ExportModules.cslExportParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        private string s_LineDelimiter = "\n";
        private string s_Tab = "\t";
        private DataTable dt_Overlap = new DataTable("FractionOverlap");

        private string s_RInstance;
        #endregion

        #region Constructors
        /// <summary>
        /// Exports an HTML file that displays the QC for 2D-LC fractions
        /// </summary>
        public clsQC_Fraction_HTML()
        {
            ModuleName = "Quality Control Module";
        }
        /// <summary>
        /// Exports an HTML file that displays the QC for 2D-LC fractions
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsQC_Fraction_HTML(string InstanceOfR)
        {
            ModuleName = "Quality Control Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Exports an HTML file that displays the QC for 2D-LC fractions
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsQC_Fraction_HTML(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Quality Control Module";
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
            if (!esp.HasTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR QC FRACTION HTML: 'tableName' was not found in the passed parameters");
                b_2Param = false;
            }
            if (!esp.HasWorkDirectory)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR QC FRACTION HTML: 'workDir' was not found in the passed parameters");
                b_2Param = false;
            }

            return b_2Param;
        }

        /// <summary>
        /// Runs module
        /// </summary>
        public override void PerformOperation()
        {
            if (Model.SuccessRunningPipeline)
            {
                Model.IncrementStep(ModuleName);
                traceLog.Info("Preparing HTML file for 2D-LC Fraction QC...");

                BuildHtmlFile();
            }
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
            }
        }

        private string WriteHtmlHeader()
        {
            string s_HTML = "<HTML>" + s_LineDelimiter + s_Tab + "<HEAD>" + s_LineDelimiter;
            return s_HTML;    
        }

        private string GetPeptideCount(string Attribute)
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            string s_Peptides = "", s_Command = string.Format("{0}${1}", 
                esp.TableName,
                Attribute);
            NumericVector nv = engine.EagerEvaluate(s_Command).AsNumeric();

            for (int i = 0; i < nv.Length; i++)
            {
                if (i == nv.Length - 1)
                {
                    s_Peptides += nv[i].ToString();
                }
                else
                {
                    s_Peptides += nv[i].ToString() + ", ";
                }
            }
            
            return s_Peptides;
        }

        private StringBuilder WriteHtmlScripts()
        {
            StringBuilder sb_ReturnScripts = new StringBuilder();

            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            string
                s_Peptides = GetPeptideCount("myPeptides"),
                s_Overlap = GetPeptideCount("Overlap"),
                s_TotalPeptides = GetPeptideCount("totalPeptides"); // comma separated lists

            sb_ReturnScripts.Append(s_Tab + "<SCRIPT type='application/javascript'>" + s_LineDelimiter);
            
            // Add in the variables
            sb_ReturnScripts.Append(string.Format(s_Tab + s_Tab + "var Rects=[{0}];" + s_LineDelimiter,
                s_Peptides));
            sb_ReturnScripts.Append(string.Format(s_Tab + s_Tab + "var Overlaps=[{0}];" + s_LineDelimiter,
                s_Overlap));
            sb_ReturnScripts.Append(string.Format(s_Tab + s_Tab + "var totalPeptides= 'Total Unique Peptides: {0}';" + 
                s_LineDelimiter,
                s_TotalPeptides));

            // Add the parameters
            sb_ReturnScripts.Append(string.Format(s_Tab + s_Tab + "var browserWidth;" + s_LineDelimiter +
                         s_Tab + s_Tab + "var browserHeight;" + s_LineDelimiter +
                         s_Tab + s_Tab + "var canvasHeight = {0};" + s_LineDelimiter +
                         s_Tab + s_Tab + "var rectHeight = {1};" + s_LineDelimiter +
                         s_Tab + s_Tab + "var overlapHeight = {2};" + s_LineDelimiter +
                         s_Tab + s_Tab + "var Margin = {3};" + s_LineDelimiter +
                         s_Tab + s_Tab + "var headerFontSize = '{4}';" + s_LineDelimiter +
                        s_Tab + s_Tab + "var fontSize = '{5}';" + s_LineDelimiter,
                        esp.CanvasHeight,           // 0
                        esp.RectangleHeight,        // 1
                        esp.OverlapHeight,          // 2
                        esp.Margin,                 // 3
                        esp.HeaderFontSize,         // 4
                        esp.FontSize));              // 5

            // Add method: GetBrowserDim()
            sb_ReturnScripts.Append(s_Tab + s_Tab + "function GetBrowserDim()" + s_LineDelimiter +
                         s_Tab + s_Tab + "{" + s_LineDelimiter +
                         s_Tab + s_Tab + s_Tab + "if (typeof window.innerWidth != 'undefined')" + s_LineDelimiter +
                         s_Tab + s_Tab + s_Tab + "{" + s_LineDelimiter +
                         s_Tab + s_Tab + s_Tab + s_Tab + "browserWidth = window.innerWidth;" + s_LineDelimiter +
                         s_Tab + s_Tab + s_Tab + s_Tab + "browserHeight = window.innerHeight;" + s_LineDelimiter +
                         s_Tab + s_Tab + s_Tab + "}" + s_LineDelimiter +
                         s_Tab + s_Tab + "}" + s_LineDelimiter);

            // Add method: drawRects()
            sb_ReturnScripts.Append(
                s_Tab + s_Tab + "function drawRects() {" + s_LineDelimiter +
			    s_Tab + s_Tab + s_Tab + "GetBrowserDim();" + s_LineDelimiter +
			    s_Tab + s_Tab + s_Tab + "var canvas = document.getElementById('myCanvas');" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "var ctx = canvas.getContext('2d');" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "ctx.canvas.width = browserWidth - Margin;" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "ctx.canvas.height = canvasHeight;" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "var lengthOfArray = Rects.length;" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "var rectWidth = ctx.canvas.width / lengthOfArray;" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "var i_Counter = 0;" + s_LineDelimiter + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "ctx.font = 'rgb(0,0,0)';" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "ctx.font = headerFontSize + 'px Arial';" + s_LineDelimiter +                          
                s_Tab + s_Tab + s_Tab + "ctx.textAlign='center';" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "ctx.fillText('Number of Unique Peptides/Fractions at 1% FDR', ctx.canvas.width / 2, Margin);" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "ctx.font = fontSize + 'px Arial';" + s_LineDelimiter + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "for (i in Rects)" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "{" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "i_Counter++;" + s_LineDelimiter + 
                s_Tab + s_Tab + s_Tab + s_Tab + "var r = Rects[i];" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "if (i % 2 == 0)" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "{" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + s_Tab +"rectColor = 'rgb(50,205,50)';" + s_LineDelimiter +
				s_Tab + s_Tab + s_Tab + s_Tab + "}" + s_LineDelimiter +
				s_Tab + s_Tab + s_Tab + s_Tab + "else" + s_LineDelimiter +
				s_Tab + s_Tab + s_Tab + s_Tab + "{" + s_LineDelimiter +
                 s_Tab + s_Tab + s_Tab + s_Tab + s_Tab +"rectColor = 'rgb(255,255,0)';" + s_LineDelimiter +
				s_Tab + s_Tab + s_Tab + s_Tab + "}" + s_LineDelimiter + s_LineDelimiter +				
                s_Tab + s_Tab + s_Tab + s_Tab + "ctx.strokeRect(i * rectWidth, rectHeight, rectWidth, rectHeight);"
                + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "ctx.textAlign='center';" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "ctx.textBaseline='middle';" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "ctx.font = 'bold' + fontSize + 'px Arial';" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "ctx.fillText('F' + i_Counter, i*rectWidth + rectWidth/2," +
					"rectHeight + rectHeight/2);" + s_LineDelimiter +
                    s_Tab + s_Tab + s_Tab + s_Tab + "ctx.font = fontSize + 'px Arial';" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "ctx.fillStyle = rectColor;" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "ctx.fillRect(i * rectWidth, (2*rectHeight), rectWidth, rectHeight);" 
                + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "ctx.strokeRect(i * rectWidth, (2*rectHeight), rectWidth, rectHeight);"
                + s_LineDelimiter + s_LineDelimiter +				 
				s_Tab + s_Tab + s_Tab + s_Tab + "ctx.fillStyle = 'rgb(0, 0, 0)';" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "ctx.fillText(Rects[i], (i * rectWidth) + rectWidth/2, " +
			    "2.5*rectHeight, rectWidth);" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "}" + s_LineDelimiter + s_LineDelimiter + 			
                s_Tab + s_Tab + s_Tab + "for (var s in Overlaps)" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "{" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "if (s % 2 == 0)" + s_LineDelimiter +
				s_Tab + s_Tab + s_Tab + s_Tab + "{" + s_LineDelimiter +
				s_Tab + s_Tab + s_Tab + s_Tab + s_Tab + "rectColor = 'rgb(255,165,0)';" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "}" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "else" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "{" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + s_Tab + "rectColor = 'rgb(186,85,211)';" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "}" + s_LineDelimiter + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "ctx.fillStyle = rectColor;" + s_LineDelimiter +
				s_Tab + s_Tab + s_Tab + s_Tab + "ctx.fillRect(s*rectWidth+rectWidth/2, 3*rectHeight, rectWidth, overlapHeight);" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "ctx.strokeRect(s*rectWidth+rectWidth/2, 3*rectHeight, rectWidth, overlapHeight);" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "ctx.textAlign='center';" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "ctx.fillStyle = 'rgb(0,0,0)';" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + s_Tab + "ctx.fillText(Overlaps[s], (s*rectWidth) + rectWidth, 3.5*rectHeight, rectWidth);" + s_LineDelimiter +                
                s_Tab + s_Tab + s_Tab + "}" + s_LineDelimiter + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "ctx.fillStyle='rgb(0,0,255)';" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "ctx.fillRect(0, 4*rectHeight+20, ctx.canvas.width, overlapHeight);" + s_LineDelimiter +
			    s_Tab + s_Tab + s_Tab + "ctx.strokeRect(0, 4*rectHeight+20, ctx.canvas.width, overlapHeight);" + s_LineDelimiter +
			    s_Tab + s_Tab + s_Tab + "ctx.fillStyle = 'rgb(255,255,255)';" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "ctx.font = 'bold' + fontSize + 'px Arial';" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "ctx.fillText(totalPeptides, ctx.canvas.width/2, 4.5*rectHeight+20, ctx.canvas.width);" + s_LineDelimiter +
                s_Tab + s_Tab + "}" +   s_LineDelimiter);


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
            string s_Body = s_Tab + "<BODY onload='drawRects()' onresize='drawRects()'>" 
                + s_LineDelimiter;

            // Write FRACTION DATASET INFORMATION
            s_Body += s_Tab + s_Tab + "<H2>Fraction identifiers and associated datasets:</H2>" + s_LineDelimiter;
            s_Body += s_Tab + s_Tab + "<TABLE><TR><TD>" + s_LineDelimiter;

            DataTable dt_Fractions = GetFractionAndDatasetInfo();
            s_Body += WriteTable(dt_Fractions, "left", 0, 2, 4);

            s_Body += s_Tab + s_Tab + "</TD></TR> <TR><TD>" + s_LineDelimiter;
            s_Body += string.Format(s_Tab + s_Tab + 
                "<A HREF='Plots/{0}'><IMG src='Plots/{0}' width='400' height='400' /></A>",
                "qc_Summary.png");
            s_Body += s_Tab + s_Tab + "</TD><TD>" + s_LineDelimiter;
            s_Body += string.Format(s_Tab + s_Tab +
                "<A HREF='Plots/{0}'><IMG src='Plots/{0}' width='400' height='400' /></A>",
                "log_qc_Summary.png");
            s_Body += s_Tab + s_Tab + "</TD></TR> </TABLE>" + s_LineDelimiter;
            s_Body += "<BR>" + s_LineDelimiter + s_LineDelimiter;


            // Write CANVAS ELEMENT
            s_Body += s_Tab + s_Tab + "<CANVAS id='myCanvas' width='600' height='300'>"
                + "Your browser does not support the 'CANVAS' tag.</CANVAS>" + s_LineDelimiter;

            // Write the PEPTIDE COUNT TABLE
            s_Body += s_LineDelimiter + WritePeptideCountTable();

            // INCLUDE HEATMAP IF REQUESTED
            if (esp.IncludeHeatmap)
            {
                s_Body += s_Tab + s_Tab +
                    "<H1>Percent Unique Peptide Overlap between Fractions</H1><BR>" +
                    s_LineDelimiter;
                s_Body += string.Format(
                    s_Tab + s_Tab +
                    "<A HREF='Plots/{0}'><IMG src='Plots/{0}' alt=Fraction intersection heatmap' width='{1}' height='{2}' /></A>",
                    esp.PercentHeatmapFileName,
                    esp.Width,
                    esp.Height) + "<BR><BR>";

                s_Body += s_Tab + s_Tab +
                    "<H1>Overall Unique Peptide Overlap between Fractions</H1><BR>" +
                    s_LineDelimiter;
                s_Body += string.Format(
                    s_Tab + s_Tab +
                    "<A HREF='Plots/{0}'><IMG src='Plots/{0}' alt=Fraction intersection heatmap' width='{1}' height='{2}' /></A>",
                    esp.HeatmapFileName,
                    esp.Width,
                    esp.Height);
            }
            
            s_Body += s_Tab + "</BODY>" + s_LineDelimiter;
            return s_Body;
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

        private string WritePeptideCountTable()
        {
            string s_Table = s_Tab + s_Tab + "<TABLE align='center' border='0' CELLPADDING=3 RULES=ALL FRAME=BOX>" +
                s_LineDelimiter + s_Tab + s_Tab + s_Tab + "<TR bgcolor='#4682B4'>" + 
                s_LineDelimiter + s_Tab + s_Tab + s_Tab + s_Tab;
            dt_Overlap = clsGenericRCalls.GetDataTable(s_RInstance, esp.TableName + "$myMatrix", true);

            // Write out the table header
            for (int i = 0; i < dt_Overlap.Columns.Count + 1; i++)
            {
                if (i == 0)
                    s_Table += "<TD><P style='color:white'><B>Fraction</B></P></TD>";
                else
                    s_Table += "<TD><P style='color:white' align='center'><B>F" + i + "</B></P></TD>";
            }
            s_Table += "</TR>" + s_LineDelimiter;

            // Write out the table rows
            for (int r = 0; r < dt_Overlap.Rows.Count; r++)
            {
                s_Table += s_Tab + s_Tab + "<TR><TD bgcolor='#4682B4'><P style='color:white' align='center'><B>F" + 
                    (r + 1) + "</B></P></TD>";
                for (int c = 0; c < dt_Overlap.Columns.Count; c++)
                {
                    if (r == c)
                    {
                        s_Table += "<TD bgcolor='#000000'><P style='color:white' align='center'><B>" +
                            dt_Overlap.Rows[r][c].ToString() + "</B></P></TD>";
                    }
                    else if (r == c + 1)
                    {
                        s_Table += "<TD bgcolor='#CAFF70'><P style='color:red' align='center'><B>" +
                            dt_Overlap.Rows[r][c].ToString() + "</B></P></TD>";
                    }
                    else if (c > r) // the upper right of the table will display percentages
                    {
                        int i_PepCnt = Math.Min(
                            Int32.Parse(dt_Overlap.Rows[r][r].ToString()),
                            Int32.Parse(dt_Overlap.Rows[c][c].ToString()));
                        int i_Pep = Int32.Parse(dt_Overlap.Rows[r][c].ToString());
                        double d_Percent = (double)i_Pep / (double)i_PepCnt * 100;
                        if (c - 1 == r)
                        {
                            s_Table += "<TD bgcolor='#CAFF70'><P style='color:red' align='center'><B>" +
                                string.Format("{0:0.0}", d_Percent) + "%</B></P></TD>";
                        }
                        else
                        {
                            s_Table += "<TD><P align='center'>" +
                                string.Format("{0:0.0}", d_Percent) + "%</P></TD>";
                        }
                    }
                    else
                    {
                        s_Table += "<TD><P align='center'>" +
                            dt_Overlap.Rows[r][c].ToString() + "</P></TD>";
                    }
                }
                s_Table += "</TR>" + s_LineDelimiter;
            }

            s_Table += s_Tab + s_Tab + "</TABLE>" + s_LineDelimiter + s_LineDelimiter;

            return s_Table;
        }

        private string WriteEndHtml()
        {
            string s_End = "</HTML>";
            return s_End;    
        }

        private DataTable GetFractionAndDatasetInfo()
        {
            string s_Command = "Select Fraction, Dataset, Dataset_ID FROM t_factors ORDER BY Fraction";

            DataTable tmp = clsSQLiteHandler.GetDataTable(
                s_Command,
                Path.Combine(esp.WorkDirectory,esp.DatabaseName));

            DataTable dt_Return = new DataTable();
            DataColumn dc_Fraction = new DataColumn("Fraction", typeof(Int32));
            dt_Return.Columns.Add(dc_Fraction);
            DataColumn dc_Dataset = new DataColumn("Dataset", typeof(String));
            dt_Return.Columns.Add(dc_Dataset);
            DataColumn dc_DatasetID = new DataColumn("Dataset_ID", typeof(Int32));
            dt_Return.Columns.Add(dc_DatasetID);

            foreach (DataRow r in tmp.Rows)
            {
                dt_Return.Rows.Add(Int32.Parse(r[0].ToString()),
                    r[1].ToString(),
                    Int32.Parse(r[2].ToString()));
            }

            //dt_Return.Select(null, "Fraction");
            dt_Return = new DataView(dt_Return, "", "Fraction ASC", DataViewRowState.CurrentRows).ToTable();
            
            return dt_Return;
        }

        #endregion
    }
}
