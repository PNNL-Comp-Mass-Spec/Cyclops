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
using System.Text;
using System.Collections.Generic;
using System.IO;

using RDotNet;

namespace Cyclops.DataModules
{
    public class clsCleanUpRSourceFileObjects : clsBaseDataModule
    {
        protected string s_RInstance;

        #region Constructors
        /// <summary>
        /// Cleans up the R Workspace, methods brought in by the R source scripts
        /// </summary>
        public clsCleanUpRSourceFileObjects()
        {
            ModuleName = "Cleanup R Source File Objects Module";
        }
        /// <summary>
        /// Cleans up the R Workspace, methods brought in by the R source scripts
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsCleanUpRSourceFileObjects(string InstanceOfR)
        {
            ModuleName = "Cleanup R Source File Objects Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Cleans up the R Workspace, methods brought in by the R source scripts
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsCleanUpRSourceFileObjects(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Cleanup R Source File Objects Module";
            Model = TheCyclopsModel;
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Methods
        /// <summary>
        ///  Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            CleanRSource();

            RunChildModules();
        }

        public void CleanRSource()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            string s_RStatement = "rm(list=objects2delete)\nrm(objects2delete)";
            engine.EagerEvaluate(s_RStatement);
        }

        /// <summary>
        /// Unit Test for Cleaning R Source Files
        /// </summary>
        /// <returns>Information regarding the result of the UnitTest</returns>
        public clsTestResult TestCleanRWorkspace()
        {
            clsTestResult result = new clsTestResult(true, "");

            try
            {                
                CleanRSource();

                // Confirm by testing if the new table exists within the environment
                List<string> l_SourceFilesLoaded = new List<string>();
                l_SourceFilesLoaded.Add("anovaPvals");
                l_SourceFilesLoaded.Add("calcFoldChanges");
                l_SourceFilesLoaded.Add("cleanmatrix.1");
                l_SourceFilesLoaded.Add("CorrelationMatrix");
                l_SourceFilesLoaded.Add("dataBoxPlots");
                l_SourceFilesLoaded.Add("DataCleaning");
                l_SourceFilesLoaded.Add("DetectOutliers");
                l_SourceFilesLoaded.Add("dist2");
                l_SourceFilesLoaded.Add("DoAnova");
                l_SourceFilesLoaded.Add("doLinearRegression");
                l_SourceFilesLoaded.Add("doLOESSreplicates");
                l_SourceFilesLoaded.Add("DoNonPara");
                l_SourceFilesLoaded.Add("DoTamuQ");
                l_SourceFilesLoaded.Add("eigen_pi");
                l_SourceFilesLoaded.Add("factor.values");
                l_SourceFilesLoaded.Add("filter.y");
                l_SourceFilesLoaded.Add("filterMissing");
                l_SourceFilesLoaded.Add("get_coefs");
                l_SourceFilesLoaded.Add("get_cutoff_informative_peptides");
                l_SourceFilesLoaded.Add("Get_dMr_Plot");
                l_SourceFilesLoaded.Add("GetBoxPlot");
                l_SourceFilesLoaded.Add("GetCorrHeatMap");
                l_SourceFilesLoaded.Add("GetKurtosis");
                l_SourceFilesLoaded.Add("GetMAD");
                l_SourceFilesLoaded.Add("GetMissingness");
                l_SourceFilesLoaded.Add("GetSkew");
                l_SourceFilesLoaded.Add("hclust2");
                l_SourceFilesLoaded.Add("IntraIndividualZscore");
                l_SourceFilesLoaded.Add("jnb_Aggregate");
                l_SourceFilesLoaded.Add("jnb_FoldChangeSpectralCountAndPackage");
                l_SourceFilesLoaded.Add("jnb_GetHeatmapMatrix");
                l_SourceFilesLoaded.Add("jnb_Log2Ratio");
                l_SourceFilesLoaded.Add("jnb_NormalizeSpectralCounts");
                l_SourceFilesLoaded.Add("jnb_Zscore");
                l_SourceFilesLoaded.Add("jnbIsPackageInstalled");
                l_SourceFilesLoaded.Add("KWPvals");
                l_SourceFilesLoaded.Add("lappend");
                l_SourceFilesLoaded.Add("LinReg_normalize");
                l_SourceFilesLoaded.Add("loess_normalize");
                l_SourceFilesLoaded.Add("MBfilter");
                l_SourceFilesLoaded.Add("MBimpute");
                l_SourceFilesLoaded.Add("MBimpute.dialog");
                l_SourceFilesLoaded.Add("MeanCenter.Div");
                l_SourceFilesLoaded.Add("MeanCenter.Sub");
                l_SourceFilesLoaded.Add("my.Psi");
                l_SourceFilesLoaded.Add("my.Psi.dash");
                l_SourceFilesLoaded.Add("objects2delete");
                l_SourceFilesLoaded.Add("OneSampleTtest");
                l_SourceFilesLoaded.Add("phi");
                l_SourceFilesLoaded.Add("plot.more.stuff");
                l_SourceFilesLoaded.Add("plot_hist");
                l_SourceFilesLoaded.Add("plot_qq");
                l_SourceFilesLoaded.Add("plotCurrProt.RefRup");
                l_SourceFilesLoaded.Add("PlotTamuQ");
                l_SourceFilesLoaded.Add("protein.Rrollup");
                l_SourceFilesLoaded.Add("protein_var");
                l_SourceFilesLoaded.Add("qqline.1");
                l_SourceFilesLoaded.Add("qqplot.1");
                l_SourceFilesLoaded.Add("remove.outliers");
                l_SourceFilesLoaded.Add("rnorm.trunc");
                l_SourceFilesLoaded.Add("RobustPCA");
                l_SourceFilesLoaded.Add("rollup.score");
                l_SourceFilesLoaded.Add("RRollup.proteins");
                l_SourceFilesLoaded.Add("splitForAnova");
                l_SourceFilesLoaded.Add("splitmissing.factor");
                l_SourceFilesLoaded.Add("splitmissing.fLevel");
                l_SourceFilesLoaded.Add("testShapiroWilks");
                l_SourceFilesLoaded.Add("Ttest");
                l_SourceFilesLoaded.Add("WilcoxPvals");

                List<string> l_NotLoaded = new List<string>();
                List<string> l_CurrentlyLoaded = clsGenericRCalls.ls(s_RInstance);
                foreach (string s in l_SourceFilesLoaded)
                {
                    if (l_CurrentlyLoaded.Contains(s))
                    {
                        l_NotLoaded.Add(s);
                    }
                }

                if (l_NotLoaded.Count > 0)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR CLEANING SOURCE FILES:\n" +
                        "The following methods were not cleaned from the current workspace:\n";
                    foreach (string s in l_NotLoaded)
                    {
                        result.Message += s + "\n";
                    }
                    return result;
                }
            }
            catch (Exception exc)
            {
                result.IsSuccessful = false;
                result.Message = "ERROR CLEANING R SOURCE FILES: " + "\n\n" + exc.ToString();
            }

            return result;
        }
        #endregion        
    }
}
