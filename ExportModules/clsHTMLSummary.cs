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
using System.IO;
using System.Linq;
using System.Text;

using RDotNet;
using log4net;

namespace Cyclops.ExportModules
{
    /// <summary>
    /// Required Parameters:
    /// tableName:  name of the table to use
    /// workDir:    working directory, typically included automatically
    /// 
    /// includeHeatmap: true or false, whether to include the heatmap in the html file
    /// </summary>
    public class clsHTMLSummary : clsBaseExportModule
    {
        #region Members
        private ExportModules.cslExportParameterHandler esp =
            new ExportModules.cslExportParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        private string s_LineDelimiter = "\n";
        private string s_Tab = "\t";

        private string s_RInstance;
        #endregion

        #region Constructors
        /// <summary>
        /// Exports an HTML summary file of the pipeline and workflow
        /// </summary>
        public clsHTMLSummary()
        {
            ModuleName = "HTML Summary Module";
        }
        /// <summary>
        /// Exports an HTML summary file of the pipeline and workflow
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsHTMLSummary(string InstanceOfR)
        {
            ModuleName = "HTML Summary Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Exports an HTML summary file of the pipeline and workflow
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsHTMLSummary(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "HTML Summary Module";
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
                traceLog.Error("ERROR HTML SUMMARY FILE: 'tableName' was not found in the passed parameters");
                b_2Param = false;
            }
            if (!esp.HasWorkDirectory)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR HTML SUMMARY FILE: 'workDir' was not found in the passed parameters");
                b_2Param = false;
            }

            return b_2Param;
        }

        /// <summary>
        /// Runs module
        /// </summary>
        public override void PerformOperation()
        {
            traceLog.Info("Preparing HTML summary file...");

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
            // TODO: Build Script
            
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
        #endregion
    }
}
