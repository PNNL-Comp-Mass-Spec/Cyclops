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
    public class iTRAQMainOperation : BaseOperationModule
    {
        public enum iTraqTypes { Standard };

        /// <summary>
        /// Required parameters to run iTRAQ MainOperation Module
        /// </summary>
        private enum RequiredParameters
        {
            Type
        }


        private string m_iTraqTableName = "T_iTRAQ_PipelineOperation";
        private readonly string m_ModuleName = "iTRAQMainOperation";

        private Dictionary<iTraqTypes, string> m_iTraqTableNames;



        public iTRAQMainOperation()
        {
            ModuleName = m_ModuleName;
            Initialize();
        }

        public iTRAQMainOperation(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
            Initialize();
        }

        public iTRAQMainOperation(CyclopsModel CyclopsModel, Dictionary<string, string> OperationParameters)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
            Parameters = OperationParameters;
            Initialize();
        }


        private void Initialize()
        {
            m_iTraqTableNames = new Dictionary<iTraqTypes, string>
            {
                {iTraqTypes.Standard, "T_iTRAQ_PipelineOperation"}
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
                        iTRAQMainOperationFunction();
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

            if (Parameters.ContainsKey("DatabaseFileName"))
            {
                OperationsDatabasePath = Parameters["DatabaseFileName"];
            }

            return true;
        }

        /// <summary>
        /// Main Method to run the iTRAQ Operation
        /// </summary>
        /// <returns>True, if the operation completes successfully</returns>
        public bool iTRAQMainOperationFunction()
        {
            SetTypes();

            var successful = ConstructModules();

            return successful;
        }

        /// <summary>
        /// Sets the type of iTRAQ Operation, and sets the SQLite table
        /// to use to run the operation.
        /// </summary>
        public void SetTypes()
        {
            switch (Parameters[RequiredParameters.Type.ToString()].ToLower())
            {
                case "standard":
                    m_iTraqTableName =
                        m_iTraqTableNames[iTraqTypes.Standard];
                    break;
            }

            Model.LogMessage(string.Format(
                        "iTRAQ Operation: {0}\nDatabase: {1}\nTable: {2}\n",
                        Parameters[RequiredParameters.Type.ToString()],
                        OperationsDatabasePath,
                        m_iTraqTableName),
                        ModuleName,
                        StepNumber);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool ConstructModules()
        {
            bool successful;

            try
            {
                var wfh = new WorkflowHandler(Model)
                {
                    InputWorkflowFileName = OperationsDatabasePath,
                    WorkflowTableName = m_iTraqTableName
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
                    "for the iTRAQ Operation:\n" +
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
