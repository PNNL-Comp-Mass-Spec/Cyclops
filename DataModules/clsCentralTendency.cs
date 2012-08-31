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
using RDotNet;

namespace Cyclops.DataModules
{
    /// <summary>
    /// "CentralTendency"
    /// Parameters include:
    /// - "newTableName"    - name of the new table to be generated.
    /// - "inputTableName"  - name of the table in R workspace
    /// - "meanCenter"      - either "TRUE" or "FALSE" - FALSE makes it median.
    /// - "center"          - either "TRUE" or "FALSE" - TRUE centers the data at zero.
    /// </summary>

    public class clsCentralTendency : clsBaseDataModule
    {
        #region Members
        private string s_RInstance;
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Contructors
        /// <summary>
        /// Normalizes data by mean or median central tendency
        /// </summary>
        public clsCentralTendency()
        {
            ModuleName = "Central Tendency";
        }
        /// <summary>
        /// Normalizes data by mean or median central tendency
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsCentralTendency(string InstanceOfR)
        {
            ModuleName = "Central Tendency";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Normalizes data by mean or median central tendency
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsCentralTendency(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Central Tendency";
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

                NormalizeTheData();

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
                traceLog.Error("ERROR: Central Tendency class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Central Tendency class: 'inputTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        private void NormalizeTheData()
        {
            dsp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                string s_RStatement = "";

                s_RStatement = string.Format("{0} <- MeanCenter.Div(" +
                    "Data={1}, Mean={2}, centerZero={3})",
                    dsp.NewTableName,
                    dsp.InputTableName,
                    dsp.MeanCenter,
                    dsp.Center);

                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "Central Tendency Statement",
                    Model.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;
            }
        }
        #endregion
    }
}
