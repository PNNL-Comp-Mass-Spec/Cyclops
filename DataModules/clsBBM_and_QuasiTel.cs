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

using log4net;
using RDotNet;

namespace Cyclops.DataModules
{
    /// <summary>
    /// Performs the Beta-Binomial Model and QuasiTel analyses
    /// and combines the results
    /// </summary>
    public class clsBBM_and_QuasiTel : clsBaseDataModule
    {
        #region Members
        private string s_RInstance, s_Current_R_Statement = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Constructors
        /// <summary>
        /// Performs BetaBinomial and QuasiTel Statistical Analyses on Spectral Count data.
        /// </summary>
        public clsBBM_and_QuasiTel()
        {
            ModuleName = "Beta-Binomial Model And QuasiTel Module";
        }
        /// <summary>
        /// Performs BetaBinomial and QuasiTel Statistical Analyses on Spectral Count data.
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsBBM_and_QuasiTel(string InstanceOfR)
        {
            ModuleName = "Beta-Binomial Model And QuasiTel Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Performs BetaBinomial and QuasiTel Statistical Analyses on Spectral Count data.
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsBBM_and_QuasiTel(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Beta-Binomial Model And QuasiTel Module";
            Model = TheCyclopsModel;
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            if (Model.SuccessRunningPipeline)
            {
                Model.IncrementStep(ModuleName);

                dsp.GetParameters(ModuleName, Parameters);
                
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
                    PerformAnalyses();
                }


                RunChildModules();
            }
        }

        /// <summary>
        /// Checks the dictionary to ensure all the necessary parameters are present
        /// </summary>
        /// <returns>True if all necessary parameters are present</returns>
        protected bool CheckPassedParameters()
        {
            bool b_2Pass = true, b_FactorTablePresent = true;

            // NECESSARY PARAMETERS
            if (string.IsNullOrEmpty(dsp.NewTableName))
            {
                //Model.SuccessRunningPipeline = false;
                traceLog.Error("BetaBinomial/QuasiTel class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (string.IsNullOrEmpty(dsp.InputTableName))
            {
                //Model.SuccessRunningPipeline = false;
                traceLog.Error("BetaBinomial/QuasiTel class: 'inputTableName': \"" +
                dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.InputTableName))
            {
                traceLog.Error(
                    string.Format("BetaBinomial/QuasiTel class: inputTableName: '{0}' " +
                    " was not found in the R environment",
                    dsp.InputTableName));
                b_2Pass = false;
            }
            if (string.IsNullOrEmpty(dsp.FactorTable))
            {
                traceLog.Error("BetaBinomial class: 'factorTable': \"" +
                    dsp.FactorComplete + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.FactorTable))
            {
                traceLog.Error(
                    string.Format("BetaBinomial/QuasiTel class: factorTable: '{0}' " +
                    " was not found in the R environment",
                    dsp.FactorTable));
                b_2Pass = false;
                b_FactorTablePresent = false;
            }
            if (Parameters.ContainsKey("Fixed_Effect"))
            {
                dsp.FactorColumn = Parameters["Fixed_Effect"];

                if (string.IsNullOrEmpty(dsp.FactorColumn))
                    b_2Pass = false;
            }
            if (b_FactorTablePresent)
            {
                if (!clsGenericRCalls.TableContainsColumn(s_RInstance,
                    dsp.FactorTable, dsp.FactorColumn))
                {
                    traceLog.Error(string.Format("Hexbin class: " +
                        "FixedEffect: '{0}' was not found in the table: '{1}'",
                        dsp.FactorColumn,
                        dsp.FactorTable));
                    b_2Pass = false;
                }
            }
            if (!dsp.HasTheta)
            {
                traceLog.Info("BetaBinomial/QuasiTel class: 'theta': \"" +
                    dsp.Theta + "\", was not found in the passed parameters, and " +
                    "automatically assigned 'TRUE'");
            }

            return b_2Pass;
        }

        private void PerformAnalyses()
        {
            string s_TmpFactorVariable = "tmpTable" + DateTime.Now.ToString("MM_dd_yy_hh_mm_ss_");

            try
            {
                string s_TmpColMetadataTable = GetOrganizedFactorsVector(
                    s_RInstance, dsp.InputTableName,
                    dsp.FactorTable, dsp.FactorColumn, Model.StepNumber,
                    Model.NumberOfModules, "Alias");

                string s_RStatement = "";
                if (dsp.RemovePeptideColumn)
                {
                    s_RStatement += string.Format(
                        "{0}_tmpT <- data.matrix({0}[,2:ncol({0})])\n",
                        dsp.InputTableName);
                    dsp.InputTableName = dsp.InputTableName + "_tmpT";
                }

                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "BETA-BINOMIAL/QUASITEL MODEL",
                    Model.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;
                
                // Make sure that the factors table contains the field to perform the comparison
                if (clsGenericRCalls.GetColumnNames(s_RInstance, dsp.FactorTable).Contains(dsp.FactorColumn))
                {
                    int i_ColNum = clsGenericRCalls.GetColumnNames(s_RInstance, dsp.InputTableName).Count;
                    int i_FactorCnt = clsGenericRCalls.GetLengthOfVector(s_RInstance, dsp.FactorComplete);
                    if (i_ColNum == i_FactorCnt)
                    {
                        s_RStatement = string.Format(
                        "{0} <- jnb_BBM_and_QTel(" +
                        "tData={1}, " +
                        "colMetadata={2}, " +
                        "colFactor='{3}', " +
                        "theta={4}, " +
                        "sinkFileName='')\n" +
                        "rm({2})\n",
                        dsp.NewTableName,
                        dsp.InputTableName,
                        s_TmpColMetadataTable,
                        dsp.FactorColumn,
                        dsp.Theta);

                        if (dsp.RemovePeptideColumn)
                        {
                            s_RStatement += string.Format("\nrm({0})",
                                dsp.InputTableName);
                        }

                        if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                            "BETA-BINOMIAL/QUASITEL MODEL",
                            Model.StepNumber, Model.NumberOfModules))
                            Model.SuccessRunningPipeline = false;
                    }
                    else
                    {
                        Model.SuccessRunningPipeline = false;
                        traceLog.Error(
                            string.Format("ERROR BetaBinomial/QuasiTel class: " +
                            "Dimensions of spectral count table " +
                            "do not match the dimensions of your factor vector\n" +
                            "Data table, '{0}', contains {1} columns\n" +
                            "Factor table, '{2}' contains {3} levels",
                            dsp.InputTableName,
                            i_ColNum,
                            dsp.FactorComplete,
                            i_FactorCnt));
                    }
                }
                else
                {
                    Model.SuccessRunningPipeline = false;
                    traceLog.Error(string.Format("ERROR Betabinomial/QuasiTel class: The factors table does not " +
                        "contain the factor, {0}", dsp.FactorColumn));
                }
            }
            catch (Exception exc)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR Performing BetaBinomial/QuasiTel Analysis: " + exc.ToString());
            }

        }
        #endregion
    }
}
