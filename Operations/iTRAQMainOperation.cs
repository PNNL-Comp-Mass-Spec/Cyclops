/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: http://omics.pnl.gov/software
 * -----------------------------------------------------
 *
 * Licensed under the Apache License, Version 2.0; you may not use this
 * file except in compliance with the License.  You may obtain a copy of the
 * License at https://opensource.org/licenses/Apache-2.0
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace Cyclops.Operations
{
    public class iTRAQMainOperation : BaseOperationModule
    {
        #region Enums
        public enum iTraqTypes { Standard };

        /// <summary>
        /// Required parameters to run iTRAQ MainOperation Module
        /// </summary>
        private enum RequiredParameters
        {
            Type
        }
        
        #endregion

        #region Members
        private string m_iTraqTableName = "T_iTRAQ_PipelineOperation";
        private string m_ModuleName = "iTRAQMainOperation";

        private Dictionary<iTraqTypes, string> m_iTraqTableNames;
        #endregion

        #region Properties

        #endregion

        #region Constructors
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
        #endregion

        #region Methods

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
            bool b_Successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running " + ModuleName, ModuleName, StepNumber);

                if (CheckParameters())
                    b_Successful =
                        iTRAQMainOperationFunction();
            }

            return b_Successful;
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
                    Model.LogWarning("Required Field Missing: " + s, ModuleName, StepNumber);
                    b_Successful = false;
                    return b_Successful;
                }
            }

            if (Parameters.ContainsKey("DatabaseFileName"))
            {
                OperationsDatabasePath = Parameters["DatabaseFileName"];
            }

            return b_Successful;
        }

        /// <summary>
        /// Main Method to run the iTRAQ Operation
        /// </summary>
        /// <returns>True, if the operation completes successfully</returns>
        public bool iTRAQMainOperationFunction()
        {
            bool b_Successful = true;

            SetTypes();

            b_Successful = ConstructModules();

            return b_Successful;
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
            bool b_Successful = true;

            try
            {
                WorkflowHandler wfh = new WorkflowHandler(Model);
                wfh.InputWorkflowFileName = OperationsDatabasePath;
                wfh.WorkflowTableName = m_iTraqTableName;
                b_Successful = wfh.ReadSQLiteWorkflow();

                if (b_Successful)
                    Model.ModuleLoader = wfh;
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encounterd while running 'ConstructModules' " +
                    "for the iTRAQ Operation:\n" +
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
        #endregion
    }
}
