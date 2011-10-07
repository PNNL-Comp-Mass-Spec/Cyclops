﻿/* Written by Joseph N. Brown
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
    /// Simple module for testing functions
    /// </summary>
    public class clsMyTestingModule : clsBaseDataModule
    {
        private string s_RInstance;

        #region Contructors
        public clsMyTestingModule()
        {
            ModuleName = "Testing Module";
        }
        public clsMyTestingModule(string InstanceOfR)
        {
            ModuleName = "Testing Module";
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Functions
        public override void PerformOperation()
        {

            RunChildModules();
        }
        #endregion
    }
}
