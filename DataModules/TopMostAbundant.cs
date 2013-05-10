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
    public class TopMostAbundant : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "TopMostAbundant",
            m_Function = "median";

        private bool m_RemoveNAs = true;
        /// <summary>
        /// Required parameters to run TopMostAbundant Module
        /// </summary>
        private enum RequiredParameters
        {
            InputTableName, NewTableName, NumberOfMostAbundant
        }
        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an TopMostAbundant Module
        /// </summary>
        public TopMostAbundant()
        {
            ModuleName = m_ModuleName;
        }

        /// <summary>
        /// TopMostAbundant module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public TopMostAbundant(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
        }

        /// <summary>
        /// TopMostAbundant module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public TopMostAbundant(CyclopsModel CyclopsModel,
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
        public override bool PerformOperation()
        {
            bool b_Successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running TopMostAbundant",
                        ModuleName, StepNumber);

                if (CheckParameters())
                    b_Successful = TopMostAbundantFunction();
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
            Dictionary<string, string> d_Parameters = new Dictionary<string, string>();

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                d_Parameters.Add(s, "");
            }

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
                    Model.LogError("Required Field Missing: " + s,
                        ModuleName, StepNumber);
                    b_Successful = false;
                    return b_Successful;
                }
            }

            if (b_Successful &&
                !Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.InputTableName.ToString()]))
            {
                Model.LogError("Error: R environment does not contain the " +
                    "input table: " +
                    Parameters[RequiredParameters.InputTableName.ToString()],
                    ModuleName, StepNumber);
                return false;
            }

            if (b_Successful &&
                Parameters.ContainsKey("Function"))
            {
                if (!string.IsNullOrEmpty(Parameters["Function"]))
                    m_Function = Parameters["Function"];
            }

            if (b_Successful &&
                Parameters.ContainsKey("RemoveNA"))
            {
                if (!string.IsNullOrEmpty(Parameters["RemoveNA"]))
                    m_RemoveNAs = Convert.ToBoolean(Parameters["RemoveNA"]);
            }

            return b_Successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool TopMostAbundantFunction()
        {
            bool b_Successful = true;

            string Command = string.Format(
                "{0} <- cbind({1}, Median=apply({1}, MARGIN=1, FUN={2}, na.rm={3}))\n" +
                    "{0} <- {0}[order({0}[,'Median'], decreasing=T),]\n" +
                    "{0} <- {0}[,-grep('Median', colnames({0}))]\n" +
                    "{0} <- {0}[1:{4},]\n",
                    Parameters[RequiredParameters.NewTableName.ToString()],
                    Parameters[RequiredParameters.InputTableName.ToString()],
                    m_Function,
                    m_RemoveNAs.ToString().ToUpper(),
                    Parameters[RequiredParameters.NumberOfMostAbundant.ToString()]);

            try
            {
                b_Successful = Model.RCalls.Run(Command,
                    ModuleName, StepNumber);
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered: " +
                    exc.ToString(),
                    ModuleName, StepNumber);
                SaveCurrentREnvironment();
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
