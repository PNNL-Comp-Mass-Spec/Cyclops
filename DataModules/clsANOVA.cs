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
        private string s_MSstatsLibrary = //"//gigasax/DMS_Workflows/Cyclops/MSstats.tar.gz";
            "G:/Downloads/MSstats.tar.gz";
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

            if (string.IsNullOrEmpty(dsp.FixedEffect))
            {
                traceLog.Error("ANOVA class: 'Fixed_Effect' factor was not identified, " +
                    "skipping over ANOVA");
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
                    // Only organize the factors IF it is NOT an iTRAQ experiment!
                    if (!dsp.AnalysisType.Contains("iTRAQ"))
                    {
                        GetOrganizedFactorsVector(s_RInstance,
                            !dsp.RemovePeptideColumn ?
                                dsp.InputTableName :
                                dsp.InputTableName + "[,-1]",
                            dsp.FactorTable, dsp.FixedEffect);
                    }

                    if (dsp.Mode.ToLower().Equals("msstats"))
                    {
                        // Perform MSstats analysis
                        traceLog.Info("ANOVA: Mode set to 'MSstats'");
                        s_MSstatsDataTablename = GetTemporaryTableName(); // input data table for MSstas analysis

                        List<string> l_Groups = new List<string>();
                        if (!string.IsNullOrEmpty(dsp.FixedEffect))
                            l_Groups.Add(dsp.FixedEffect);
                        if (!string.IsNullOrEmpty(dsp.RandomEffect))
                            l_Groups.Add(dsp.RandomEffect);

                        // Load the library
                        bool b_ContinueMSstats = LoadMSstatsLibrary();
                        string s_Table4MSstats = "", s_FitModel = "",
                            s_DataForFitModel = "";

                        // Prepare the Data
                        if (b_ContinueMSstats)
                            s_Table4MSstats = PrepareDataForMSstats();

                        // Transform the Data
                        s_DataForFitModel = TransformData(s_Table4MSstats,
                            dsp.RowFactor, dsp.LinkRow, dsp.BioRep,
                            l_Groups, "Abundance",
                            "T_TransformedData_4MSstatsAnalysis");


                        // Make Contrasts, Fit the Model, & Perform Group Comparisons
                        List<string> l_Conditions =
                            clsGenericRCalls.GetUniqueColumnElementsWithinTable(s_RInstance,
                            s_DataForFitModel, "GROUP");

                        List<string> l_Comparison = new List<string>();
                        List<string> l_FDR = new List<string>();
                        List<string> l_ContrastTables = new List<string>();
                        List<string> l_FitModels = new List<string>();
                        for (int i = 0; i < l_Conditions.Count - 1; i++)
                        {
                            for (int j = i + 1; j < l_Conditions.Count; j++)
                            {
                                string
                                    tmpFilteredTable = GetTemporaryTableName(),
                                    tmpContrastsTable = GetTemporaryTableName(),
                                    tmpFitModelTable = GetTemporaryTableName();

                                string s_ContrastStatement = string.Format(
                                    "GROUP{0} - GROUP{1}"
                                    , l_Conditions[i]    // 0
                                    , l_Conditions[j]    // 1
                                );
                                string s_Levels = string.Format(
                                    "c(\"{0}\", \"{1}\")\n"
                                    , l_Conditions[i]
                                    , l_Conditions[j]
                                );

                                // Make Contrasts
                                tmpContrastsTable = MakeContrasts(
                                    s_ContrastStatement, s_Levels,
                                    "T_Contrast_" +
                                    l_Conditions[i] + "_" +
                                    l_Conditions[j]);
                                l_ContrastTables.Add(tmpContrastsTable);

                                // Filter Data
                                tmpFilteredTable = FilterTable(s_DataForFitModel,
                                    "GROUP", s_Levels);

                                // Fit the Model
                                tmpFitModelTable = FitTheModel(tmpFilteredTable,
                                    "PROTEIN", "FEATURE",
                                    "BIO.REP", "GROUP", "ABUNDANCE",
                                    dsp.Model, "FALSE", "FALSE",
                                    "T_FitModel_" + l_Conditions[i] +
                                    "_" + l_Conditions[j]);
                                l_FitModels.Add(tmpFitModelTable);

                                // Group Comparison
                                l_Comparison.Add(GroupComparison(tmpFitModelTable,
                                    tmpContrastsTable,
                                    "T_GroupComparison_" +
                                    l_Conditions[i] + "_" +
                                    l_Conditions[j]));
                                l_FDR.Add(TopProteins(l_Comparison[l_Comparison.Count - 1], 7,
                                    "BH", "T_TopProteins_" + l_Conditions[i] + "_" +
                                    l_Conditions[j]));

                                clsGenericRCalls.RemoveObject(s_RInstance, tmpFilteredTable);
                            }
                        }

                        GroupIntoList("MSstats_Contrasts", l_ContrastTables);
                        GroupIntoList("MSstats_FitModels", l_FitModels);
                        GroupIntoList("MSstats_Comparisons", l_Comparison);
                        GroupIntoList("MSstats_FDR", l_FDR);

                        // Calculate the protein abundance in individual samples
                        // Overall Fit Model
                        string s_MainFitModel = FitTheModel(s_DataForFitModel,
                            "PROTEIN", "FEATURE", "BIO.REP", "GROUP",
                            "ABUNDANCE", "fixed", "FALSE", "FALSE",
                            "T_FitModel_Main");
                        string s_ProteinAbundance = SubjectQuantification(
                            "T_MSstats_SubjectQuantification", s_MainFitModel, "TRUE");

                        List<string> l_MSstats = new List<string>();
                        l_MSstats.Add("MSstats_Contrasts");
                        l_MSstats.Add("MSstats_FitModels");
                        l_MSstats.Add("MSstats_Comparisons");
                        l_MSstats.Add("MSstats_FDR");
                        l_MSstats.Add("T_TransformedData_4MSstatsAnalysis");
                        l_MSstats.Add("T_MSstats_SubjectQuantification");
                        l_MSstats.Add("T_FitModel_Main");

                        GroupIntoList("MSstatsAnalysis", l_MSstats);
                    }
                    else if (dsp.Mode.ToLower().Equals("anova"))
                    {
                        // Perform the ANOVA from Ashoka's DAnTE5
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

        /// <summary>
        /// Prepares the dataset for MSstats analysis
        /// </summary>
        /// <returns>The name of the temporary table to run the analysis on</returns>
        private string PrepareDataForMSstats()
        {
            /// MSstats requires the data to be in a certain format and must
            /// meet specific conditions (i.e., each peptide must map to only
            /// a single protein or proteingroup).
            /// 1. Remove redundant peptides - peptides that map to more than
            /// one protein or proteingroup
            
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            string s_RStatement = "",
                s_TmpProtPepFact = GetTemporaryTableName(); // Merged group, peptide, abudance, factors to be returned

            List<string> l_Factors = new List<string>();
            if (!string.IsNullOrEmpty(dsp.FixedEffect))
                l_Factors.Add(dsp.FixedEffect);
            if (!string.IsNullOrEmpty(dsp.RandomEffect))
                l_Factors.Add(dsp.RandomEffect);
            if (!string.IsNullOrEmpty(dsp.BioRep))
                l_Factors.Add(dsp.BioRep);

            string s_TmpFeatures = GetTemporaryTableName(), // Name of features (Peptides) to get from Row_Metadata
                    s_TmpFeaturesCnt = GetTemporaryTableName(), // Counts features per group (ProteinGroup)
                    s_TmpUniqueFeatures = GetTemporaryTableName(), // Features that only map to a single group
                    s_TmpUniqueData = GetTemporaryTableName(), // Filters data for only those unique feature
                    s_TmpMeltedData = GetTemporaryTableName(), // Melts data
                    s_TmpRowMetadata = GetTemporaryTableName(), // Important columns from Row_Metadata table
                    s_TmpProtPep = GetTemporaryTableName(), // Merged unique data and important columns from Row_Metadata
                    s_TmpColMetadata = GetTemporaryTableName(); // Important columns from Column_Metadata table

            string s_ColumnTableFactors = string.Join("\", \"", l_Factors);

            if (string.IsNullOrEmpty(dsp.AnalysisType) |
                dsp.AnalysisType.Contains("Label_Free"))
            {

                s_RStatement = string.Format(
                    "{0} <- {1}[,\"{2}\"]\n" +
                    "{0} <- {0}[{0}%in%rownames({3})]\n" +
                    "{4} <- table({0})\n" +
                    "rm({0})\n" +
                    "{5} <- names({4}[{4} == 1])\n" +
                    "rm({4})\n" +
                    "{6} <- {3}[which(rownames({3})%in%{5}),]\n" +
                    "rm({5})\n" +
                    "require(reshape)\n" +
                    "{7} <- melt({6})\n" +
                    "rm({6})\n" +
                    "colnames({7}) <- c(\"{2}\", \"{8}\", \"Abundance\")\n" +
                    "{9} <- {1}[,c(\"{2}\", \"{10}\")]\n" +
                    "{11} <- unique(merge(x={9}, " +
                        "y={7}, by.x=\"{2}\", by.y=\"{2}\", all=F))\n" +
                    "rm({7})\n" +
                    "{12} <- {13}[,c(\"{8}\", \"{14}\")]\n" +
                    "{15} <- unique(merge(x={11}, " +
                        "y={12}, by.x=\"{8}\", by.y=\"{8}\", all=F))\n" +
                    "rm({9})\n" +
                    "rm({11})\n" +
                    "rm({12})\n"
                    , s_TmpFeatures          // 0
                    , dsp.RowMetadataTable   // 1
                    , dsp.LinkRow            // 2
                    , dsp.InputTableName     // 3
                    , s_TmpFeaturesCnt       // 4
                    , s_TmpUniqueFeatures    // 5
                    , s_TmpUniqueData        // 6
                    , s_TmpMeltedData        // 7
                    , dsp.LinkCol            // 8
                    , s_TmpRowMetadata       // 9
                    , dsp.RowFactor          // 10
                    , s_TmpProtPep           // 11
                    , s_TmpColMetadata       // 12
                    , dsp.FactorTable        // 13
                    , s_ColumnTableFactors   // 14
                    , s_TmpProtPepFact      // 15
                    );
            }
            else if (dsp.AnalysisType.Contains("iTRAQ"))
            {
                CheckForReshape2Package();

                s_RStatement = string.Format(
                    "{0} <- {1}[,\"{2}\"]\n" +
                    "{0} <- {0}[{0}%in%rownames({3})]\n" +
                    "{4} <- table({0})\n" +
                    "rm({0})\n" +
                    "{5} <- names({4}[{4} == 1])\n" +
                    "rm({4})\n" +
                    "{6} <- {3}[which(rownames({3})%in%{5}),]\n" +
                    "rm({5})\n" +
                    "require(reshape)\n" +
                    "{7} <- melt({6})\n" +
                    "rm({6})\n" +
                    "colnames({7}) <- c(\"{2}\", \"{8}\", \"Abundance\")\n" +
                    "{9} <- {1}[,c(\"{2}\", \"{10}\")]\n" +
                    "{11} <- unique(merge(x={9}, " +
                        "y={7}, by.x=\"{2}\", by.y=\"{2}\", all=F))\n" +
                    "rm({7})\n" +
                    "require(reshape2)\n"
                    , s_TmpFeatures          // 0
                    , dsp.RowMetadataTable   // 1
                    , dsp.LinkRow            // 2
                    , dsp.InputTableName     // 3
                    , s_TmpFeaturesCnt       // 4
                    , s_TmpUniqueFeatures    // 5
                    , s_TmpUniqueData        // 6
                    , s_TmpMeltedData        // 7
                    , dsp.LinkCol            // 8
                    , s_TmpRowMetadata       // 9
                    , dsp.RowFactor          // 10
                    , s_TmpProtPep           // 11
                    , s_TmpColMetadata       // 12
                    , dsp.FactorTable        // 13
                    , s_ColumnTableFactors   // 14
                    , s_TmpProtPepFact      // 15
                    );


                s_RStatement += string.Format(
                    "{0} <- {1}[1, grep(\"Ion\", colnames({1}))]\n" +
                    "{0} <- colsplit(unlist({0}), \"_\", c(\"{7}\", \"BioRep\"))\n" +
                    "{0} <- cbind({2}=colnames({1})[grep(\"Ion\", colnames({1}))], {0})\n" +
                    "{4} <- unique(merge(x={5}, " +
                        "y={0}, by.x=\"{2}\", by.y=\"{2}\", all=F))\n" +
                    "rm({6})\n" +
                    "rm({5})\n" +
                    "rm({0})\n"
                    , s_TmpColMetadata       // 0
                    , dsp.FactorTable        // 1
                    , dsp.LinkCol            // 2
                    , s_ColumnTableFactors   // 3
                    , s_TmpProtPepFact       // 4
                    , s_TmpProtPep           // 5
                    , s_TmpRowMetadata       // 6
                    , dsp.FixedEffect        // 7
                    );
            }

            try
            {
                traceLog.Info("ANOVA preparing data for analysis:\n" + s_RStatement);
                engine.EagerEvaluate(s_RStatement);
                return s_TmpProtPepFact;
            }
            catch (Exception exc)
            {
                traceLog.Error("Error preparing data in ANOVA: " + exc.ToString());
                Model.SuccessRunningPipeline = false;
                return null;
            }
        }

        /// <summary>
        /// Filters the data for fitModels and groupComparison
        /// </summary>
        /// <param name="TableName">Name of Table to filter</param>
        /// <param name="ColumnName">Name of Column in Table to filter on</param>
        /// <param name="Levels">Name of the character vector of factor giving the names of all levels
        /// corresponding to the group variable or to the concatenation of group variables if more than
        /// one was specified in 'fitModels'</param>
        /// <returns>Name of the matrix containing the contrasts.</returns>
        private string FilterTable(string TableName, string ColumnName, string Levels)
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            string s_TmpTable = GetTemporaryTableName();

            string s_RStatement = string.Format(
                "{0} <- {1}[which({1}${2}%in%{3}),]\n"
                , s_TmpTable    // 0
                , TableName     // 1
                , ColumnName    // 2
                , Levels        // 3
            );

            try
            {
                traceLog.Info("MSstats ANOVA, filtering Table (" +
                    TableName + "):\n" +
                    s_RStatement);
                engine.EagerEvaluate(s_RStatement);
                return s_TmpTable;
            }
            catch (ParseException pe)
            {
                traceLog.Error("PARSE EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while filtering Table (" +
                    TableName + "):\n" +
                    pe.ToString());
                return null;
            }
            catch (Exception exc)
            {
                traceLog.Error("EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while filtering Table (" +
                    TableName + "):\n" +
                    exc.ToString());
                return null;
            }
        }

        /// <summary>
        /// Construct a model to describe the variation in 
        /// measurements observed for each protein
        /// </summary>
        /// <param name="InputTableName">Name of long format table containing all parameters</param>
        /// <param name="Protein">Column specifying the protein ids</param>
        /// <param name="Feature">Column specifying the peptide ids</param>
        /// <param name="BioRep">Column specifying the biological replicates ids</param>
        /// <param name="Group">Column(s) specifying the factor(s) to test</param>
        /// <param name="Abundance">Column specifying the peptide abundance</param>
        /// <param name="Model">Type of model to test (e.g. 'fixed' or 'mixed')</param>
        /// <param name="FeatureVar">'TRUE' or 'FALSE' logical variable for whether
        /// the model should account for hetergeneous variation among intensities
        /// from different features.</param>
        /// <param name="Progress">'TRUE' or 'FALSE' logical variable for whether
        /// progress statements should be printed to the console. [NOTE: use FALSE]</param>
        /// <returns>Name of table containing the fitted models.</returns>
        private string FitTheModel(string InputTableName,
            string Protein, string Feature, string BioRep,
            string Group, string Abundance, string Model,
            string FeatureVar, string Progress,
            string NameOfFitModelTable)
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            string s_RStatement = string.Format(
                "{0} <- fitModels(protein=\"{1}\", " +
                    "feature=\"{2}\", bio.rep=\"{3}\", " +
                    "group=c(\"{4}\"), " +
                    "abundance=\"{5}\", model=\"{6}\", " +
                    "feature.var={7}, progress={8}, " +
                    "data={9})\n" //+
                    //"rm({9})\n"
                    , NameOfFitModelTable              // 0
                    , Protein                        // 1
                    , Feature                        // 2
                    , BioRep                         // 3
                    , Group                          // 4
                    , Abundance                      // 5
                    , Model                          // 6
                    , FeatureVar                     // 7
                    , Progress                       // 8
                    , InputTableName                 // 9
            );

            try
            {
                traceLog.Info("MSstats ANOVA, Fitting Model:\n" +
                    s_RStatement);
                engine.EagerEvaluate(s_RStatement);
                return NameOfFitModelTable;
            }
            catch (ParseException pe)
            {
                traceLog.Error("PARSE EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while Fitting the Model:\n" +
                    pe.ToString());
                return null;
            }
            catch (Exception exc)
            {
                traceLog.Error("EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while Fitting the Model:\n" +
                    exc.ToString());
                return null;
            }            
        }

        /// <summary>
        /// MSstats: Given a data frame in 'long' format with multiple groups, it creates
        /// a data frame with a concatenated variable whose levels are combinations of the
        /// levels of the original variables.
        /// </summary>
        /// <param name="InputTableName">Name of long format table containing all parameters</param>
        /// <param name="Protein">Column specifying the protein ids</param>
        /// <param name="Feature">Column specifying the peptide ids</param>
        /// <param name="BioRep">Column specifying the biological replicates ids</param>
        /// <param name="Group">Column(s) specifying the factor(s) to test</param>
        /// <param name="Abundance">Column specifying the peptide abundance</param>        
        /// progress statements should be printed to the console. [NOTE: use FALSE]</param>
        /// <returns>Name of table containing the fitted models.</returns>
        private string TransformData(string InputTableName,
            string Protein, string Feature, string BioRep,
            List<string> Group, string Abundance, 
            string NameOfTransformedDataTable)
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            string s_RStatement = string.Format(
                "{0} <- transformData(protein=\"{1}\", " +
                    "feature=\"{2}\", bio.rep=\"{3}\", " +
                    "group=c(\"{4}\"), " +
                    "abundance=\"{5}\", " +
                    "data={6})\n" +
                    "rm({6})\n"
                    , NameOfTransformedDataTable              // 0
                    , Protein                        // 1
                    , Feature                        // 2
                    , BioRep                         // 3
                    , string.Join("\", \"", Group)   // 4
                    , Abundance                      // 5
                    , InputTableName                 // 6
            );

            try
            {
                traceLog.Info("MSstats ANOVA, Transforming Data:\n" +
                    s_RStatement);
                engine.EagerEvaluate(s_RStatement);
                return NameOfTransformedDataTable;
            }
            catch (ParseException pe)
            {
                traceLog.Error("PARSE EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while Transforming Data:\n" +
                    pe.ToString());
                return null;
            }
            catch (Exception exc)
            {
                traceLog.Error("EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while Transforming Data:\n" +
                    exc.ToString());
                return null;
            }
        }

        /// <summary>
        /// MSstats: Produces a matrix of comparisons to be tested using 'groupComparison'
        /// </summary>
        /// <param name="Contrasts">Statement/expression to be evaluated for making the contrasts</param>
        /// <param name="Levels">Name of the character vector of factor giving the names of all levels
        /// corresponding to the group variable or to the concatenation of group variables if more than
        /// one was specified in 'fitModels'</param>
        /// <returns>Name of the matrix containing the contrasts.</returns>
        private string MakeContrasts(string Contrasts, string Levels,
            string NameOfContrastsTable)
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            
            string s_RStatement = string.Format(
                "{0} <- makeContrasts({1}, levels = {2})\n"
                , NameOfContrastsTable    // 0
                , Contrasts     // 1
                , Levels        // 2
            );

            try
            {
                traceLog.Info("MSstats ANOVA, making contrasts:\n" +
                    s_RStatement);
                engine.EagerEvaluate(s_RStatement);
                return NameOfContrastsTable;
            }
            catch (ParseException pe)
            {
                traceLog.Error("PARSE EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while Making Contrasts:\n" +
                    pe.ToString());
                return null;
            }
            catch (Exception exc)
            {
                traceLog.Error("EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while Making Contrasts:\n" +
                    exc.ToString());
                return null;
            }
        }

        /// <summary>
        /// MSstats: Performs statistical hypothesis tests of differential
        /// abundance between conditions.
        /// </summary>
        /// <param name="FitModelTableName">Name of table 
        /// containing the fitted model.</param>
        /// <param name="ContrastsMatrix">Name of the contrasts matrix</param>
        /// <returns>Name of the resulting table</returns>
        private string GroupComparison(string FitModelTableName,
            string ContrastsMatrix, string NameOfGroupComparisonTable)
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            
            string s_RStatement = string.Format(
                "{0} <- groupComparison(modelFits={1}, " +
                    "contrast.matrix={2}, progress=FALSE)\n" +
                "colnames({0}) <- c(\"Protein\", \"Comparison\", " +
                    "\"Est\", \"StdError\", \"tValue\", " +
                    "\"DF\", \"pValue\")\n" +
                "{0} <- {0}[with({0}, order(pValue)),]\n"
                , NameOfGroupComparisonTable            // 0
                , FitModelTableName     // 1
                , ContrastsMatrix       // 2
            );

            try
            {
                traceLog.Info("MSstats ANOVA, group comparison:\n" +
                    s_RStatement);
                engine.EagerEvaluate(s_RStatement);
                return NameOfGroupComparisonTable;
            }
            catch (ParseException pe)
            {
                traceLog.Error("PARSE EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while Comparing Groups:\n" +
                    pe.ToString());
                return null;
            }
            catch (Exception exc)
            {
                traceLog.Error("EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while Comparing Groups:\n" +
                    exc.ToString());
                return null;
            }
        }

        /// <summary>
        /// MSstats: Model-based condition-specific estimates of protein 
        /// abundance. Uses fitted statistical models to produce 
        /// condition-specific estimates of protein abundance.
        /// </summary>
        /// <param name="FitModelTableName">Name of table 
        /// containing the fitted model.</param>
        /// <param name="Table">'TRUE' or 'FALSE', indicator for whether
        /// output should be in table format, where rows correspond to
        /// conditions and columns to proteins.</param>
        /// <returns>Name of the resulting table</returns>
        private string GroupQuantification(string FitModelTableName,
            string Table, string NameOfGroupQuantificationTable)
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            
            string s_RStatement = string.Format(
                "{0} <- groupQuantification(modelFits={1}, " +
                    "contrast.matrix={2}, progress=FALSE)\n"
                , NameOfGroupQuantificationTable            // 0
                , FitModelTableName     // 1
                , Table                 // 2
            );

            try
            {
                traceLog.Info("MSstats ANOVA, group quantification:\n" +
                    s_RStatement);
                engine.EagerEvaluate(s_RStatement);
                return NameOfGroupQuantificationTable;
            }
            catch (ParseException pe)
            {
                traceLog.Error("PARSE EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while Quantifying within Groups:\n" +
                    pe.ToString());
                return null;
            }
            catch (Exception exc)
            {
                traceLog.Error("EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while Quantifying within Groups:\n" +
                    exc.ToString());
                return null;
            }
        }

        /// <summary>
        /// MSstats: Model-based protein quantification in biological samples,
        /// Uses fitted linear models to estimate protein abundance in biological
        /// samples from multiple LC-MS features.
        /// </summary>
        /// <param name="FitModelTableName">Name of table 
        /// containing the fitted model.</param>
        /// <param name="Table">'TRUE' or 'FALSE', indicator for whether
        /// output should be in table format, where rows correspond to
        /// conditions and columns to proteins.</param>
        /// <returns>Name of the resulting table</returns>
        private string SubjectQuantification(string ResultTableName,
            string FitModelTableName, string Table)
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            string s_RStatement = string.Format(
                "{0} <- subjectQuantification(modelFits={1}, " +
                    "table={2}, progress=FALSE)\n"
                , ResultTableName            // 0
                , FitModelTableName     // 1
                , Table                 // 2
            );

            try
            {
                traceLog.Info("MSstats ANOVA, subject quantification:\n" +
                    s_RStatement);
                engine.EagerEvaluate(s_RStatement);
                return ResultTableName;
            }
            catch (ParseException pe)
            {
                traceLog.Error("PARSE EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while Quantifying within Subjects:\n" +
                    pe.ToString());
                return null;
            }
            catch (Exception exc)
            {
                traceLog.Error("EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while Quantifying within Subjects:\n" +
                    exc.ToString());
                return null;
            }
        }

        /// <summary>
        /// MSstats: Table of top proteins based on a comparison. Extracts
        /// the top proteins based on either the log fold change or p-value of 
        /// a comparison
        /// </summary>
        /// <param name="ComparisonResults">Name of data frame of testing
        /// results from groupComparison</param>
        /// <param name="ContrastsMatrix">Name of matrix of comparisons that were 
        /// tested in groupComparison, where columns correspond to comparisons
        /// and rows to levels of the group variable.</param>
        /// <param name="ComparisonColumn">Name of the column that indicates the 
        /// contrast matrix for which the proteins will be ranked.</param>
        /// <param name="RankBy">Specifies whether to sort the proteins according
        /// to the adjusted p-value of the comparison (rank.by = 1) or by the 
        /// absolute value of the log fold change (rank.by = 2)</param>
        /// <param name="NumberOfProteins">Number of proteins to return</param>
        /// <param name="AdjustMethod">Method by which to adjust the p-values to
        /// correct for performing multiple hypothesis tests. Possible options are
        /// 'holm', 'hochberg', 'hommel', 'bonferroni', 'BH' [default], 'BY', 'fdr',
        /// 'and 'none'.</param>
        /// <returns>Name of the resulting table</returns>
        private string TopProteins(string ComparisonResults, 
            int ColumnToAdjust,
            string AdjustMethod, string NameOfTopProteinsTable)
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            // default the adjust method to Benjamini-Hochberg
            if (!string.IsNullOrEmpty(AdjustMethod))
                AdjustMethod = "BH";

            string s_RStatement  = string.Format(
                "{0} <- cbind({1}, " +
                    "adjPvalue=p.adjust({1}[,{2}], method=\"{3}\"))\n" +
                "{0} <- {0}[with({0}, order(adjPvalue)),]\n"
                , NameOfTopProteinsTable // 0
                , ComparisonResults      // 1
                , ColumnToAdjust         // 2
                , AdjustMethod           // 3
            );

            try
            {
                traceLog.Info("MSstats ANOVA, Top Proteins:\n" +
                    s_RStatement);
                engine.EagerEvaluate(s_RStatement);
                return NameOfTopProteinsTable;
            }
            catch (ParseException pe)
            {
                traceLog.Error("PARSE EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while getting Top Proteins:\n" +
                    pe.ToString());
                return null;
            }
            catch (Exception exc)
            {
                traceLog.Error("EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while getting Top Proteins:\n" +
                    exc.ToString());
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="NameOfList"></param>
        /// <param name="ListOfTables2Add"></param>
        /// <param name="RemoveTableAfterAdding2List"></param>
        /// <returns></returns>
        private string AddTablesToList(string NameOfList,
            List<string> ListOfTables2Add, bool RemoveTableAfterAdding2List)
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            string s_RStatement = "";

            try
            {
                foreach (string s in ListOfTables2Add)
                {
                    s_RStatement = string.Format(
                        "{0} <- c({0}, {1}={1}\n" +
                        "{2}"
                        ,NameOfList
                        ,s
                        ,RemoveTableAfterAdding2List ? "rm(" + s + ")\n" : ""
                        );

                    traceLog.Info("MSstats, Adding table: " + 
                        s + " to list: " + NameOfList + ":\n" +
                        s_RStatement);
                    engine.EagerEvaluate(s_RStatement);                    
                }

                return NameOfList;
            }
            catch (ParseException pe)
            {
                traceLog.Error("PARSE EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while adding table to list:\n" +
                    pe.ToString());
                return null;
            }
            catch (Exception exc)
            {
                traceLog.Error("EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while adding table to list:\n" +
                    exc.ToString());
                return null;
            }
        }

        /// <summary>
        /// Groups items into a list. The name of the item is used as the 
        /// name of the item in the list.
        /// </summary>
        /// <param name="NameOfList">Name of the resulting list</param>
        /// <param name="Items2Add2List">List of items to add to list</param>
        /// <returns>Name of the resulting list</returns>
        private string GroupIntoList(string NameOfList, 
            List<string> Items2Add2List)
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            string s_RStatement = string.Format(
                "{0} <- list("
                , NameOfList);

            for (int i = 0; i < Items2Add2List.Count; i++)
            {
                s_RStatement += Items2Add2List[i] +
                        "=" + Items2Add2List[i];
                if (i != Items2Add2List.Count - 1)
                    s_RStatement += ", ";
            }

            s_RStatement += ")\n";

            foreach (string s in Items2Add2List)
            {
                s_RStatement += string.Format("rm({0})\n"
                    , s);
            }

            try
            {
                traceLog.Info("MSstats ANOVA, making contrasts:\n" +
                    s_RStatement);
                engine.EagerEvaluate(s_RStatement);
                return NameOfList;
            }
            catch (ParseException pe)
            {
                traceLog.Error("PARSE EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while Grouping Elements into a List:\n" +
                    pe.ToString());
                return null;
            }
            catch (Exception exc)
            {
                traceLog.Error("EXCEPTION ERROR " +
                    "CAUGHT IN MSstats ANOVA while Grouping Elements into a List:\n" +
                    exc.ToString());
                return null;
            }
        }

        private void CheckForReshape2Package()
        {
            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "reshape2"))
            {
                REngine engine = REngine.GetInstanceFromID(s_RInstance);
                traceLog.Info("Installing 'reshape2' package...");

                try
                {
                    engine.EagerEvaluate("install.package(\"reshape2\")");
                }
                catch (ParseException pe)
                {
                    traceLog.Error("ANOVA Parse Exception caught while trying to install " +
                        "'reshape2' package: " + pe.ErrorStatement + "\n" + pe.Message);
                    Model.SuccessRunningPipeline = false;
                }
                catch (Exception exc)
                {
                    traceLog.Error("ANOVA Exception caught while trying to install " +
                        "'reshape2' package: " + exc.Message);
                    Model.SuccessRunningPipeline = false;
                }
            }
        }
        #endregion
    }
}
