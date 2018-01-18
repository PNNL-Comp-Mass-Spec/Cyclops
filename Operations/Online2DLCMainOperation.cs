/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: http://omics.pnl.gov/software
 * -----------------------------------------------------
 *
 * Licensed under the Apache License, Version 2.0; you may not use this
 * file except in compliance with the License.  You may obtain a copy of the
 * License at https://opensource.org/licenses/Apache-2.0
 * -----------------------------------------------------*/

using System.Collections.Generic;

namespace Cyclops.Operations
{
    public class Online2DLCMainOperation : BaseOperationModule
    {
        #region Members
        private string m_ModuleName = "Online2DLCMainOperation";
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

        public Online2DLCMainOperation(CyclopsModel CyclopsModel,
            Dictionary<string, string> OperationParameters)
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
