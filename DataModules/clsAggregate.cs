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

namespace Cyclops
{
    /// <summary>
    /// Aggregates Tables based on Columns, ColumnMetadata, Rows, or RowMetadata
    /// </summary>
    public class clsAggregate : clsBaseDataModule
    {
        private string s_RInstance;

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

        #region Functions
        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            //clsLogTools.WriteLog(clsLogTools.LoggerTypes.LogFile, clsLogTools.LogLevels.INFO,
            //    "Cyclops is performing an aggregation of the data.");

            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            string s_Factor = Parameters["factor"];
            string[] s_FactorsComplete = s_Factor.Split('$');
            GetOrganizedFactorsVector(s_RInstance, Parameters["dataTable"],
                s_FactorsComplete[0], s_FactorsComplete[1]);

            string s_RStatement = string.Format(
                "{0} <- jnb_Aggregate(x=data.matrix({1}), " +
                "myFactor={2}, MARGIN={3}, FUN={4})",
                Parameters["newTableName"],
                Parameters["dataTable"],
                Parameters["factor"],
                Parameters["margin"],
                Parameters["function"]);
            engine.EagerEvaluate(s_RStatement);

            RunChildModules();
        }
        #endregion
    }
}
