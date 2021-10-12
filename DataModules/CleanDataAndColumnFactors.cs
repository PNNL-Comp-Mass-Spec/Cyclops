/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
 * -----------------------------------------------------
 *
 * Licensed under the 2-Clause BSD License; you may not use this
 * file except in compliance with the License.  You may obtain
 * a copy of the License at https://opensource.org/licenses/BSD-2-Clause
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace Cyclops.DataModules
{
    public class CleanDataAndColumnFactors : BaseDataModule
    {
        #region Members
        private readonly string m_ModuleName = "CleanDataAndColumnFactors";
        private readonly string m_Description = "";

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
        public string MergeColumn { get; set; } = "Alias";

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an CleanDataAndColumnFactors Module
        /// </summary>
        public CleanDataAndColumnFactors()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// CleanDataAndColumnFactors module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public CleanDataAndColumnFactors(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// CleanDataAndColumnFactors module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public CleanDataAndColumnFactors(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
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
            var successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running CleanDataAndColumnFactors", ModuleName, StepNumber);

                if (CheckParameters())
                    successful = CleanDataAndColumnFactorsFunction();
            }

            return successful;
        }

        /// <summary>
        /// Retrieves a dictionary of all parameters used by the module,
        /// and the corresponding default values
        /// </summary>
        /// <returns>Parameters used by module</returns>
        public override Dictionary<string, string> GetParametersTemplate()
        {
            var paramDictionary = new Dictionary<string, string>();

            foreach (var s in Enum.GetNames(typeof(RequiredParameters)))
            {
                paramDictionary.Add(s, "");
            }

            return paramDictionary;
        }

        /// <summary>
        /// Checks the parameters to ensure that all required keys are present
        /// </summary>
        /// <returns>True, if all required keys are included in the
        /// Parameters</returns>
        public override bool CheckParameters()
        {
            var successful = true;

            foreach (var s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogError("Required Field Missing: " + s, ModuleName, StepNumber);
                    return false;
                }

                if (successful &&
                    !Model.RCalls.ContainsObject(
                    Parameters[RequiredParameters.InputTableName.ToString()]))
                {
                    Model.LogError("Error encountered: R work environment " +
                        "does not contain the 'InputTableName': " +
                        Parameters[RequiredParameters.InputTableName.ToString()],
                        ModuleName, StepNumber);
                    successful = false;
                }

                if (successful &&
                    !Model.RCalls.ContainsObject(
                    Parameters[RequiredParameters.FactorTable.ToString()]))
                {
                    Model.LogError("Error encountered: R work environment " +
                        "does not contain the 'FactorTable': " +
                        Parameters[RequiredParameters.FactorTable.ToString()],
                        ModuleName, StepNumber);
                    successful = false;
                }

                if (successful &&
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
                    successful = false;
                }

                if (successful &&
                    Parameters.ContainsKey("MergeColumn"))
                {
                    MergeColumn = Parameters["MergeColumn"];
                }
            }

            return successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool CleanDataAndColumnFactorsFunction()
        {
            var temporaryTableName = GetOrganizedFactorsVector(
                Parameters[RequiredParameters.InputTableName.ToString()],
                Parameters[RequiredParameters.FactorTable.ToString()],
                Parameters[RequiredParameters.FactorColumn.ToString()],
                StepNumber,
                MergeColumn,
                "T_Alias_");

            if (string.IsNullOrEmpty(temporaryTableName))
            {
                Model.LogError("Error occurred while running: " +
                    "'GetOrganizedFactorsVector'!",
                    ModuleName, StepNumber);
                return false;
            }

            var successful = AreDataColumnLengthAndColumnMetadataRowsEqual(
                Parameters[RequiredParameters.InputTableName.ToString()],
                temporaryTableName,
                Parameters[RequiredParameters.FactorColumn.ToString()]);

            return successful;
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
        /// Retrieves the Type Description for automatically
        /// registering the module assembly
        /// </summary>
        /// <returns>Module's Description</returns>
        protected override string GetTypeDescription()
        {
            return Description;
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
            string DataTableName,
            string ColumnMetadataTableName,
            string ColumnMetadataFactor)
        {
            bool successful;

            var data = Model.RCalls.GetDimensions(DataTableName);

            var columnMetadata = Model.RCalls.GetDimensions(ColumnMetadataTableName);

            if (data.Columns != columnMetadata.Rows)
            {
                successful = ModifyFactorAndDataTables(DataTableName, ColumnMetadataTableName, ColumnMetadataFactor);
            }
            else
            {
                var rCmd = string.Format(
                    "dim({0}[-which({0}${1}%in%colnames({2})),])[1] == 0",
                    ColumnMetadataTableName,
                    ColumnMetadataFactor,
                    DataTableName);

                if (Model.RCalls.AssessBoolean(rCmd))
                {
                    // Equal so good to go
                    return true;
                }

                successful = ModifyFactorAndDataTables(DataTableName, ColumnMetadataTableName, ColumnMetadataFactor);
            }

            return successful;
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
            string DataTableName,
            string ColumnMetadataTableName,
            string ColumnMetadataFactor)
        {
            bool successful;

            var rCmd = string.Format(
                "{0} <- merge(x=cbind({1}=colnames({2}))," +
                "y={0}, by.x=\"{1}\", by.y=\"{1}\"," +
                "all.y=F, all.x=F)\n\n" +
                "{2} <- {2}[,which(colnames({2})%in%{0}${1})]\n",
                ColumnMetadataTableName,
                ColumnMetadataFactor,
                DataTableName);

            try
            {
                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while running " +
                    "'ModifyFactorAndDataTables': " + ex,
                    ModuleName, StepNumber);
                successful = false;
            }

            return successful;
        }
        #endregion
    }
}
