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
    /// Main Base class for the pipeline modules
    /// </summary>
    public abstract class clsBaseModule : IBaseModule
    {
        private string s_ModuleName = "";
        private Dictionary<string, dynamic> d_Parameters;
        private clsBaseDataModule bdm_ParentModule;

        #region Properties
        public string ModuleName
        {
            get { return s_ModuleName; }
            set { s_ModuleName = value; }
        }

        public Dictionary<string, dynamic> Parameters
        {
            get { return d_Parameters; }
            set { d_Parameters = value; }
        }
        #endregion

        #region Methods
        public bool HasParent()
        {
            if (bdm_ParentModule != null)
                return true;
            else
                return false;
        }

        public void SetParent(clsBaseDataModule Parent)
        {
            bdm_ParentModule = Parent;
        }

        public clsBaseDataModule GetParent()
        {
            return bdm_ParentModule;
        }

        public virtual void PerformOperation()
        {
        }

        public virtual void PrintModule()
        {
            Console.WriteLine(ModuleName);
        }

        public virtual string GetDescription()
        {
            return ModuleName;
        }
        #endregion
    }
}
