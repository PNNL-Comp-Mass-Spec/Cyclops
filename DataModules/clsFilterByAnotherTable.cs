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

namespace Cyclops
{
    public class clsFilterByAnotherTable : clsBaseDataModule
    {
        private string s_RInstance;

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
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            string s_RStatement = "",
                xLink = Parameters["xLink"],
                yLink = Parameters["yLink"];
            

            // Determine how to setup the link in the subset, either should be
            // "rownames" or the index of the column
            if (xLink.Equals("rownames") & yLink.Equals("rownames"))
            {
                s_RStatement = string.Format(
                    "{0} <- {1}[which(rownames({1}) %in% rownames({2})),]",
                    Parameters["newTableName"],
                    Parameters["xTable"],
                    Parameters["yTable"]
                    );
            }

            engine.EagerEvaluate(s_RStatement);
            RunChildModules();
        }
        #endregion
    }
}
