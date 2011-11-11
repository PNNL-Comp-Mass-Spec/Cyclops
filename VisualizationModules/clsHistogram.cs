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
using System.IO;

using RDotNet;

namespace Cyclops
{
    /// <summary>
    /// Constructs histogram plots and saves the image file
    /// </summary>
    public class clsHistogram : clsBaseVisualizationModule
    {
        protected string s_RInstance;
        //protected Dictionary<string, string> d_PlotParameters = new Dictionary<string,string>();

        #region Constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        public clsHistogram()
        {
            ModuleName = "Histogram Module";
        }
        /// <summary>
        /// Basic constructor that passes in the R workspace
        /// </summary>
        /// <param name="InstanceOfR">Instance of R Workspace</param>
        public clsHistogram(string InstanceOfR)
        {
            ModuleName = "Histogram Module";
            s_RInstance = InstanceOfR;            
        }
        #endregion

        #region Properties
        
        #endregion

        #region Methods
        /// <summary>
        ///  Runs module
        /// </summary>
        public override void PerformOperation()
        {
            CreatePlotsFolder();

        }
        #endregion
    }
}
