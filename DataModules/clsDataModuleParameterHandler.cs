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
using System.Text;

//using log4net;

namespace Cyclops.DataModules
{
    /// <summary>
    /// This class holds the parameters used by all other Data classes 
    /// </summary>
    public class clsDataModuleParameterHandler
    {
        #region Variables
        //private static ILog traceLog = LogManager.GetLogger("TraceLog");
        private Dictionary<string, dynamic> d_Param = new Dictionary<string, dynamic>();

        private bool
            b_AutoScale = false;

        private string
            s_Add = "0",
            s_AllX = "TRUE",
            s_AllY = "TRUE",
            s_AltInputTableName = "",
            s_AnalysisType = "",
            s_AsDataMatrix="true",
            s_BioRep="",
            s_Center="FALSE",                           // used in RRollup, center peptide abundace to 0
            s_CommaSep_wQuotesIdentifiers = "",
            s_CommaSep_woQuotesIdentifiers = "",
            s_ConsolidationFactor,                      // passed through the ATM
            s_ColMetadataTable = "T_Column_Metadata",
            s_ColFactor = "",
            s_ColumnName = "",
            s_Contrasts = "",                          // MSstats: used for making special contrasts
            s_DatabasePath = "",
            s_DecoyPrefix = "Reversed_",              // used in IDPicker
            s_FactorColumn = "",
            s_FactorComplete = "",
            s_FactorTable = "",
            s_FilePath = @"\\gigasax\DMS_Workflows\Cyclops\ProteinProphet.zip",
            s_FixedEffect = "",                         // passed through the ATM
            s_FoldChangeTable = "",
            s_FractionSetColumnName = "FractionSet",
            s_Function = "",
            s_gMinPCount="5",                           // used in RRollup
            s_gpValue="0.05",                           // used in RRollup
            s_ID = "id",
            s_ImportDatasetType = "",
            s_InputFileName = "Results.db3",
            s_InputTableName = "",
            s_Interaction = "FALSE",
            s_IonColumnNamePrefix = "Ion_",
            s_LinkRow = "",                             // column used to link to Row Metadata
            s_LinkCol = "Alias",                        // column used to link to Column Metadata
            s_LogBase = "2",
            s_Margin = "",
            s_MaxAmbiguousIds="2",                      // used in IDPicker
            s_MaxFDR = "0.1",                           // used in IDPicker
            s_Maximum = "",
            s_MaxProtValue = "",
            s_MaxPepValue = "",
            s_MeanCenter="FALSE",                       // used in CentralTendency
            s_MinOverlap="3",                           // used in RRollup method
            s_MinAdditionalPeptides="2",                // used in IDPicker            
            s_MinDistinctPeptides="2",                  // used in IDPicker
            s_Minimum="1",
            s_MinPresence="10",                         // used in RRollup
            s_MinProtValue= "",
            s_MinPepValue = "",
            s_MinSpectraPerProtein="2",                 // used in IDPicker
            s_Mode="median",                            // used in RRollup, ANOVA
            s_Model="fixed",                            // used in MSstats
            s_ModsAreDistinctByDefault="true",          // used in IDPicker
            s_ModuleName = "",
            s_NewRowMetadataTableName="",
            s_NewTableName = "",
            s_NormalizedTable = "",
            s_NormalizedSearchScores = "msgfspecprob",  // used in IDPicker
            s_OneHitWonders="FALSE",                    // used in RRollup method, default in algorithm is true
            s_OptimizeScoreWeights = "1",               // used in IDPicker
            s_OutputTextReport="true",                  // used in IDPicker
            s_Password = "",
            s_PeptideColumn="3",                        // used in RRollup
            s_ProteinColumn="1",                        // used in RRollup
            s_ProteinInfo="",                           // used in RRollup
            s_P_ValueTable = "",            
            s_PvalueThreshold = "0.0001",
            s_QuantitativeMethod="",                    // used in IDPicker
            s_RandomEffect="",
            s_RawSourcePath="",                         // used in IDPicker
            s_Reference="",
            s_RemoveFirstCharacters = "",
            s_RemovePeptideColumn = "false",
            s_RowMetadataTable = "T_Row_Metadata",
            s_RowFactor = "",
            s_Rowname = "",
            s_RunAnalysis = "false",
            s_Scale = "1",
            s_SearchScoreWeights = "msgfspecprob -1",   // used in IDPicker
            s_Semicolon_wQuotesSepIdentifiers = "",
            s_Semicolon_woQuotesSepIdentifiers = "",
            s_SetZeroToNA = "false",
            s_SkipTheFirstColumn = "false",
            s_Source = "",
            s_SummaryTableName = "",
            s_TableType = "",
            s_TabSep_wQuotesIdentifiers = "",
            s_TabSep_woQuotesIdentifiers = "",
            s_Target = "",
            s_TechRepColumn = "",
            s_TechRepComplete = "",
            s_Theta = "TRUE",
            s_Threshold = "3",
            s_Unbalanced = "TRUE",
            s_UseREML = "TRUE",
            s_Variable = "variable",
            s_WorkDir = "",
            s_xLink = "",            
            s_xTable = "",
            s_yLink = "",
            s_yTable = "";
        #endregion

        #region Constructors
        /// Basic constructor
        public clsDataModuleParameterHandler()
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Parameters">Dictionary of Visualization Parameters</param>
        public clsDataModuleParameterHandler(Dictionary<string, dynamic> Parameters)
        {
            d_Param = Parameters;
            GetParameters();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Value to add to matrix in Transform
        /// </summary>
        public string Add
        {
            get { return s_Add; }
            set { s_Add = value; }
        }

        public string AllX
        {
            get { return s_AllX; }
            set { s_AllX = value; }
        }

        public string AllY
        {
            get { return s_AllY; }
            set { s_AllY = value; }
        }

        /// <summary>
        /// If the initial table name is not present in the 
        /// workspace, this parameter allows for an alternative
        /// table to be used for the analysis
        /// </summary>
        public string AltInputTableName
        {
            get { return s_AltInputTableName; }
            set { s_AltInputTableName = value; }
        }

        public string AnalysisType
        {
            get { return s_AnalysisType; }
            set { s_AnalysisType = value; }
        }

        public string AsDataMatrix
        {
            get { return s_AsDataMatrix.ToLower(); }
            set { s_AsDataMatrix = value; }
        }

        /// <summary>
        /// Used in the Transform function to prevent negative
        /// numbers, it tells the function to dynamically
        /// determine if and by how much scaling should occur.
        /// </summary>
        public bool AutoScale
        {
            get { return b_AutoScale; }
            set { b_AutoScale = value; }
        }

        public string BioRep
        {
            get { return s_BioRep; }
            set { s_BioRep = value; }
        }

        public string ColumnMetadataTable
        {
            get { return s_ColMetadataTable; }
            set { s_ColMetadataTable = value; }
        }

        public string ColumnFactor
        {
            get { return s_ColFactor; }
            set { s_ColFactor = value; }
        }

        public string ColumnName
        {
            get { return s_ColumnName; }
            set { s_ColumnName = value; }
        }

        public string Contrasts
        {
            get { return s_Contrasts; }
            set { s_Contrasts = value; }
        }

        public bool HasCommaSeparatedListWithQuotes
        {
            get
            {
                return s_CommaSep_wQuotesIdentifiers.Length > 0 ? true : false;
            }
        }

        public bool HasCommaSeparatedListWithoutQuotes
        {
            get
            {
                return s_CommaSep_woQuotesIdentifiers.Length > 0 ? true : false;
            }
        }

        public string ConsolidationFactor
        {
            get { return s_ConsolidationFactor; }
            set { s_ConsolidationFactor = value; }
        }
        
        /// <summary>
        /// Describes where the data is coming from, e.g. SQLite, CSV, TXT, etc.
        /// </summary>
        public string Source
        {
            get { return s_Source; }
            set { s_Source = value; }
        }

        /// <summary>
        /// Generic string used to identify a file.
        /// Initialized for the ProteinProphet.zip because that's what it was 
        /// initially for, but can be reset for other uses.
        /// </summary>
        public string FilePath
        {
            get { return s_FilePath; }
            set { s_FilePath = value; }
        }

        public string FixedEffect
        {
            get { return s_FixedEffect; }
            set { s_FixedEffect = value; }
        }

        public string FractionSetColumnName
        {
            get { return s_FractionSetColumnName; }
            set { s_FractionSetColumnName = value; }
        }

        public string DatabasePath
        {
            get { return s_DatabasePath; }
            set { s_DatabasePath = value; }
        }

        public string Interaction
        {
            get { return s_Interaction; }
            set { s_Interaction = value; }
        }

        /// <summary>
        /// Stores password, typically used for extracting files from zip
        /// </summary>
        public string Password
        {
            get { return s_Password; }
            set { s_Password = value; }
        }

        /// <summary>
        /// Working Directory to get and save files.
        /// </summary>
        public string WorkDirectory
        {
            get { return s_WorkDir; }
            set { s_WorkDir = value; }
        }

        /// <summary>
        /// Name and location of the file to pull data from
        /// </summary>
        public string InputFileName
        {
            get { return s_InputFileName; }
            set { s_InputFileName = value; }
        }

        /// <summary>
        /// If coming from a database, indicates the name of the 
        /// table to pull from
        /// </summary>
        public string InputTableName
        {
            get { return s_InputTableName; }
            set { s_InputTableName = value; }
        }

        public string IonColumnNamePrefix
        {
            get { return s_IonColumnNamePrefix; }
            set { s_IonColumnNamePrefix = value; }
        }

        public string LinkRow
        {
            get { return s_LinkRow; }
            set { s_LinkRow = value; }
        }

        public string LinkCol
        {
            get { return s_LinkCol; }
            set { s_LinkCol = value; }
        }

        public string DecoyPrefix
        {
            get { return s_DecoyPrefix; }
            set { s_DecoyPrefix = value; }
        }

        public string MaxAmbiguousIds
        {
            get { return s_MaxAmbiguousIds; }
            set { s_MaxAmbiguousIds = value; }
        }

        public string MaxFDR
        {
            get { return s_MaxFDR; }
            set { s_MaxFDR = value; }
        }

        public string Maximum
        {
            get { return s_Maximum; }
            set { s_Maximum = value; }
        }

        public string MinAdditionalPeptides
        {
            get { return s_MinAdditionalPeptides; }
            set { s_MinAdditionalPeptides = value; }
        }

        public string MinDistinctPeptides
        {
            get { return s_MinDistinctPeptides; }
            set { s_MinDistinctPeptides = value; }
        }

        public string Minimum
        {
            get { return s_Minimum; }
            set { s_Minimum = value; }
        }

        public string MinSpectraPerProtein
        {
            get { return s_MinSpectraPerProtein; }
            set { s_MinSpectraPerProtein = value; }
        }

        public string ModsAreDistinctByDefault
        {
            get { return s_ModsAreDistinctByDefault.ToLower(); }
            set { s_ModsAreDistinctByDefault = value; }
        }

        public string NormalizedSearchScores
        {
            get { return s_NormalizedSearchScores; }
            set { s_NormalizedSearchScores = value; }
        }

        public string OptimizeScoreWeights
        {
            get { return s_OptimizeScoreWeights; }
            set { s_OptimizeScoreWeights = value; }
        }

        public string OutputTextReport
        {
            get { return s_OutputTextReport.ToLower(); }
            set { s_OutputTextReport = value; }
        }

        public string QuantitativeMethod
        {
            get { return s_QuantitativeMethod; }
            set { s_QuantitativeMethod = value; }
        }

        public string RandomEffect
        {
            get { return s_RandomEffect; }
            set { s_RandomEffect = value; }
        }

        public string RawSourcePath
        {
            get { return s_RawSourcePath; }
            set { s_RawSourcePath = value; }
        }

        public string Reference
        {
            get { return s_Reference; }
            set { s_Reference = value; }
        }

        public string RowMetadataTable
        {
            get { return s_RowMetadataTable; }
            set { s_RowMetadataTable = value; }
        }

        public string RowFactor
        {
            get { return s_RowFactor; }
            set { s_RowFactor = value; }
        }

        public string SearchScoreWeights
        {
            get { return s_SearchScoreWeights; }
            set { s_SearchScoreWeights = value; }
        }

        public string MeanCenter
        {
            get { return s_MeanCenter; }
            set { s_MeanCenter = value; }
        }

        /// <summary>
        /// Name to save the imported data to
        /// </summary>
        public string NewTableName
        {
            get { return s_NewTableName; }
            set { s_NewTableName = value; }
        }

        public string SkipTheFirstColumn
        {
            get { return s_SkipTheFirstColumn; }
            set { s_SkipTheFirstColumn = value; }
        }

        public string SummaryTableName
        {
            get { return s_SummaryTableName; }
            set { s_SummaryTableName = value; }
        }

        /// <summary>
        /// For special cases involving saving the new table under
        /// a specific structure, e.g. as AssayData within an
        /// ExpressionSet object in R
        /// </summary>
        public string TableType
        {
            get { return s_TableType; }
            set { s_TableType = value; }
        }

        /// <summary>
        /// Determines where the data will be placed, e.g. R, C#, etc.
        /// </summary>
        public string Target
        {
            get { return s_Target; }
            set { s_Target = value; }
        }

        public string Unbalanced
        {
            get { return s_Unbalanced; }
            set { s_Unbalanced = value; }
        }

        public string UseREML
        {
            get { return s_UseREML; }
            set { s_UseREML = value; }
        }

        /// <summary>
        /// Specific for R, indicates how the data is stored.
        /// e.g. As an expressionSet, data.frame, matrix, etc.
        /// </summary>
        public string ImportDatasetType
        {
            get { return s_ImportDatasetType; }
            set { s_ImportDatasetType = value; }
        }

        public string FactorTable
        {
            get { return s_FactorTable; }
            set
            {
                s_FactorTable = value;

                if (HasFactorTable & HasFactorColumn & !HasFactorComplete)
                {
                    s_FactorComplete = s_FactorTable + "$" + s_FactorColumn;
                }
            }
        }        

        public string FactorColumn
        {
            get { return s_FactorColumn; }
            set 
            { 
                s_FactorColumn = value;

                if (HasFactorTable & HasFactorColumn & !HasFactorComplete)
                {
                    s_FactorComplete = s_FactorTable + "$" + s_FactorColumn;
                }
            }
        }

        public string TechRepColumn
        {
            get { return s_TechRepColumn; }
            set
            {
                s_TechRepColumn = value;

                if (HasFactorTable & HasTechRepColumn & !HasTechRepComplete)
                {
                    s_TechRepComplete = s_FactorTable + "$" + s_TechRepColumn;                    
                }
            }
        }

        public bool HasTechRepColumn
        {
            get
            {
                return s_TechRepColumn.Length > 0 ? true : false;
            }
        }

        public bool HasTechRepComplete
        {
            get
            {
                return s_TechRepComplete.Length > 0 ? true : false;
            }
        }

        public string TechRepComplete
        {
            get { return s_TechRepComplete; }
            set
            {
                s_TechRepComplete = value;

                if (s_TechRepComplete.Length > 0 &
                    (!HasTechRepColumn | !HasFactorTable))
                {
                    string[] s = TechRepComplete.Split('$');
                    if (s.Length == 2)
                    {
                        FactorTable = s[0];
                        s_TechRepColumn = s[1];;
                    }
                }
            }
        }

        public string FactorComplete
        {
            get { return s_FactorComplete; }
            set 
            { 
                s_FactorComplete = value;

                if (s_FactorComplete.Length > 0 &
                    (!HasFactorColumn | !HasFactorTable))
                {
                    string[] s = FactorComplete.Split('$');
                    if (s.Length == 2)
                    {
                        FactorTable = s[0];
                        FactorColumn = s[1];
                    }
                    else
                    {
                        // TODO: Throw an error because there 
                        // should only be 2 elements in the array.
                    }
                }
            }
        }

        public string Margin
        {
            get { return s_Margin; }
            set { s_Margin = value; }
        }

        public string Function
        {
            get { return s_Function; }
            set { s_Function = value; }
        }

        /// <summary>
        /// Theta parameter used in the Beta-Binomial Model Analysis
        /// for Spectral Counting
        /// </summary>
        public string Theta
        {
            get { return s_Theta; }
            set { s_Theta = value; }
        }

        public string Threshold
        {
            get { return s_Threshold; }
            set { s_Threshold = value; }
        }

        /// <summary>
        /// Name of the p-value table, used for combining result tables
        /// </summary>
        public string P_ValueTable
        {
            get { return s_P_ValueTable; }
            set { s_P_ValueTable = value; }
        }

        /// <summary>
        /// Name of the fold-change table, used for combining result tables
        /// </summary>
        public string FoldChangeTable
        {
            get { return s_FoldChangeTable; }
            set { s_FoldChangeTable = value; }
        }

        /// <summary>
        /// Name of the normalized table, used for combining result tables
        /// </summary>
        public string NormalizedTable
        {
            get { return s_NormalizedTable; }
            set { s_NormalizedTable = value; }
        }

        /// <summary>
        /// For merging tables, the name of the column in table X used for 
        /// linking to table Y
        /// </summary>
        public string X_Link
        {
            get { return s_xLink; }
            set { s_xLink = value; }
        }

        /// <summary>
        /// For merging tables, the name of the column in table Y used for
        /// linking to table X
        /// </summary>
        public string Y_Link
        {
            get { return s_yLink; }
            set { s_yLink = value; }
        }

        /// <summary>
        /// Name of Table X
        /// </summary>
        public string X_Table
        {
            get { return s_xTable; }
            set { s_xTable = value; }
        }

        /// <summary>
        /// Name of Table Y
        /// </summary>
        public string Y_Table
        {
            get { return s_yTable; }
            set { s_yTable = value; }
        }

        /// <summary>
        /// For multiple identifiers entered into the dictionary as a List<string>
        /// this property returns them as a comma-separated string with quotes
        /// </summary>
        public string CommaSeparatedWithQuotesIdentifiers
        {
            get { return s_CommaSep_wQuotesIdentifiers; }
            set { s_CommaSep_wQuotesIdentifiers = value; }
        }

        /// <summary>
        /// For multiple identifiers entered into the dictionary as a List<string>
        /// this property returns them as a comma-separated string without quotes
        /// </summary>
        public string CommaSeparatedWithoutQuotesIdentifiers
        {
            get { return s_CommaSep_woQuotesIdentifiers; }
            set { s_CommaSep_woQuotesIdentifiers = value; }
        }

        /// <summary>
        /// For multiple identifiers entered into the dictionary as a List<string>
        /// this property returns them as a tab-separated string with quotes
        /// </summary>
        public string TabSeparatedWithQuotesIdentifiers
        {
            get { return s_TabSep_wQuotesIdentifiers; }
            set { s_TabSep_wQuotesIdentifiers = value; }
        }

        /// <summary>
        /// For multiple identifiers entered into the dictionary as a List<string>
        /// this property returns them as a tab-separated string without quotes
        /// </summary>
        public string TabSeparatedWithoutQuotesIdentifiers
        {
            get { return s_TabSep_woQuotesIdentifiers; }
            set { s_TabSep_woQuotesIdentifiers = value; }
        }

        /// <summary>
        /// For multiple identifiers entered into the dictionary as a List<string>
        /// this property returns them as a semicolon-separated string with quotes
        /// </summary>
        public string SemicolonSeparatedWithQuotesIdentifiers
        {
            get { return s_Semicolon_wQuotesSepIdentifiers; }
            set { s_Semicolon_wQuotesSepIdentifiers = value; }
        }

        /// <summary>
        /// For multiple identifiers entered into the dictionary as a List<string>
        /// this property returns them as a semicolon-separated string without quotes
        /// </summary>
        public string SemicolonSeparatedWithoutQuotesIdentifiers
        {
            get { return s_Semicolon_woQuotesSepIdentifiers; }
            set { s_Semicolon_woQuotesSepIdentifiers = value; }
        }

        /// <summary>
        /// Defaults to 0.0001 (used in RMD analysis)
        /// </summary>
        public string PvalueThreshold
        {
            get { return s_PvalueThreshold; }
            set { s_PvalueThreshold = value; }
        }

        

        /// <summary>
        /// Value to scale the matrix in Transform
        /// </summary>
        public string Scale
        {
            get { return s_Scale; }
            set { s_Scale = value; }
        }

        /// <summary>
        /// Base to transform the matrix in Transform
        /// </summary>
        public string LogBase
        {
            get { return s_LogBase; }
            set { s_LogBase = value; }
        }

        /// <summary>
        /// Indicates the column to use for rownames when
        /// importing a table
        /// </summary>
        public string RowNames
        {
            get { return s_Rowname; }
            set { s_Rowname = value; }
        }

        public string MaxProtValue
        {
            get { return s_MaxProtValue; }
            set { s_MaxProtValue = value; }
        }

        public string MaxPepValue
        {
            get { return s_MaxPepValue; }
            set { s_MaxPepValue = value; }
        }


        public string MinProtValue
        {
            get { return s_MinProtValue; }
            set { s_MinProtValue = value; }
        }

        public string MinPepValue
        {
            get { return s_MinPepValue; }
            set { s_MinPepValue = value; }
        }

        public string NewRowMetadataTableName
        {
            get { return s_NewRowMetadataTableName; }
            set { s_NewRowMetadataTableName = value; }
        }

        /// <summary>
        /// Used in RRollup method, points to the T_Row_Metadata table
        /// </summary>
        public string ProteinInformationTable
        {
            get { return s_ProteinInfo; }
            set { s_ProteinInfo = value; }            
        }

        /// <summary>
        /// Used in RRollup, indicates whether a Protein information table has been designated
        /// </summary>
        public bool HasProteinInformationTable
        {
            get { return s_ProteinInfo.Length > 0 ? true : false; }
        }

        /// <summary>
        /// Used in RRollup method, minimum presence of peptide considered in rollup, default is 10
        /// </summary>
        public string MinimumPresence
        {
            get { return s_MinPresence; }
            set { s_MinPresence = value; }
        }

        /// <summary>
        /// Used in RRollup, method used to rollup up peptide abundances, default is median
        /// </summary>
        public string Mode
        {
            get { return s_Mode; }
            set { s_Mode = value; }
        }

        public string Model
        {
            get { return s_Model; }
            set { s_Model = value; }
        }

        /// <summary>
        /// Used in RRollup, index of protein id column in T_Row_Metadata table, default is 1
        /// </summary>
        public string ProteinInfo_ProteinColumn
        {
            get { return s_ProteinColumn; }
            set { s_ProteinColumn = value; }
        }

        /// <summary>
        /// Used in RRollup, index of peptide id column in T_Row_Metadata table, default is 3
        /// </summary>
        public string ProteinInfo_PeptideColumn
        {
            get { return s_PeptideColumn; }
            set { s_PeptideColumn = value; }
        }

        /// <summary>
        /// Used in RRollup, minimum overlap of peptides, default is 3
        /// </summary>
        public string MinimumOverlap
        {
            get { return s_MinOverlap; }
            set { s_MinOverlap = value; }
        }

        /// <summary>
        /// Used in RRollup, TRUE or FALSE whether to include one-hit-wonders, default is TRUE
        /// </summary>
        public string OneHitWonders
        {
            get { return s_OneHitWonders; }
            set { s_OneHitWonders = value; }
        }

        /// <summary>
        /// Used in RRollup, gp-value threshold, default 0.05
        /// </summary>
        public string GpValue
        {
            get { return s_gpValue; }
            set { s_gpValue = value; }
        }

        /// <summary>
        /// Used in RRollup, default is 5
        /// </summary>
        public string GminPCount
        {
            get { return s_gMinPCount; }
            set { s_gMinPCount = value; }
        }

        /// <summary>
        /// Used in RRollup, TRUE or FALSE whether to center the data around 0, default is FALSE
        /// </summary>
        public string Center
        {
            get { return s_Center; }
            set { s_Center = value; }
        }

        public string ID
        {
            get { return s_ID; }
            set { s_ID = value; }
        }

        public string Variable
        {
            get { return s_Variable; }
            set { s_Variable = value; }
        }

        public string RemoveFirstCharacters
        {
            get { return s_RemoveFirstCharacters; }
            set { s_RemoveFirstCharacters = value; }
        }

        public bool HasSource
        {
            get 
            {
                return s_Source.Length > 0 ? true : false;
            }
        }

        public bool Set_0_to_NA
        {
            get
            {
                return s_SetZeroToNA.ToLower().Equals("true") ? true : false;
            }
        }

        public bool RemovePeptideColumn
        {
            get
            {
                return s_RemovePeptideColumn.ToLower().Equals("true") ? true : false;
            }
        }
        
        public string SetRemovePeptideColumn
        {
            set
            {
                s_RemovePeptideColumn = value;
            }
        }

        public bool RunAnalysis
        {
            get
            {
                return s_RunAnalysis.ToLower().Equals("true") ? true : false;
            }
        }

        public string SetRunAnalysis
        {
            set
            {
                s_RunAnalysis = value;
            }
        }

        public string Set02NA
        {
            get
            {
                return s_SetZeroToNA;
            }
            set
            {
                s_SetZeroToNA = value;
            }
        }

        public bool HasTarget
        {
            get
            {
                return s_Target.Length > 0 ? true : false;
            }
        }

        public bool HasImportDatasetType
        {
            get
            {
                return s_ImportDatasetType.Length > 0 ? true : false;
            }
        }

        public bool HasWorkDir
        {
            get
            {
                return s_WorkDir.Length > 0 ? true : false;
            }
        }

        public bool HasInputFileName
        {
            get
            {
                return s_InputFileName.Length > 0 ? true : false;
            }
        }

        public bool HasInputTableName
        {
            get
            {
                return s_InputTableName.Length > 0 ? true : false;
            }
        }

        public bool HasNewTableName
        {
            get
            {
                return s_NewTableName.Length > 0 ? true : false;
            }
        }

        public bool HasTableType
        {
            get
            {
                return s_TableType.Length > 0 ? true : false;
            }
        }

        public bool HasFactorTable
        {
            get
            {
                return s_FactorTable.Length > 0 ? true : false;
            }
        }

        public bool HasFactorColumn
        {
            get
            {
                return s_FactorColumn.Length > 0 ? true : false;
            }
        }

        public bool HasFactorComplete
        {
            get
            {
                return s_FactorComplete.Length > 0 ? true : false;
            }
        }

        public bool HasMargin
        {
            get
            {
                return s_Margin.Length > 0 ? true : false;
            }
        }

        public bool HasFunction
        {
            get
            {
                return s_Function.Length > 0 ? true : false;
            }
        }

        public bool HasTheta
        {
            get
            {
                return s_Theta.Length > 0 ? true : false;
            }
        }

        public bool HasPvalueTable
        {
            get
            {
                return s_P_ValueTable.Length > 0 ? true : false;
            }
        }

        public bool HasFoldChangeTable
        {
            get
            {
                return s_FoldChangeTable.Length > 0 ? true : false;
            }
        }

        public bool HasNormalizedTable
        {
            get 
            {
                return s_NormalizedTable.Length > 0 ? true : false;
            }
        }

        public bool HasXLink
        {
            get
            {
                return s_xLink.Length > 0 ? true : false;
            }
        }

        public bool HasYLink
        {
            get
            {
                return s_yLink.Length > 0 ? true : false;
            }
        }

        public bool HasXTable
        {
            get
            {
                return s_xTable.Length > 0 ? true : false;
            }
        }

        public bool HasYTable
        {
            get
            {
                return s_yTable.Length > 0 ? true : false;
            }
        }

        public bool HasLogBase
        {
            get
            {
                return s_LogBase.Length > 0 ? true : false;
            }
        }        

        public bool HasAltInputTableName
        {
            get
            {
                return s_AltInputTableName.Length > 0 ? true : false;
            }
        }

        public bool HasAllX
        {
            get
            {
                return s_AllX.Length > 0 ? true : false;
            }
        }

        public bool HasAllY
        {
            get
            {
                return s_AllY.Length > 0 ? true : false;
            }
        }

        public bool HasRemoveFirstCharacters
        {
            get
            {
                return s_RemoveFirstCharacters.Length > 0 ? true : false;
            }
        }                
        #endregion

        #region Methods
        public void GetParameters()
        {
            if (d_Param.Count > 0)
            {
                SetValues();
            }
        }

        public void GetParameters(string ModuleName)
        {
            s_ModuleName = ModuleName;
            if (d_Param.Count > 0)
            {
                SetValues();
            }
        }
        public void GetParameters(string ModuleName,
            Dictionary<string, dynamic> Parameters)
        {
            s_ModuleName = ModuleName;
            d_Param = Parameters;
            if (d_Param.Count > 0)
            {
                SetValues();
            }
        }        

        private void SetValues()
        {
            foreach (KeyValuePair<string, dynamic> kvp in d_Param)
            {
                switch (kvp.Key)
                {
                    case "add":
                        Add = kvp.Value;
                        break;
                    case "allX":
                        AllX = kvp.Value;
                        break;
                    case "allY":
                        AllY = kvp.Value;
                        break;
                    case "altInputTableName":
                        AltInputTableName = kvp.Value;
                        break;
                    case "analysisType":
                        AnalysisType = kvp.Value;
                        break;
                    case "asDataMatrix":
                        AsDataMatrix = kvp.Value;
                        break;
                    case "autoScale":
                        string s_AutoScale = kvp.Value;
                        AutoScale = s_AutoScale.ToLower().Equals("true") ? true : false;
                        break;
                    case "bioRep":
                        BioRep = kvp.Value;
                        break;
                    case "colFactor":
                        ColumnFactor = kvp.Value;
                        break;
                    case "colMetadataTable":
                        ColumnMetadataTable = kvp.Value;
                        break;
                    case "columnName":
                        ColumnName = kvp.Value;
                        break;
                    case "commaSepWithQuotesIdentifiers":
                        List<string> l_csqi = kvp.Value;
                        string s_csqi = SeparateListOfStrings(l_csqi, ',', true);
                        CommaSeparatedWithQuotesIdentifiers = s_csqi;
                        break;
                    case "commaSepWithoutQuotesIdentifiers":
                        List<string> l_csnqi = kvp.Value;
                        string s_csnqi = SeparateListOfStrings(l_csnqi, ',', false);
                        CommaSeparatedWithoutQuotesIdentifiers = s_csnqi;
                        break;
                    case "Consolidation_Factor":
                        ConsolidationFactor = kvp.Value;
                        break;
                    case "contrasts":
                        Contrasts = kvp.Value;
                        break;
                    case "decoyPrefix":
                        DecoyPrefix = kvp.Value;
                        break;
                    case "Fixed_Effect":
                        FixedEffect = kvp.Value;
                        break;
                    case "interaction":
                        Interaction = kvp.Value;
                        break;
                    case "linkRow":
                        LinkRow = kvp.Value;
                        break;
                    case "linkCol":
                        LinkCol = kvp.Value;
                        break;
                    case "maxAmbiguousIds":
                        MaxAmbiguousIds = kvp.Value;
                        break;
                    case "maxFDR":
                        MaxFDR = kvp.Value;
                        break;
                    case "maxPepValue":
                        MaxPepValue = kvp.Value;
                        break;
                    case "maxProtValue":
                        MaxProtValue = kvp.Value;
                        break;                        
                    case "minAdditionalPeptides":
                        MinAdditionalPeptides = kvp.Value;
                        break;
                    case "minDistinctPeptides":
                        MinDistinctPeptides = kvp.Value;
                        break;
                    case "minPepValue":
                        MinPepValue = kvp.Value;
                        break;
                    case "minProtValue":
                        MinProtValue = kvp.Value;
                        break;
                    case "minSpectraPerProtein":
                        MinSpectraPerProtein = kvp.Value;
                        break;
                    case "newRowMetadataTableName":
                        NewRowMetadataTableName = kvp.Value;
                        break;
                    case "normalizedSearchScore":
                        NormalizedSearchScores = kvp.Value;
                        break;
                    case "optimizedSearchScore":
                        OptimizeScoreWeights = kvp.Value;
                        break;
                    case "outputTextReport":
                        OutputTextReport = kvp.Value;
                        break;
                    case "quantitativeMethod":
                        QuantitativeMethod = kvp.Value;
                        break;
                    case "rawSourcePath":
                        RawSourcePath = kvp.Value;
                        break;
                    case "rowFactor":
                        RowFactor = kvp.Value;
                        break;
                    case "rowMetadataTable":
                        RowMetadataTable = kvp.Value;
                        break;
                    case "orgdbdir":
                        DatabasePath = kvp.Value;
                        break;
                    case "source":
                        Source = kvp.Value;
                        break;
                    case "filePath":
                        FilePath = kvp.Value;
                        break;
                    case "password":
                        Password = kvp.Value;
                        break;                    
                    case "target":
                        Target = kvp.Value;
                        break;
                    case "unbalanced":
                        Unbalanced = kvp.Value;
                        break;
                    case "useREML":
                        UseREML = kvp.Value;
                        break;
                    case "importDatasetType":
                        ImportDatasetType = kvp.Value;
                        break;
                    case "workDir":
                        WorkDirectory = kvp.Value.Replace('\\', '/');
                        break;
                    case "inputFileName":
                        InputFileName = kvp.Value;
                        break;
                    case "inputTableName":
                        InputTableName = kvp.Value;
                        break;
                    case "ionColumnNamePrefix":
                        IonColumnNamePrefix = kvp.Value;
                        break;
                    case "meanCenter":
                        MeanCenter = kvp.Value; // default FALSE
                        break;
                    case "newTableName":
                        NewTableName = kvp.Value;
                        break;
                    case "tableType":
                        TableType = kvp.Value;
                        break;
                    case "factorTable":
                        FactorTable = kvp.Value;                       
                        break;
                    case "factorColumn":
                        FactorColumn = kvp.Value;                       
                        break;
                    case "factorComplete":
                        FactorComplete = kvp.Value;                       
                        break;
                    case "margin":
                        Margin = kvp.Value;
                        break;
                    case "maximum":
                        Maximum = kvp.Value;
                        break;
                    case "minimum":
                        Minimum = kvp.Value;
                        break;
                    case "function":
                        Function = kvp.Value;
                        break;
                    case "set02na":
                        Set02NA = kvp.Value;
                        break;
                    case "theta":
                        Theta = kvp.Value;
                        break;
                    case "threshold":
                        Threshold = kvp.Value;
                        break;
                    case "pValueTable":
                        P_ValueTable = kvp.Value;
                        break;
                    case "foldChangeTable":
                        FoldChangeTable = kvp.Value;
                        break;
                    case "fractionSetColumnName":
                        s_FractionSetColumnName = kvp.Value;
                        break;
                    case "normalizedTable":
                        NormalizedTable = kvp.Value;
                        break;
                    case "xLink":
                        X_Link = kvp.Value;
                        break;
                    case "yLink":
                        Y_Link = kvp.Value;
                        break;
                    case "xTable":
                        X_Table = kvp.Value;
                        break;
                    case "yTable":
                        Y_Table = kvp.Value;
                        break;
                    
                    case "tabSepWithQuotesIdentifiers":
                        List<string> l_tsqi = kvp.Value;
                        string s_tsqi = SeparateListOfStrings(l_tsqi, '\t', true);
                        TabSeparatedWithQuotesIdentifiers = s_tsqi;
                        break;
                    case "tabSepWithoutQuotesIdentifiers":
                        List<string> l_tsnqi = kvp.Value;
                        string s_tsnqi = SeparateListOfStrings(l_tsnqi, '\t', false);
                        TabSeparatedWithoutQuotesIdentifiers = s_tsnqi;
                        break;
                    case "reference":
                        Reference = kvp.Value;
                        break;
                    case "removePeptideColumn":
                        s_RemovePeptideColumn = kvp.Value;
                        break;
                    case "RunProteinProphet":
                        s_RunAnalysis = kvp.Value;
                        break;
                    case "runAnalysis":
                        s_RunAnalysis = kvp.Value;
                        break;
                    case "semicolonSepWithQuotesIdentifiers":
                        List<string> l_ssqi = kvp.Value;
                        string s_ssqi = SeparateListOfStrings(l_ssqi, ';', true);
                        SemicolonSeparatedWithQuotesIdentifiers = s_ssqi;
                        break;
                    case "semicolonSepWithoutQuotesIdentifiers":
                        List<string> l_ssnqi = kvp.Value;
                        string s_ssnqi = SeparateListOfStrings(l_ssnqi, ';', false);
                        SemicolonSeparatedWithQuotesIdentifiers = s_ssnqi;
                        break;
                    case "skipTheFirstColumn":
                        SkipTheFirstColumn = kvp.Value;
                        break;
                    case "pValueThreshold":
                        PvalueThreshold = kvp.Value;
                        break;                    
                    case "scale":
                        Scale = kvp.Value;
                        break;
                    case "logBase":
                        LogBase = kvp.Value;
                        break;
                    case "rowNames":
                        RowNames = kvp.Value;
                        break;                    
                    case "id":
                        ID = kvp.Value;
                        break;
                    case "variable":
                        Variable = kvp.Value;
                        break;
                    case "removeFirstCharacters":
                        RemoveFirstCharacters = kvp.Value;
                        break;

                    case "Random_Effect":
                        RandomEffect = kvp.Value;
                        break;
                    // RRollup parameter names
                    case "proteinInfoTable":
                        ProteinInformationTable = kvp.Value;
                        break;
                    case "minPresence":
                        MinimumPresence = kvp.Value;
                        break;
                    case "mode":
                        Mode = kvp.Value;
                        break;
                    case "model":
                        Model = kvp.Value;
                        break;
                    case "proteinInfo_ProteinCol":
                        ProteinInfo_ProteinColumn = kvp.Value;
                        break;
                    case "proteinInfo_PeptideCol":
                        ProteinInfo_PeptideColumn = kvp.Value;
                        break;
                    case "minOverlap":
                        MinimumOverlap = kvp.Value;
                        break;
                    case "oneHitWonders":
                        OneHitWonders = kvp.Value;
                        break;
                    case "gpvalue":
                        GpValue = kvp.Value;
                        break;
                    case "gminPCount":
                        GminPCount = kvp.Value;
                        break;
                    case "center":
                        Center = kvp.Value;
                        break;
                    case "summaryTableName":
                        SummaryTableName = kvp.Value;
                        break;
                }
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
