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
    public class FilterByPeptideProteinCount : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "FilterByPeptideProteinCount",
            m_MaxProtValue = "NULL",
            m_MinProtValue = "NULL",
            m_MaxPepValue = "NULL",
            m_MinPepValue = "NULL";
        /// <summary>
        /// Required parameters to run FilterByPeptideProteinCount Module
        /// </summary>
        private enum RequiredParameters
        {
            NewTableName, InputTableName, NewRowMetadataTableName,
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
        }

        /// <summary>
        /// FilterByPeptideProteinCount module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public FilterByPeptideProteinCount(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
        }

        /// <summary>
        /// FilterByPeptideProteinCount module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public FilterByPeptideProteinCount(CyclopsModel CyclopsModel,
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

                Model.LogMessage("Running FilterByPeptideProteinCount",
                        ModuleName, StepNumber);

                if (CheckParameters())
                    b_Successful = FilterByPeptideProteinCountFunction();
            }

            return b_Successful;
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
            }

            #region Check R Environment For Objects
            if (b_Successful && 
                !Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.InputTableName.ToString()]))
            {
                Model.LogError("ERROR 'InputTableName' object, " +
                    Parameters[RequiredParameters.InputTableName.ToString()] +
                    ", not present in R environment!", ModuleName,
                    StepNumber);
                b_Successful = false;
            }

            if (b_Successful &&
                !Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.RowMetadataTable.ToString()]))
            {
                Model.LogError("ERROR 'RowMetadataTable' object, " +
                    Parameters[RequiredParameters.RowMetadataTable.ToString()] +
                    ", not present in R environment!", ModuleName,
                    StepNumber);
                b_Successful = false;
            }

            if (b_Successful &&
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
                    ModuleName,
                    StepNumber);
                b_Successful = false;
            }

            if (b_Successful &&
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
                    ModuleName,
                    StepNumber);
                b_Successful = false;
            }

            if (b_Successful &&
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
                    ModuleName,
                    StepNumber);
                b_Successful = false;
            }

            if (b_Successful &&
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
                    ModuleName,
                    StepNumber);
                b_Successful = false;
            }
            #endregion

            #region Check For Parameters Specific To FilterByPeptideProteinCount
            if (Parameters.ContainsKey("MaxProtValue"))
            {
                if (!string.IsNullOrEmpty(Parameters["MaxProtValue"]))
                    m_MaxProtValue = Parameters["MaxProtValue"];
            }
            if (Parameters.ContainsKey("MinProtValue"))
            {
                if (!string.IsNullOrEmpty(Parameters["MinProtValue"]))
                    m_MinProtValue = Parameters["MinProtValue"];
            }
            if (Parameters.ContainsKey("MaxPepValue"))
            {
                if (!string.IsNullOrEmpty(Parameters["MaxPepValue"]))
                    m_MaxPepValue = Parameters["MaxPepValue"];
            }
            if (Parameters.ContainsKey("MinPepValue"))
            {
                if (!string.IsNullOrEmpty(Parameters["MinPepValue"]))
                    m_MinPepValue = Parameters["MinPepValue"];
            }
            #endregion

            return b_Successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool FilterByPeptideProteinCountFunction()
        {
            bool b_Successful = true;

            string s_TmpTable = GetTemporaryTableName("T_FilterPepProtCnt_"),
                Command = string.Format(
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

                    "if (!is.null({0}) {\n" +
                    "{1} <- {0}$DataTable\n" +
                    "{12} <- {0}$RowMetaData\n" +
                    "}\n" +
                    "rm({0})\n\n",

                    s_TmpTable,
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
                Model.RCalls.Run(Command,
                    ModuleName, StepNumber);
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while " +
                    "running 'FilterByPeptideProteinCountFunction': " +
                    exc.ToString(), ModuleName, StepNumber);
                SaveCurrentREnvironment();
                b_Successful = false;
            }

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
        #endregion
    }
}
