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

namespace Cyclops.ExportModules
{
    public class clsSummaryHTMLTemplate : clsBaseExportModule
    {
         #region Members
        private ExportModules.cslExportParameterHandler esp =
            new ExportModules.cslExportParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        private string s_LineDelimiter = "\n";
        private string s_Tab = "\t";
        private DataTable dt_Overlap = new DataTable("Template");

        private string s_RInstance;
        #endregion

        #region Constructors
        /// <summary>
        /// HTMLSummaryTemplate
        /// </summary>
        public clsSummaryHTMLTemplate()
        {
            ModuleName = "HTMLSummary Module";
        }
        /// <summary>
        /// HTMLSummaryTemplate
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsSummaryHTMLTemplate(string InstanceOfR)
        {
            ModuleName = "HTMLSummary Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// HTMLSummaryTemplate
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsSummaryHTMLTemplate(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "HTMLSummary Module";
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
                traceLog.Error("ERROR HTMLSummaryTemplate: 'tableName' was not found in the passed parameters");
                b_2Param = false;
            }
            if (!esp.HasWorkDirectory)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR HTMLSummaryTemplate: 'workDir' was not found in the passed parameters");
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

                traceLog.Info("Preparing to write HTMLSummaryTemplate File...");

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
            string s_Body = s_Tab + "<BODY>" 
                + s_LineDelimiter;

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
        #endregion
    }
}
