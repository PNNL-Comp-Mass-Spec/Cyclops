/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: http://omics.pnl.gov/software
 * -----------------------------------------------------
 *
 * Licensed under the Apache License, Version 2.0; you may not use this
 * file except in compliance with the License.  You may obtain a copy of the
 * License at https://opensource.org/licenses/Apache-2.0
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
            return string.Format("\t\t<TITLE>{0}</TITLE>\n", Title);
        }

        /// <summary>
        /// Links page to CSS file, automatically sets rel to 'stylesheet'
        /// </summary>
        /// <param name="CssFileName">Name of CSS file, e.g. 'MyStyles.css'</param>
        /// <returns>HTML code</returns>
        public static string GetCSSLink(string CssFileName)
        {
            return string.Format("\t\t<LINK rel='stylesheet' href='{0}'>\n", CssFileName);
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
            StringBuilder css = new StringBuilder();

            switch (style)
            {
                case CssStyle.NavBar:
                    css.Append("ul#list-nav {\n" +
                        '\t' + "list-style:none;\n" +
                        '\t' + "top:70px;\n" +
                        '\t' + "margin:5px;\n" +
                        '\t' + "padding:0;\n" +
                        '\t' + "width:600px;\n" +
                        '\t' + "position:fixed;\n" +
                        "}\n\n");

                    css.Append("ul#list-nav li {\n" +
                        '\t' + "display:table-row;\n" +
                        '\t' + "font-weight:bold;\n" +
                        "}\n\n");

                    css.Append("ul#list-nav li a {\n" +
                        '\t' + "text-decoration:none;\n" +
                        '\t' + "padding:5px 0;\n" +
                        '\t' + "width:180px;\n" +
                        '\t' + "background:#0000FF;\n" +
                        '\t' + "color:#eee;\n" +
                        '\t' + "float:left;\n" +
                        '\t' + "text-align:center;\n" +
                        '\t' + "border-left:1px solid #fff;\n" +
                        '\t' + "display: inline;\n" +
                        "}\n\n");

                    css.Append("ul#list-nav li a:hover {\n" +
                        '\t' + "background:#87CEFA;\n" +
                        '\t' + "color:#000\n" +
                        "}\n");

                    css.Append(string.Format("ul#interal_nav {{{0}" +
                        "{1}list-style:none;{0}" +
                        "{1}top:{2}px;{0}" +
                        "{1}margin:5px;{0}" +
                        "{1}padding:0;{0}" +
                        "{1}width:600px;{0}" +
                        "{1}position:fixed;{0}" +
                        "}}{0}",
                        '\n',
                        '\t',
                        InternalNavTop));

                    css.Append("ul#interal_nav li {\n" +
                        '\t' + "display:table-row;\n" +
                        '\t' + "font-weight:bold;\n" +
                        "}\n");

                    css.Append("ul#interal_nav li a {\n" +
                        '\t' + "text-decoration:none;\n" +
                        '\t' + "padding:5px 0;\n" +
                        '\t' + "width:180px;\n" +
                        '\t' + "background:#FF0000;\n" +
                        '\t' + "color:#eee;\n" +
                        '\t' + "float:left;\n" +
                        '\t' + "text-align:center;\n" +
                        '\t' + "border-left:1px solid #fff;\n" +
                        '\t' + "display: inline;\n" +
                        "}\n");

                    css.Append("ul#interal_nav li a:hover {\n" +
                        '\t' + "background:#FF66FF;\n" +
                        '\t' + "color:#000\n" +
                        "}\n\n");


                    // Build Nav Table
                    css.Append(string.Format(
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
                        , '\n'
                        , '\t'));

                    break;
                case CssStyle.LeftIndent:
                    css.Append("#main_content\n" +
                        "{\n" +
                        '\t' + "padding: 20px;\n" +
                        '\t' + "margin-left: 200px;\n" +
                        "}\n");
                    break;
                case CssStyle.Th:
                    css.Append("p#table_header {\n" +
                        '\t' + "text-align: left;\n" +
                        '\t' + "font-weight: bold;\n" +
                        '\t' + "font-size: 20px;\n" +
                        "}\n");
                    css.Append("th\n" +
                        "{\n" +
                        '\t' + "background-color:blue;\n" +
                        '\t' + "color:white;\n" +
                        "}\n\n");
                    css.Append("table\n" +
                        "{\n" +
                        '\t' + "border-collapse: collapse;\n" +
                        "}\n");
                    break;
            }

            return css.ToString();
        }

        /// <summary>
        /// Generic end of HEAD and BODY elements in HTML code
        /// </summary>
        /// <returns></returns>
        public static string GetEndHeadStartBody()
        {
            return string.Format("{0}</HEAD>{1}{0}<BODY>{1}"
                , '\t'
                , '\n');
        }

        public static string GetNavBar(List<HtmlLinkNode> NavBar, string NavBarAlignment)
        {
            string htmlText = "";
            string styleType = "main_content";

            // Write out navigation bar
            //htmlText += string.Format('\t' + "<{0}>\n", NavBarAlignment);
            //htmlText += '\t' + '\t' + "<NAV>\n";
            htmlText += '\t' + '\t' + '\t' + "<UL ID='list-nav'>\n";

            foreach (HtmlLinkNode n in NavBar)
            {
                if (!n.IsInternalLink)
                {
                    htmlText += string.Format('\t' + '\t' + '\t' + '\t' +
                        "<LI><A HREF='{0}{1}'>{2}</A>\n",
                        n.IsInternalLink ? "#" : "",
                        n.Link,
                        n.Title);
                }
            }

            htmlText += '\t' + '\t' + '\t' + "</UL>\n";
            htmlText += '\t' + '\t' + '\t' + "<UL ID='interal_nav'>\n";

            foreach (HtmlLinkNode n in NavBar)
            {
                if (n.IsInternalLink)
                {
                    htmlText += string.Format('\t' + '\t' + '\t' + '\t' +
                        "<LI><A HREF='{0}{1}'>{2}</A></LI>\n",
                        n.IsInternalLink ? "#" : "",
                        n.Link,
                        n.Title);
                }
            }

            htmlText += '\t' + '\t' + '\t' + "</UL>\n";

            htmlText += string.Format("<DIV ID='{0}'>{1}"
                , styleType
                , '\n');

            return htmlText;
        }

        public static string GetNavTable(List<HtmlLinkNode> NavTable)
        {
            string htmlText = "";

            htmlText = string.Format("{0}{0}<DIV>{1}" +
                "{0}{0}{0}<TABLE align='left' ID='nav_table'>{1}"
                , '\t'
                , '\n'
                );

            foreach (HtmlLinkNode n in NavTable)
            {
                if (!n.IsInternalLink)
                {
                    htmlText += string.Format("{0}{0}{0}{0}<TR ID='nav_table_main' " +
                        "CELLPADDING=10 BORDER=0>{1}" +
                        "{0}{0}{0}{0}{0}<TD>" +
                        "<A HREF='{2}' ID='nav_table_main'>" +
                        "{3}</A>" +
                        "{0}{0}{0}{0}{0}</TD>{1}" +
                        "{0}{0}{0}{0}</TR>{1}"
                        , '\t'
                        , '\n'
                        , n.Link
                        , n.Title);
                }
            }

            foreach (HtmlLinkNode n in NavTable)
            {
                if (n.IsInternalLink)
                {
                    htmlText += string.Format("{0}{0}{0}{0}<TR ID='nav_table_internal' " +
                        "CELLPADDING=10 BORDER=0>{1}" +
                        "{0}{0}{0}{0}{0}<TD>" +
                        "<A HREF='#{2}' ID='nav_table_main'>" +
                        "{3}</A>" +
                        "{0}{0}{0}{0}{0}</TD>{1}" +
                        "{0}{0}{0}{0}</TR>{1}"
                        , '\t'
                        , '\n'
                        , n.Link
                        , n.Title);
                }
            }

            htmlText += string.Format("{0}{0}{0}</TABLE>{1}" +
                "{0}{0}</DIV>{1}"
                , '\t'
                , '\n'
                );

            return htmlText;
        }


        /// <summary>
        /// HTML code for the datasets table
        /// </summary>
        /// <param name="databaseFileName">Full path to the Results.DB3 file</param>
        /// <param name="title">Title above the data table</param>
        /// <param name="titleStyle"></param>
        /// <param name="tableAlignment">Alignment of table in html page</param>
        /// <param name="border">Table border</param>
        /// <param name="tabSpaces">Table tabspaces</param>
        /// <param name="cellPadding">Table cell padding</param>
        /// <returns>HTML code</returns>
        public static string GetDatasetTableHtml(
            string databaseFileName, string title,
            string titleStyle, string tableAlignment, int border, int tabSpaces, int cellPadding)
        {
            string htmlText = string.Format(
                "{0}{0}{0}<DIV>{1}" +
                "{0}{0}{0}{0}<P ID='{3}'>{2}</P>{1}"
                , '\t'
                , '\n'
                , !string.IsNullOrEmpty(title) ? title : "Datasets used in the Analysis"
                , titleStyle);
            DataTable datasets = GetDatasetInfo(databaseFileName);
            htmlText += GetTableHtml(datasets, tableAlignment, border, tabSpaces, cellPadding);
            htmlText += string.Format("{0}{0}{0}</DIV>{1}"
                , '\t'
                , '\n');
            return htmlText;
        }

        public static string GetSummaryTableHtml(
            DataTable Table, string Title,
            string TitleStyle, int Border, int TabSpaces, int CellPadding)
        {
            string htmlText = string.Format(
                "{0}{0}{0}<DIV>{1}" +
                "{0}{0}{0}{0}<P ID='{3}'>{2}</P>{1}"
                , '\t'
                , '\n'
                , !string.IsNullOrEmpty(Title) ? Title : "Summary Table"
                , TitleStyle
                );

            if (Table != null)
                htmlText += GetTableHtml(Table, null, Border, TabSpaces, CellPadding);

            htmlText += string.Format(
                "{0}{0}{0}</DIV>{1}{0}{0}<BR>{1}"
                , '\t'
                , '\n');

            return htmlText;
        }

        public static string GetQCElement(
            string Title, string TitleStyle,
            string PictureFileName, DataTable Table, int Border,
            int TabSpaces, int CellPadding)
        {
            string htmlText = string.Format("{0}{0}{0}<DIV>{1}" +
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
                , '\t'
                , '\n'
                , Title
                , TitleStyle
                , GetPictureCode(PictureFileName, true, "", null, null)
                , GetTableHtml(Table, null, Border, TabSpaces, CellPadding)
                );

            return htmlText;
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
            SQLiteHandler sqlHandler = new SQLiteHandler {
                DatabaseFileName = DatabaseFileName
            };

            string sql = string.Format(
                "Select Alias, Dataset, Dataset_ID FROM {0} ORDER BY Alias",
                TableName);
            if (sqlHandler.TableExists(TableName))
            {
                return sqlHandler.SelectTable(sql);
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
        /// <param name="imageFileName">Name of image file, e.g. MyHeatmap.png</param>
        /// <param name="addPlotsDirectory">Whether or not to add the 'Plots/' directory before image file name</param>
        /// <param name="cssCode"></param>
        /// <param name="width">Width of image, if null then set to 400</param>
        /// <param name="height">Height of image, if null then set to 400</param>
        /// <returns>HTML code</returns>
        public static string GetPictureCode(string imageFileName, bool addPlotsDirectory, string cssCode, int? width, int? height)
        {
            return string.Format("{0}{0}{0}{0}<A {4}HREF='{2}{3}'>" +
                "<IMG src='{2}{3}' width='{5}' height='{6}' /></A>{1}"
                , '\t'
                , '\n'
                , addPlotsDirectory ? "Plots/" : ""
                , imageFileName
                , !string.IsNullOrEmpty(cssCode) ? "class='" + cssCode + "' " : ""
                , width != null ? width.ToString() : "400"
                , height != null ? height.ToString() : "400");
        }

        /// <summary>
        /// End BODY and HTML elements
        /// </summary>
        /// <returns></returns>
        public static string GetEndBodyEndHtml()
        {
            return string.Format("{0}{0}</DIV>{1}{0}</BODY>{1}</HTML>{1}"
                , '\t'
                , '\n'
                );
        }

        /// <summary>
        /// Generates the html code to display a table on the webpage
        /// </summary>
        /// <param name="table">Table to display</param>
        /// <param name="alignment">How to align the table on the page, eg. left, center, right</param>
        /// <param name="border">Size of border</param>
        /// <param name="TabSpaces">Number of tab spaces before the table tag</param>
        /// <returns></returns>
        public static string GetTableHtml(DataTable table, string alignment, int? border, int? tabSpaces, int? cellPadding)
        {
            string tableTabs = "";
            string rowHtml = "";

            for (int i = 0; i < tabSpaces; i++)
            {
                tableTabs += '\t';
                rowHtml += '\t';
            }
            rowHtml += '\t';

            string htmlText = string.Format(
                tableTabs + "<TABLE {0}border='{1}' " +
                "CELLPADDING={2}>" +
                '\n',
                !string.IsNullOrEmpty(alignment) ? "align='" + alignment + "' " : "",
                border != null ? border.ToString() : "0",
                cellPadding != null ? cellPadding.ToString() : "4");

            htmlText += rowHtml + "<THEAD>\n" +
                rowHtml + "<TR>";

            // Write the table headers
            foreach (DataColumn c in table.Columns)
            {
                htmlText += "<TH><B><P align='center'>" + c.ColumnName + "</P></B></TH>";
            }
            htmlText += "</TR>\n" + rowHtml + "</THEAD>\n";

            int rowNum = 0;
            foreach (DataRow r in table.Rows)
            {
                rowNum++;
                if (rowNum % 2 == 0)
                    htmlText += rowHtml + "<TR bgcolor='lightblue'>";
                else
                    htmlText += rowHtml + "<TR>";
                for (int c = 0; c < table.Columns.Count; c++)
                {
                    htmlText += "<TD><P align='center'>" + r[c].ToString() + "</P></TD>";
                }
                htmlText += "</TR>\n";
            }

            htmlText += tableTabs + "</TABLE>\n\n";
            return htmlText;
        }
        #endregion
    }
}
