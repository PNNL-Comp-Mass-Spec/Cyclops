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

namespace Cyclops.DataModules
{
    public class ANOVA : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "ANOVA",
            m_Description = "";
        private bool m_RemoveFirstColumn = false;
        /// <summary>
        /// Required parameters to run ANOVA Module
        /// </summary>
        private enum RequiredParameters
        {
            NewTableName, InputTableName, Mode, FactorTable, Fixed_Effect
        }

        private enum MSstatRequiredParameters
        {
            RowMetadataTable, ColumnMetadataTable, RowMetadataProteinColumn,
            RowMetadataPeptideColumn, BioRep, NewProteinQuantTable
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
        public string RandomEffect { get; set; } = "Null";

        public string Interaction { get; set; } = "False";

        public string Unbalanced { get; set; } = "True";

        public string Threshold { get; set; } = "3";

        public string UseREML { get; set; } = "True";

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an ANOVA Module
        /// </summary>
        public ANOVA()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// ANOVA module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public ANOVA(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
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
            bool b_Successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running " + ModuleName,
                        ModuleName, StepNumber);

                if (CheckParameters())
                    b_Successful = ANOVAFunction();
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
            Dictionary<string, string> d_Parameters = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                d_Parameters.Add(s, "");
            }

            d_Parameters.Add("Random_Effect", RandomEffect);
            d_Parameters.Add("RowMetadataTable", "");
            d_Parameters.Add("Interaction", Interaction);
            d_Parameters.Add("Unbalanced", Unbalanced);
            d_Parameters.Add("Threshold", Threshold);
            d_Parameters.Add("UseREML", UseREML);

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
                    RandomEffect = Parameters[
                        AnovaParameters.Random_Effect.ToString()];
            }
            /// Get Interaction parameter
            if (Parameters.ContainsKey(
                AnovaParameters.Interaction.ToString()))
            {
                if (!string.IsNullOrEmpty(
                    Parameters[AnovaParameters.Interaction.ToString()]))
                    Interaction = Parameters[AnovaParameters.Interaction.ToString()];
            }
            /// Get Threshold parameter
            if (Parameters.ContainsKey(
                AnovaParameters.Threshold.ToString()))
            {
                if (!string.IsNullOrEmpty(
                    Parameters[AnovaParameters.Threshold.ToString()]))
                    Threshold = Parameters[AnovaParameters.Threshold.ToString()];
            }
            /// Get Unbalanced parameter
            if (Parameters.ContainsKey(
                AnovaParameters.Unbalanced.ToString()))
            {
                if (!string.IsNullOrEmpty(
                    Parameters[AnovaParameters.Unbalanced.ToString()]))
                    Unbalanced = Parameters[AnovaParameters.Unbalanced.ToString()];
            }

            /// Get REML parameter
            if (Parameters.ContainsKey(
                AnovaParameters.UseREML.ToString()))
            {
                if (!string.IsNullOrEmpty(
                    Parameters[AnovaParameters.UseREML.ToString()]))
                    UseREML = Parameters[AnovaParameters.UseREML.ToString()];
            }

            return b_Successful;
        }

        /// <summary>
        /// Runs Ashoka's ANOVA method
        /// </summary>
        /// <returns>True, if the ANOVA completes successfully</returns>
        public bool ANOVAFunction()
        {
            bool b_Successful = true;

            string Command = "",
                s_TmpInputTable = GetTemporaryTableName("tmpInputAnova_");

            Command = string.Format(
                            "options(warn=-1)\n" +
                            "{9} <- {1}\n" +
                            "{0} <- performAnova(Data={9}, FixedEffects='{2}', " +
                            "RandomEffects={3}, interact={4}, " +
                            "unbalanced={5}, useREML={6}, Factors=t({7}), " +
                            "thres={8})\n" +
                            "rm({9})\n\n",
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

            try
            {
                b_Successful = Model.RCalls.Run(Command,
                    ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while running ANOVA:\n" +
                    ex.ToString(), ModuleName, StepNumber);
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
