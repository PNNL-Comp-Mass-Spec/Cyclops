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

namespace Cyclops.Operations
{
    /// <summary>
    /// Module that runs the Spectral Count Operation Workflow
    /// </summary>
    public class SpectralCountMainOperation : BaseOperationModule
    {
        // Ignore Spelling: Sco, Workflow

        public enum ScoTypes { Standard, Iterator, Practice, ScoHtmlPractice };

        /// <summary>
        /// Required parameters to run SpectralCountMainOperation Module
        /// </summary>
        private enum RequiredParameters
        {
            Type
        }

        private string m_SpectralCountTableName = "T_SpectralCountPipelineOperation";

        private const string m_ModuleName = "SpectralCountMainOperation";

        private Dictionary<ScoTypes, string> m_SpectralCountTableNames;

        public SpectralCountMainOperation()
        {
            ModuleName = m_ModuleName;
            Initialize();
        }

        public SpectralCountMainOperation(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
            Initialize();
        }

        public SpectralCountMainOperation(CyclopsModel CyclopsModel, Dictionary<string, string> OperationParameters)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
            Parameters = OperationParameters;
            Initialize();
        }

        private void Initialize()
        {
            m_SpectralCountTableNames = new Dictionary<ScoTypes, string>
            {
                {ScoTypes.Standard,        "T_SpectralCountPipelineOperation"},
                {ScoTypes.Iterator,        "T_SpectralCountIteratorPipelineOperation"},
                {ScoTypes.Practice,        "T_PracticeOperation"},
                {ScoTypes.ScoHtmlPractice, "T_Sco_HTML_Practice"}
            };
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

                Model.LogMessage("Running " + ModuleName, ModuleName, StepNumber);

                if (CheckParameters())
                {
                    successful =
                        SpectralCountMainOperationFunction();
                }
            }

            return successful;
        }

        /// <summary>
        /// Checks the parameters to ensure that all required keys are present
        /// </summary>
        /// <returns>True, if all required keys are included in the
        /// Parameters</returns>
        public override bool CheckParameters()
        {
            foreach (var s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogWarning("Required Field Missing: " + s, ModuleName, StepNumber);
                    return false;
                }
            }

            if (Parameters.TryGetValue("DatabaseFileName", out var parameter))
            {
                OperationsDatabasePath = parameter;
            }

            return true;
        }

        /// <summary>
        /// Main Method to run the Spectral Count Operation
        /// </summary>
        /// <returns>True, if the operation completes successfully</returns>
        public bool SpectralCountMainOperationFunction()
        {
            SetTypes();

            return ConstructModules();
        }

        /// <summary>
        /// Sets the type of Spectral Count Operation, and sets the SQLite table
        /// to use to run the operation.
        /// </summary>
        public void SetTypes()
        {
            switch (Parameters[nameof(RequiredParameters.Type)].ToLower())
            {
                case "standard":
                    m_SpectralCountTableName =
                        m_SpectralCountTableNames[ScoTypes.Standard];
                    break;
                case "iterator":
                    m_SpectralCountTableName =
                        m_SpectralCountTableNames[ScoTypes.Iterator];
                    break;
                case "practice":
                    m_SpectralCountTableName =
                        m_SpectralCountTableNames[ScoTypes.Practice];
                    break;
                case "scohtmlpractice":
                    m_SpectralCountTableName =
                        m_SpectralCountTableNames[ScoTypes.ScoHtmlPractice];
                    break;
            }

            Model.LogMessage(string.Format(
                        "Spectral Count Operation: {0}\nDatabase: {1}\nTable: {2}\n",
                        Parameters[nameof(RequiredParameters.Type)],
                        OperationsDatabasePath,
                        m_SpectralCountTableName),
                        ModuleName,
                        StepNumber);
        }

        /// <summary>
        /// Construct modules
        /// </summary>
        public bool ConstructModules()
        {
            bool successful;

            try
            {
                var wfh = new WorkflowHandler(Model)
                {
                    InputWorkflowFileName = OperationsDatabasePath,
                    WorkflowTableName = m_SpectralCountTableName
                };
                successful = wfh.ReadSQLiteWorkflow();

                if (successful)
                {
                    Model.ModuleLoader = wfh;
                }
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while running 'ConstructModules' " +
                    "for the Spectral Count Operation:\n" +
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
    }
}
