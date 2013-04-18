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
using System.Data;
using System.IO;
using System.Reflection;

namespace Cyclops.Operations
{
    public abstract class BaseOperationModule : BaseModule
    {
        #region Members

        #endregion

        #region Properties

        protected abstract string GetDefaultValue();
        protected abstract string GetTypeName();
        private static Dictionary<string, Type> sTypeMap = CreateTypeMap();
        #endregion

        #region Methods
        // <summary>
        /// Instantiates a new type of Operations Module
        /// </summary>
        /// <param name="TypeName">Name of the module type</param>
        /// <param name="TheModel">CyclopsModel</param>
        /// <returns>An Operation Module</returns>
        public static BaseOperationModule Create(string TypeName, 
            CyclopsModel TheModel, Dictionary<string, string> TheParameters)
        {
            Type derivedType = null;
            if (sTypeMap.TryGetValue(TypeName, out derivedType))
            {
                return System.Activator.CreateInstance(derivedType, 
                    TheModel, TheParameters) as BaseOperationModule;
            }
            return null;
        }

        private static Dictionary<string, Type> CreateTypeMap()
        {
            Dictionary<string, Type> typeMap =
                new Dictionary<string, Type>();

            Assembly currAssembly = Assembly.GetExecutingAssembly();

            Type baseType = typeof(BaseOperationModule);

            foreach (Type type in currAssembly.GetTypes())
            {
                if (!type.IsClass || type.IsAbstract ||
                    !type.IsSubclassOf(baseType))
                {
                    continue;
                }

                BaseOperationModule derivedObject =
                    System.Activator.CreateInstance(type) as BaseOperationModule;

                if (derivedObject != null)
                {
                    typeMap.Add(
                        derivedObject.GetTypeName(),
                        derivedObject.GetType());
                }
            }

            return typeMap;
        }

        #endregion
    }
}
