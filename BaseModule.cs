﻿/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
 * -----------------------------------------------------
 *
 * Licensed under the 2-Clause BSD License; you may not use this
 * file except in compliance with the License.  You may obtain
 * a copy of the License at https://opensource.org/licenses/BSD-2-Clause
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;

namespace Cyclops
{
    public abstract class BaseModule
    {
        private DataModules.BaseDataModule m_ParentModule;

        /// <summary>
        /// Name of the module
        /// </summary>
        public string ModuleName { get; set; } = "";

        /// <summary>
        /// Description of the module
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Parameters that can be used by the module
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// Number of the module in order supplied to Cyclops
        /// </summary>
        public int StepNumber { get; set; } = 0;

        /// <summary>
        /// Instance of the Model Class
        /// </summary>
        public CyclopsModel Model { get; set; }

        /// <summary>
        /// Runs the module's operation
        /// </summary>
        public virtual bool PerformOperation()
        {
            return true;
        }

        /// <summary>
        /// Checks the parameters to ensure that all required keys are present
        /// </summary>
        /// <returns>True, if all required keys are included in the
        /// Parameters</returns>
        public virtual bool CheckParameters()
        {
            return true;
        }

        public virtual void WriteModuleToXML(XmlWriter Writer)
        {
        }

        public virtual DataTable WriteModuleToDataTable(DataTable Table)
        {
            return Table;
        }

        /// <summary>
        /// Indicates whether the module has a parent
        /// </summary>
        /// <returns>True, if module has a parent</returns>
        public bool HasParent()
        {
            return m_ParentModule != null;
        }

        /// <summary>
        /// Sets the parent for the module
        /// </summary>
        /// <param name="Parent">Parent module (must be a BaseDataModule)</param>
        public void SetParent(DataModules.BaseDataModule Parent)
        {
            m_ParentModule = Parent;
        }

        /// <summary>
        /// Get the parent module
        /// </summary>
        /// <returns>Parent module</returns>
        public DataModules.BaseDataModule GetParent()
        {
            return m_ParentModule;
        }

        /// <summary>
        /// Print the module's name
        /// </summary>
        public virtual void PrintModule()
        {
            Console.WriteLine(ModuleName);
        }

        /// <summary>
        /// Returns the module's name as a string
        /// </summary>
        /// <returns>Module's name</returns>
        public virtual string GetDescription()
        {
            return ModuleName;
        }

        /// <summary>
        /// Generates a random string with the prefix "tmpTable_"
        /// </summary>
        /// <returns>Random temporary table name</returns>
        public virtual string GetTemporaryTableName()
        {
            return Model.RCalls.GetTemporaryTableName();
        }

        /// <summary>
        /// Generates a random temporary table name
        /// </summary>
        /// <param name="Prefix">Prefix appended to random name</param>
        /// <returns>random name for a temporary table</returns>
        public virtual string GetTemporaryTableName(string Prefix)
        {
            return Model.RCalls.GetTemporaryTableName(Prefix);
        }
    }
}
