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
using System.Linq;
using System.Text;

using RDotNet;
using log4net; 

namespace Cyclops.DataModules
{
    /// <summary>
    /// Parameters:
    /// inputTableName: table to perform linear regression on
    /// newTableName: new table name
    /// </summary>
    public class clsPValueAdjust : clsBaseDataModule
    {
        #region Members
        private string s_RInstance, s_Current_R_Statement = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Contructors
        /// <summary>
        /// Module to adjust p-values to correct to multiple comparisons
        /// </summary>
        public clsPValueAdjust()
        {
            ModuleName = "P-value adjustment Module";
        }
        /// <summary>
        /// Module to adjust p-values to correct to multiple comparisons
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsPValueAdjust(string InstanceOfR)
        {
            ModuleName = "P-value adjustment Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Module to adjust p-values to correct to multiple comparisons
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsPValueAdjust(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "P-value adjustment Module";
            Model = TheCyclopsModel;            
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        public override void PerformOperation()
        {
            if (Model.SuccessRunningPipeline)
            {
                Model.IncrementStep(ModuleName);

                AdjustPvalues();

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
                traceLog.Error("ERROR: P-value adjustment class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: P-value adjustment class: 'inputTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        private void AdjustPvalues()
        {
            dsp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                string s_RStatement = "";

                // TODO : make R statement to adjust p-values

                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "Adjusting P-values",
                    this.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;
            }
        }

        #endregion
    }
}
