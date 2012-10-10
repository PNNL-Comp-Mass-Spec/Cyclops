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

using log4net;

namespace Cyclops.DataModules
{
    public class clsKBaseFormat : clsBaseDataModule
    {
        #region Members
        private string s_RInstance, s_Current_R_Statement = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Constructors
        /// <summary>
        /// Module generates KBase Format Table.
        /// </summary>
        public clsKBaseFormat()
        {
            ModuleName = "KBase Format Module";
        }
        /// <summary>
        /// Module generates KBase Format Table
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        public clsKBaseFormat(string InstanceOfR)
        {
            ModuleName = "KBase Format Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Module generates KBase Format Table
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        public clsKBaseFormat(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "KBase Format Module";
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

                Format4KBase();

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
                traceLog.Error("ERROR: KBase Format class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: KBase Format class: 'inputTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasXLink)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: KBase Format class: 'xLink': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasFactorTable)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: KBase Format class: 'factorTable': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasFactorColumn)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: KBase Format class: 'factorColumn': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            return b_2Pass;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Format4KBase()
        {
            dsp.GetParameters();

            if (CheckPassedParameters())
            {
                string s_RStatement = string.Format(
                    "{0} <- jnb_OutputForKBase(" +
                    "x={1}, factorTable={2}, " +
                    "factorMergeField={3}, " +
                    "factorColumn={4})",
                    dsp.NewTableName,
                    dsp.InputTableName,
                    dsp.FactorTable,
                    dsp.X_Link,
                    dsp.FactorColumn);

                Model.SuccessRunningPipeline = clsGenericRCalls.Run(
                    s_RStatement, s_RInstance,
                    "Constructing KBase Formatted Table",
                    Model.StepNumber, Model.NumberOfModules);
            }
        }
        #endregion
    }
}
