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

using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Cyclops.DataModules
{
    /// <summary>
    /// Class specifically designed to deal with coding html
    /// </summary>
    public static class HtmlFileHandler
    {
        #region Enums
        public enum CssStyle { NavBar, LeftIndent, Th };
        #endregion

        #region Static Methods
        /// <summary>
        /// Generic HTML and HEAD elements to start an HTML page
        /// </summary>
        /// <returns>HTML code</returns>
        public static string GetHtmlHeader()
        {
            return "<HTML>\n\t<HEAD>\n";
        }

        /// <summary>
        /// HTML Title script
        /// </summary>
        /// <param name="Title">Text that you would like displayed as the title of the page</param>
        /// <returns>HTML code</returns>
        public static string GetTitle(string Title)
        {
            return string.Format("\t\t<TITLE>{0}</TITLE>\n",
                Title);
        }

        /// <summary>
        /// Links page to CSS file, automatically sets rel to 'stylesheet'
        /// </summary>
        /// <param name="CssFileName">Name of CSS file, e.g. 'MyStyles.css'</param>
        /// <returns>HTML code</returns>
        public static string GetCSSLink(string CssFileName)
        {
            return string.Format("\t\t<LINK rel='stylesheet' href='{0}'>\n",
                CssFileName);
        }

        /// <summary>
        /// Links page to CSS file
        /// </summary>
        /// <param name="RelAttribute">Relationship between html page and the linked document, e.g. 'stylesheet'</param>
        /// <param name="CssFileName">Name of CSS file, e.g. 'MyStyles.css'</param>
        /// <returns>HTML code</returns>
        public static string GetCSSLink(string RelAttribute, string CssFileName)
        {
            return string.Format("\t\t<LINK rel='{0}' href='{1}'>\n",
                RelAttribute,
                CssFileName);
        }

        /// <summary>
        /// Generic SCRIPT element start of javascript
        /// </summary>
        /// <returns>HTML code</returns>
        public static string GetHtmlJavascriptStart()
        {
            return "\t<SCRIPT type='application/javascript'>\n";
        }

        /// <summary>
        /// Ends the Script element
        /// </summary>
        /// <returns>HTML code</returns>
        public static string GetHtmlScriptEnd()
        {
            return "\t</SCRIPT>\n";
        }

        /// <summary>
        /// Generates the code for the CSS file
        /// </summary>
        /// <param name="style">Indicates the type of css code you want</param>
        /// <returns>CSS code</returns>
        public static string GetCSS(CssStyle style, int InternalNavTop)
        {
            StringBuilder s_Return = new StringBuilder();
            string s_Tab = "\t", s_LineDelimiter = "\n";

            switch (style)
            {
                case CssStyle.NavBar:
                    s_Return.Append("ul#list-nav {" + s_LineDelimiter +
                        s_Tab + "list-style:none;" + s_LineDelimiter +
                        s_Tab + "top:70px;" + s_LineDelimiter +
                        s_Tab + "margin:5px;" + s_LineDelimiter +
                        s_Tab + "padding:0;" + s_LineDelimiter +
                        s_Tab + "width:600px;" + s_LineDelimiter +
                        s_Tab + "position:fixed;" + s_LineDelimiter +
                        "}" + s_LineDelimiter + s_LineDelimiter);

                    s_Return.Append("ul#list-nav li {" + s_LineDelimiter +
                        s_Tab + "display:table-row;" + s_LineDelimiter +
                        s_Tab + "font-weight:bold;" + s_LineDelimiter +
                        "}" + s_LineDelimiter + s_LineDelimiter);

                    s_Return.Append("ul#list-nav li a {" + s_LineDelimiter +
                        s_Tab + "text-decoration:none;" + s_LineDelimiter +
                        s_Tab + "padding:5px 0;" + s_LineDelimiter +
                        s_Tab + "width:180px;" + s_LineDelimiter +
                        s_Tab + "background:#0000FF;" + s_LineDelimiter +
                        s_Tab + "color:#eee;" + s_LineDelimiter +
                        s_Tab + "float:left;" + s_LineDelimiter +
                        s_Tab + "text-align:center;" + s_LineDelimiter +
                        s_Tab + "border-left:1px solid #fff;" + s_LineDelimiter +
                        s_Tab + "display: inline;" + s_LineDelimiter +
                        "}" + s_LineDelimiter + s_LineDelimiter);

                    s_Return.Append("ul#list-nav li a:hover {" + s_LineDelimiter +
                        s_Tab + "background:#87CEFA;" + s_LineDelimiter +
                        s_Tab + "color:#000" + s_LineDelimiter +
                        "}" + s_LineDelimiter);

                    s_Return.Append(string.Format("ul#interal_nav {{{0}" +
                        "{1}list-style:none;{0}" +
                        "{1}top:{2}px;{0}" +
                        "{1}margin:5px;{0}" +
                        "{1}padding:0;{0}" +
                        "{1}width:600px;{0}" +
                        "{1}position:fixed;{0}" +
                        "}}{0}",
                        s_LineDelimiter,
                        s_Tab,
                        InternalNavTop));

                    s_Return.Append("ul#interal_nav li {" + s_LineDelimiter +
                        s_Tab + "display:table-row;" + s_LineDelimiter +
                        s_Tab + "font-weight:bold;" + s_LineDelimiter +
                        "}" + s_LineDelimiter);

                    s_Return.Append("ul#interal_nav li a {" + s_LineDelimiter +
                        s_Tab + "text-decoration:none;" + s_LineDelimiter +
                        s_Tab + "padding:5px 0;" + s_LineDelimiter +
                        s_Tab + "width:180px;" + s_LineDelimiter +
                        s_Tab + "background:#FF0000;" + s_LineDelimiter +
                        s_Tab + "color:#eee;" + s_LineDelimiter +
                        s_Tab + "float:left;" + s_LineDelimiter +
                        s_Tab + "text-align:center;" + s_LineDelimiter +
                        s_Tab + "border-left:1px solid #fff;" + s_LineDelimiter +
                        s_Tab + "display: inline;" + s_LineDelimiter +
                        "}" + s_LineDelimiter);

                    s_Return.Append("ul#interal_nav li a:hover {" + s_LineDelimiter +
                        s_Tab + "background:#FF66FF;" + s_LineDelimiter +
                        s_Tab + "color:#000" + s_LineDelimiter +
                        "}" + s_LineDelimiter + s_LineDelimiter);


                    // Build Nav Table
                    s_Return.Append(string.Format(
                        "table#nav_table{0}" +
                        "{{{0}" +
                        "{1}width:200px;{0}" +
                        "{1}position:fixed;{0}" +
                        "}}{0}{0}" +
                        "tr#nav_table_main{0}" +
                        "{{{0}" +
                        "{1}background:#1111FF;{0}" +
                        "{1}text-align:center;{0}" +
                        "{1}text-decoration:none;{0}" +
                        "{1}font-weight:bold;{0}" +
                        "{1}height:32px;{0}" +
                        "}}{0}{0}" +
                        "tr#nav_table_main:hover{0}" +
                        "{{{0}" +
                        "{1}background:#87CEFA;{0}" +
                        "{1}color:#111;{0}" +
                        "}}{0}{0}" +
                        "a#nav_table_main{0}" +
                        "{{{0}" +
                        "{1}color:white;{0}" +
                        "{1}text-decoration:none;{0}" +
                        "}}{0}{0}" +
                        "a#nav_table_main:hover{0}" +
                        "{{{0}" +
                        "{1}color:black;{0}" +
                        "}}{0}{0}" +
                        "tr#nav_table_internal{0}" +
                        "{{{0}" +
                        "{1}background:red;{0}" +
                        "{1}text-align:center;{0}" +
                        "{1}text-decoration:none;{0}" +
                        "{1}font-weight:bold;{0}" +
                        "{1}height:32px;{0}" +
                        "}}{0}{0}" +
                        "tr#nav_table_internal:hover{0}" +
                        "{{{0}" +
                        "{1}background:#FF66FF;{0}" +
                        "{1}color:#111{0}" +
                        "}}{0}{0}"
                        , s_LineDelimiter
                        , s_Tab));

                    break;
                case CssStyle.LeftIndent:
                    s_Return.Append("#main_content" + s_LineDelimiter +
                        "{" + s_LineDelimiter +
                        s_Tab + "padding: 20px;" + s_LineDelimiter +
                        s_Tab + "margin-left: 200px;" + s_LineDelimiter +
                        "}" + s_LineDelimiter);
                    break;
                case CssStyle.Th:
                    s_Return.Append("p#table_header {" + s_LineDelimiter +
                        s_Tab + "text-align: left;" + s_LineDelimiter +
                        s_Tab + "font-weight: bold;" + s_LineDelimiter +
                        s_Tab + "font-size: 20px;" + s_LineDelimiter +
                        "}" + s_LineDelimiter);
                    s_Return.Append("th" + s_LineDelimiter +
                        "{" + s_LineDelimiter +
                        s_Tab + "background-color:blue;" + s_LineDelimiter +
                        s_Tab + "color:white;" + s_LineDelimiter +
                        "}" + s_LineDelimiter + s_LineDelimiter);
                    s_Return.Append("table" + s_LineDelimiter +
                        "{" + s_LineDelimiter +
                        s_Tab + "border-collapse: collapse;" + s_LineDelimiter +
                        "}" + s_LineDelimiter);
                    break;
            }

            return s_Return.ToString();
        }

        /// <summary>
        /// Generic end of HEAD and BODY elements in HTML code
        /// </summary>
        /// <returns></returns>
        public static string GetEndHeadStartBody()
        {
            string s_Tab = "\t", s_LineDelimiter = "\n";

            return string.Format("{0}</HEAD>{1}{0}<BODY>{1}"
                , s_Tab
                , s_LineDelimiter);
        }

        public static string GetNavBar(List<HtmlLinkNode> NavBar, string NavBarAlignment)
        {
            string s_Return = "", s_Tab = "\t", s_LineDelimiter = "\n", s_Style = "main_content";

            // Write out navigation bar
            //s_Return += string.Format(s_Tab + "<{0}>" + s_LineDelimiter, NavBarAlignment);
            //s_Return += s_Tab + s_Tab + "<NAV>" + s_LineDelimiter;
            s_Return += s_Tab + s_Tab + s_Tab + "<UL ID='list-nav'>" + s_LineDelimiter;

            foreach (HtmlLinkNode n in NavBar)
            {
                if (!n.IsInternalLink)
                {
                    s_Return += string.Format(s_Tab + s_Tab + s_Tab + s_Tab +
                        "<LI><A HREF='{0}{1}'>{2}</A>" + s_LineDelimiter,
                        n.IsInternalLink ? "#" : "",
                        n.Link,
                        n.Title);
                }
            }

            s_Return += s_Tab + s_Tab + s_Tab + "</UL>" + s_LineDelimiter;
            s_Return += s_Tab + s_Tab + s_Tab + "<UL ID='interal_nav'>" + s_LineDelimiter;

            foreach (HtmlLinkNode n in NavBar)
            {
                if (n.IsInternalLink)
                {
                    s_Return += string.Format(s_Tab + s_Tab + s_Tab + s_Tab +
                        "<LI><A HREF='{0}{1}'>{2}</A></LI>" + s_LineDelimiter,
                        n.IsInternalLink ? "#" : "",
                        n.Link,
                        n.Title);
                }
            }

            s_Return += s_Tab + s_Tab + s_Tab + "</UL>" + s_LineDelimiter;

            s_Return += string.Format("<DIV ID='{0}'>{1}"
                , s_Style
                , s_LineDelimiter);

            return s_Return;
        }

        public static string GetNavTable(List<HtmlLinkNode> NavTable)
        {
            string s_Return = "", s_Tab = "\t", s_LineDelimiter = "\n";

            s_Return = string.Format("{0}{0}<DIV>{1}" +
                "{0}{0}{0}<TABLE align='left' ID='nav_table'>{1}"
                , s_Tab
                , s_LineDelimiter
                );

            foreach (HtmlLinkNode n in NavTable)
            {
                if (!n.IsInternalLink)
                {
                    s_Return += string.Format("{0}{0}{0}{0}<TR ID='nav_table_main' " +
                        "CELLPADDING=10 BORDER=0>{1}" +
                        "{0}{0}{0}{0}{0}<TD>" +
                        "<A HREF='{2}' ID='nav_table_main'>" +
                        "{3}</A>" +
                        "{0}{0}{0}{0}{0}</TD>{1}" +
                        "{0}{0}{0}{0}</TR>{1}"
                        , s_Tab
                        , s_LineDelimiter
                        , n.Link
                        , n.Title);
                }
            }

            foreach (HtmlLinkNode n in NavTable)
            {
                if (n.IsInternalLink)
                {
                    s_Return += string.Format("{0}{0}{0}{0}<TR ID='nav_table_internal' " +
                        "CELLPADDING=10 BORDER=0>{1}" +
                        "{0}{0}{0}{0}{0}<TD>" +
                        "<A HREF='#{2}' ID='nav_table_main'>" +
                        "{3}</A>" +
                        "{0}{0}{0}{0}{0}</TD>{1}" +
                        "{0}{0}{0}{0}</TR>{1}"
                        , s_Tab
                        , s_LineDelimiter
                        , n.Link
                        , n.Title);
                }
            }

            s_Return += string.Format("{0}{0}{0}</TABLE>{1}" +
                "{0}{0}</DIV>{1}"
                , s_Tab
                , s_LineDelimiter
                );

            return s_Return;
        }


        /// <summary>
        /// HTML code for the datasets table
        /// </summary>
        /// <param name="DatabaseFileName">Full path to the Results.DB3 file</param>
        /// <param name="Title">Title above the data table</param>
        /// <param name="Alignment">Alignment of table in html page</param>
        /// <param name="Border">Table border</param>
        /// <param name="TabSpaces">Table tabspaces</param>
        /// <param name="CellPadding">Table cell padding</param>
        /// <returns>HTML code</returns>
        public static string GetDatasetTableHtml(string DatabaseFileName, string Title,
            string TitleStyle, string TableAlignment, int Border, int TabSpaces, int CellPadding)
        {
            string s_Tab = "\t", s_LineDelimiter = "\n";

            string s_Return = string.Format(
                "{0}{0}{0}<DIV>{1}" +
                "{0}{0}{0}{0}<P ID='{3}'>{2}</P>{1}"
                , s_Tab
                , s_LineDelimiter
                , !string.IsNullOrEmpty(Title) ? Title : "Datasets used in the Analysis"
                , TitleStyle);
            DataTable dt_Datasets = GetDatasetInfo(DatabaseFileName);
            s_Return += GetTableHtml(dt_Datasets, TableAlignment, Border, TabSpaces, CellPadding);
            s_Return += string.Format("{0}{0}{0}</DIV>{1}"
                , s_Tab
                , s_LineDelimiter);
            return s_Return;
        }

        public static string GetSummaryTableHtml(DataTable Table, string Title,
            string TitleStyle, int Border, int TabSpaces, int CellPadding)
        {
            string s_Tab = "\t", s_LineDelimiter = "\n";

            string s_Return = string.Format(
                "{0}{0}{0}<DIV>{1}" +
                "{0}{0}{0}{0}<P ID='{3}'>{2}</P>{1}"
                , s_Tab
                , s_LineDelimiter
                , !string.IsNullOrEmpty(Title) ? Title : "Summary Table"
                , TitleStyle
                );

            if (Table != null)
                s_Return += GetTableHtml(Table, null, Border, TabSpaces, CellPadding);

            s_Return += string.Format(
                "{0}{0}{0}</DIV>{1}{0}{0}<BR>{1}"
                , s_Tab
                , s_LineDelimiter);

            return s_Return;
        }

        public static string GetQCElement(string Title, string TitleStyle,
            string PictureFileName, DataTable Table, int Border,
            int TabSpaces, int CellPadding)
        {
            string s_Tab = "\t", s_LineDelimiter = "\n";

            string s_Return = string.Format("{0}{0}{0}<DIV>{1}" +
                "{0}{0}{0}{0}<P ID='{3}'>{2}</P>{1}" +
                "{0}{0}{0}{0}<TABLE>{1}" +
                "{0}{0}{0}{0}{0}<TR>{1}" +
                "{0}{0}{0}{0}{0}{0}<TD>{1}" +
                "{0}{0}{0}{0}{0}{0}{0}" +
                "{4}{1}" +
                "{0}{0}{0}{0}{0}{0}</TD>{1}" +
                "{0}{0}{0}{0}{0}{0}<TD>{1}" +
                "{0}{0}{0}{0}{0}{0}{0}" +
                "{5}{1}" +
                "{0}{0}{0}{0}{0}{0}</TD>{1}" +
                "{0}{0}{0}{0}{0}</TR>{1}" +
                "{0}{0}{0}{0}</TABLE>{1}" +
                "{0}{0}{0}</DIV>{1}"
                , s_Tab
                , s_LineDelimiter
                , Title
                , TitleStyle
                , GetPictureCode(PictureFileName, true, "", null, null)
                , GetTableHtml(Table, null, Border, TabSpaces, CellPadding)
                );

            return s_Return;
        }

        /// <summary>
        /// Retrieve Dataset information from the Column Metadata table in the database,
        /// including Alias, Dataset, and Dataset_ID
        /// </summary>
        /// <param name="DatabaseFileName">Full path to the Results.DB3 file</param>
        /// <param name="TableName">Name of the Column Metadata table</param>
        /// <returns>Selected columns from Column Metadata table</returns>
        private static DataTable GetDatasetInfo(string DatabaseFileName, string TableName)
        {
            SQLiteHandler sql = new SQLiteHandler();
            sql.DatabaseFileName = DatabaseFileName;
            string Command = string.Format(
                "Select Alias, Dataset, Dataset_ID FROM {0} ORDER BY Alias",
                TableName);
            if (sql.TableExists(TableName))
            {
                return sql.SelectTable(Command);
            }
            else
                return null;
        }

        /// <summary>
        /// Retrieve Dataset information from the 't_factors' table in the database,
        /// including Alias, Dataset, and Dataset_ID
        /// </summary>
        /// <param name="DatabaseFileName">Full path to the Results.DB3 file</param>
        /// <returns></returns>
        private static DataTable GetDatasetInfo(string DatabaseFileName)
        {
            return GetDatasetInfo(DatabaseFileName, "t_factors");
        }

        /// <summary>
        /// Generic code to add a picture to the webpage
        /// </summary>
        /// <param name="PictureFileName">Name of image file, e.g. MyHeatmap.png</param>
        /// <param name="AddPlotsDirectory">Whether or not to add the 'Plots/' directory before image file name</param>
        /// <param name="Width">Width of image, if null then set to 400</param>
        /// <param name="Height">Height of image, if null then set to 400</param>
        /// <returns>HTML code</returns>
        public static string GetPictureCode(string PictureFileName,
            bool AddPlotsDirectory, string CssCode, int? Width, int? Height)
        {
            string s_Tab = "\t", s_LineDelimiter = "\n";

            return string.Format("{0}{0}{0}{0}<A {4}HREF='{2}{3}'>" +
                "<IMG src='{2}{3}' width='{5}' height='{6}' /></A>{1}"
                , s_Tab
                , s_LineDelimiter
                , AddPlotsDirectory ? "Plots/" : ""
                , PictureFileName
                , !string.IsNullOrEmpty(CssCode) ? "class='" + CssCode + "' " : ""
                , Width != null ? Width.ToString() : "400"
                , Height != null ? Height.ToString() : "400");
        }

        /// <summary>
        /// End BODY and HTML elements
        /// </summary>
        /// <returns></returns>
        public static string GetEndBodyEndHtml()
        {
            string s_Tab = "\t", s_LineDelimiter = "\n";
            return string.Format("{0}{0}</DIV>{1}{0}</BODY>{1}</HTML>{1}"
                , s_Tab
                , s_LineDelimiter
                );
        }

        /// <summary>
        /// Generates the html code to display a table on the webpage
        /// </summary>
        /// <param name="Table">Table to display</param>
        /// <param name="alignment">How to align the table on the page, eg. left, center, right</param>
        /// <param name="border">Size of border</param>
        /// <param name="TabSpaces">Number of tab spaces before the table tag</param>
        /// <returns></returns>
        public static string GetTableHtml(DataTable Table, string alignment, int? border, int? TabSpaces, int? CellPadding)
        {
            string s_TableTabs = "", s_RowTabs = "", s_Tab = "\t", s_LineDelimiter = "\n";

            for (int i = 0; i < TabSpaces; i++)
            {
                s_TableTabs += s_Tab;
                s_RowTabs += s_Tab;
            }
            s_RowTabs += s_Tab;

            string s_Table = string.Format(s_TableTabs + "<TABLE {0}border='{1}' " +
                "CELLPADDING={2}>" +
                s_LineDelimiter,
                !string.IsNullOrEmpty(alignment) ? "align='" + alignment + "' " : "",
                border != null ? border.ToString() : "0",
                CellPadding != null ? CellPadding.ToString() : "4");

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
                    s_Table += s_RowTabs + "<TR bgcolor='lightblue'>";
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
