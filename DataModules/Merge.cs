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
    public class Merge : BaseDataModule
    {
        private readonly string m_ModuleName = "Merge";
        private readonly string m_Description = "";

        /// <summary>
        /// Required parameters to run Merge Module
        /// </summary>
        private enum RequiredParameters
        {
            NewTableName, XTable, YTable, XLink, YLink, AllX, AllY
        }

        /// <summary>
        /// Generic constructor creating an Merge Module
        /// </summary>
        public Merge()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// Merge module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public Merge(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// Merge module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public Merge(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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
            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running Merge", ModuleName, StepNumber);

                if (CheckParameters())
                {
                    Model.PipelineCurrentlySuccessful = MergeFunction();
                }
            }

            return true;
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

            if (!Model.RCalls.ContainsObject(Parameters[nameof(RequiredParameters.XTable)]))
            {
                Model.LogError("ERROR R environment does not contain the X table: " + Parameters[
                    nameof(RequiredParameters.XTable)]);
                successful = false;
            }
            if (!Model.RCalls.ContainsObject(Parameters[nameof(RequiredParameters.YTable)]))
            {
                Model.LogError("ERROR R environment does not contain the Y table: " + Parameters[
                    nameof(RequiredParameters.YTable)]);
                successful = false;
            }
            if (!Model.RCalls.TableContainsColumn(
                Parameters[nameof(RequiredParameters.XTable)],
                Parameters[nameof(RequiredParameters.XLink)]))
            {
                Model.LogError("ERROR The X table does not contain the column: " + Parameters[
                    nameof(RequiredParameters.XLink)]);
                successful = false;
            }
            if (!Model.RCalls.TableContainsColumn(
                Parameters[nameof(RequiredParameters.YTable)],
                Parameters[nameof(RequiredParameters.YLink)]))
            {
                Model.LogError("ERROR The Y table does not contain the column: " + Parameters[
                    nameof(RequiredParameters.YLink)]);
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool MergeFunction()
        {
            bool successful;

            // Construct the R statement
            var rCmd = string.Format("{0} <- merge(x={1}," +
                "y={2}, by.x=\"{3}\", by.y=\"{4}\", all.x={5}, all.y={6})",
                Parameters[nameof(RequiredParameters.NewTableName)],
                Parameters[nameof(RequiredParameters.XTable)],
                Parameters[nameof(RequiredParameters.YTable)],
                Parameters[nameof(RequiredParameters.XLink)],
                Parameters[nameof(RequiredParameters.YLink)],
                Parameters[nameof(RequiredParameters.AllX)].ToUpper(),
                Parameters[nameof(RequiredParameters.AllY)].ToUpper());

            try
            {
                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while performing merge:\n" + ex);
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
