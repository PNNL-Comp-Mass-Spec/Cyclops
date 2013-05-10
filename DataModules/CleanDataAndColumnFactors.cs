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
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Cyclops.DataModules
{
    public class CleanDataAndColumnFactors : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "CleanDataAndColumnFactors",
            m_MergeColumnName = "Alias";
        /// <summary>
        /// Required parameters to run CleanDataAndColumnFactors Module
        /// </summary>
        private enum RequiredParameters
        {
            InputTableName, FactorTable, FactorColumn
        }
        #endregion

        #region Properties
        /// <summary>
        /// Column in Column Metadata Table that corresponds
        /// to the column names in the Data Table.
        /// Defaults to 'Alias'
        /// </summary>
        public string MergeColumn
        {
            get { return m_MergeColumnName; }
            set { m_MergeColumnName = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an CleanDataAndColumnFactors Module
        /// </summary>
        public CleanDataAndColumnFactors()
        {
            ModuleName = m_ModuleName;
        }

        /// <summary>
        /// CleanDataAndColumnFactors module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public CleanDataAndColumnFactors(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
        }

        /// <summary>
        /// CleanDataAndColumnFactors module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public CleanDataAndColumnFactors(CyclopsModel CyclopsModel,
            Dictionary<string, string> ExportParameters)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
            Parameters = ExportParameters;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override bool PerformOperation()
        {
            bool b_Successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running CleanDataAndColumnFactors",
                        ModuleName, StepNumber);

                if (CheckParameters())
                    b_Successful = CleanDataAndColumnFactorsFunction();
            }

            return b_Successful;
        }

        /// <summary>
        /// Retrieves a dictionary of all parameters used by the module, 
        /// and the corresponding default values
        /// </summary>
        /// <returns>Parameters used by module</returns>
        public override Dictionary<string, string> GetParametersTemplate()
        {
            Dictionary<string, string> d_Parameters = new Dictionary<string, string>();

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                d_Parameters.Add(s, "");
            }

            return d_Parameters;
        }

        /// <summary>
        /// Checks the parameters to ensure that all required keys are present
        /// </summary>
        /// <returns>True, if all required keys are included in the
        /// Parameters</returns>
        public override bool CheckParameters()
        {
            bool b_Successful = true;

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogError("Required Field Missing: " + s,
                        ModuleName, StepNumber);
                    b_Successful = false;
                    return b_Successful;
                }

                if (b_Successful &&
                    !Model.RCalls.ContainsObject(
                    Parameters[RequiredParameters.InputTableName.ToString()]))
                {
                    Model.LogError("Error encountered: R work environment " +
                        "does not contain the 'InputTableName': " +
                        Parameters[RequiredParameters.InputTableName.ToString()],
                        ModuleName, StepNumber);
                    b_Successful = false;
                }

                if (b_Successful &&
                    !Model.RCalls.ContainsObject(
                    Parameters[RequiredParameters.FactorTable.ToString()]))
                {
                    Model.LogError("Error encountered: R work environment " +
                        "does not contain the 'FactorTable': " +
                        Parameters[RequiredParameters.FactorTable.ToString()],
                        ModuleName, StepNumber);
                    b_Successful = false;
                }

                if (b_Successful &&
                    !Model.RCalls.TableContainsColumn(
                    Parameters[RequiredParameters.FactorTable.ToString()],
                    Parameters[RequiredParameters.FactorColumn.ToString()]))
                {
                    Model.LogError("Error encountered: R work environment " +
                        "'FactorTable', " +
                        Parameters[RequiredParameters.FactorTable.ToString()] +
                        ", does not contain the 'FactorColumn', " +
                        Parameters[RequiredParameters.FactorColumn.ToString()],
                        ModuleName, StepNumber);
                    b_Successful = false;
                }

                if (b_Successful &&
                    Parameters.ContainsKey("MergeColumn"))
                    m_MergeColumnName = Parameters["MergeColumn"];
            }

            return b_Successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool CleanDataAndColumnFactorsFunction()
        {
            bool b_Successful = true;

            string s_TemporaryTableName = GetOrganizedFactorsVector(
                Parameters[RequiredParameters.InputTableName.ToString()],
                Parameters[RequiredParameters.FactorTable.ToString()],
                Parameters[RequiredParameters.FactorColumn.ToString()],
                StepNumber,
                MergeColumn,
                "T_Alias_");

            if (string.IsNullOrEmpty(s_TemporaryTableName))
            {
                Model.LogError("Error occurred while running: " +
                    "'GetOrganizedFactorsVector'!",
                    ModuleName, StepNumber);                    
                return false;
            }

            b_Successful = AreDataColumnLengthAndColumnMetadataRowsEqual(
                Parameters[RequiredParameters.InputTableName.ToString()],
                s_TemporaryTableName,
                Parameters[RequiredParameters.FactorColumn.ToString()]);

            return b_Successful;
        }

        /// <summary>
        /// Retrieves the Default Value
        /// </summary>
        /// <returns>Default Value</returns>
        protected override string GetDefaultValue()
        {
            return "false";
        }

        /// <summary>
        /// Retrieves the Type Name for automatically 
        /// registering the module assembly
        /// </summary>
        /// <returns>Module's Name</returns>
        protected override string GetTypeName()
        {
            return ModuleName;
        }

        /// <summary>
        /// Checks to see if the number of datasets in the DataTable
        /// are the same number as in the T_Column_Metadata table. If
        /// they are the same, it checks that the dataset names are 
        /// represented in the T_Column_Metadata table.
        /// </summary>
        /// <param name="DataTableName">Name of the table containing the main data</param>
        /// <param name="ColumnMetadataTableName">Name of the Column Metadata table</param>
        /// <param name="ColumnMetadataFactor">Column in the Column Metadata table that maps to the datasets</param>
        /// <returns>True, if the method completes successfully</returns>
        public bool AreDataColumnLengthAndColumnMetadataRowsEqual(
            string DataTableName, string ColumnMetadataTableName,
            string ColumnMetadataFactor)
        {
            bool b_Successful = true;

            TableInfo td_Data =
                Model.RCalls.GetDimensions(DataTableName);

            TableInfo td_ColumnMetadata =
                Model.RCalls.GetDimensions(ColumnMetadataTableName);

            if (td_Data.Columns != td_ColumnMetadata.Rows)
            {
                b_Successful = ModifyFactorAndDataTables(
                    DataTableName, ColumnMetadataTableName, 
                    ColumnMetadataFactor);
            }
            else
            {
                string Command = string.Format(
                    "dim({0}[-which({0}${1}%in%colnames({2})),])[1] == 0",
                    ColumnMetadataTableName,
                    ColumnMetadataFactor,
                    DataTableName);

                if (Model.RCalls.AssessBoolean(Command))
                {
                    // Equal so good to go
                    return true;
                }
                else
                {
                    b_Successful = ModifyFactorAndDataTables(
                        DataTableName, ColumnMetadataTableName,
                        ColumnMetadataFactor);
                }
            }

            return b_Successful;
        }

        /// <summary>
        /// Removes rows in the Column Metadata table that are not in the data table, and
        /// removes columns in the data table that do not have factor information.
        /// </summary>        
        /// <param name="DataTableName">Name of the table containing the main data</param>
        /// <param name="ColumnMetadataTableName">Name of the Column Metadata table</param>
        /// <param name="ColumnMetadataFactor">Column in the Column Metadata table that maps to the datasets</param>
        /// <returns>True if the method completes successfully</returns>
        public bool ModifyFactorAndDataTables(
            string DataTableName, string ColumnMetadataTableName,
            string ColumnMetadataFactor)
        {
            bool b_Successful = true;

            string Command = string.Format(
                            "{0} <- merge(x=cbind({1}=colnames({2}))," +
                            "y={0}, by.x=\"{1}\", by.y=\"{1}\"," +
                            "all.y=F, all.x=F)\n\n" +
                            "{2} <- {2}[,which(colnames({2})%in%{0}${1})]\n",
                            ColumnMetadataTableName,
                            ColumnMetadataFactor,
                            DataTableName);

            try
            {
                b_Successful = Model.RCalls.Run(
                    Command, ModuleName, StepNumber);
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while running " +
                    "'ModifyFactorAndDataTables': " + exc.ToString(),
                    ModuleName, StepNumber);
                b_Successful = false;
            }

            return b_Successful;
        }
        #endregion
    }
}
