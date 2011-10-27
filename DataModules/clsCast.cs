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
    /// Performs a cast call in R, similar to a table pivot
    /// </summary>
    public class clsCast : clsBaseDataModule
    {
        private string s_RInstance;

        #region Constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        public clsCast()
        {
            ModuleName = "Cast Module";
        }
        /// <summary>
        /// Constructor that requires the Name of the R instance
        /// </summary>
        /// <param name="InstanceOfR">Path to R DLL</param>
        public clsCast(string InstanceOfR)
        {
            ModuleName = "Cast Module";
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
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            // Construct the R statement
            string s_RStatement = "";
            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "reshape"))
                clsGenericRCalls.InstallPackage(s_RInstance, "reshape");

            s_RStatement = "require(reshape)\n";

            s_RStatement += string.Format("{0} <- cast({1}, {2}~{3}, {4})",
                Parameters["newTableName"],
                Parameters["tableName"],
                Parameters["id"],
                Parameters["variable"],
                Parameters["function"]);

            try
            {
                engine.EagerEvaluate(s_RStatement);
            }
            catch (Exception)
            {
                // TODO, evaluate the exception
            }

            RunChildModules();
        }
        #endregion
    }
}
