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

using RDotNet;
using log4net;

namespace Cyclops.DataModules
{
    public class clsNormalizingSpectralCounts : clsBaseDataModule
    {
        private string s_RInstance;
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        #region Constructors
        /// <summary>
        /// Module for normalizing spectral count results
        /// </summary>
        public clsNormalizingSpectralCounts()
        {
            ModuleName = "Normalizing Spectral Count Module";
        }
        /// <summary>
        /// Module for normalizing spectral count results
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsNormalizingSpectralCounts(string InstanceOfR)
        {
            ModuleName = "Normalizing Spectral Count Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Module for normalizing spectral count results
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsNormalizingSpectralCounts(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Normalizing Spectral Count Module";
            Model = TheCyclopsModel;            
            s_RInstance = InstanceOfR;
        }
        #endregion
        
        #region Properties

        #endregion

        #region Methods
        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            if (Model.SuccessRunningPipeline)
            {
                Model.IncrementStep(ModuleName);

                RunChildModules();
            }
        }

        /// <summary>
        /// Checks the dictionary to ensure all the necessary parameters are present
        /// </summary>
        /// <returns>True if all necessary parameters are present</returns>
        protected bool CheckPassedParameters()
        {
            bool b_2Pass = true;

            // NECESSARY PARAMETERS
            if (!dsp.HasNewTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Normalize Spectral Count class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Normalize Spectral Count class: 'inputTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        /// <summary>
        /// Normalizes the datasets
        /// </summary>
        public void NormalizeTheData()
        {
            dsp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                // Types of Spectral Count Normalizations:
                // Total Signal (type = 1)
                // Z-normalization (type = 2)
                // Natural Log Preprocessing (type = 3)
                // Hybrid (TS followed by Z) (type = 4)

                string s_RStatement = string.Format(
                    "{0} <- jnb_NormalizeSpectralCounts({1}, type={2})",
                    dsp.NewTableName,
                    dsp.InputTableName,
                    dsp.TableType);

                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "Normalizing Spectral Count Datasets",
                    Model.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;
            }
        }
        #endregion
    }
}
