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
    public class ANOVA : BaseDataModule
    {
        #region Members
        private readonly string m_ModuleName = "ANOVA";
        private readonly string m_Description = "";
        private bool m_RemoveFirstColumn;

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
        public ANOVA(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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

                Model.LogMessage("Running " + ModuleName, ModuleName, StepNumber);

                if (CheckParameters())
                {
                    successful = ANOVAFunction();
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
            var paramDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var s in Enum.GetNames(typeof(RequiredParameters)))
            {
                paramDictionary.Add(s, "");
            }

            paramDictionary.Add("Random_Effect", RandomEffect);
            paramDictionary.Add("RowMetadataTable", "");
            paramDictionary.Add("Interaction", Interaction);
            paramDictionary.Add("Unbalanced", Unbalanced);
            paramDictionary.Add("Threshold", Threshold);
            paramDictionary.Add("UseREML", UseREML);

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
                    Model.LogWarning("Required Field Missing: " + s, ModuleName, StepNumber);
                    return false;
                }
            }

            if (!Model.RCalls.ContainsObject(Parameters[RequiredParameters.InputTableName.ToString()]))
            {
                Model.LogError("Error in ANOVA function: " +
                    "the R environment does not contain the " +
                    "selected input table, '" +
                    Parameters[RequiredParameters.InputTableName.ToString()] +
                    "'.", ModuleName, StepNumber);
                successful = false;
            }

            if (Parameters.ContainsKey("RemovePeptideColumn"))
            {
                m_RemoveFirstColumn = Convert.ToBoolean(Parameters["RemovePeptideColumn"]);
            }

            // Get Random Effect if passed in
            if (Parameters.ContainsKey(AnovaParameters.Random_Effect.ToString()) &&
                !string.IsNullOrEmpty(Parameters[AnovaParameters.Random_Effect.ToString()]))
            {
                RandomEffect = Parameters[
                    AnovaParameters.Random_Effect.ToString()];
            }

            // Get Interaction parameter
            if (Parameters.ContainsKey(AnovaParameters.Interaction.ToString()) &&
                !string.IsNullOrEmpty(Parameters[AnovaParameters.Interaction.ToString()]))
            {
                Interaction = Parameters[AnovaParameters.Interaction.ToString()];
            }

            // Get Threshold parameter
            if (Parameters.ContainsKey(AnovaParameters.Threshold.ToString()) &&
                !string.IsNullOrEmpty(Parameters[AnovaParameters.Threshold.ToString()]))
            {
                Threshold = Parameters[AnovaParameters.Threshold.ToString()];
            }

            // Get Unbalanced parameter
            if (Parameters.ContainsKey(AnovaParameters.Unbalanced.ToString()) &&
                !string.IsNullOrEmpty(Parameters[AnovaParameters.Unbalanced.ToString()]))
            {
                Unbalanced = Parameters[AnovaParameters.Unbalanced.ToString()];
            }

            // Get REML parameter
            if (Parameters.ContainsKey(AnovaParameters.UseREML.ToString()) &&
                !string.IsNullOrEmpty(Parameters[AnovaParameters.UseREML.ToString()]))
            {
                UseREML = Parameters[AnovaParameters.UseREML.ToString()];
            }

            return successful;
        }

        /// <summary>
        /// Runs ANOVA method developed by Ashoka
        /// </summary>
        /// <returns>True, if the ANOVA completes successfully</returns>
        public bool ANOVAFunction()
        {
            bool successful;

            var tmpInputTable = GetTemporaryTableName("tmpInputAnova_");

            var rCmd = string.Format(
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
                tmpInputTable);

            try
            {
                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while running ANOVA:\n" +
                    ex, ModuleName, StepNumber);
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
