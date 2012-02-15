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

using RDotNet;
using log4net;

namespace Cyclops.ExportModules
{
    public class clsQC_Fraction_HTML : clsBaseExportModule
    {
        private ExportModules.cslExportParameterHandler esp =
            new ExportModules.cslExportParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        private string s_LineDelimiter = "\n";
        private string s_Tab = "\t";

        private string s_RInstance;

        #region Constructors
        /// <summary>
        /// Exports an HTML file that displays the QC for 2D-LC fractions
        /// </summary>
        public clsQC_Fraction_HTML()
        {
            ModuleName = "Quality Control Module";
        }
        /// <summary>
        /// /// Exports an HTML file that displays the QC for 2D-LC fractions
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsQC_Fraction_HTML(string InstanceOfR)
        {
            ModuleName = "Quality Control Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// /// Exports an HTML file that displays the QC for 2D-LC fractions
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
            traceLog.Info("Preparing HTML file for 2D-LC Fraction QC...");

            BuildHtmlFile();            
        }

        private void RunQC_Analysis()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            string s_RStatement = string.Format("{0} <- ja_OverlapMatrix(" +
                "x={1}, y=unique({1}$Fraction))",
                esp.NewTableName,
                esp.TableName);

            try
            {
                traceLog.Info("Executing QC Fraction Overlap Call in R: " +
                    s_RStatement);
                engine.EagerEvaluate(s_RStatement);
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR Executing QC Fraction Overlap Call in R: " +
                    exc.ToString());
                Model.SuccessRunningPipeline = false;
            }
        }

        private void BuildHtmlFile()
        {
            esp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                RunQC_Analysis();

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
                esp.NewTableName,
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

            // TODO : Get the Arrays from R needed to design the rectangles
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
                s_Tab + s_Tab + s_Tab + "ctx.fillText('Fractions', ctx.canvas.width / 2, Margin);" + s_LineDelimiter +
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
                s_Tab + s_Tab + s_Tab + s_Tab + "ctx.fillText('F' + i_Counter, i*rectWidth + rectWidth/2," +
					"rectHeight + rectHeight/2);" + s_LineDelimiter +
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
                s_Tab + s_Tab + s_Tab + "ctx.fillStyle='rgb(173,216,230)';" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "ctx.fillRect(0, 4*rectHeight, ctx.canvas.width, overlapHeight);" + s_LineDelimiter +
			    s_Tab + s_Tab + s_Tab + "ctx.strokeRect(0, 4*rectHeight, ctx.canvas.width, overlapHeight);" + s_LineDelimiter +
			    s_Tab + s_Tab + s_Tab + "ctx.fillStyle = 'rgb(0,0,0)';" + s_LineDelimiter +
                s_Tab + s_Tab + s_Tab + "ctx.fillText(totalPeptides, ctx.canvas.width/2, 4.5*rectHeight, ctx.canvas.width);" + s_LineDelimiter +
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
            s_Body += s_Tab + s_Tab + "<CANVAS id='myCanvas' width='600' height='300'>"
                + "Your browser does not support the 'CANVAS' tag.</CANVAS>" + s_LineDelimiter;
            s_Body += s_Tab + "</BODY>" + s_LineDelimiter;
            return s_Body;
        }

        private string WriteEndHtml()
        {
            string s_End = "</HTML>";
            return s_End;    
        }

        #endregion
    }
}
