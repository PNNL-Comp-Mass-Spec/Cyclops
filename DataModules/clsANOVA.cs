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
    /// Parameters:
    /// inputTableName: table to perform linear regression on
    /// newTableName: new table name
    /// mode: type of anova (includes 'msstats')
    /// rowMetadataTable: name of the row metadata table
    /// linkRow: column in row metadata table that links to the data table
    /// rowFactor: column in the row metadata that specifies the protein
    /// factorTable: name of column metadata table
    /// linkCol: column in column metadata table that links to column headers, default 'Alias'
    /// </summary>
    public class clsANOVA : clsBaseDataModule
    {
        #region Members
        private string s_RInstance, s_Current_R_Statement = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        private string s_MSstatsLibrary = "//gigasax/DMS_Workflows/Cyclops/MSstats.tar.gz";
        private string s_MSstatsDataTablename;
        #endregion

        #region Contructors
        /// <summary>
        /// Module to perform ANOVA
        /// </summary>
        public clsANOVA()
        {
            ModuleName = "ANOVA Module";
        }
        /// <summary>
        /// Module to perform ANOVA
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsANOVA(string InstanceOfR)
        {
            ModuleName = "ANOVA Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Module to perform ANOVA
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsANOVA(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "ANOVA Module";
            Model = TheCyclopsModel;
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        public override void PerformOperation()
        {
            ANOVA();

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
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: ANOVA class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: ANOVA class: 'inputTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            if (string.IsNullOrEmpty(dsp.Mode))
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: ANOVA class: 'mode' was not specified " +
                    "in the parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        /// <summary>
        /// Run the ANOVA analysis
        /// </summary>
        private void ANOVA()
        {
            dsp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                REngine engine = REngine.GetInstanceFromID(s_RInstance);
                string s_RStatement = "";

                try
                {
                    GetOrganizedFactorsVector(s_RInstance,
                        !dsp.RemovePeptideColumn ?
                            dsp.InputTableName :
                            dsp.InputTableName + "[,-1]",
                        dsp.FactorTable, dsp.FixedEffect);

                    if (dsp.Mode.ToLower().Equals("msstats"))
                    {
                        // Perform MSstats analysis
                        traceLog.Info("ANOVA: Mode set to 'MSstats'");
                        s_MSstatsDataTablename = GetTemporaryTableName(); // input data table for MSstas analysis

                        bool b_ContinueMSstats = LoadMSstatsLibrary();
                        if (b_ContinueMSstats)
                            b_ContinueMSstats = PrepareDataForMSstats();
                    }
                    else if (dsp.Mode.ToLower().Equals("anova"))
                    {
                        // Perform the ANOVA from Ashoka's DAnTE
                        traceLog.Info("ANOVA: Mode set to 'Anova'");

                        string s_Random = "NULL";
                        if (!string.IsNullOrEmpty(dsp.RandomEffect) &&
                            !dsp.RandomEffect.Equals("NULL"))
                            s_Random = dsp.RandomEffect;

                        string s_TmpTableName = GetTemporaryTableName();

                        //s_RStatement = "tmpTable <- performAnova(Data=" +
                        //    dsp.InputTableName + ")";


                        s_RStatement = string.Format(
                            "options(warn=-1)\n" +
                            "{8} <- {1}\n" +
                            "{0} <- performAnova(Data={8}, FixedEffects=\"{2}\", " +
                            "RandomEffects={3}, interact={4}, " +
                            "unbalanced={5}, useREML={6}, Factors=t({7}))\n" +
                            "rm({8})\n",
                            dsp.NewTableName,
                            dsp.RemovePeptideColumn ?
                                dsp.InputTableName + "[1:20,-1]" :
                                dsp.InputTableName,
                            dsp.FixedEffect,
                            !string.IsNullOrEmpty(dsp.RandomEffect) &&
                                !dsp.RandomEffect.Equals("NULL") ?
                                    dsp.RandomEffect :
                                    "NULL",
                            dsp.Interaction,
                            dsp.Unbalanced,
                            dsp.UseREML,
                            dsp.FactorTable,
                            s_TmpTableName
                            );
                    }

                    traceLog.Info("Performing ANOVA: \n\t" + s_RStatement);

                    engine.EagerEvaluate(s_RStatement);
                }
                catch (ParseException pexc)
                {
                    traceLog.Error("Parse Exception caught in ANOVA:\n" +
                        pexc.ToString());
                    Model.SuccessRunningPipeline = false;
                }
                catch (Exception exc)
                {
                    traceLog.Error("Error ANOVA: " + exc.ToString());
                    Model.SuccessRunningPipeline = false;
                }
            }
        }

        /// <summary>
        /// Check if MSstats is already installed, if not - install & load it. 
        /// Otherwise, just load it.
        /// </summary>
        private bool LoadMSstatsLibrary()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            string s_RStatement = "";
            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "plyr"))
            {
                clsGenericRCalls.InstallPackage(s_RInstance, "plyr");
            }
            else
            {
                s_RStatement += "require(plyr)\n";
            }

            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "MSstats"))
            {
                // Installing MSstats
                s_RStatement += string.Format("source(\"http://bioconductor.org/biocLite.R\")\n" +
                    "biocLite(\"Biobase\")\n" +
                    "install.packages(\"lme4\")\n" +
                    "install.packages(\"gmodels\")\n" +
                    "install.packages(\"lattice\")\n" +
                    "library(Biobase)\n" +
                    "library(lme4)\n" +
                    "library(gmodels)\n" +
                    "library(lattice)\n" +
                    "install.packages(pkgs=\"{0}\", " +
                    "repos=NULL, type=\"source\")\n" +
                    "require(MSstats)",
                    s_MSstatsLibrary);
            }
            else
            {
                s_RStatement = "require(MSstats)";
            }

            try
            {
                traceLog.Info("ANOVA loading libraries:\n" + s_RStatement);
                engine.EagerEvaluate(s_RStatement);
                return true;
            }
            catch (Exception exc)
            {
                traceLog.Error("Error loading libraries in ANOVA: " + exc.ToString());
                Model.SuccessRunningPipeline = false;
                return false;
            }
        }

        private bool PrepareDataForMSstats()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            string s_RStatement = "";

            string s_TmpRowMetadataTable = GetTemporaryTableName();
            string s_TmpRowMetadataCountTable = GetTemporaryTableName();
            string s_TmpDataTable = GetTemporaryTableName();
            string s_TmpMeltTable = GetTemporaryTableName();
            string s_TmpCBindRowTable = GetTemporaryTableName();
            string s_TmpCBindColTable = GetTemporaryTableName();

            s_RStatement = string.Format(
                "require(plyr)\n" +
                "{0} <- unique(cbind({2}${3}, {2}${4}))\n" +
                "{1} <- cbind({0}, rep(1, nrow({0})))\n" +
                "colnames({0}) <- c(\"{3}\", \"{4}\")\n" +
                "colnames({1}) <- c(\"{3}\", \"{4}\", \"Count\")\n" +
                "{1} <- transform({1}, Count = as.numeric(Count))\n" +
                "{1} <- ddply({1}, c(\"{3}\"), summarise, ProteinCount=sum(Count))\n" +
                "{1} <- {1}[which({1}$ProteinCount == 1),]\n" +
                "{5} <- {6}[which(rownames({6}) %in% {1}${3}),]\n" +
                "require(reshape)\n" +
                "{7} <- melt({5})\n" +
                "colnames({7}) <- c(\"{3}\", \"dataset\", \"abundance\")\n" +
                "{8} <- unique(merge(x={7}, y={0}, by.x=\"{3}\", by.y=\"{3}\", " +
                "all.x=T, all.y=F))\n" +
                "{9} <- unique(merge(x={8}, y={10}, by.x=\"dataset\", by.y=\"{11}\", " +
                "all=T))\n",
                s_TmpRowMetadataTable,
                s_TmpRowMetadataCountTable,
                dsp.RowMetadataTable,
                dsp.LinkRow,
                dsp.RowFactor,
                s_TmpDataTable,
                dsp.InputTableName,
                s_TmpMeltTable,
                s_TmpCBindRowTable,
                s_TmpCBindColTable,
                dsp.FactorTable,
                dsp.LinkCol);

            try
            {
                traceLog.Info("ANOVA preparing data for analysis:\n" + s_RStatement);
                engine.EagerEvaluate(s_RStatement);
                return true;
            }
            catch (Exception exc)
            {
                traceLog.Error("Error preparing data in ANOVA: " + exc.ToString());
                Model.SuccessRunningPipeline = false;
                return false;
            }
        }

        private void PreprocessDataForMSstats()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            /// MSstats requires the data to be in a certain format and must
            /// meet specific conditions (i.e., each peptide must map to only
            /// a single protein or proteingroup).
            /// 1. Remove redundant peptides - peptides that map to more than
            /// one protein or proteingroup

            



        }
        #endregion
    }
}
