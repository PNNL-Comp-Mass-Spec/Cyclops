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
    /// Performs a cast call in R, similar to a table pivot
    /// </summary>
    public class clsCast : clsBaseDataModule
    {
        private string s_RInstance;
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

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
            dsp.GetParameters(ModuleName, Parameters);

            traceLog.Info("Aggregating Datasets...");

            if (CheckPassedParameters())
            {
                CastDataset();
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
                traceLog.Error("ERROR: Cast class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                traceLog.Error("ERROR: Cast class: 'inputTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasFunction)
            {
                traceLog.Error("ERROR: Cast class: 'function': \"" +
                    dsp.Function + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        private void CastDataset()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            // Construct the R statement
            string s_RStatement = "";
            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "reshape"))
                clsGenericRCalls.InstallPackage(s_RInstance, "reshape");

            s_RStatement = "require(reshape)\n";

            s_RStatement += string.Format("{0} <- cast({1}, {2}~{3}, {4})",
                dsp.NewTableName,
                dsp.InputTableName,
                dsp.ID,
                dsp.Variable,
                dsp.Function);

            try
            {
                traceLog.Info("Casting dataset: " + s_RStatement);
                engine.EagerEvaluate(s_RStatement);
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR Casting dataset: " + exc.ToString());
            }
        }
        #endregion
    }
}
