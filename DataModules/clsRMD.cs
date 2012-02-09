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
    /// Runs the RMD_RUNS function in R that will log10 transform peptide peak intensity, that is,
    /// peptide abundance data and determine if any LC-MS analyses (ie, runs) in a N x P peptide data
    /// set are statistical outliers. The statistical analysis is based on summarizing each LC-MS run 
    /// as a vector of q=5 summary statistics which describe the peptide abundance distribution for 
    /// a specific run; a N x q matrix is then analyzed using robust PCA to compute a robust estimate 
    /// of the covariance matrix used in the calculation of a robust Mahalanobis distance.
    /// 
    /// Publication:
    /// Improved quality control processing of peptide-centric LC-MS proteomics data.
    /// Matzke MM, Waters KM, Metz TO, Jacobs JM, Sims AC, Baric RS, Pounds JG, Webb-Robertson BJ.
    /// Bioinformatics. 2011 Oct 15;27(20):2866-72. Epub 2011 Aug 18.
    /// PMID: 21852304
    /// </summary>
    public class clsRMD : clsBaseDataModule
    {
        private string s_RInstance;
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        #region Constructors
        /// <summary>
        /// Performs Bobbie-Jo's RMD analysis to identify statistical outlier datasets
        /// </summary>
        public clsRMD()
        {
            ModuleName = "RMD Module";
        }
        /// <summary>
        /// Performs Bobbie-Jo's RMD analysis to identify statistical outlier datasets
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsRMD(string InstanceOfR)
        {
            ModuleName = "RMD Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Performs Bobbie-Jo's RMD analysis to identify statistical outlier datasets
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsRMD(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "RMD Module";
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
            dsp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                RunRMD();                
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
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Aggregation class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        private void InstallLibraries()
        {
            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "moment"))
                clsGenericRCalls.InstallPackage(s_RInstance, "moment");
            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "fields"))
                clsGenericRCalls.InstallPackage(s_RInstance, "fields");
            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "geoR"))
                clsGenericRCalls.InstallPackage(s_RInstance, "geoR");
            if (!clsGenericRCalls.IsPackageInstalled(s_RInstance, "pcaPP"))
                clsGenericRCalls.InstallPackage(s_RInstance, "pcaPP");
        }

        private void RunRMD()
        {
            InstallLibraries();

            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            // Construct the R statement
            // load in the libraries
            string s_RStatement = "require(moments)\n" +
                "require(fields)\n" +
                "require(geoR)\n" +
                "require(pcaPP)\n";

            s_RStatement += string.Format(
                "{0} <- DetectOutliers({1}, {2}, {3}, {4}, {5})",
                Parameters["newListName"], // returns a list of objects with datasets to keep
                Parameters["sampleMatrix"], // your dataset as a matrix
                Parameters["factorsList"], // the vector of factors (BioRep, Condition, etc.)
                Parameters["techRep"], // the vector of technical replicates
                Parameters["pValueThreshold"]); // p-value cutoff, default is 0.0001

            try
            {
                traceLog.Info("RMD MODULE: " + s_RStatement);
                engine.EagerEvaluate(s_RStatement);
            }
            catch (Exception exc)
            {
                // TODO handle problems with execution
                Model.SuccessRunningPipeline = false;
                traceLog.Error("RMD MODULE ERROR: " + exc.ToString());
            }
        }
        #endregion
    }
}
