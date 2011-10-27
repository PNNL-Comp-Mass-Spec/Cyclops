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

namespace Cyclops
{
    /// <summary>
    /// Base class for Data Modules
    /// </summary>
    public abstract class clsBaseDataModule : clsBaseModule
    {
        private List<clsBaseVisualizationModule> l_ChildVisualizationModules = new List<clsBaseVisualizationModule>();
        private List<clsBaseExportModule> l_ChildExportModules = new List<clsBaseExportModule>();
        private List<clsBaseDataModule> l_ChildDataModules = new List<clsBaseDataModule>();

        #region Properties
        
        #endregion

        #region Methods
        
        /// <summary>
        /// Adds a child data module
        /// </summary>
        /// <param name="Child">Data Child Module to Add</param>
        public void AddDataChild(clsBaseDataModule Child)
        {
            l_ChildDataModules.Add(Child);
        }

        /// <summary>
        /// Adds a child visualization module
        /// </summary>
        /// <param name="Child">Visualization Child Module to Add</param>
        public void AddVisualChild(clsBaseVisualizationModule Child)
        {
            l_ChildVisualizationModules.Add(Child);
        }

        /// <summary>
        /// Adds a child export module
        /// </summary>
        /// <param name="Child">Export Child Module to Add</param>
        public void AddExportChild(clsBaseExportModule Child)
        {
            l_ChildExportModules.Add(Child);
        }

        /// <summary>
        /// Runs the module's operation
        /// </summary>
        public virtual void PerformOperation()
        {
        }

        /// <summary>
        /// Runs the child modules in order: 1. visualization, 2. export, 3. data
        /// </summary>
        public void RunChildModules()
        {
            foreach (clsBaseVisualizationModule bvm in l_ChildVisualizationModules)
            {
                bvm.PerformOperation();
            }
            foreach (clsBaseExportModule bem in l_ChildExportModules)
            {
                bem.PerformOperation();
            }
            foreach (clsBaseDataModule bdm in l_ChildDataModules)
            {
                bdm.PerformOperation();
            }

        }

        public void PrintModule()
        {
            Console.WriteLine(ModuleName);
            PrintChildModules();
        }

        /// <summary>
        /// Prints the modules name to the console
        /// </summary>
        public void PrintChildModules()
        {
            foreach (clsBaseVisualizationModule bvm in l_ChildVisualizationModules)
            {
                bvm.PrintModule();
            }
            foreach (clsBaseExportModule bem in l_ChildExportModules)
            {
                bem.PrintModule();
            }
            foreach (clsBaseDataModule bdm in l_ChildDataModules)
            {
                bdm.PrintModule();
            }
        }
        #endregion
    }
}
