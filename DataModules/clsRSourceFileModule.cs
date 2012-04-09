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
using System.IO;
using System.Collections.Generic;

using RDotNet;
using log4net;

namespace Cyclops.DataModules
{
    /// <summary>
    /// Loads all the R source files in the directory and subdirectories
    /// </summary>
    public class clsRSourceFileModule : clsBaseDataModule
    {
        protected string s_RInstance;
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        #region Constructors
        /// <summary>
        /// Loads all necessary R scripts in the workspace
        /// </summary>
        public clsRSourceFileModule()
        {
            ModuleName = "R Source File Module - Loading R Functions";
        }
        /// <summary>
        /// Loads all necessary R scripts in the workspace
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsRSourceFileModule(string InstanceOfR)
        {
            ModuleName = "R Source File Module - Loading R Functions";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Loads all necessary R scripts in the workspace
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsRSourceFileModule(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "R Source File Module - Loading R Functions";
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
            Run_LoadRSourceFiles();

            RunChildModules();
        }

        /// <summary>
        /// Checks the dictionary to ensure all the necessary parameters are present
        /// </summary>
        /// <returns>True if all necessary parameters are present</returns>
        protected bool CheckPassedParameters()
        {
            bool b_2Pass = true;

            return b_2Pass;
        }

        /// <summary>
        /// Externally accessible function to load the R source files
        /// </summary>
        public void Run_LoadRSourceFiles()
        {
            dsp.GetParameters(ModuleName, Parameters);

            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            // Load ALL the files in the directory
            if (!dsp.HasSource)
            {
                if (dsp.HasWorkDir)
                {
                    try
                    {
                        GetDirectoriesAndLoadRSourceFiles(Path.Combine(
                            dsp.WorkDirectory, "R_Scripts"),
                            s_RInstance);
                    }
                    catch (Exception exc)
                    {
                        Model.SuccessRunningPipeline = false;
                        traceLog.Error("ERROR loading R script files.\nLocation = "
                            + Path.Combine(dsp.WorkDirectory, "R_Scripts") 
                            + "\nError Message = " + exc.ToString());
                    }
                }
            }
            else // Load the specified files
            {
                string[] s_Files = dsp.Source.Split(';');
                foreach (string s in s_Files)
                {
                    try
                    {
                        string s_Command = string.Format("source(\"{0}/{1}\")",
                            Directory.GetCurrentDirectory().Replace('\\', '/'), s.Replace('\\', '/'));
                        engine.EagerEvaluate(s_Command);
                    }
                    catch (ParseException pe)
                    {
                        Model.SuccessRunningPipeline = false;
                        traceLog.Error("ERROR LOADING R SCRIPTS: Encountered a Parse Excpetion: " 
                            + pe.ToString());
                    }
                }
            }
            string s_RStatement = string.Format("objects2delete <- ls()"); // index of all objects loaded into R from source files
            engine.EagerEvaluate(s_RStatement);
        }

        /// <summary>
        /// Iterative function that parses through directories, runs all R source files in the 
        /// directory, then iterates through the subdirectories
        /// </summary>
        /// <param name="MyDirectory">Path to Directory</param>
        /// <param name="RInstance">Instance of R Workspace</param>
        protected void GetDirectoriesAndLoadRSourceFiles(string MyDirectory, string RInstance)
        {
            foreach (string s in Directory.GetFiles(MyDirectory))
            {
                if (Path.GetExtension(s).ToUpper().Equals(".R"))
                    LoadRSourceFile(s, RInstance);
            }
            foreach (string s in Directory.GetDirectories(MyDirectory))
            {
                GetDirectoriesAndLoadRSourceFiles(s, RInstance);
            }
        }

        /// <summary>
        /// Loads a R source file (*.R) into the workspace environment
        /// </summary>
        /// <param name="MyFile">Name of the source file to load</param>
        /// <param name="RInstance">Instance of R Workspace</param>
        protected void LoadRSourceFile(string MyFile, string RInstance)
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            try
            {
                /// Visual Studios adds a 3 character format to the
                /// beginning of each ".R" file, so just need to
                /// clean it up before reading it.
                if (Parameters.ContainsKey("removeFirstCharacters"))
                {
                    if (Parameters["removeFirstCharacters"].ToString().Equals("true"))
                    {
                        StreamReader sr = new StreamReader(MyFile);
                        string s_Content = sr.ReadToEnd();
                        sr.Close();
                        //s_Content = s_Content.Remove(0, 2);
                        s_Content.Replace("ï»¿", "");
                        StreamWriter sw = new StreamWriter(MyFile);
                        sw.Write(s_Content);
                        sw.Close();
                    }
                }

                string s_Command = string.Format("source(\"{0}\")",
                    MyFile.Replace("\\", "/"));
                engine.EagerEvaluate(s_Command);
            }
            catch (ParseException pe)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR LOADING R SCRIPTS: Encountered a Parse Excpetion: "
                    + pe.ToString());
            }
        }

        /// <summary>
        /// Unit Test for Loading R Source Files
        /// </summary>
        /// <returns>Information regarding the result of the UnitTest</returns>
        public clsTestResult TestLoadRSourceFiles()
        {
            dsp.GetParameters(ModuleName, Parameters);
            clsTestResult result = new clsTestResult(true, "");

            try
            {
                if (!CheckPassedParameters())
                {
                    result.IsSuccessful = false;

                    result.Message = "ERROR LOADING R SOURCE FILES: Not all required parameters were passed in!";
                    return result;
                }

                Run_LoadRSourceFiles();

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
                    if (!l_CurrentlyLoaded.Contains(s))
                    {
                        l_NotLoaded.Add(s);
                    }
                }

                if (l_NotLoaded.Count > 0)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR LOADING SOURCE FILES:\n" +
                        "The following methods were not found in the current workspace:\n";
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
                result.Message = "ERROR LOADING R SOURCE FILES: " + "\n\n" + exc.ToString();
            }

            return result;
        }
        #endregion
    }
}
