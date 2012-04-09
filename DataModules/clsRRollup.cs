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
    public class clsRRollup : clsBaseDataModule
    {
        #region Variables
        private string s_RInstance, s_Current_R_Statement = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Contructors
        /// <summary>
        /// Module to roll peptides up to proteins using the RRollup algorithm
        /// </summary>
        public clsRRollup()
        {
            ModuleName = "RRollup Module";
        }
        /// <summary>
        /// Module to roll peptides up to proteins using the RRollup algorithm
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsRRollup(string InstanceOfR)
        {
            ModuleName = "RRollup Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Module to roll peptides up to proteins using the RRollup algorithm
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsRRollup(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "RRollup Module";
            Model = TheCyclopsModel;
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        /// <summary>
        /// Run RRollup on the data
        /// </summary>
        public override void PerformOperation()
        {
            traceLog.Info("Performing RRollup...");

            RRollupThePeptides();

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
                traceLog.Error("ERROR: RRollup class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: RRollup class: 'inputTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasProteinInformationTable)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: RRollup class: 'proteinInfoTable' was not " +
                    "found in the passed parameters");
                b_2Pass = false;
            }

            // if the table does not exist, do not throw an cyclops error, but do not run RRollup
            // this is because Linear Regression only runs if there is a consolidation factor
            // if the linear regression table does not exist, then don't run RRollup
            if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.InputTableName))
            {
                //Model.SuccessRunningPipeline = false;
                traceLog.Info("ERROR RRollup class: '" + dsp.InputTableName +
                    "' was not found in the R workspace!");
                b_2Pass = false;
            }

            return b_2Pass;
        }
        
        /// <summary>
        /// Performs the RRollup
        /// </summary>
        public void RRollupThePeptides()
        {
            dsp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                REngine engine = REngine.GetInstanceFromID(s_RInstance);

                // check to see if the outliers package is installed
                if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "outliers"))
                {
                    traceLog.Info("RRollup depends on the outliers package, which is not installed " +
                        "in the R environment.\n" +
                        "RRollup class is now installing the outliers package...");
                    try
                    {
                        // if not install the package
                        clsGenericRCalls.InstallPackage(s_RInstance, "outliers");
                        traceLog.Info("Outliers package was successfully installed in R!");
                    }
                    catch (Exception exc)
                    {
                        traceLog.Error("ERROR RRollup class, outliers package was not successfully " +
                            "installed in the R environment, error message below...\n" +
                            exc.ToString());
                    }
                }
                else
                {
                    traceLog.Info("RRollup contains the 'outliers' package!");
                }

                s_Current_R_Statement += string.Format("{0} <- RRollup.proteins(" +
                    "Data={1}, ProtInfo={2}, minPresence={3}, Mode=\"{4}\", " +
                    "protInfo_ProtCol={5}, protInfo_PepCol={6}, minOverlap={7}, " +
                    "oneHitWonders={8}, gpvalue={9}, gminPCount={10}, center={11})",
                    dsp.NewTableName,               // 0
                    dsp.InputTableName,             // 1
                    dsp.ProteinInformationTable,    // 2
                    dsp.MinimumPresence,            // 3
                    dsp.Mode,                       // 4
                    dsp.ProteinInfo_ProteinColumn,  // 5
                    dsp.ProteinInfo_PeptideColumn,  // 6
                    dsp.MinimumOverlap,             // 7
                    dsp.OneHitWonders,              // 8
                    dsp.GpValue,                    // 9
                    dsp.GminPCount,                 // 10
                    dsp.Center);                    // 11

                try
                {
                    traceLog.Info("RRollup class, performing an RRollup on " + dsp.InputTableName);
                    traceLog.Info(s_Current_R_Statement);
                    engine.EagerEvaluate(s_Current_R_Statement);
                }
                catch (Exception exc)
                {
                    Model.SuccessRunningPipeline = false;
                }
            }
        }
        #endregion
    }
}
