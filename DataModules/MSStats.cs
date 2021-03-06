﻿/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: joseph.brown@pnnl.gov
 * Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/
 * -----------------------------------------------------
 * 
 * Licensed under the 2-Clause BSD License; you may not use this
 * file except in compliance with the License.  You may obtain
 * a copy of the License at https://opensource.org/licenses/BSD-2-Clause
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
        private string m_ModuleName = "MSStats";
        private string m_Description = "";
        private string m_ColumnMetadataLink = "Alias";
        private string m_Abundance = "Abundance";
        private string m_AnovaModel = "fixed";
        private string m_FeatureVar = "FALSE";
        private string m_Progress = "FALSE";
            
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
        public MSStats(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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
            bool successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running MSStats", ModuleName, StepNumber);

                if (CheckParameters())
                    successful = MSStatsFunction();
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
            Dictionary<string, string> paramDictionary = new Dictionary<string, string>();

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                paramDictionary.Add(s, "");
            }

            paramDictionary.Add("ColumnMetadataLink", ColumnMetadataLink);
            paramDictionary.Add("Abundance", Abundance);
            paramDictionary.Add("FeatureVar", FeatureVariance);
            paramDictionary.Add("ReportProgress", ReportProgress);

            return paramDictionary;
        }

        /// <summary>
        /// Checks the parameters to ensure that all required keys are present
        /// </summary>
        /// <returns>True, if all required keys are included in the
        /// Parameters</returns>
        public override bool CheckParameters()
        {
            bool successful = true;

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogError("Required Field Missing: " + s, ModuleName, StepNumber);
                    return false;
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

            return successful;
        }

        /// <summary>
        /// Performs MSstats from Olga's group
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool MSStatsFunction()
        {
            bool successful = true;

            string tTable4MSstats = GetTemporaryTableName("Tmp4MSstats_");
            string tFitTable = GetTemporaryTableName("tmpFit_");

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
                tTable4MSstats,
                Parameters[RequiredParameters.InputTableName.ToString()],
                Parameters[RequiredParameters.RowMetadataTable.ToString()],
                Parameters[RequiredParameters.ColumnMetadataTable.ToString()],
                Parameters[RequiredParameters.RowMetadataProteinColumn.ToString()],
                Parameters[RequiredParameters.RowMetadataPeptideColumn.ToString()],
                ColumnMetadataLink,
                Parameters[RequiredParameters.BioRep.ToString()],
                Parameters[RequiredParameters.TechRep.ToString()],
                Parameters[RequiredParameters.Fixed_Effect.ToString()],
                tFitTable,
                Abundance,
                AnovaModel,
                FeatureVariance,
                ReportProgress,
                Parameters[RequiredParameters.NewTableName.ToString()]
                );

            try
            {
                successful = Model.RCalls.Run(Command, ModuleName, StepNumber);
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while running MSstats:\n" +
                    exc.ToString(), ModuleName, StepNumber);
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
