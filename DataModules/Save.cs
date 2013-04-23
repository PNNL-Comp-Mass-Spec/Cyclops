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
    public class Save : BaseDataModule
    {
        #region Members
        /// <summary>
        /// Required parameters to run MissedCleavageSummary Module
        /// </summary>
        private enum RequiredParameters
        {
        }

        private string m_ModuleName = "Save";
        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an SaveEnvironment Module
        /// </summary>
        public Save()
        {
            ModuleName = m_ModuleName;
        }

        /// <summary>
        /// SaveEnvironment module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public Save(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
        }

        /// <summary>
        /// SaveEnvironment module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public Save(CyclopsModel CyclopsModel,
            Dictionary<string, string> ExportParameters)
        {
            ModuleName = "Save Environment";
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
                    Model.PipelineCurrentlySuccessful = SaveFunction();

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

        protected override string GetDefaultValue()
        {
            return "false";
        }

        protected override string GetTypeName()
        {
            return ModuleName;
        }

        /// <summary>
        /// Saves the R environment
        /// </summary>
        /// <returns>True, if the R environment is saved successfully</returns>
        public bool SaveFunction()
        {
            bool b_Successful = true;
            string s_DefaultOutputFileName = "Results.RData", s_FileName;

            try
            {
                if (!string.IsNullOrEmpty(Model.WorkDirectory) &&
                    Parameters.ContainsKey("fileName"))
                    s_FileName = Model.WorkDirectory + "/" + Parameters["fileName"];
                else if (!string.IsNullOrEmpty(Model.WorkDirectory))
                    s_FileName = Model.WorkDirectory + "/" + s_DefaultOutputFileName;
                else
                    s_FileName = s_DefaultOutputFileName;

                s_FileName = s_FileName.Replace("\\", "/");

				Model.LogMessage("Saving R environment to: " + s_FileName,
                    ModuleName, StepNumber);

				b_Successful = Model.RCalls.SaveEnvironment(s_FileName);

                if (b_Successful)
                    Model.RWorkEnvironment = s_FileName;
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while Saving R Environment: " +
                    exc.ToString());
                b_Successful = false;
            }

            return b_Successful;
        }
        #endregion
    }
}
