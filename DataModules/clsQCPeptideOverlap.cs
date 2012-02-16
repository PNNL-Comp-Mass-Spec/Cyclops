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
    /// QC Peptide Overlap runs the ja_OverlapMatrix function in QualityControl.R to determine 
    /// overlap between fractions
    /// 
    /// Parameters include:
    /// - "newTableName"    - name of the new table to be generated.
    /// - "inputTableName"  - name of the table in R workspace
    /// </summary>
    public class clsQCPeptideOverlap : clsBaseDataModule
    {
        #region Variables
        private string s_RInstance, s_Current_R_Statement = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Contructors
        /// <summary>
        /// Module to count the number of unique peptide sequences within fractions
        /// </summary>
        public clsQCPeptideOverlap()
        {
            ModuleName = "QC Peptide Overlap Module";
        }
        /// <summary>
        /// Module to count the number of unique peptide sequences within fractions
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsQCPeptideOverlap(string InstanceOfR)
        {
            ModuleName = "QC Peptide Overlap Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Module to count the number of unique peptide sequences within fractions
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsQCPeptideOverlap(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "QC Peptide Overlap Module";
            Model = TheCyclopsModel;
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        public override void PerformOperation()
        {
            traceLog.Info("Performing QC Peptide Overlap on Datasets...");

            RunQC_on_Fractions();

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
                traceLog.Error("ERROR: QC Peptide Overlap class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: QC Peptide Overlap class: 'inputTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        private void RunQC_on_Fractions()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            dsp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                string s_RStatement = string.Format("{0} <- ja_OverlapMatrix(" +
                    "x={1}, y=unique({1}$Fraction))",
                    dsp.NewTableName,
                    dsp.InputTableName);

                try
                {
                    traceLog.Info("Executing QC Fraction Overlap Call in R: " +
                        s_RStatement);
                    engine.EagerEvaluate(s_RStatement);
                }
                catch (Exception exc)
                {
                    traceLog.Error("ERROR Executing QC Fraction Overlap Call in R: " +
                        exc.ToString());
                    Model.SuccessRunningPipeline = false;
                }
            }
        }
        #endregion
    }
}
