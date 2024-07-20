/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://www.pnnl.gov/integrative-omics
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
    public class FilterByPeptideProteinCount : BaseDataModule
    {
        private readonly string m_ModuleName = "FilterByPeptideProteinCount";
        private readonly string m_Description = "";
        private string m_MaxProtValue = "NULL";
        private string m_MinProtValue = "NULL";
        private string m_MaxPepValue = "NULL";
        private string m_MinPepValue = "NULL";

        /// <summary>
        /// Required parameters to run FilterByPeptideProteinCount Module
        /// </summary>
        private enum RequiredParameters
        {
            NewTableName,
            InputTableName,
            NewRowMetadataTableName,
            RowMetadataTable,
            ProteinColumn,          // protein column in the RowMetadataTable
            PeptideColumn,          // peptides column in the RowMetadataTable (corresponds to the row names in the T_Data table)
            ProteinInfo_ProteinCol, // protein count column in the RowMetadataTable
            ProteinInfo_PeptideCol  // peptide count column in the RowMetadataTable
        }

        /// <summary>
        /// Generic constructor creating an FilterByPeptideProteinCount Module
        /// </summary>
        public FilterByPeptideProteinCount()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// FilterByPeptideProteinCount module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public FilterByPeptideProteinCount(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// FilterByPeptideProteinCount module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public FilterByPeptideProteinCount(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
            Parameters = ExportParameters;
        }

        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override bool PerformOperation()
        {
            var successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running FilterByPeptideProteinCount", ModuleName, StepNumber);

                if (CheckParameters())
                {
                    successful = FilterByPeptideProteinCountFunction();
                }
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
            }

            if (!Model.RCalls.ContainsObject(Parameters[nameof(RequiredParameters.InputTableName)]))
            {
                Model.LogError("ERROR 'InputTableName' object, " +
                    Parameters[nameof(RequiredParameters.InputTableName)] +
                    ", not present in R environment!", ModuleName, StepNumber);
                successful = false;
            }

            if (successful &&
                !Model.RCalls.ContainsObject(Parameters[nameof(RequiredParameters.RowMetadataTable)]))
            {
                Model.LogError("ERROR 'RowMetadataTable' object, " +
                    Parameters[nameof(RequiredParameters.RowMetadataTable)] +
                    ", not present in R environment!",
                    ModuleName, StepNumber);
                successful = false;
            }

            if (successful &&
                !Model.RCalls.TableContainsColumn(
                Parameters[nameof(RequiredParameters.RowMetadataTable)],
                Parameters[nameof(RequiredParameters.ProteinInfo_ProteinCol)]))
            {
                Model.LogError("ERROR 'RowMetadataTable' object, " +
                    Parameters[nameof(RequiredParameters.RowMetadataTable)] +
                    ", does not contain the 'ProteinInfo_ProteinCol' column: " +
                    Parameters[nameof(RequiredParameters.ProteinInfo_ProteinCol)] +
                    "! This column designates the protein count within the RowMetadataTable.",
                    ModuleName, StepNumber);
                successful = false;
            }

            if (successful &&
                !Model.RCalls.TableContainsColumn(
                Parameters[nameof(RequiredParameters.RowMetadataTable)],
                Parameters[nameof(RequiredParameters.ProteinInfo_PeptideCol)]))
            {
                Model.LogError("ERROR 'RowMetadataTable' object, " +
                    Parameters[nameof(RequiredParameters.RowMetadataTable)] +
                    ", does not contain the 'ProteinInfo_PeptideCol' column: " +
                    Parameters[nameof(RequiredParameters.ProteinInfo_PeptideCol)] +
                    "! This column designates the peptide count within the RowMetadataTable.",
                    ModuleName, StepNumber);
                successful = false;
            }

            if (successful &&
                !Model.RCalls.TableContainsColumn(
                Parameters[nameof(RequiredParameters.RowMetadataTable)],
                Parameters[nameof(RequiredParameters.PeptideColumn)]))
            {
                Model.LogError("ERROR 'RowMetadataTable' object, " +
                    Parameters[nameof(RequiredParameters.RowMetadataTable)] +
                    ", does not contain the 'PeptideColumn' column: " +
                    Parameters[nameof(RequiredParameters.PeptideColumn)] +
                    "! This column designates the peptide within the " +
                    "RowMetadataTable, that correspond to the row names in " +
                    "the InputTableName (data table).",
                    ModuleName, StepNumber);
                successful = false;
            }

            if (successful &&
                !Model.RCalls.TableContainsColumn(
                Parameters[nameof(RequiredParameters.RowMetadataTable)],
                Parameters[nameof(RequiredParameters.ProteinColumn)]))
            {
                Model.LogError("ERROR 'RowMetadataTable' object, " +
                    Parameters[nameof(RequiredParameters.RowMetadataTable)] +
                    ", does not contain the 'ProteinColumn' column: " +
                    Parameters[nameof(RequiredParameters.ProteinColumn)] +
                    "! This column designates the proteins within the RowMetadataTable.",
                    ModuleName, StepNumber);
                successful = false;
            }

            if (Parameters.ContainsKey("MaxProtValue") && !string.IsNullOrEmpty(Parameters["MaxProtValue"]))
            {
                m_MaxProtValue = Parameters["MaxProtValue"];
            }
            if (Parameters.ContainsKey("MinProtValue") && !string.IsNullOrEmpty(Parameters["MinProtValue"]))
            {
                m_MinProtValue = Parameters["MinProtValue"];
            }
            if (Parameters.ContainsKey("MaxPepValue") && !string.IsNullOrEmpty(Parameters["MaxPepValue"]))
            {
                m_MaxPepValue = Parameters["MaxPepValue"];
            }
            if (Parameters.ContainsKey("MinPepValue") && !string.IsNullOrEmpty(Parameters["MinPepValue"]))
            {
                m_MinPepValue = Parameters["MinPepValue"];
            }

            return successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool FilterByPeptideProteinCountFunction()
        {
            var successful = true;

            string tTable = GetTemporaryTableName("T_FilterPepProtCnt_"),
                   rCmd = string.Format(
                    "if (!is.null({0})) {{\n" +
                    "\t{1} <- {0}$DataTable\n" +
                    "\t{2} <- {0}$RowMetaData\n" +
                    "}}\n"+

                    "{2} <- jnb_PeptideCountProteinCount(" +
                    "RowMetadataTable={2}, " +
                    "ProteinColumn='{3}', " +
                    "PeptideColumn='{4}')\n" +

                    "{0} <- jnb_FilterByPeptideProteinCount(" +
                    "df={5}, " +
                    "RowMetadataTable={2}, " +
                    "ProteinCountColumn='{6}', " +
                    "PeptideCountColumn='{7}', " +
                    "PeptideColumnName='{4}', " +
                    "MaxProtValue={8}, " +
                    "MinProtValue={9}, " +
                    "MaxPepValue={10}, " +
                    "MinPepValue={11})\n\n" +

                    "if (!is.null({0}) {{\n" +
                    "{1} <- {0}$DataTable\n" +
                    "{12} <- {0}$RowMetaData\n" +
                    "}}\n" +
                    "rm({0})\n\n",

                    tTable,
                    Parameters[nameof(RequiredParameters.NewTableName)],
                    Parameters[nameof(RequiredParameters.RowMetadataTable)],
                    Parameters[nameof(RequiredParameters.ProteinColumn)],
                    Parameters[nameof(RequiredParameters.PeptideColumn)],
                    Parameters[nameof(RequiredParameters.InputTableName)],
                    Parameters[nameof(RequiredParameters.ProteinInfo_ProteinCol)],
                    Parameters[nameof(RequiredParameters.ProteinInfo_PeptideCol)],
                    m_MaxProtValue,
                    m_MinProtValue,
                    m_MaxPepValue,
                    m_MinPepValue,
                    Parameters[nameof(RequiredParameters.NewRowMetadataTableName)]
                );

            try
            {
                Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while " +
                    "running 'FilterByPeptideProteinCountFunction': " +
                    ex, ModuleName, StepNumber);
                SaveCurrentREnvironment();
                successful = false;
            }

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
    }
}
