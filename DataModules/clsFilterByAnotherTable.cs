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
    public class clsFilterByAnotherTable : clsBaseDataModule
    {
        private string s_RInstance, s_xLink="", s_yLink="", 
            s_NewTableName="", s_xTable="", s_yTable="";
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        #region Constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        public clsFilterByAnotherTable()
        {
            ModuleName = "Filter By Another Table Module";
        }
        /// <summary>
        /// Constructor that requires the instance of the R workspace
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        public clsFilterByAnotherTable(string InstanceOfR)
        {
            ModuleName = "Filter By Another Table Module";
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
            traceLog.Info("Filtering by another table...");

            if (CheckPassedParameters())
            {
                if (CheckTablesExist())
                {
                    FilterByAnotherTable();
                }
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
            if (Parameters.ContainsKey("xLink"))
                s_xLink = Parameters["xLink"];
            else
            {
                traceLog.Error("FilterByAnotherTable class: 'xLink' was not found in the passed parameters");
                return false;
            }
            if (Parameters.ContainsKey("yLink"))
                s_yLink = Parameters["yLink"];
            else
            {
                traceLog.Error("FilterByAnotherTable class: 'yLink' was not found in the passed parameters");
                return false;
            }
            if (Parameters.ContainsKey("newTableName"))
                s_NewTableName = Parameters["newTableName"];
            else
            {
                traceLog.Error("FilterByAnotherTable class: 'newTableName' was not found in the passed parameters");
                return false;
            }
            if (Parameters.ContainsKey("xTable"))
                s_xTable = Parameters["xTable"];
            else
            {
                traceLog.Error("FilterByAnotherTable class: 'xTable' was not found in the passed parameters");
                return false;
            }
            if (Parameters.ContainsKey("yTable"))
                s_yTable = Parameters["yTable"];
            else
            {
                traceLog.Error("FilterByAnotherTable class: 'yTable' was not found in the passed parameters");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Performs a check to make sure that the tables exist in the R workspace.
        /// </summary>
        /// <returns></returns>
        private bool CheckTablesExist()
        {
            if (clsGenericRCalls.ContainsObject(s_RInstance, s_xTable) &
                clsGenericRCalls.ContainsObject(s_RInstance, s_yTable))
            {
                return true;
            }
            else
            {
                traceLog.Error("ERROR FilterByAnotherTable class: one of the tables does not exist!");
                return false;
            }
        }

        private void FilterByAnotherTable()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            // Determine how to setup the link in the subset, either should be
            // "rownames" or the index of the column
            if (s_xLink.Equals("rownames") & s_yLink.Equals("rownames"))
            {
                string s_RStatement = string.Format(
                    "{0} <- {1}[which(rownames({1}) %in% rownames({2})),]",
                    s_NewTableName,
                    s_xTable,
                    s_yTable
                    );

                try
                {
                    traceLog.Info("Filtering Table: " + s_RStatement);
                    engine.EagerEvaluate(s_RStatement);
                }
                catch (Exception exc)
                {
                    traceLog.Error("ERROR Filtering Table: " + exc.ToString());
                }
            }
        }
        #endregion
    }
}
