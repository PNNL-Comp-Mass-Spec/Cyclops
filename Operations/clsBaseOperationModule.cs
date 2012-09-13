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
using System.Data;
using System.Linq;
using System.Text;

using log4net;
using RDotNet;

namespace Cyclops.Operations
{
    /// <summary>
    /// Base Operation Module
    /// </summary>
    public abstract class clsBaseOperationModule : DataModules.clsBaseDataModule
    {
        #region Members

        #endregion

        #region Properties
        // instance of the model class
        public clsCyclopsModel Model
        {
            get;
            set;
        }
        #endregion

        #region Methods
        protected StepValueNode GetMaximumStepValueInOperationsTable(DataTable OperationsTable)
        {
            int minStepValue = int.MaxValue;
            int maxStepValue = int.MinValue;
            StepValueNode svn_Node = new StepValueNode();

            foreach (DataRow dr in OperationsTable.Rows)
            {
                string tmp = dr["Step"].ToString();
                int stepValue = !string.IsNullOrEmpty(tmp) ? Convert.ToInt16(tmp) : 1;
                minStepValue = Math.Min(minStepValue, stepValue);
                maxStepValue = Math.Max(maxStepValue, stepValue);
            }
            svn_Node.MinimumValue = minStepValue;
            svn_Node.MaximumValue = maxStepValue;
            return svn_Node;
        }

        /// <summary>
        /// Returns the parameters in a format that a Cyclops Operation can read
        /// </summary>
        /// <param name="Parameters">The array of DataRows that contain my parameters</param>
        /// <returns>The Dictionary of parameters for a Cyclops Operation</returns>
        protected Dictionary<string, dynamic> GetParameters(DataRow[] Parameters)
        {
            Dictionary<string, dynamic> d_Params = new Dictionary<string, dynamic>();

            foreach (KeyValuePair<string, string> kvp in Model.CyclopsParameters)
            {
                d_Params.Add(kvp.Key, kvp.Value.Trim());
            }

            // initialize the Parameters with 'workDir'
            //d_Params.Add("workDir", @"C:\DMS_WorkDir");

            foreach (DataRow dr in Parameters)
            {
                d_Params.Add(dr["Parameter"].ToString(), dr["Value"].ToString().Trim());
            }
            return d_Params;
        }
        #endregion
    }

    public sealed class StepValueNode
    {
        public StepValueNode()
        {
        }

        public int MinimumValue { get; set; }
        public int MaximumValue { get; set; }
    }
}
