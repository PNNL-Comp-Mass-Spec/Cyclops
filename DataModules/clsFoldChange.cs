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

namespace Cyclops
{
    public class clsFoldChange : clsBaseDataModule
    {
        private string s_RInstance, s_NewTableName="", 
            s_TableName="";
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        #region Constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        public clsFoldChange()
        {
            ModuleName = "Fold-Change Model Module";
        }
        /// <summary>
        /// Constructor that requires the instance of the R workspace
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        public clsFoldChange(string InstanceOfR)
        {
            ModuleName = "Fold-Change Model Module";
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Members

        #endregion

        #region Properties

        #endregion

        #region Methods
        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            bool b_Params = CheckPassedParameters();

            if (b_Params)
            {
                CalculateFoldChange();
            }
            
            RunChildModules();
        }

        /// <summary>
        /// Checks the dictionary to ensure all the necessary parameters are present
        /// </summary>
        /// <returns>True if all necessary parameters are present</returns>
        protected bool CheckPassedParameters()
        {
            // NECESSARY PARAMETERS
            if (Parameters.ContainsKey("newTableName"))
                s_NewTableName = Parameters["newTableName"];
            else
            {
                traceLog.Error("FoldChange class: 'newTableName' was not found in the passed parameters");
                return false;
            }
            if (Parameters.ContainsKey("tableName"))
                s_TableName = Parameters["tableName"];
            else
            {
                traceLog.Error("FoldChange class: 'tableName' was not found in the passed parameters");
                return false;
            }

            return true;
        }

        private void CalculateFoldChange()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            
            string s_RStatement = string.Format(
                "{0} <- jnb_FoldChangeSpectralCountAndPackage({1}, {2})",
                s_NewTableName,
                s_TableName,
                "0");   // column(s) indicating the p-values

            try
            {
                traceLog.Info("Calculating Fold Change: " + s_RStatement);
                engine.EagerEvaluate(s_RStatement);
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR calculating fold-change: " + exc.ToString());
            }
        }
        #endregion
    }
}
