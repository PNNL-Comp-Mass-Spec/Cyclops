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

using RDotNet;
using log4net;

namespace Cyclops.DataModules
{
    /// <summary>
    /// Performs a melt call. Basically turns a crosstab tab into long form
    /// </summary>
    public class clsMelt : clsBaseDataModule
    {
        private string s_RInstance;
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        #region Constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        public clsMelt()
        {
            ModuleName = "Melt Module";
        }
        /// <summary>
        /// Constructor that requires the Name of the R instance
        /// </summary>
        /// <param name="InstanceOfR">Path to R DLL</param>
        public clsMelt(string InstanceOfR)
        {
            ModuleName = "Melt Module";
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        /// <summary>
        ///  Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            traceLog.Info("Entered Melt Class, Performing Melt...");
            if (CheckPassedParameters())
            {
                MeltData();
            }

            RunChildModules();
        }

        /// <summary>
        /// Checks the dictionary to ensure all the necessary parameters are present
        /// </summary>
        /// <returns>True if all necessary parameters are present</returns>
        protected bool CheckPassedParameters()
        {
            bool b_2Pass = true;

            // NECESSARY PARAMETERS
            if (!dsp.HasNewTableName)
            {
                traceLog.Error("BetaBinomial class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                traceLog.Error("BetaBinomial class: 'inputTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasCommaSeparatedListWithQuotes)
            {
                traceLog.Error("BetaBinomial class: 'commaSepWithQuotesIdentifiers': \"" +
                    dsp.CommaSeparatedWithQuotesIdentifiers + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        private void MeltData()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            // Construct the R statement
            string s_RStatement = "";
            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "reshape"))
                clsGenericRCalls.InstallPackage(s_RInstance, "reshape");

            s_RStatement = "require(reshape)\n";

            s_RStatement += string.Format("{0} <- melt({1}, id=c({2})",
                dsp.NewTableName,
                dsp.InputTableName,
                dsp.CommaSeparatedWithQuotesIdentifiers);

            try     
            {
                traceLog.Info("Melt class: " + s_RStatement);
                engine.EagerEvaluate(s_RStatement);
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR Melt Class: " + exc.ToString());
            }
        }

        /// <summary>
        /// Converts a list of strings to values separated by a given character
        /// </summary>
        /// <param name="Identifiers">List of strings</param>
        /// <param name="Separator">Character to separate the values</param>
        /// <param name="Quotes">Whether to wrap values in quotation marks</param>
        /// <returns>Separated values</returns>
        public string SeparateListOfStrings(List<string> Identifiers, char Separator, bool Quotes)
        {
            string s_Return = "";

            s_Return += Quotes ? "\"" : "";

            for (int i = 0; i < Identifiers.Count; i++)
            {
                s_Return += Identifiers[i];

                if (i < Identifiers.Count - 1)
                {
                    if (Quotes)
                        s_Return += "\"" + Separator + "\"";
                    else
                        s_Return += Separator;
                }
            }

            s_Return += Quotes ? "\"" : "";

            return s_Return;
        }
        #endregion
    }
}
