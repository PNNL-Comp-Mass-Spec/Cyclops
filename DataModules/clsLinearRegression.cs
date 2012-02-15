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
    public class clsLinearRegression : clsBaseDataModule
    {
        #region Variables
        private string s_RInstance, s_Current_R_Statement = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Contructors
        /// <summary>
        /// Module to perform linear regression on datasets
        /// </summary>
        public clsLinearRegression()
        {
            ModuleName = "Linear Regression Module";
        }
        /// <summary>
        /// Module to perform linear regression on datasets
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsLinearRegression(string InstanceOfR)
        {
            ModuleName = "Linear Regression Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Module to perform linear regression on datasets
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsLinearRegression(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Linear Regression Module";
            Model = TheCyclopsModel;
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        public override void PerformOperation()
        {
            traceLog.Info("Transforming Datasets...");

            RegressData();

            RunChildModules();
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
                traceLog.Error("ERROR: Linear Regression class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Linear Regression class: 'inputTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        public void RegressData()
        {
            dsp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                REngine engine = REngine.GetInstanceFromID(s_RInstance);
                string s_RStatement = "";

                // TODO : Prepare the R statement

                try
                {
                    traceLog.Info("Linear Regression on Datasets: " + s_RStatement);


                    engine.EagerEvaluate(s_RStatement);

                }
                catch (Exception exc)
                {
                    traceLog.Error("Error Linear Regression on datasets: " + exc.ToString());
                    Model.SuccessRunningPipeline = false;
                }

            }
        }

        #endregion
    }
}
