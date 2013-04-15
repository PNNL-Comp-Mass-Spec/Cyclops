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
    public class ANOVA : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "ANOVA",
            m_RandomEffect = "Null",
            m_Interaction = "False",
            m_Unbalanced = "True",
            m_Threshold = "3",
            m_UseREML = "True";
        private bool m_RemoveFirstColumn = false;
        /// <summary>
        /// Required parameters to run ANOVA Module
        /// </summary>
        private enum RequiredParameters
        {
            NewTableName, InputTableName, Mode, FactorTable, Fixed_Effect
        }

        /// <summary>
        /// Optional parameters that apply to ANOVA
        /// </summary>
        private enum AnovaParameters
        {
            Random_Effect, RowMetaDataTable, Interaction, 
            Unbalanced, Threshold, UseREML
        }
        #endregion

        #region Properties
        public string RandomEffect
        {
            get { return m_RandomEffect; }
            set { m_RandomEffect = value; }
        }

        public string Interaction
        {
            get { return m_Interaction; }
            set { m_Interaction = value; }
        }

        public string Unbalanced
        {
            get { return m_Unbalanced; }
            set { m_Unbalanced = value; }
        }

        public string Threshold
        {
            get { return m_Threshold; }
            set { m_Threshold = value; }
        }

        public string UseREML
        {
            get { return m_UseREML; }
            set { m_UseREML = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an ANOVA Module
        /// </summary>
        public ANOVA()
        {
            ModuleName = m_ModuleName;
        }

        /// <summary>
        /// ANOVA module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public ANOVA(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
        }

        /// <summary>
        /// ANOVA module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public ANOVA(CyclopsModel CyclopsModel,
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
        public override void PerformOperation()
        {
            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running " + ModuleName,
                        ModuleName, StepNumber);

                if (CheckParameters())
                    Model.PipelineCurrentlySuccessful = ANOVAFunction();

                RunChildModules();
            }
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
                    Model.LogWarning("Required Field Missing: " + s,
                        ModuleName, StepNumber);
                    b_Successful = false;
                    return b_Successful;
                }
            }

            if (b_Successful && !Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.InputTableName.ToString()]))
            {
                Model.LogError("Error in ANOVA function: " +
                    "the R environment does not contain the " +
                    "selected input table, '" +
                    Parameters[RequiredParameters.InputTableName.ToString()] +
                    "'.", ModuleName, StepNumber);
                b_Successful = false;
            }

            if (Parameters.ContainsKey("RemovePeptideColumn"))
                m_RemoveFirstColumn = Convert.ToBoolean(Parameters["RemovePeptideColumn"]);


            /// Get Random Effect if passed in
            if (Parameters.ContainsKey(
                AnovaParameters.Random_Effect.ToString()))
            {
                if (!string.IsNullOrEmpty(
                    Parameters[AnovaParameters.Random_Effect.ToString()]))
                    m_RandomEffect = Parameters[
                        AnovaParameters.Random_Effect.ToString()];
            }
            /// Get Interaction parameter
            if (Parameters.ContainsKey(
                AnovaParameters.Interaction.ToString()))
            {
                if (!string.IsNullOrEmpty(
                    Parameters[AnovaParameters.Interaction.ToString()]))
                    m_Interaction = Parameters[AnovaParameters.Interaction.ToString()];
            }
            /// Get Threshold parameter
            if (Parameters.ContainsKey(
                AnovaParameters.Threshold.ToString()))
            {
                if (!string.IsNullOrEmpty(
                    Parameters[AnovaParameters.Threshold.ToString()]))
                    m_Threshold = Parameters[AnovaParameters.Threshold.ToString()];
            }
            /// Get Unbalanced parameter
            if (Parameters.ContainsKey(
                AnovaParameters.Unbalanced.ToString()))
            {
                if (!string.IsNullOrEmpty(
                    Parameters[AnovaParameters.Unbalanced.ToString()]))
                    m_Unbalanced = Parameters[AnovaParameters.Unbalanced.ToString()];
            }

            /// Get REML parameter
            if (Parameters.ContainsKey(
                AnovaParameters.UseREML.ToString()))
            {
                if (!string.IsNullOrEmpty(
                    Parameters[AnovaParameters.UseREML.ToString()]))
                    m_UseREML = Parameters[AnovaParameters.UseREML.ToString()];
            }

            return b_Successful;
        }

        public bool ANOVAFunction()
        {
            bool b_Successful = true;

            switch (Parameters[RequiredParameters.Mode.ToString()].ToLower())
            {
                case "anova":
                    b_Successful = RunStandardAnova();
                    break;
                case "msstats":
                    b_Successful = RunMSstats();
                    break;
                case "itraq":
                    b_Successful = RunAnovaForiTRAQ();
                    break;
            }

            return b_Successful;
        }

        /// <summary>
        /// Runs Ashoka's ANOVA method
        /// </summary>
        /// <returns>True, if the ANOVA completes successfully</returns>
        private bool RunStandardAnova()
        {
            bool b_Successful = true;

            string Command = "",
                s_TmpInputTable = GetTemporaryTableName("tmpInputAnova_");           

            Command = string.Format(
                            "options(warn=-1)\n" +
                            "{9} <- {1}\n" +
                            "{0} <- performAnova(Data={9}, FixedEffects=\"{2}\", " +
                            "RandomEffects={3}, interact={4}, " +
                            "unbalanced={5}, useREML={6}, Factors=t({7}), " +
                            "thres={8})\n" +
                            "rm({9})\n",
                            Parameters[RequiredParameters.NewTableName.ToString()],
                            m_RemoveFirstColumn ?
                                Parameters[RequiredParameters.InputTableName.ToString()] + "[,-1]" :
                                Parameters[RequiredParameters.InputTableName.ToString()],
                            Parameters[RequiredParameters.Fixed_Effect.ToString()],
                            RandomEffect.ToUpper(),
                            Interaction.ToUpper(),
                            Unbalanced.ToUpper(),
                            UseREML.ToUpper(),
                            Parameters[RequiredParameters.FactorTable.ToString()],
                            Threshold.ToUpper(),
                            s_TmpInputTable);
            
            return b_Successful;
        }

        /// <summary>
        /// Performs MSstats from Olga's group
        /// </summary>
        /// <returns>True, if MSstats completes successfully</returns>
        private bool RunMSstats()
        {
            bool b_Successful = true;



            return b_Successful;
        }

        /// <summary>
        /// Performs ANOVA specific for iTRAQ samples
        /// </summary>
        /// <returns>True, if ANOVA completes successfully</returns>
        private bool RunAnovaForiTRAQ()
        {
            bool b_Successful = true;



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
