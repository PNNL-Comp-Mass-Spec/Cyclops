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
    public class FilterByPeptideProteinCount : BaseDataModule
    {
        #region Members
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
            ProteinColumn, // designates the protein column in the RowMetadataTable
            PeptideColumn, // designates the peptides column in the RowMetadataTable (corresponds to the rownames in the T_Data table)
            ProteinInfo_ProteinCol, // designates the protein count column in the RowMetadataTable
            ProteinInfo_PeptideCol // designates the peptide count column in the RowMetadataTable
        }

        #endregion

        #region Properties

        #endregion

        #region Constructors
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

                Model.LogMessage("Running FilterByPeptideProteinCount", ModuleName, StepNumber);

                if (CheckParameters())
                    successful = FilterByPeptideProteinCountFunction();
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

            #region Check R Environment For Objects
            if (!Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.InputTableName.ToString()]))
            {
                Model.LogError("ERROR 'InputTableName' object, " +
                    Parameters[RequiredParameters.InputTableName.ToString()] +
                    ", not present in R environment!", ModuleName, StepNumber);
                successful = false;
            }

            if (successful &&
                !Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.RowMetadataTable.ToString()]))
            {
                Model.LogError("ERROR 'RowMetadataTable' object, " +
                    Parameters[RequiredParameters.RowMetadataTable.ToString()] +
                    ", not present in R environment!",
                    ModuleName, StepNumber);
                successful = false;
            }

            if (successful &&
                !Model.RCalls.TableContainsColumn(
                Parameters[RequiredParameters.RowMetadataTable.ToString()],
                Parameters[RequiredParameters.ProteinInfo_ProteinCol.ToString()]))
            {
                Model.LogError("ERROR 'RowMetadataTable' object, " +
                    Parameters[RequiredParameters.RowMetadataTable.ToString()] +
                    ", does not contain the 'ProteinInfo_ProteinCol' column: " +
                    Parameters[RequiredParameters.ProteinInfo_ProteinCol.ToString()] +
                    "! This column designates the protein count within the " +
                    "RowMetadataTable.",
                    ModuleName, StepNumber);
                successful = false;
            }

            if (successful &&
                !Model.RCalls.TableContainsColumn(
                Parameters[RequiredParameters.RowMetadataTable.ToString()],
                Parameters[RequiredParameters.ProteinInfo_PeptideCol.ToString()]))
            {
                Model.LogError("ERROR 'RowMetadataTable' object, " +
                    Parameters[RequiredParameters.RowMetadataTable.ToString()] +
                    ", does not contain the 'ProteinInfo_PeptideCol' column: " +
                    Parameters[RequiredParameters.ProteinInfo_PeptideCol.ToString()] +
                    "! This column designates the peptide count within the " +
                    "RowMetadataTable.",
                    ModuleName, StepNumber);
                successful = false;
            }

            if (successful &&
                !Model.RCalls.TableContainsColumn(
                Parameters[RequiredParameters.RowMetadataTable.ToString()],
                Parameters[RequiredParameters.PeptideColumn.ToString()]))
            {
                Model.LogError("ERROR 'RowMetadataTable' object, " +
                    Parameters[RequiredParameters.RowMetadataTable.ToString()] +
                    ", does not contain the 'PeptideColumn' column: " +
                    Parameters[RequiredParameters.PeptideColumn.ToString()] +
                    "! This column designates the peptide within the " +
                    "RowMetadataTable, that correspond to the rownames in " +
                    "the InputTableName (data table).",
                    ModuleName, StepNumber);
                successful = false;
            }

            if (successful &&
                !Model.RCalls.TableContainsColumn(
                Parameters[RequiredParameters.RowMetadataTable.ToString()],
                Parameters[RequiredParameters.ProteinColumn.ToString()]))
            {
                Model.LogError("ERROR 'RowMetadataTable' object, " +
                    Parameters[RequiredParameters.RowMetadataTable.ToString()] +
                    ", does not contain the 'ProteinColumn' column: " +
                    Parameters[RequiredParameters.ProteinColumn.ToString()] +
                    "! This column designates the proteins within the " +
                    "RowMetadataTable.",
                    ModuleName, StepNumber);
                successful = false;
            }
            #endregion

            #region Check For Parameters Specific To FilterByPeptideProteinCount
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
            #endregion

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
                    Parameters[RequiredParameters.NewTableName.ToString()],
                    Parameters[RequiredParameters.RowMetadataTable.ToString()],
                    Parameters[RequiredParameters.ProteinColumn.ToString()],
                    Parameters[RequiredParameters.PeptideColumn.ToString()],
                    Parameters[RequiredParameters.InputTableName.ToString()],
                    Parameters[RequiredParameters.ProteinInfo_ProteinCol.ToString()],
                    Parameters[RequiredParameters.ProteinInfo_PeptideCol.ToString()],
                    m_MaxProtValue,
                    m_MinProtValue,
                    m_MaxPepValue,
                    m_MinPepValue,
                    Parameters[RequiredParameters.NewRowMetadataTableName.ToString()]
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
        #endregion
    }
}
