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
    public class RRollup : BaseDataModule
    {
        // Ignore Spelling: Rollup

        private readonly string m_ModuleName = "RRollup";
        private readonly string m_Description = "";
        private string m_MinimumPresence = "1";
        private string m_Mode = "median";
        private string m_ProteinInfo_ProteinColumn = "1";
        private readonly string m_ProteinInfo_PeptideColumn = "2";
        private string m_MinimumOverlap = "2";
        private string m_OneHitWonders = "FALSE";
        private string m_GrubbsPValue = "0.05";
        private string m_GminPCount = "5";
        private string m_Center = "FALSE";

        /// <summary>
        /// Required parameters to run RRollup Module
        /// </summary>
        private enum RequiredParameters
        {
            NewTableName, InputTableName, ProteinInfoTable
        }

        /// <summary>
        /// Generic constructor creating an RRollup Module
        /// </summary>
        public RRollup()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// RRollup module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public RRollup(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// RRollup module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public RRollup(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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

                Model.LogMessage("Running RRollup", ModuleName, StepNumber);

                if (CheckParameters())
                {
                    successful = RRollupFunction();
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
                    Model.LogWarning("Required Field Missing: " + s, ModuleName, StepNumber);
                    return false;
                }
            }

            if (!Model.RCalls.ContainsObject(Parameters[nameof(RequiredParameters.InputTableName)]))
            {
                Model.LogWarning("WARNING in RRollup: The R environment does not contain the input table, " +
                    Parameters[nameof(RequiredParameters.InputTableName)]);
                successful = false;
            }

            if (Parameters.TryGetValue("MinPresence", out var minPresence))
            {
                m_MinimumPresence = minPresence;
            }

            if (Parameters.TryGetValue("ProteinInfo_ProteinCol", out var proteinColumn))
            {
                m_ProteinInfo_ProteinColumn = proteinColumn;
            }

            if (Parameters.TryGetValue("Mode", out var mode))
            {
                m_Mode = mode;
            }

            if (Parameters.TryGetValue("MinOverlap", out var minOverlap))
            {
                m_MinimumOverlap = minOverlap;
            }

            if (Parameters.TryGetValue("OneHitWonders", out var oneHitWonders))
            {
                m_OneHitWonders = oneHitWonders;
            }

            if (Parameters.TryGetValue("GpValue", out var gpValue))
            {
                m_GrubbsPValue = gpValue;
            }

            // ReSharper disable once StringLiteralTypo
            if (Parameters.TryGetValue("GminPCount", out var gMinPCount))
            {
                m_GminPCount = gMinPCount;
            }

            if (Parameters.TryGetValue("Center", out var center))
            {
                m_Center = center;
            }

            return successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool RRollupFunction()
        {
            bool successful;

            var rCmd = string.Format("{0} <- RRollup.proteins(" +
                    "Data={1}, ProtInfo={2}, minPresence={3}, Mode=\"{4}\", " +
                    "protInfo_ProtCol={5}, protInfo_PepCol={6}, minOverlap={7}, " +
                    "oneHitWonders={8}, gpvalue={9}, gminPCount={10}, center={11})",
                    Parameters[nameof(RequiredParameters.NewTableName)],     // 0
                    Parameters[nameof(RequiredParameters.InputTableName)],   // 1
                    Parameters[nameof(RequiredParameters.ProteinInfoTable)], // 2
                    m_MinimumPresence,                                          // 3
                    m_Mode,                                                     // 4
                    m_ProteinInfo_ProteinColumn,                                // 5
                    m_ProteinInfo_PeptideColumn,                                // 6
                    m_MinimumOverlap,                                           // 7
                    m_OneHitWonders,                                            // 8
                    m_GrubbsPValue,                                             // 9
                    m_GminPCount,                                               // 10
                    m_Center);                                                  // 11

            try
            {
                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while running " +
                    "'RRollupFunction': " + ex, ModuleName,
                    StepNumber);
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
