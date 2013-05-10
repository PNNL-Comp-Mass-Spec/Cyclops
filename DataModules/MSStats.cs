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
    public class MSStats : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "MSStats",
            m_Description = "",
            m_ColumnMetadataLink = "Alias",
            m_Abundance = "Abundance",
            m_AnovaModel = "fixed",
            m_FeatureVar = "FALSE",
            m_Progress = "FALSE";
        /// <summary>
        /// Required parameters to run MSStats Module
        /// </summary>
        private enum RequiredParameters
        {
            NewTableName, InputTableName, Fixed_Effect,
            RowMetadataTable, ColumnMetadataTable, RowMetadataProteinColumn,
            RowMetadataPeptideColumn, BioRep, TechRep
        }
        #endregion

        #region Properties
        public string ColumnMetadataLink
        {
            get { return m_ColumnMetadataLink; }
            set { m_ColumnMetadataLink = value; }
        }

        public string Abundance
        {
            get { return m_Abundance; }
            set { m_Abundance = value; }
        }

        public string AnovaModel
        {
            get { return m_AnovaModel; }
            set { m_AnovaModel = value; }
        }

        public string FeatureVariance
        {
            get { return m_FeatureVar; }
            set { m_FeatureVar = value; }
        }

        public string ReportProgress
        {
            get { return m_Progress; }
            set { m_Progress = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an MSStats Module
        /// </summary>
        public MSStats()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// MSStats module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public MSStats(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// MSStats module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public MSStats(CyclopsModel CyclopsModel,
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

                Model.LogMessage("Running MSStats",
                        ModuleName, StepNumber);

                if (CheckParameters())
                    b_Successful = MSStatsFunction();
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

            d_Parameters.Add("ColumnMetadataLink", ColumnMetadataLink);
            d_Parameters.Add("Abundance", Abundance);
            d_Parameters.Add("FeatureVar", FeatureVariance);
            d_Parameters.Add("ReportProgress", ReportProgress);

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
            }

            if (Parameters.ContainsKey("FeatureVar"))
            {
                if (!string.IsNullOrEmpty(Parameters["FeatureVar"]))
                    m_FeatureVar = Parameters["FeatureVar"].ToUpper();
            }

            if (Parameters.ContainsKey("ReportProgress"))
            {
                if (!string.IsNullOrEmpty(Parameters["ReportProgress"]))
                    m_Progress = Parameters["ReportProgress"].ToUpper();
            }

            return b_Successful;
        }

        /// <summary>
        /// Performs MSstats from Olga's group
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool MSStatsFunction()
        {
            bool b_Successful = true;

            string s_TmpTable4MSstats = GetTemporaryTableName("Tmp4MSstats_"),
                s_TmpFitTable = GetTemporaryTableName("tmpFit_");

            string Command = string.Format(
                "{0} <- jnb_Prepare4MSstats(" +
                "dm=data.matrix({1}), " +
                "RowMetadataTable={2}, " +
                "ColMetadataTable={3}, " +
                "rmd_ProteinColumn='{4}', " +
                "rmd_PeptideColumn='{5}', " +
                "cmd_Link='{6}', " +
                "cmd_BioRep='{7}', " +
                "cmd_TechRep='{8}', " +
                "cmd_Factor='{9}')\n\n" +
                "{10} <- fitModels(" +
                "protein='{4}', " +
                "feature='{5}', " +
                "bio.rep='{7}', " +
                "group='{9}', " +
                "abundance='{11}', " +
                "model='{12}', " +
                "feature.var='{13}', " +
                "progress={14}, " +
                "data={0})\n\n" +
                "{15} <- subjectQuantification({10}, " +
                "table=F, progress={14})\n" +
                "{colnames({15})[4] <- 'value'\n" +
                "{15} <- cast({15}, {4}~{7})\n" +
                "rownames({15}) <- {15}[,1]\n" +
                "{15} <- {15}[,-1]\n\n",
                s_TmpTable4MSstats,
                Parameters[RequiredParameters.InputTableName.ToString()],
                Parameters[RequiredParameters.RowMetadataTable.ToString()],
                Parameters[RequiredParameters.ColumnMetadataTable.ToString()],
                Parameters[RequiredParameters.RowMetadataProteinColumn.ToString()],
                Parameters[RequiredParameters.RowMetadataPeptideColumn.ToString()],
                ColumnMetadataLink,
                Parameters[RequiredParameters.BioRep.ToString()],
                Parameters[RequiredParameters.TechRep.ToString()],
                Parameters[RequiredParameters.Fixed_Effect.ToString()],
                s_TmpFitTable,
                Abundance,
                AnovaModel,
                FeatureVariance,
                ReportProgress,
                Parameters[RequiredParameters.NewTableName.ToString()]
                );

            try
            {
                b_Successful = Model.RCalls.Run(Command,
                    ModuleName, StepNumber);
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while running MSstats:\n" +
                    exc.ToString(), ModuleName, StepNumber);
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
