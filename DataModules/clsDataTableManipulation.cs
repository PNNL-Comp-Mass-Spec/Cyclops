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
using System.Data;
using System.Text;

using RDotNet;

namespace Cyclops
{
    /// <summary>
    /// Brings DataFrames and Matrices from R over to C# for further manipulation
    /// </summary>
    public class clsDataTableManipulation : clsBaseDataModule
    {
        private string s_RInstance;

        #region Constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        public clsDataTableManipulation()
        {
            ModuleName = "DataTable Manipulation Module";
        }
        /// <summary>
        /// Constructor that requires the instance of the R workspace
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        public clsDataTableManipulation(string InstanceOfR)
        {
            ModuleName = "DataTable Manipulation Module";
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
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            if (!Parameters.ContainsKey("table"))
            {
                // TODO: create an error message
                return;
            }

            DataTable dt = clsGenericRCalls.GetDataTable(s_RInstance, Parameters["table"]);

            Console.WriteLine(dt.Rows.Count + " rows!");
            Console.WriteLine(dt.Columns.Count + " columns!");

            string s = Parameters["table"];
            DataFrame df = engine.EagerEvaluate(s).AsDataFrame();


            RunChildModules();
        }
        #endregion
    }
}
