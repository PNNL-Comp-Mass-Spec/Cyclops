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

namespace Cyclops.Operations
{
    /// <summary>
    /// Module that runs the Spectral Count Operation Workflow
    /// </summary>
    public class SpectralCountMainOperation : BaseOperationModule
    {
        #region Enums
        public enum ScoTypes { Standard, Iterator, Practice, ScoHtmlPractice };

        /// <summary>
        /// Required parameters to run SpectralCountMainOperation Module
        /// </summary>
        private enum RequiredParameters
        {
            Type
        }
        #endregion

        #region Members
        private string 
            m_SpectralCountTableName = "T_SpectralCountPipelineOperation",
            m_ModuleName = "SpectralCountMainOperation";
        
        private string[] m_SpectralCountTableNames = new string[] {
            "T_SpectralCountPipelineOperation",
            "T_SpectralCountIteratorPipelineOperation",
            "T_PracticeOperation",
            "T_Sco_HTML_Practice"
        };
        #endregion

        #region Properties
        
        #endregion

        #region Constructors
        public SpectralCountMainOperation()
        {
            ModuleName = m_ModuleName;
        }

        public SpectralCountMainOperation(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
        }

        public SpectralCountMainOperation(CyclopsModel CyclopsModel,
            Dictionary<string, string> OperationParameters)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
            Parameters = OperationParameters;
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
                    b_Successful =
                        SpectralCountMainOperationFunction();
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
                    Model.LogWarning("Required Field Missing: " + s,
                        ModuleName, StepNumber);
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
        /// Main Method to run the Spectral Count Operation
        /// </summary>
        /// <returns>True, if the operation completes successfully</returns>
        public bool SpectralCountMainOperationFunction()
        {
            bool b_Successful = true;

            SetTypes();

            b_Successful = ConstructModules();

            return b_Successful;
        }

        /// <summary>
        /// Sets the type of Spectral Count Operation, and sets the SQLite table
        /// to use to run the operation.
        /// </summary>
        public void SetTypes()
        {
            switch (Parameters[RequiredParameters.Type.ToString()].ToLower())
            {
                case "standard":
                    m_SpectralCountTableName =
                        m_SpectralCountTableNames[(int)ScoTypes.Standard];                    
                    break;
                case "iterator":
                    m_SpectralCountTableName =
                        m_SpectralCountTableNames[(int)ScoTypes.Iterator];
                    break;
                case "practice":
                    m_SpectralCountTableName =
                        m_SpectralCountTableNames[(int)ScoTypes.Practice];
                    break;
                case "scohtmlpractice":
                    m_SpectralCountTableName =
                        m_SpectralCountTableNames[(int)ScoTypes.ScoHtmlPractice];
                    break;
            }

            Model.LogMessage(string.Format(
                        "Spectral Count Operation: {0}\nDatabase: {1}\nTable: {2}\n",
                        Parameters[RequiredParameters.Type.ToString()],
                        OperationsDatabasePath,
                        m_SpectralCountTableName),
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
                wfh.WorkflowTableName = m_SpectralCountTableName;
                b_Successful = wfh.ReadSQLiteWorkflow();

                if (b_Successful)
                    Model.ModuleLoader = wfh;
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encounterd while running 'ConstructModules' " +
                    "for the Spectral Count Operation:\n" +
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
