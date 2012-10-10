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

using RDotNet;

namespace Cyclops.DataModules
{
    /// <summary>
    /// Base class for Data Modules
    /// </summary>
    public abstract class clsBaseDataModule : clsBaseModule
    {
        private List<VisualizationModules.clsBaseVisualizationModule> l_ChildVisualizationModules = new List<VisualizationModules.clsBaseVisualizationModule>();
        private List<ExportModules.clsBaseExportModule> l_ChildExportModules = new List<ExportModules.clsBaseExportModule>();
        private List<clsBaseDataModule> l_ChildDataModules = new List<clsBaseDataModule>();

        #region Properties
        // instance of the model class
        public clsCyclopsModel Model
        {
            get;
            set;
        }
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
        public void AddVisualChild(VisualizationModules.clsBaseVisualizationModule Child)
        {
            l_ChildVisualizationModules.Add(Child);
        }

        /// <summary>
        /// Adds a child export module
        /// </summary>
        /// <param name="Child">Export Child Module to Add</param>
        public void AddExportChild(ExportModules.clsBaseExportModule Child)
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
            foreach (VisualizationModules.clsBaseVisualizationModule bvm in l_ChildVisualizationModules)
            {
                bvm.PerformOperation();
            }
            foreach (ExportModules.clsBaseExportModule bem in l_ChildExportModules)
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
            foreach (VisualizationModules.clsBaseVisualizationModule bvm in l_ChildVisualizationModules)
            {
                bvm.PrintModule();
            }
            foreach (ExportModules.clsBaseExportModule bem in l_ChildExportModules)
            {
                bem.PrintModule();
            }
            foreach (clsBaseDataModule bdm in l_ChildDataModules)
            {
                bdm.PrintModule();
            }
        }

        /// <summary>
        /// Organizes the ColumnMetadataTable so that the factors column will directly
        /// match the columns of the data table.
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        /// <param name="NameOfDataTable">The Data Table</param>
        /// <param name="NameOfColumnMetadataTable">The Column Metadata Table</param>
        /// <param name="FactorColumn">Name of the column that contains the factor of interest</param>
        public void GetOrganizedFactorsVector(string InstanceOfR, string NameOfDataTable,
            string NameOfColumnMetadataTable, string FactorColumn, int? Step, int? Total)
        {
            string yMergeColumn = "Alias";
            string s_TmpTable = GetTemporaryTableName();

            string s_RStatement = string.Format(
                "{0} <- cbind(\"{3}\"=colnames({1}))\n" +
                "{2} <- merge(x={0}, y={2}, by.x=\"{3}\", by.y=\"{3}\", all.x=T, all.y=F, sort=F)\n" +
                "rm({0})",
                s_TmpTable,
                NameOfDataTable,
                NameOfColumnMetadataTable,
                yMergeColumn);

            clsGenericRCalls.Run(s_RStatement, InstanceOfR,
                "Organizing Factors Vector",
                Step, Total);
        }


        public clsLink LinkUpWithBetaBinomialModelWithQuasiTel(string InstanceOfR)
        {
            clsLink link = new clsLink();
            link.Run = false;
            REngine engine = REngine.GetInstanceFromID(InstanceOfR);

            List<string> l_BBM = new List<string>();
            List<string> l_Quasi = new List<string>();

            // Get any potential beta-binomial results table(s)
            CharacterVector cv = engine.EagerEvaluate("ls()[grep('^BBM_', ls())]").AsCharacter();
            foreach (string s in cv)
            {
                l_BBM.Add(s);
            }

            // Retrieve the QuasiTel results table(s)
            cv = engine.EagerEvaluate("ls()[grep('^QuasiTel_', ls())]").AsCharacter();
            foreach (string s in cv)
            {
                l_Quasi.Add(s);
            }

            if (l_BBM.Count > 0 & l_Quasi.Count > 0)
            {
                link.Run = true;
                string s_RStatement = string.Format(
                    "BBM_QuasiTel_Analysis <- cbind(");

                foreach (string s in l_BBM)
                {
                    s_RStatement += "'BBM'=" + s + "[,1], " +
                        "'BBM_AdjP'=p.adjust(" + s + "[,1], method='BH'), ";
                }

                foreach (string s in l_Quasi)
                {
                    s_RStatement += "'" + s + "'=" + s + "[,8], " +
                        "'" + s + "_AdjP'=p.adjust(" + s + "[,8], method='BH'), ";
                }

                s_RStatement += " T_SpectralCounts)";
                link.Statement = s_RStatement;
            }

            return link;
        }

        public bool AreDatasetAndColumnMetadataReady(
            string InstanceOfR, string DataTableName,
            string ColumnMetadataTableName, 
            string ColumnMetadataFactor)
        {
            bool b_Return = true;



            return b_Return;
        }
        #endregion
    }

    public class clsLink
    {
        public clsLink()
        {
        }

        public bool Run
        {
            get;
            set;
        }

        public string Statement
        {
            get;
            set;
        }
    }
}
