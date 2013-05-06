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

namespace Cyclops.DataModules
{
    public abstract class BaseDataModule : BaseModule
    {
        #region Members

        #region Visualization Members
        private string m_BackgroundColor = "white";

        private int m_Height = 1200, m_Width = 1200,
            m_Resolution = 600, m_FontSize = 12;
        #endregion
        #endregion

        #region Properties



        protected abstract string GetDefaultValue();
        protected abstract string GetTypeName();
        private static Dictionary<string, Type> sTypeMap = CreateTypeMap();

        #region Visualization Properties
        protected string BackgroundColor
        {
            get { return m_BackgroundColor; }
            set { m_BackgroundColor = value; }
        }

        protected int Height
        {
            get { return m_Height; }
            set { m_Height = value; }
        }

        protected int Width
        {
            get { return m_Width; }
            set { m_Width = value; }
        }

        public int FontSize
        {
            get { return m_FontSize; }
            set { m_FontSize = value; }
        }

        public int Resolution
        {
            get { return m_Resolution; }
            set { m_Resolution = value; }
        }

        public string Main { get; set; }

        public string XLabel { get; set; }

        public string YLabel { get; set; }
        #endregion
        #endregion

        #region Methods

        /// <summary>
        /// Writes the Data Module out to XML
        /// </summary>
        /// <param name="Writer">XML Stream to write the module out to</param>
        public override void WriteModuleToXML(XmlWriter Writer)
        {
            Writer.WriteStartElement("Module");
            Writer.WriteStartAttribute("Type");
            Writer.WriteValue("Data");
            Writer.WriteEndAttribute();
            Writer.WriteStartAttribute("Name");
            Writer.WriteValue(ModuleName);
            Writer.WriteEndAttribute();
            Writer.WriteStartAttribute("Step");
            Writer.WriteValue(StepNumber);
            Writer.WriteEndAttribute();

            foreach (KeyValuePair<string, string> kvp in Parameters)
            {
                Writer.WriteStartElement("Parameter");
                Writer.WriteStartAttribute("key");
                Writer.WriteValue(kvp.Key);
                Writer.WriteEndAttribute();
                Writer.WriteStartAttribute("value");
                Writer.WriteValue(kvp.Value);
                Writer.WriteEndAttribute();
                Writer.WriteEndElement();
            }

            Writer.WriteEndElement();
        }


        /// <summary>
        /// Writes out Data modules to DataTable for export
        /// </summary>
        /// <param name="Table">Table to add to</param>
        /// <returns>DataTable of workflow modules</returns>
        public override DataTable WriteModuleToDataTable(DataTable Table)
        {
            if (Parameters.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in Parameters)
                {
                    DataRow dr = Table.NewRow();

                    dr["Step"] = StepNumber;
                    dr["Module"] = ModuleName;
                    dr["ModuleType"] = 1;
                    dr["Parameter"] = kvp.Key;
                    dr["Value"] = kvp.Value;

                    Table.Rows.Add(dr);
                }
            }
            else
            {
                DataRow dr = Table.NewRow();

                dr["Step"] = StepNumber;
                dr["Module"] = ModuleName;
                dr["ModuleType"] = 1;
                Table.Rows.Add(dr);
            }
            
            return Table;
        }

        /// <summary>
        /// Organizes the ColumnMetadataTable so that the factors column will directly
        /// match the columns of the data table.
        /// </summary>
        /// <param name="InstanceOfR">Instance of the R workspace</param>
        /// <param name="NameOfDataTable">The Data Table</param>
        /// <param name="NameOfColumnMetadataTable">The Column Metadata Table</param>
        /// <param name="FactorColumn">Name of the column that contains the factor of interest</param>
        /// <returns>Name of temporary table that has the organized factors</returns>
        public string GetOrganizedFactorsVector(string NameOfDataTable,
            string NameOfColumnMetadataTable, string FactorColumn, int? Step,
            string yMergeColumn, string TempTablePrefix)
        {
            string s_TmpTable = GetTemporaryTableName(TempTablePrefix);

            string s_RStatement = string.Format(
                "{0} <- cbind('{3}'=colnames({1}))\n" +
                "{0} <- unique(merge(x={0}, y={2}[,c('{3}', '{4}')], " +
                "by.x='{3}', " +
                "by.y='{3}', all.x=T, all.y=F, sort=F))\n",
                s_TmpTable,
                NameOfDataTable,
                NameOfColumnMetadataTable,
                yMergeColumn,
                FactorColumn);

            Model.RCalls.Run(s_RStatement, 
                "Organizing Factors Vector",
                Step);

            return s_TmpTable;
        }

        /// <summary>
        /// Writes out module's name to console
        /// </summary>
		public override void PrintModule()
        {
            Console.WriteLine(ModuleName);
        }

        /// <summary>
        /// Instantiates a new type of Data Module
        /// </summary>
        /// <param name="TypeName">Name of the module type</param>
        /// <param name="TheModel">CyclopsModel</param>
        /// <returns>A Data Module</returns>
        public static BaseDataModule Create(string TypeName, 
            CyclopsModel TheModel, Dictionary<string, string> TheParameters)
        {
            Type derivedType = null;
            if (sTypeMap.TryGetValue(TypeName, out derivedType))
            {
                return System.Activator.CreateInstance(derivedType, 
                    TheModel, TheParameters) as BaseDataModule;
            }
            return null;
        }

        private static Dictionary<string, Type> CreateTypeMap()
        {
            Dictionary<string, Type> typeMap =
                new Dictionary<string, Type>(
                    StringComparer.OrdinalIgnoreCase);

            Assembly currAssembly = Assembly.GetExecutingAssembly();

            Type baseType = typeof(BaseDataModule);

            foreach (Type type in currAssembly.GetTypes())
            {
                if (!type.IsClass || type.IsAbstract ||
                    !type.IsSubclassOf(baseType))
                {
                    continue;
                }

                BaseDataModule derivedObject =
                    System.Activator.CreateInstance(type) as BaseDataModule;

                if (derivedObject != null)
                {
                    typeMap.Add(
                        derivedObject.GetTypeName(),
                        derivedObject.GetType());
                }
            }

            return typeMap;
        }


        /// <summary>
        /// Gets the list of data modules in Cyclops
        /// </summary>
        /// <returns>List of module names</returns>
        public static List<string> GetModuleNames()
        {
            List<string> Names = new List<string>();

            Assembly currAssembly = Assembly.GetExecutingAssembly();

            Type baseType = typeof(BaseDataModule);

            foreach (Type type in currAssembly.GetTypes())
            {
                if (!type.IsClass || type.IsAbstract ||
                    !type.IsSubclassOf(baseType))
                {
                    continue;
                }

                BaseDataModule derivedObject =
                    System.Activator.CreateInstance(type) as BaseDataModule;

                if (derivedObject != null)
                {
                    Names.Add(derivedObject.GetTypeName());
                }
            }

            Names.Sort();

            return Names;
        }

        public void CheckForPlotsDirectory()
        {
            string s_Dir = Path.Combine(Model.WorkDirectory, "Plots");
            if (!Directory.Exists(s_Dir))
                Directory.CreateDirectory(s_Dir);
        }

        /// <summary>
        /// Saves the current work environment, important for 
        /// debugging instances when Cyclops fails.
        /// </summary>
        public void SaveCurrentREnvironment()
        {
            if (!string.IsNullOrEmpty(Model.RWorkEnvironment))
            {
                Model.LogMessage("Saving current work environment...",
                    ModuleName, StepNumber);
                Model.RCalls.Run(string.Format("save.image('{0}')\n",
                    Model.RWorkEnvironment), ModuleName, StepNumber);
            }
            else
            {
                Model.LogMessage("Saving current work environment...",
                    ModuleName, StepNumber);
                string s_SaveFileName = Path.Combine(Model.WorkDirectory, "Results.RData").Replace("\\", "/");
                Model.RCalls.Run(string.Format("save.image('{0}')\n",
                    s_SaveFileName), ModuleName, StepNumber);
            }
        }
        #endregion
    }
}
