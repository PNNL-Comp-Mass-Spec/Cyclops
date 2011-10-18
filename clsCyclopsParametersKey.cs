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


namespace Cyclops
{
    /// <summary>
    /// Static class that keeps track of all the parameters that are passed to Cyclops by the ATM.
    /// I created this class because I don't know what the parameter names are going to be that will 
    /// be passed, and this class provides a static centralized place to keep all these conversions.
    /// 
    /// The keys are what my code calls these parameters, and the values are the names used by ATM.
    /// Note: I need to update the values as soon as I find out what the real parameters are going to be.
    /// </summary>
    public static class clsCyclopsParametersKey
    {
        #region Methods
        /// <summary>
        /// Using a common name for a parameter, GetParameterName will convert that identifier to 
        /// the identifier that is passed to CyclopsModel by the ATM
        /// </summary>
        /// <param name="Key">My Parameter Identifier</param>
        /// <returns></returns>
        public static string GetParameterName(string Key)
        {
            Dictionary<string, string> d_CyclopsParametersKey = new Dictionary<string, string>();

            // Keys needed in the Dictionary of parameters passed to clsCyclopsModel:
            d_CyclopsParametersKey.Add("PipelineID", "Jobs"); // name of the pipe line job, also name of top directory and will be used to set the instance of the R session.
            d_CyclopsParametersKey.Add("RDLL", "RDLL"); // path to the directory containing the R.dll
            d_CyclopsParametersKey.Add("Workflow", "CyclopsWorkflowName");           // path to Cyclops XML workflow
            d_CyclopsParametersKey.Add("workDir", "workDir");                   // working directory
            d_CyclopsParametersKey.Add("InputFileName", "inputFileName");       // path to the input file
            d_CyclopsParametersKey.Add("OutputDirectory", "outputFilePath");    // directory to where Cyclops output will be directed
            d_CyclopsParametersKey.Add("ConsolidationFactor", "Consolidation_Factor");   // Factor to sum results across (e.g. SCX fractions in spectral count data)
            d_CyclopsParametersKey.Add("FixedEffect", "Fixed_Effect");       // main factor for comparison analysis
            d_CyclopsParametersKey.Add("BioRep", "bioRep");                 // factor designating biological replicates
            d_CyclopsParametersKey.Add("TechRep", "techRep");               // factor designating technical replicates

            string value = "";
            d_CyclopsParametersKey.TryGetValue(Key, out value);
            return value;
        }
        #endregion
    }
}
