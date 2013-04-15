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
using System.Xml;

namespace Cyclops
{
    public abstract class BaseModule
    {
        #region Members
        private string s_ModuleName = "";
        private Dictionary<string, string> d_Parameters;
        private DataModules.BaseDataModule bdm_ParentModule;
        private int m_StepNumber = 0;
        #endregion

        #region Properties
        /// <summary>
        /// Name of the module
        /// </summary>
        public string ModuleName
        {
            get { return s_ModuleName; }
            set { s_ModuleName = value; }
        }
        
        /// <summary>
        /// Parameters that can be used by the module
        /// </summary>
        public Dictionary<string, string> Parameters
        {
            get { return d_Parameters; }
            set { d_Parameters = value; }
        }

        /// <summary>
        /// Number of the module in order supplied to Cyclops
        /// </summary>
        public int StepNumber
        {
            get { return m_StepNumber; }
            set { m_StepNumber = value; }
        }

        /// <summary>
        /// Instance of the Model Class
        /// </summary>
        public CyclopsModel Model { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Runs the module's operation
        /// </summary>
        public virtual void PerformOperation()
        {
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
            if (bdm_ParentModule != null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Sets the parent for the module
        /// </summary>
        /// <param name="Parent">Parent module (must be a BaseDataModule)</param>
        public void SetParent(DataModules.BaseDataModule Parent)
        {
            bdm_ParentModule = Parent;
        }

        /// <summary>
        /// Get the parent module
        /// </summary>
        /// <returns>Parent module</returns>
        public DataModules.BaseDataModule GetParent()
        {
            return bdm_ParentModule;
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
        #endregion
    }
}
