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

using log4net;
using RDotNet;

namespace Cyclops.DataModules
{
    /// <summary>
    /// Performs QuasiTel statistical analysis on spectral count data
    /// </summary>
    public class clsQuasiTel : clsBaseDataModule
    {
        private string s_RInstance, s_Current_R_Statement = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        private int i_NumberOfFactors = 2;

        #region Constructors
        /// <summary>
        /// Performs a QuasiTel Statistical Analysis on Spectral Count data.
        /// </summary>
        public clsQuasiTel()
        {
            ModuleName = "QuasiTel Module";
        }
        /// <summary>
        /// Performs a QuasiTel Statistical Analysis on Spectral Count data.
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsQuasiTel(string InstanceOfR)
        {
            ModuleName = "QuasiTel Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Performs a QuasiTel Statistical Analysis on Spectral Count data.
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsQuasiTel(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "QuasiTel Module";
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
            if (Model.SuccessRunningPipeline)
            {
                Model.IncrementStep(ModuleName);

                string rhome = System.Environment.GetEnvironmentVariable("R_HOME");
                if (string.IsNullOrEmpty(rhome))
                    rhome = @"C:\Program Files\R\R-2.13.1";

                System.Environment.SetEnvironmentVariable("R_HOME", rhome);
                System.Environment.SetEnvironmentVariable("PATH", System.Environment.GetEnvironmentVariable("PATH") + ";" + rhome + @"\bin\i386");

                dsp.GetParameters(ModuleName, Parameters);

                traceLog.Info("Cyclops performing QuasiTel Analysis");

                if (CheckPassedParameters())
                {
                    PerformQuasiTelAnalysis();
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
            bool b_2Pass = true;

            // NECESSARY PARAMETERS
            if (!dsp.HasNewTableName)
            {
                //Model.SuccessRunningPipeline = false;
                traceLog.Error("QuasiTel class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                //Model.SuccessRunningPipeline = false;
                traceLog.Error("QuasiTel class: 'inputTableName': \"" +
                dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            if (Parameters.ContainsKey("Fixed_Effect"))
            {
                dsp.FactorColumn = Parameters["Fixed_Effect"];

                if (string.IsNullOrEmpty(dsp.FactorColumn))
                {
                    b_2Pass = false;
                    return b_2Pass;
                }
            }

            if (!dsp.HasFactorTable)
            {
                //Model.SuccessRunningPipeline = false;
                traceLog.Error("QuasiTel class: 'factorTable': \"" +
                    dsp.FactorComplete + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            if (!dsp.HasFactorComplete)
            {
                //Model.SuccessRunningPipeline = false;
                traceLog.Error("QuasiTel class: 'factor': \"" +
                    dsp.FactorComplete + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            // Grab the number of factors
            i_NumberOfFactors = 0;
            if (!string.IsNullOrEmpty(dsp.FactorTable) &
                !string.IsNullOrEmpty(dsp.FactorColumn))
            {
                i_NumberOfFactors = clsGenericRCalls.GetUniqueLengthOfColumn(s_RInstance,
                     dsp.FactorTable, dsp.FactorColumn);
            }

            if (i_NumberOfFactors == 0)
            {
                b_2Pass = false;
            }
            else if (i_NumberOfFactors < 2)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("QuasiTel class: 'factor': \"" +
                    dsp.FactorComplete + "\", contains less than 2 values. " +
                    "QuasiTel can not make a comparison");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        private void PerformQuasiTelAnalysis()
        {
            string s_TmpDataTable = "tmpDataTable_" + DateTime.Now.ToString("MM_dd_yy_hh_mm_ss_");
            string s_TmpFactorTable = "tmpFactorTable_" + DateTime.Now.ToString("MM_dd_yy_hh_mm_ss_");
            string s_TmpFactor1 = "tmpFactor1_" + DateTime.Now.ToString("MM_dd_yy_hh_mm_ss_");
            string s_TmpFactor2 = "tmpFactor2_" + DateTime.Now.ToString("MM_dd_yy_hh_mm_ss_");

            int i_ColNum = 0, i_FactorCnt = 0;

            try
            {
                string s_RStatement = "";
                if (dsp.RemovePeptideColumn)
                {
                    s_RStatement = string.Format(
                        "{0} <- data.matrix({1}[,2:ncol({1})])\n",
                        s_TmpDataTable,
                        dsp.InputTableName);
                }

                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "QuasiTel Analysis",
                    Model.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;

                GetOrganizedFactorsVector(s_RInstance, s_TmpDataTable,
                    dsp.FactorTable, dsp.FactorColumn, Model.StepNumber,
                    Model.NumberOfModules, "Alias");

                // Make sure that the factors table contains the field to perform the comparison
                if (clsGenericRCalls.GetColumnNames(s_RInstance, dsp.FactorTable).Contains(dsp.FactorColumn))
                {
                    i_ColNum = clsGenericRCalls.GetColumnNames(s_RInstance, s_TmpDataTable).Count;
                    i_FactorCnt = clsGenericRCalls.GetLengthOfVector(s_RInstance, dsp.FactorComplete);
                    if (i_ColNum == i_FactorCnt)
                    {

                        List<string> l_Factors = clsGenericRCalls.GetUniqueColumnElementsWithinTable(s_RInstance,
                            dsp.FactorTable, dsp.FactorColumn);

                        // setup the pairwise comparisons
                        for (int i = 0; i < l_Factors.Count - 1; i++)
                        {
                            for (int j = 1; j < l_Factors.Count; j++)
                            {
                                string s_ComparisonTableName = "QuasiTel_" +
                                    l_Factors[i] + "_v_" + l_Factors[j];

                                if (!l_Factors[i].Equals(l_Factors[j]))
                                {

                                    // grab the variables
                                    s_RStatement = string.Format(
                                        "{0} <- as.vector(unlist(subset({1}, " +
                                        "{2} == '{3}' | {2} == '{4}', " +
                                        "select=c('Alias'))))\n",
                                        s_TmpFactorTable,
                                        dsp.FactorTable,
                                        dsp.FactorColumn,
                                        l_Factors[i],
                                        l_Factors[j]);
                                    // grab the relevant data
                                    s_RStatement += string.Format(
                                        "{0} <- {1}[,which(colnames({1}) %in% {2})]\n",
                                        s_TmpDataTable,
                                        dsp.InputTableName,
                                        s_TmpFactorTable);
                                    // 0 out the null values
                                    s_RStatement += string.Format(
                                        "{0} <- data.matrix({0})\n" +
                                        "{0}[is.na({0})] <- 0\n",
                                        s_TmpDataTable);
                                    // get the column names to pass in as factors
                                    s_RStatement += string.Format(
                                        "{0} <- as.vector(unlist(subset({1}, " +
                                        "{2} == '{3}', select=c('Alias'))))\n",
                                        s_TmpFactor1,
                                        dsp.FactorTable,
                                        dsp.FactorColumn,
                                        l_Factors[i]);
                                    s_RStatement += string.Format(
                                        "{0} <- as.vector(unlist(subset({1}, " +
                                        "{2} == '{3}', select=c('Alias'))))\n",
                                        s_TmpFactor2,
                                        dsp.FactorTable,
                                        dsp.FactorColumn,
                                        l_Factors[j]);
                                    // run the analysis
                                    s_RStatement += string.Format(
                                        "{0} <- quasitel({1}, {2}, {3})\n",
                                        s_ComparisonTableName,
                                        s_TmpDataTable,
                                        s_TmpFactor1,
                                        s_TmpFactor2);
                                    // remove temp tables                                
                                    s_RStatement += string.Format(
                                        "rm({0})\nrm({1})\n" +
                                        "rm({2})\nrm({3})\n",
                                        s_TmpDataTable,
                                        s_TmpFactorTable,
                                        s_TmpFactor1,
                                        s_TmpFactor2);

                                    if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                                        "Running QuasiTel Analysis",
                                        Model.StepNumber, Model.NumberOfModules))
                                        Model.SuccessRunningPipeline = false;
                                }
                            }
                        }

                        //clsLink LinkUpWithBBM = LinkUpWithBetaBinomialModelWithQuasiTel(s_RInstance);
                        //if (LinkUpWithBBM.Run)
                        //{
                        //    traceLog.Info("Preparing to Linking up with BBM Results: " + LinkUpWithBBM.Statement);
                        //    engine.EagerEvaluate(LinkUpWithBBM.Statement);
                        //}
                    }
                    else
                    {
                        Model.SuccessRunningPipeline = false;
                        traceLog.Error(string.Format(
                            "ERROR QuasiTel class: Dimensions of spectral count table ({0}) " +
                            "do not match the dimensions of your factor vector ({1})",
                            i_ColNum,
                            i_FactorCnt));
                    }
                }
                else
                {
                    Model.SuccessRunningPipeline = false;
                    traceLog.Error(string.Format("ERROR QuasiTel class: The factors table does not " +
                        "contain the factor, {0}", dsp.FactorColumn));
                }
            }
            catch (Exception exc)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR Performing QuasiTel Analysis: " + exc.ToString());
            }
        }

        

        /// <summary>
        /// Unit Test for QuasiTel
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
                    result.Message = "ERROR QuasiTel: Not all required parameters were passed in!";
                    return result;
                }

                // input table has 1,162 rows, so trim the table down before running the analysis
                REngine engine = REngine.GetInstanceFromID(s_RInstance);
                engine.EagerEvaluate("t <- " + dsp.InputTableName + "[c(1:4),]");

                dsp.InputTableName = "t";

                PerformQuasiTelAnalysis();

                // Confirm by testing if the new table exists within the environment
                if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.NewTableName))
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR QuasiTel MODEL: After running beta-binomial model " +
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
