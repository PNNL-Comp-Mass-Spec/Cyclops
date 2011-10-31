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
using System.Text;
using System.IO;

using RDotNet;

namespace Cyclops
{
    public class clsCleanUpRSourceFileObjects : clsBaseDataModule
    {
        protected string s_RInstance;

        #region Constructors
        public clsCleanUpRSourceFileObjects()
        {
            ModuleName = "Cleanup R Source File Objects Module";
        }
        public clsCleanUpRSourceFileObjects(string InstanceOfR)
        {
            ModuleName = "Cleanup R Source File Objects Module";
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Methods
        /// <summary>
        ///  Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);
            string s_RStatement = "rm(list=objects2delete)\nrm(objects2delete)";
            engine.EagerEvaluate(s_RStatement);

            RunChildModules();
        }
        #endregion        
    }
}
