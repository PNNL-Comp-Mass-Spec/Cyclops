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
using System.Threading;

using RDotNet;
using log4net;

namespace Cyclops
{
    /// <summary>
    /// Performs the Beta-Binomial Model analysis on spectral count datasets
    /// </summary>
    public class clsBetaBinomialModelModule : clsBaseDataModule
    {
        private string s_RInstance, s_ResultTableName="",
            s_SpectralCountTable="", s_Factor="",
            s_Theta="TRUE";
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        #region Constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        public clsBetaBinomialModelModule()
        {
            ModuleName = "Beta-Binomial Model Module";
        }
        /// <summary>
        /// Constructor that requires the instance of the R workspace
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        public clsBetaBinomialModelModule(string InstanceOfR)
        {
            ModuleName = "Beta-Binomial Model Module";
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Members

        #endregion

        #region Properties

        #endregion

        #region Methods
        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            
            string s_RStatement = "";

            // Check if the package is already installed, if not install it
            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "BetaBinomial"))
            {
                traceLog.Error("BetaBinomial Package not loaded into R!");
                // TODO: INSTALL THE BETABINOMIAL PACKAGE FROM ZIP
            }


            if (CheckPassedParameters())
            {
                PerformBetaBinomialAnalysis();
            }

            
            RunChildModules();
        }

        /// <summary>
        /// Checks the dictionary to ensure all the necessary parameters are present
        /// </summary>
        /// <returns>True if all necessary parameters are present</returns>
        protected bool CheckPassedParameters()
        {
            // NECESSARY PARAMETERS
            if (Parameters.ContainsKey("resultTableName"))
                s_ResultTableName = Parameters["resultTableName"];
            else
            {
                traceLog.Error("BetaBinomial class: 'resultTableName' was not found in the passed parameters");
                return false;
            }
            if (Parameters.ContainsKey("spectralCountTable"))
                s_SpectralCountTable = Parameters["spectralCountTable"];
            else
            {
                traceLog.Error("BetaBinomial class: 'spectralCountTable' was not found in the passed parameters");
                return false;
            }
            if (Parameters.ContainsKey("factor"))
                s_Factor = Parameters["factor"];
            else
            {
                traceLog.Error("BetaBinomial class: 'factor' was not found in the passed parameters");
                return false;
            }
            if (Parameters.ContainsKey("theta"))
                s_Theta = Parameters["theta"];
            else
            {
                traceLog.Error("BetaBinomial class: 'theta' was not found in the passed parameters");
                return false;
            }

            return true;
        }

        private void PerformBetaBinomialAnalysis()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            try
            {
                string[] s_FactorsComplete = s_Factor.Split('$');
                GetOrganizedFactorsVector(s_RInstance, Parameters["spectralCountTable"],
                    s_FactorsComplete[0], s_FactorsComplete[1]);

                // Make sure that the factors table contains the field to perform the comparison
                if (clsGenericRCalls.GetColumnNames(s_RInstance, s_FactorsComplete[0]).Contains(s_FactorsComplete[1]))
                {
                    string s_RStatement = string.Format(
                        "require(BetaBinomial)\n" +
                        "{1} <- data.matrix({1})\n" +
                        "{1}[is.na({1})] <- 0\n" +
                        "sink(\"\")\n" +
                        "tmp <- largescale.bb.test({1}, {2}, " +
                        "theta.equal={3})\n" +
                        "sink()\n" +
                        "{0} <- cbind(\"pValue\"=tmp, {1})\n" +
                        "colnames({0})[1] <- \"pValue\"\n" +
                        "rm(tmp)",
                        s_ResultTableName,
                        s_SpectralCountTable,
                        s_Factor,
                        s_Theta
                        );
                    engine.EagerEvaluate(s_RStatement);
                }
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR Performing BetaBinomial Analysis: " + exc.ToString());
            }

        }
        #endregion
    }
}
