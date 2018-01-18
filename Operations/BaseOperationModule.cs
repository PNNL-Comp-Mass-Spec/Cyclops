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

using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Path to SQLite database that contains the table to
        /// run a Cyclops Workflow
        /// </summary>
        public string OperationsDatabasePath { get; set; } = @"\\gigasax\DMS_Workflows\Cyclops\Cyclops_Operations.db3";

        #endregion

        #region Methods
        // <summary>
        /// Instantiates a new type of Operations Module
        /// </summary>
        /// <param name="TypeName">Name of the module type</param>
        /// <param name="TheModel">CyclopsModel</param>
        /// <returns>An Operation Module</returns>
        public static BaseOperationModule Create(string TypeName, CyclopsModel TheModel, Dictionary<string, string> TheParameters)
        {
            if (sTypeMap.TryGetValue(TypeName, out var derivedType))
            {
                return System.Activator.CreateInstance(derivedType, TheModel, TheParameters) as BaseOperationModule;
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
                    typeMap.Add(derivedObject.GetTypeName(), derivedObject.GetType());
                }
            }

            return typeMap;
        }

        #endregion
    }
}
