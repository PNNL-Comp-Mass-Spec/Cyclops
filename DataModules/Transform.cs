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
    public class Transform : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "Transform";
        /// <summary>
        /// Required parameters to run Transform Module
        /// </summary>
        private enum RequiredParameters
        { InputTableName, NewTableName
        }
        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an Transform Module
        /// </summary>
        public Transform()
        {
            ModuleName = m_ModuleName;
        }

        /// <summary>
        /// Transform module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public Transform(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
        }

        /// <summary>
        /// Transform module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public Transform(CyclopsModel CyclopsModel,
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
                    Model.PipelineCurrentlySuccessful = TransformFunction();

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

            return b_Successful;
        }

        /// <summary>
        /// Performs the transformation
        /// </summary>
        /// <returns>True, if the transformation completes successfully</returns>
        public bool TransformFunction()
        {
            bool b_Successful = true;

            string Command = "";

            if (Parameters.ContainsKey("logBase"))
            {
                Command = string.Format(
                    "{0} <- log((data.matrix({1})+{2})*{3},{4})",
                        Parameters[RequiredParameters.NewTableName.ToString()],
                        Parameters[RequiredParameters.InputTableName.ToString()],
                        Parameters.ContainsKey("add") ? Parameters["add"] : "0",
                        Parameters.ContainsKey("scale") ? Parameters["scale"] : "1",
                        Parameters["logBase"]);
            }
            else
            {
                Command = string.Format(
                    "{0} <- ({1}+{2})*{3}",
                        Parameters[RequiredParameters.NewTableName.ToString()],
                        Parameters[RequiredParameters.InputTableName.ToString()],
                        Parameters.ContainsKey("add") ? Parameters["add"] : "0",
                        Parameters.ContainsKey("scale") ? Parameters["scale"] : "1");
            }

            try
            {
                b_Successful = Model.RCalls.Run(Command, ModuleName, StepNumber);
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while performing transformation:\n" +
                    exc.ToString());
                SaveCurrentREnvironment();
                b_Successful = false;
            }

            return b_Successful;
        }

        protected override string GetDefaultValue()
        {
            return "false";
        }

        protected override string GetTypeName()
        {
            return ModuleName;
        }
        #endregion
    }
}
