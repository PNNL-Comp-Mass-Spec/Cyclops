﻿/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: joseph.brown@pnnl.gov
 * Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/
 * -----------------------------------------------------
 * 
 * Licensed under the 2-Clause BSD License; you may not use this
 * file except in compliance with the License.  You may obtain
 * a copy of the License at https://opensource.org/licenses/BSD-2-Clause
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Cyclops
{
    public class CyclopsModuleLoader
    {
        #region Members
        
        #endregion

        #region Properties
        public DataModules.BaseDataModule Root { get; set; }

        public BaseModule CurrentNode { get; set; }
        #endregion

        #region Constructors
        public CyclopsModuleLoader()
        {
        }
        #endregion

        #region Methods

        #endregion
    }
}
