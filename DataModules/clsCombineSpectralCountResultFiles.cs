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
    /// <summary>
    /// This is a specialized class specifically for combining tables 
    /// following a spectral counting analysis
    /// </summary>
    public class clsCombineSpectralCountResultFiles : clsBaseDataModule
    {
        private string s_RInstance;
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        #region Constructors
        /// <summary>
        /// Special module for combining tables from a spectral count analysis
        /// </summary>
        public clsCombineSpectralCountResultFiles()
        {
            ModuleName = "Combine Spectral Count Result Tables Module";
        }
        /// <summary>
        /// Special module for combining tables from a spectral count analysis
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsCombineSpectralCountResultFiles(string InstanceOfR)
        {
            ModuleName = "Combine Spectral Count Result Tables Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Special module for combining tables from a spectral count analysis
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsCombineSpectralCountResultFiles(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Combine Spectral Count Result Tables Module";
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

                dsp.GetParameters(ModuleName, Parameters);

                if (CheckPassedParameters())
                {
                    CombineSpectralCounts();
                }

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
                traceLog.Error("ERROR: Aggregation class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasPvalueTable)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Aggregation class: 'pValueTable': \"" +
                    dsp.P_ValueTable + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasFoldChangeTable)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Aggregation class: 'foldChangeTable': \"" +
                    dsp.FoldChangeTable + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        private void CombineSpectralCounts()
        {
            string s_RStatement = "";

            if (!dsp.HasNormalizedTable)
            {
                s_RStatement = string.Format(
                    "{0} <- cbind(\"pValue\"={1}[,1], {2}, {1}[,2:ncol({1})])",
                    dsp.NewTableName,
                    dsp.HasPvalueTable,
                    dsp.FoldChangeTable);
            }
            else
            {
                s_RStatement = string.Format(
                    "{0} <- cbind(\"pValue\"={1}[,1], {2}, {3}, {1}[,2:ncol({1})])",
                    dsp.NewTableName,
                    dsp.HasPvalueTable,
                    dsp.FoldChangeTable,
                    dsp.NormalizedTable);
            }

            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "COMBINING SPECTRAL COUNT TABLES",
                Model.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;
        }

        #endregion
    }
}
