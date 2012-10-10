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

using RDotNet;
using log4net;

namespace Cyclops.DataModules
{
    /// <summary>
    /// Module that filters the RowMetadata and Data tables based on 
    /// Peptide and Protein counts.
    /// </summary>
    public class clsFilterByPeptideProteinCount : clsBaseDataModule
    {
        #region Members
        private string s_RInstance, s_Current_R_Statement = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Constructors
        /// <summary>
        /// Generic clsFilterByPeptideProteinCount constructor.
        /// </summary>
        public clsFilterByPeptideProteinCount()
        {
            ModuleName = "Filter by Peptide/Protein Count Module";
        }
        /// <summary>
        /// clsFilterByPeptideProteinCount constructor.
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        public clsFilterByPeptideProteinCount(string InstanceOfR)
        {
            ModuleName = "Filter by Peptide/Protein Count Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// clsFilterByPeptideProteinCount constructor.
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        public clsFilterByPeptideProteinCount(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Filter by Peptide/Protein Count Module";
            Model = TheCyclopsModel;
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            if (Model.SuccessRunningPipeline)
            {
                Model.IncrementStep(ModuleName);

                FilterData();

                RunChildModules();
            }
        }

        /// <summary>
        /// Checks the dictionary to ensure all the necessary parameters are present
        /// </summary>
        /// <returns>True if all necessary parameters are present</returns>
        protected bool CheckPassedParameters()
        {
            bool b_2Pass = true;

            // NECESSARY PARAMETERS
            if (!dsp.HasNewTableName) // New Data table name
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: FilterByPeptideProteinCount class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName) // Data table
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: FilterByPeptideProteinCount class: 'inputTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (string.IsNullOrEmpty(dsp.NewRowMetadataTableName)) // New RowMetadata table name
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: FilterByPeptideProteinCount class: 'newRowMetadataTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (string.IsNullOrEmpty(dsp.RowMetadataTable)) // Name of RowMetadata table
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: FilterByPeptideProteinCount class: 'rowMetadataTable': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (string.IsNullOrEmpty(dsp.FactorColumn)) // Name of column in RowMetadata table designating Peptides (corresponds to rownames for t_data)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: FilterByPeptideProteinCount class: 'factorColumn': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (string.IsNullOrEmpty(dsp.ProteinInfo_ProteinColumn)) // Name of column in RowMetadata table designating protein count
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: FilterByPeptideProteinCount class: 'proteinInfo_ProteinCol': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (string.IsNullOrEmpty(dsp.ProteinInfo_PeptideColumn)) // Name of column in RowMetadata table designating peptide count
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: FilterByPeptideProteinCount class: 'proteinInfo_PeptideCol': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        /// <summary>
        /// Performs the Filter
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        private void FilterData()
        {
            dsp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                string s_TmpList = GetTemporaryTableName();

                string test = string.Format(
                    "if (!is.null({0})) {{\n" +
                    "\t{1} <- {0}$DataTable\n" +
                    "\t{2} <- {0}$RowMetaData\n" +
                    "}}\n",
                    s_TmpList,
                    dsp.NewTableName,
                    dsp.NewRowMetadataTableName);

                string s_RStatement = string.Format(
                    "{0} <- jnb_PeptideCountProteinCount(" +
                    "RowMetadataTable={0}, " +
                    "ProteinColumn='{1}', " +
                    "PeptideColumn='{2}')\n",
                    dsp.RowMetadataTable,
                    dsp.ColumnFactor,
                    dsp.FactorColumn);

                s_RStatement += string.Format(
                    "{0} <- jnb_FilterByPeptideProteinCount(" +
                    "df={1}, " +
                    "RowMetadataTable={2}, " +
                    "ProteinCountColumn='{3}', " +
                    "PeptideCountColumn='{4}', " +
                    "PeptideColumnName='{5}', " +
                    "MaxProtValue={6}, " +
                    "MinProtValue={7}, " +
                    "MaxPepValue={8}, " +
                    "MinPepValue={9})\n\n",
                    s_TmpList,
                    dsp.InputTableName,
                    dsp.RowMetadataTable,
                    dsp.ProteinInfo_ProteinColumn,
                    dsp.ProteinInfo_PeptideColumn,
                    dsp.FactorColumn,
                    string.IsNullOrEmpty(dsp.MaxProtValue) ? "NULL" : dsp.MaxProtValue,
                    string.IsNullOrEmpty(dsp.MinProtValue) ? "NULL" : dsp.MinProtValue,
                    string.IsNullOrEmpty(dsp.MaxPepValue) ? "NULL" : dsp.MaxPepValue,
                    string.IsNullOrEmpty(dsp.MinPepValue) ? "NULL" : dsp.MinPepValue);
                
                Model.SuccessRunningPipeline = clsGenericRCalls.Run(
                    s_RStatement, s_RInstance,
                    "Filtering data by peptide and protein counts",
                    Model.StepNumber, Model.NumberOfModules);

                s_RStatement = "";

                if (!clsGenericRCalls.IsNull(s_RInstance, s_TmpList))
                {
                    s_RStatement = string.Format(
                        "{0} <- {1}$DataTable\n" +
                        "{2} <- {1}$RowMetaData\n",
                        dsp.NewTableName,
                        s_TmpList,
                        dsp.NewRowMetadataTableName);
                }
                s_RStatement += string.Format("rm({0})\n",
                    s_TmpList);

                Model.SuccessRunningPipeline = clsGenericRCalls.Run(
                    s_RStatement, s_RInstance,
                    "Filtering data by peptide and protein counts",
                    Model.StepNumber, Model.NumberOfModules);
            }
            else
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: FilterByPeptideProteinCount class. " +
                    "Failed at Check Passed Parameters.");
            }
        }
        #endregion
    }
}
