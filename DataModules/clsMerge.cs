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
    /// Merges two tables together in R, works like a SQL inner join
    /// Parameters required:
    /// newTableName            = Name of the new table to create
    /// firstTable              = Name of table that falls in x position
    /// secondTable             = Name of table that falls in y position
    /// firstTableLinkColumn    = Column of table x that links to table y
    /// secondTableLinkColumn   = Column of table y that links to table x
    /// allX                    = Boolean to include all data from table x (TRUE or FALSE)
    /// allY                    = Boolean to include all data from table y (TRUE or FALSE)
    /// </summary>
    public class clsMerge : clsBaseDataModule
    {
        private string s_RInstance;
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        #region Constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        public clsMerge()
        {
            ModuleName = "Merge Module";
        }
        /// <summary>
        /// Constructor that requires the Name of the R instance
        /// </summary>
        /// <param name="InstanceOfR">Path to R DLL</param>
        public clsMerge(string InstanceOfR)
        {
            ModuleName = "Merge Module";
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        /// <summary>
        ///  Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            traceLog.Info("Merging Datasets...");
            dsp.GetParameters(ModuleName, Parameters);


            if (CheckPassedParameters())
            {
                
            }

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
                traceLog.Error("ERROR: Aggregation class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasXTable)
            {
                traceLog.Error("ERROR: Aggregation class: 'xTable': \"" +
                    dsp.X_Table + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasYTable)
            {
                traceLog.Error("ERROR: Aggregation class: 'yTable': \"" +
                    dsp.Y_Table + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasXLink)
            {
                traceLog.Error("ERROR: Aggregation class: 'xLink': \"" +
                    dsp.X_Link + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasYLink)
            {
                traceLog.Error("ERROR: Aggregation class: 'yLink': \"" +
                    dsp.Y_Link + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasAllX)
            {
                traceLog.Error("ERROR: Aggregation class: 'allX': \"" +
                    dsp.AllX + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasAllY)
            {
                traceLog.Error("ERROR: Aggregation class: 'allY': \"" +
                    dsp.AllY + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        private void MergeTables()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            // Construct the R statement
            string s_RStatement = string.Format("{0} <- merge(x={1}," +
                "y={2}, by.x={3}, by.y={4}, all.x={5}, all.y={6})",
                Parameters["newTableName"],
                Parameters["firstTable"],
                Parameters["secondTable"],
                Parameters["firstTableLinkColumn"],
                Parameters["secondTableLinkColumn"],
                Parameters["allX"],
                Parameters["allY"]);

            try
            {
                engine.EagerEvaluate(s_RStatement);
            }
            catch (Exception)
            {
                // TODO, evaluate the exception
            }
        }
        #endregion
    }
}
