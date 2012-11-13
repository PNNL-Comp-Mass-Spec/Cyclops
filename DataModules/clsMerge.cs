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

        /// <summary>
        /// Module to merge two tables together
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsMerge(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Merge Module";
            Model = TheCyclopsModel;            
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
            if (Model.SuccessRunningPipeline)
            {
                Model.IncrementStep(ModuleName);

                dsp.GetParameters(ModuleName, Parameters);


                if (CheckPassedParameters())
                {
                    MergeTables();
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
                traceLog.Error("ERROR: Merge class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            if (!dsp.HasXTable)
            {
                traceLog.Error("ERROR: Merge class: 'xTable': \"" +
                    dsp.X_Table + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            else if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.X_Table))
            {
                traceLog.Error("ERROR: Merge class: 'xTable': \"" +
                    dsp.X_Table + "\", was not found in the R workspace.");
                b_2Pass = false;
            }
            else if (!dsp.HasXLink)
            {
                traceLog.Error("ERROR: Merge class: 'xLink': \"" +
                    dsp.X_Link + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            else if (!dsp.X_Link.Equals("row.names"))
            {
                if (!clsGenericRCalls.TableContainsColumn(s_RInstance,
                    dsp.X_Table, dsp.X_Link))
                {
                    traceLog.Error("ERROR: Merge class: 'xTable': \"" +
                        dsp.X_Table + "\" does not contain the 'xLink' column: \"" +
                        dsp.X_Link + "\"");
                    b_2Pass = false;
                }
            }

            if (!dsp.HasYTable)
            {
                traceLog.Error("ERROR: Merge class: 'yTable': \"" +
                    dsp.Y_Table + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            else if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.Y_Table))
            {
                traceLog.Error("ERROR: Merge class: 'yTable': \"" +
                    dsp.Y_Table + "\", was not found in the R workspace.");
                b_2Pass = false;
            }            
            else if (!dsp.HasYLink)
            {
                traceLog.Error("ERROR: Merge class: 'yLink': \"" +
                    dsp.Y_Link + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            else if (!dsp.Y_Link.Equals("row.names"))
            {
                if (!clsGenericRCalls.TableContainsColumn(s_RInstance,
                    dsp.Y_Table, dsp.Y_Link))
                {
                    traceLog.Error("ERROR: Merge class: 'yTable': \"" +
                        dsp.Y_Table + "\" does not contain the 'yLink' column: \"" +
                        dsp.Y_Link + "\"");
                    b_2Pass = false;
                }
            }

            if (!dsp.HasAllX)
            {
                traceLog.Error("ERROR: Merge class: 'allX': \"" +
                    dsp.AllX + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasAllY)
            {
                traceLog.Error("ERROR: Merge class: 'allY': \"" +
                    dsp.AllY + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        private void MergeTables()
        {
            // Construct the R statement
            string s_AllX = Parameters["allX"],
                s_AllY = Parameters["allY"];
            string s_RStatement = string.Format("{0} <- merge(x={1}," +
                "y={2}, by.x=\"{3}\", by.y=\"{4}\", all.x={5}, all.y={6})",
                Parameters["newTableName"],
                Parameters["xTable"],
                Parameters["yTable"],
                Parameters["xLink"],
                Parameters["yLink"],
                s_AllX.ToUpper(),
                s_AllY.ToUpper());

            if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                "Merging Tables",
                Model.StepNumber, Model.NumberOfModules))
                Model.SuccessRunningPipeline = false;
        }
        #endregion
    }
}
