/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/
 * -----------------------------------------------------
 *
 * Licensed under the 2-Clause BSD License; you may not use this
 * file except in compliance with the License.  You may obtain
 * a copy of the License at https://opensource.org/licenses/BSD-2-Clause
 * -----------------------------------------------------*/

using System.Collections.Generic;

namespace Cyclops.Operations
{
    public class Online2DLCMainOperation : BaseOperationModule
    {
        #region Members
        private readonly string m_ModuleName = "Online2DLCMainOperation";
        #endregion

        #region Properties

        #endregion

        #region Constructors
        public Online2DLCMainOperation()
        {
            ModuleName = m_ModuleName;
        }

        public Online2DLCMainOperation(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
        }

        public Online2DLCMainOperation(CyclopsModel CyclopsModel, Dictionary<string, string> OperationParameters)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
            Parameters = OperationParameters;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Retrieves the Default Value
        /// </summary>
        /// <returns>Default Value</returns>
        protected override string GetDefaultValue()
        {
            return "false";
        }

        /// <summary>
        /// Retrieves the Type Name for automatically
        /// registering the module assembly
        /// </summary>
        /// <returns>Module's Name</returns>
        protected override string GetTypeName()
        {
            return ModuleName;
        }
        #endregion
    }
}
