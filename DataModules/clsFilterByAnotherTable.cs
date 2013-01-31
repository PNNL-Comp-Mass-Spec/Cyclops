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
    public class clsFilterByAnotherTable : clsBaseDataModule
    {
        #region Members
        private string s_RInstance;
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion 

        #region Constructors
        /// <summary>
        /// Filters a table based on the values within another table
        /// </summary>
        public clsFilterByAnotherTable()
        {
            ModuleName = "Filter By Another Table Module";
        }
        /// <summary>
        /// Filters a table based on the values within another table
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsFilterByAnotherTable(string InstanceOfR)
        {
            ModuleName = "Filter By Another Table Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Filters a table based on the values within another table
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsFilterByAnotherTable(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Filter By Another Table Module";
            Model = TheCyclopsModel;            
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
            if (Model.SuccessRunningPipeline)
            {
                Model.IncrementStep(ModuleName);

                dsp.GetParameters(ModuleName, Parameters);
                
                if (CheckPassedParameters())
                {
                    if (CheckTablesExist())
                    {
                        FilterByAnotherTable();
                    }
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
            if (!dsp.HasXLink)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("FilterByAnotherTable class: 'xLink': \"" +
                    dsp.X_Link + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasYLink)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("FilterByAnotherTable class: 'yLink': \"" +
                    dsp.Y_Link + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasNewTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("FilterByAnotherTable class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasXTable)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("FilterByAnotherTable class: 'xTable': \"" +
                    dsp.X_Table + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasYTable)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("FilterByAnotherTable class: 'yTable': \"" +
                    dsp.Y_Table + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        /// <summary>
        /// Performs a check to make sure that the tables exist in the R workspace.
        /// </summary>
        /// <returns></returns>
        private bool CheckTablesExist()
        {
            if (clsGenericRCalls.ContainsObject(s_RInstance, dsp.X_Table) &
                clsGenericRCalls.ContainsObject(s_RInstance, dsp.Y_Table))
            {
                return true;
            }
            else
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR FilterByAnotherTable class: one of the tables does not exist!");
                return false;
            }
        }

        private void FilterByAnotherTable()
        {
            // Determine how to setup the link in the subset, either should be
            // "rownames" or the index of the column
            if (dsp.X_Link.Equals("rownames") & dsp.Y_Link.Equals("rownames"))
            {
                string s_RStatement = string.Format(
                    "{0} <- {1}[which(rownames({1}) %in% rownames({2})),]",
                    dsp.NewTableName,
                    dsp.X_Table,
                    dsp.Y_Table
                    );

                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "Filtering Table",
                    this.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;
            }
            else if (dsp.X_Link.Equals("rownames"))
            {
                string s_RStatement = string.Format(
                    "{0} <- {1}[which(rownames({1}) %in% {2}${3}),]",
                    dsp.NewTableName,
                    dsp.X_Table,
                    dsp.Y_Table,
                    dsp.Y_Link
                    );

                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "Filtering Table",
                    this.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;

            }
            else if (dsp.Y_Link.Equals("rownames"))
            {
                string s_RStatement = string.Format(
                    "{0} <- {1}[which({1}${3} %in% rownames({2}))),]",
                    dsp.NewTableName,
                    dsp.X_Table,
                    dsp.Y_Table,
                    dsp.X_Link
                    );

                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "Filtering Table",
                    this.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;
            }
            else
            {
                string s_RStatement = string.Format(
                    "{0} <- {1}[which({1}${3} %in% {2}${4})),]",
                    dsp.NewTableName,
                    dsp.X_Table,
                    dsp.Y_Table,
                    dsp.X_Link,
                    dsp.Y_Link
                    );

                if (!clsGenericRCalls.Run(s_RStatement, s_RInstance,
                    "Filtering Table",
                    this.StepNumber, Model.NumberOfModules))
                    Model.SuccessRunningPipeline = false;
            }
        }
        #endregion
    }
}
