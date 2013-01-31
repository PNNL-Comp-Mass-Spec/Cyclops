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
    class clsSummarizeData : clsBaseDataModule
    {
        #region Variables
        private string s_RInstance, s_Current_R_Statement = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Contructors
        /// <summary>
        /// Module to summarize the data (min, max, missingness, etc.)
        /// </summary>
        public clsSummarizeData()
        {
            ModuleName = "Data Summary Module";
        }
        /// <summary>
        /// Module to summarize the data (min, max, missingness, etc.)
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsSummarizeData(string InstanceOfR)
        {
            ModuleName = "Data Summary Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Module to summarize the data (min, max, missingness, etc.)
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsSummarizeData(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Data Summary Module";
            Model = TheCyclopsModel;            
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        /// <summary>
        /// Run Data Summary
        /// </summary>
        public override void PerformOperation()
        {
            if (Model.SuccessRunningPipeline)
            {
                Model.IncrementStep(ModuleName);

                SummarizeTheData();

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
                traceLog.Error("ERROR: Data Summarize class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Data Summarize class: 'inputTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            // if the table does not exist, do not throw an cyclops error, but do not run Data Summary
            if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.InputTableName))
            {
                //Model.SuccessRunningPipeline = false;
                traceLog.Info("ERROR Data Summarize class: '" + dsp.InputTableName +
                    "' was not found in the R workspace!");
                b_2Pass = false;
            }

            return b_2Pass;
        }
        
        /// <summary>
        /// Performs the Data Summary
        /// </summary>
        public void SummarizeTheData()
        {
            dsp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                s_Current_R_Statement = string.Format("{0} <- jnb_Summarize({1})",
                    dsp.NewTableName,
                    dsp.InputTableName);

                if (!clsGenericRCalls.Run(s_Current_R_Statement, s_RInstance,
                    "Summarizing the Data",
                    this.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;
            }
        }
        #endregion
    }
}
