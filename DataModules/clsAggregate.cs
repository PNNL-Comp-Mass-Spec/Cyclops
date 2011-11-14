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

namespace Cyclops
{
    /// <summary>
    /// Aggregates Tables based on Columns, ColumnMetadata, Rows, or RowMetadata
    /// </summary>
    public class clsAggregate : clsBaseDataModule
    {
        private string s_RInstance, s_NewTableName="", s_DataTable="",
            s_Factor="", s_Margin="", s_Function="";
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        #region Constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        public clsAggregate()
        {
            ModuleName = "Aggregate Module";
        }
        /// <summary>
        /// Constructor that requires the instance of the R workspace
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        public clsAggregate(string InstanceOfR)
        {
            ModuleName = "Aggregate Module";
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
            traceLog.Info("Aggregating Datasets...");
            
            if (CheckPassedParameters())
            {               
                string[] s_FactorsComplete = s_Factor.Split('$');
                GetOrganizedFactorsVector(s_RInstance, s_DataTable,
                    s_FactorsComplete[0], s_FactorsComplete[1]);

                // check that the table exists
                if (clsGenericRCalls.ContainsObject(s_RInstance, s_FactorsComplete[0]))
                {
                    // check that the table has the expected column
                    if (clsGenericRCalls.TableContainsColumn(s_RInstance, s_FactorsComplete[0],
                        s_FactorsComplete[1]))
                    {
                        AggregateData();
                    }
                    else
                    {
                        traceLog.Error("ERROR: Aggregation class. The factors table does not contain " +
                            "the necessary column: " + s_FactorsComplete[1]);
                    }
                }
                else
                {
                    traceLog.Error("ERROR: Aggregation class. The factors table does not exist: " +
                        s_FactorsComplete[0]);
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
            if (Parameters.ContainsKey("newTableName"))
                s_NewTableName = Parameters["newTableName"];
            else
            {
                traceLog.Error("ERROR: Aggregation class: 'newTableName' was not found in the passed parameters");
                return false;
            }
            if (Parameters.ContainsKey("dataTable"))
                s_DataTable = Parameters["dataTable"];
            else
            {
                traceLog.Error("ERROR: Aggregation class: 'dataTable' was not found in the passed parameters");
                return false;
            }
            if (Parameters.ContainsKey("factor"))
                s_Factor = Parameters["factor"];
            else
            {
                traceLog.Error("ERROR: Aggregation class: 'factor' was not found in the passed parameters");
                return false;
            }
            if (Parameters.ContainsKey("margin"))
                s_Margin = Parameters["margin"];
            else
            {
                traceLog.Error("ERROR: Aggregation class: 'margin' was not found in the passed parameters");
                return false;
            }
            if (Parameters.ContainsKey("function"))
                s_Function = Parameters["function"];
            else
            {
                traceLog.Error("ERROR: Aggregation class: 'function' was not found in the passed parameters");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Performs the Aggregation
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        private void AggregateData()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            string s_RStatement = string.Format(
                    "{0} <- jnb_Aggregate(x=data.matrix({1}), " +
                    "myFactor={2}, MARGIN={3}, FUN={4})",
                    s_NewTableName,
                    s_DataTable,
                    s_Factor,
                    s_Margin,
                    s_Function);
            try
            {
                traceLog.Info("Aggregating datasets: " + s_RStatement);
                engine.EagerEvaluate(s_RStatement);
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR: Aggregating data: " + exc.ToString());
            }
        }
        #endregion
    }
}
