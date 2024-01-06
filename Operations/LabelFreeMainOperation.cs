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
    public class LabelFreeMainOperation : BaseOperationModule
    {
        #region Enums
        /// <summary>
        /// Log2: Simple Log2 transformation, RRollup
        /// Log2LR: Log2 transformation, Linear Regression, RRollup
        /// Log2CT: Log2 transformation, Central Tendency, RRollup
        /// </summary>
        public enum LbfTypes { Log2, Log2LR, Log2CT, Log2All, AnovaPractice, MainAnovaPractice, HtmlPractice };

        /// <summary>
        /// Required parameters to run SpectralCountMainOperation Module
        /// </summary>
        private enum RequiredParameters
        {
            Type
        }

        #endregion

        #region Members

        private string m_LabelFreeTableName = "T_LabelFreeLog2PipelineOperation";

        private const string m_ModuleName = "LabelFreeMainOperation";

        private Dictionary<LbfTypes, string> m_LabelFreeTableNames;

        #endregion

        #region Properties

        #endregion

        #region Constructors
        public LabelFreeMainOperation()
        {
            ModuleName = m_ModuleName;
            Initialize();
        }

        public LabelFreeMainOperation(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
            Initialize();
        }

        public LabelFreeMainOperation(CyclopsModel CyclopsModel, Dictionary<string, string> OperationParameters)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
            Parameters = OperationParameters;
            Initialize();
        }
        #endregion

        #region Methods

        private void Initialize()
        {
            m_LabelFreeTableNames = new Dictionary<LbfTypes, string>
            {
                {LbfTypes.Log2,              "T_LabelFreeLog2PipelineOperation"},
                {LbfTypes.Log2LR,            "T_LabelFreeLog2_LR_PipelineOperation"},
                {LbfTypes.Log2CT,            "T_LabelFreeLog2_CT_PipelineOperation"},
                {LbfTypes.Log2All,           "T_LabelFreeLog2_All_PipelineOperation"},
                {LbfTypes.AnovaPractice,     "T_LabelFree_AnovaPractice"},
                {LbfTypes.MainAnovaPractice, "T_LabelFree_MainAnovaPractice"},
                {LbfTypes.HtmlPractice,      "T_LabelFree_HtmlPractice"}
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
                        LabelFreeMainOperationFunction();
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
        public bool LabelFreeMainOperationFunction()
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
                case "log2":
                    m_LabelFreeTableName =
                        m_LabelFreeTableNames[LbfTypes.Log2];
                    break;
                case "log2lr":
                    m_LabelFreeTableName =
                        m_LabelFreeTableNames[LbfTypes.Log2LR];
                    break;
                case "log2ct":
                    m_LabelFreeTableName =
                        m_LabelFreeTableNames[LbfTypes.Log2CT];
                    break;
                case "log2all":
                    m_LabelFreeTableName =
                        m_LabelFreeTableNames[LbfTypes.Log2All];
                    break;
                case "htmlpractice":
                    m_LabelFreeTableName =
                        m_LabelFreeTableNames[LbfTypes.HtmlPractice];
                    break;
                case "anovapractice":
                    m_LabelFreeTableName =
                        m_LabelFreeTableNames[LbfTypes.AnovaPractice];
                    break;
                case "mainanovapractice":
                    m_LabelFreeTableName =
                        m_LabelFreeTableNames[LbfTypes.MainAnovaPractice];
                    break;
            }
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
                    WorkflowTableName = m_LabelFreeTableName
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
                    "for the LabelFree Operation:\n" + ex, ModuleName, StepNumber);
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
        #endregion
    }
}
