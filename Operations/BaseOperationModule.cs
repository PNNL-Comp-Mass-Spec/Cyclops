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

        // ReSharper disable once UnusedMember.Global
        protected abstract string GetDefaultValue();

        protected abstract string GetTypeName();

        private static readonly Dictionary<string, Type> mTypeMap = CreateTypeMap();

        /// <summary>
        /// Path to SQLite database that contains the table to
        /// run a Cyclops Workflow
        /// </summary>
        public string OperationsDatabasePath { get; set; } = @"\\gigasax\DMS_Workflows\Cyclops\Cyclops_Operations.db3";

        #endregion

        #region Methods

        /// <summary>
        /// Instantiates a new type of Operations Module
        /// </summary>
        /// <param name="typeName">Name of the module type</param>
        /// <param name="model">CyclopsModel</param>
        /// <param name="parameters"></param>
        /// <returns>An Operation Module</returns>
        public static BaseOperationModule Create(string typeName, CyclopsModel model, Dictionary<string, string> parameters)
        {
            if (mTypeMap.TryGetValue(typeName, out var derivedType))
            {
                return Activator.CreateInstance(derivedType, model, parameters) as BaseOperationModule;
            }
            return null;
        }

        private static Dictionary<string, Type> CreateTypeMap()
        {
            var typeMap = new Dictionary<string, Type>();

            var currentAssembly = Assembly.GetExecutingAssembly();

            var baseType = typeof(BaseOperationModule);

            foreach (var type in currentAssembly.GetTypes())
            {
                if (!type.IsClass || type.IsAbstract ||
                    !type.IsSubclassOf(baseType))
                {
                    continue;
                }

                if (Activator.CreateInstance(type) is BaseOperationModule derivedObject)
                {
                    typeMap.Add(derivedObject.GetTypeName(), derivedObject.GetType());
                }
            }

            return typeMap;
        }

        #endregion
    }
}
