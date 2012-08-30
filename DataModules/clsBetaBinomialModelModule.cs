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

namespace Cyclops.DataModules
{
    /// <summary>
    /// Performs the Beta-Binomial Model analysis on spectral count datasets
    /// </summary>
    public class clsBetaBinomialModelModule : clsBaseDataModule
    {
        #region Members
        private string s_RInstance, s_Current_R_Statement = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Constructors
        /// <summary>
        /// Performs a BetaBinomial Statistical Analysis on Spectral Count data.
        /// </summary>
        public clsBetaBinomialModelModule()
        {
            ModuleName = "Beta-Binomial Model Module";
        }
        /// <summary>
        /// Performs a BetaBinomial Statistical Analysis on Spectral Count data.
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsBetaBinomialModelModule(string InstanceOfR)
        {
            ModuleName = "Beta-Binomial Model Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Performs a BetaBinomial Statistical Analysis on Spectral Count data.
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsBetaBinomialModelModule(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Beta-Binomial Model Module";
            Model = TheCyclopsModel;
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
            dsp.GetParameters(ModuleName, Parameters);

            traceLog.Info("Cyclops performing Beta-Binomial Model");

            string s_RStatement = "";

            // Check if the package is already installed, if not install it
            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "BetaBinomial"))
            {
                Model.SuccessRunningPipeline = false;
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
            bool b_2Pass = true;

            // NECESSARY PARAMETERS
            if (!dsp.HasNewTableName)
            {
                //Model.SuccessRunningPipeline = false;
                traceLog.Error("BetaBinomial class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                //Model.SuccessRunningPipeline = false;
                traceLog.Error("BetaBinomial class: 'inputTableName': \"" +
                dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }            
            if (!dsp.HasTheta)
            {
                traceLog.Error("BetaBinomial class: 'theta': \"" +
                    dsp.Theta + "\", was not found in the passed parameters, and " +
                    "automatically assigned 'TRUE'");
            }

            if (Parameters.ContainsKey("Fixed_Effect"))
            {
                dsp.FactorColumn = Parameters["Fixed_Effect"];

                if (string.IsNullOrEmpty(dsp.FactorColumn))
                    b_2Pass = false;
            }

            if (!dsp.HasFactorTable)
            {
                traceLog.Error("BetaBinomial class: 'factorTable': \"" +
                    dsp.FactorComplete + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            if (!dsp.HasFactorComplete)
            {
                traceLog.Error("BetaBinomial class: 'factor': \"" +
                    dsp.FactorComplete + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        private void PerformBetaBinomialAnalysis()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            string s_TmpFactorVariable = "tmpTable" + DateTime.Now.ToString("MM_dd_yy_hh_mm_ss_");

            try
            {
                string s_RStatement = "require(BetaBinomial)\n";
                if (dsp.RemovePeptideColumn)
                {
                    s_RStatement += string.Format(
                        "{0}_tmpT <- data.matrix({0}[,2:ncol({0})])\n",
                        dsp.InputTableName);
                    dsp.InputTableName = dsp.InputTableName + "_tmpT";
                }

                traceLog.Info("BETA-BINOMIAL MODEL: " + s_RStatement);
                engine.EagerEvaluate(s_RStatement);

                GetOrganizedFactorsVector(s_RInstance, dsp.InputTableName,
                    dsp.FactorTable, dsp.FactorColumn);

                // Make sure that the factors table contains the field to perform the comparison
                if (clsGenericRCalls.GetColumnNames(s_RInstance, dsp.FactorTable).Contains(dsp.FactorColumn))
                {
                    int i_ColNum = clsGenericRCalls.GetColumnNames(s_RInstance, dsp.InputTableName).Count;
                    int i_FactorCnt = clsGenericRCalls.GetLengthOfVector(s_RInstance, dsp.FactorComplete);
                    if (i_ColNum == i_FactorCnt)
                    {
                        s_RStatement = string.Format(
                        "{1} <- data.matrix({1})\n" +
                        "{1}[is.na({1})] <- 0\n" +
                        "sink(\"\")\n" +
                        "{4} <- largescale.bb.test({1}, {2}, " +
                        "theta.equal={3})\n" +
                        "sink()\n" +
                        "{0} <- cbind(\"pValue\"={4}, {1})\n" +
                        "colnames({0})[1] <- \"pValue\"\n" +
                        "rm({4})",
                        dsp.NewTableName,
                        dsp.InputTableName,
                        dsp.FactorComplete,
                        dsp.Theta,
                        s_TmpFactorVariable);

                        if (dsp.RemovePeptideColumn)
                        {
                            s_RStatement += string.Format("\nrm({0})",
                                dsp.InputTableName);
                        }

                        traceLog.Info("BETA-BINOMIAL MODEL: " + s_RStatement);

                        s_Current_R_Statement = s_RStatement;
                        engine.EagerEvaluate(s_RStatement);
                    }
                    else
                    {
                        Model.SuccessRunningPipeline = false;
                        traceLog.Error("ERROR BetaBinomial class: Dimensions of spectral count table " +
                            "do not match the dimensions of your factor vector");
                    }
                }
                else
                {
                    Model.SuccessRunningPipeline = false;
                    traceLog.Error(string.Format("ERROR Betabinomial class: The factors table does not " +
                        "contain the factor, {0}", dsp.FactorColumn));
                }

                //clsLink LinkUpWithQuasiTel = LinkUpWithBetaBinomialModelWithQuasiTel(s_RInstance);
                //if (LinkUpWithQuasiTel.Run)
                //{
                //    traceLog.Info("Preparing to Linking up with QuasiTel Results: " + LinkUpWithQuasiTel.Statement);
                //    engine.EagerEvaluate(LinkUpWithQuasiTel.Statement);
                //}


            }
            catch (Exception exc)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR Performing BetaBinomial Analysis: " + exc.ToString());
            }

        }

        /// <summary>
        /// Unit Test for Beta-binomial Model data
        /// </summary>
        /// <returns>Information regarding the result of the UnitTest</returns>
        public clsTestResult TestBetaBinomialModel()
        {
            dsp.GetParameters(ModuleName, Parameters);
            clsTestResult result = new clsTestResult(true, "");
            result.Module = ModuleName;

            try
            {
                if (!CheckPassedParameters())
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR BETA-BINOMIAL MODEL: Not all required parameters were passed in!";
                    return result;
                }

                // input table has 1,162 rows, so trim the table down before running the analysis
                REngine engine = REngine.GetInstanceFromID(s_RInstance);
                engine.EagerEvaluate("t <- " + dsp.InputTableName + "[c(1:4),]");

                dsp.InputTableName = "t";

                PerformBetaBinomialAnalysis();

                // Confirm by testing if the new table exists within the environment
                if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.NewTableName))
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR BETA-BINOMIAL MODEL: After running beta-binomial model " +
                        dsp.InputTableName +
                        ", the new table name could not be found within the R workspace";
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }

                System.Data.DataTable dt = clsGenericRCalls.GetDataTable(s_RInstance, dsp.NewTableName);
                if (dt.Columns.Count != 41)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR BETA-BINOMIAL MODEL: After running beta-binomial model, " +
                        "the new table was supposed to have 41 columns, and instead has " +
                        dt.Columns.Count;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
                if (dt.Rows.Count != 4)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR BETA-BINOMIAL MODEL: After running beta-binomial model, " +
                        "the new table was supposed to have 4 columns, and instead has " +
                        dt.Rows.Count;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
                double d1 = 0, d2 = 0, d3 = 0, d4 = 0;
                if (Double.TryParse(dt.Rows[0][0].ToString(), out d1))
                {
                    d1 = Math.Round(d1 * 1000);
                    if (d1 != 278)
                    {
                        result.IsSuccessful = false;
                        result.Message = "ERROR BETA-BINOMIAL MODEL: p-values are incorrect! " +
                            "From " + dsp.InputTableName + ", expected 0.278 and received " + (d1 / 1000);
                        result.R_Statement = s_Current_R_Statement;
                        return result;
                    }
                }
                if (Double.TryParse(dt.Rows[1][0].ToString(), out d2))
                {
                    d1 = Math.Round(d2 * 1000);
                    if (d2 != 198)
                    {
                        result.IsSuccessful = false;
                        result.Message = "ERROR BETA-BINOMIAL MODEL: p-values are incorrect! " +
                            "From " + dsp.InputTableName + ", expected 0.198 and received " + (d2 / 1000);
                        result.R_Statement = s_Current_R_Statement;
                        return result;
                    }
                }
                if (Double.TryParse(dt.Rows[2][0].ToString(), out d3))
                {
                    d1 = Math.Round(d3 * 1000);
                    if (d3 != 648)
                    {
                        result.IsSuccessful = false;
                        result.Message = "ERROR BETA-BINOMIAL MODEL: p-values are incorrect! " +
                            "From " + dsp.InputTableName + ", expected 0.648 and received " + (d3 / 1000);
                        result.R_Statement = s_Current_R_Statement;
                        return result;
                    }
                }
                if (Double.TryParse(dt.Rows[2][0].ToString(), out d4))
                {
                    d1 = Math.Round(d4 * 1000);
                    if (d4 != 631)
                    {
                        result.IsSuccessful = false;
                        result.Message = "ERROR BETA-BINOMIAL MODEL: p-values are incorrect! " +
                            "From " + dsp.InputTableName + ", expected 0.631 and received " + (d4 / 1000);
                        result.R_Statement = s_Current_R_Statement;
                        return result;
                    }
                }
            }
            catch (Exception exc)
            {
                result.IsSuccessful = false;
                result.Message = "ERROR BETA-BINOMIAL MODEL: " + dsp.InputFileName + "\n\n" + exc.ToString();
                result.R_Statement = s_Current_R_Statement;
            }

            return result;
        }
        #endregion
    }
}
