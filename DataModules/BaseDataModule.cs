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

        #endregion
        #endregion

        #region Properties

        protected abstract string GetDefaultValue();
        protected abstract string GetTypeName();
        protected abstract string GetTypeDescription();
        private static readonly Dictionary<string, Type> sTypeMap = CreateTypeMap();

        #region Visualization Properties

        protected string BackgroundColor { get; set; } = "white";

        protected int Height { get; set; } = 1200;

        protected int Width { get; set; } = 1200;

        public int FontSize { get; set; } = 12;

        public int Resolution { get; set; } = 600;

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

            foreach (var kvp in Parameters)
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
                foreach (var kvp in Parameters)
                {
                    var dr = Table.NewRow();

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
                var dr = Table.NewRow();

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
        /// <param name="NameOfDataTable">The Data Table</param>
        /// <param name="NameOfColumnMetadataTable">The Column Metadata Table</param>
        /// <param name="FactorColumn">Name of the column that contains the factor of interest</param>
        /// <param name="step"></param>
        /// <param name="yMergeColumn"></param>
        /// <param name="TempTablePrefix"></param>
        /// <returns>Name of temporary table that has the organized factors</returns>
        public string GetOrganizedFactorsVector(
            string NameOfDataTable,
            string NameOfColumnMetadataTable, string FactorColumn, int step,
            string yMergeColumn, string TempTablePrefix)
        {
            var s_TmpTable = GetTemporaryTableName(TempTablePrefix);

            var s_RStatement = string.Format(
                "{0} <- cbind('{3}'=colnames({1}))\n" +
                "{0} <- unique(merge(x={0}, y={2}[,c('{3}', '{4}')], " +
                "by.x='{3}', " +
                "by.y='{3}', all.x=T, all.y=F, sort=F))\n",
                s_TmpTable,
                NameOfDataTable,
                NameOfColumnMetadataTable,
                yMergeColumn,
                FactorColumn);

            Model.RCalls.Run(s_RStatement, "Organizing Factors Vector", step);

            return s_TmpTable;
        }

        /// <summary>
        /// Writes out module's name to console
        /// </summary>
        public override void PrintModule()
        {
            Console.WriteLine(ModuleName);
        }

        public static BaseDataModule Create(string TypeName)
        {
            if (sTypeMap.TryGetValue(TypeName, out var derivedType))
            {
                return Activator.CreateInstance(derivedType)
                    as BaseDataModule;
            }
            return null;
        }

        /// <summary>
        /// Instantiates a new type of Data Module
        /// </summary>
        /// <param name="TypeName">Name of the module type</param>
        /// <param name="TheModel">CyclopsModel</param>
        /// <param name="TheParameters"></param>
        /// <returns>A Data Module</returns>
        public static BaseDataModule Create(
            string TypeName,
            CyclopsModel TheModel,
            Dictionary<string, string> TheParameters)
        {
            if (sTypeMap.TryGetValue(TypeName, out var derivedType))
            {
                return Activator.CreateInstance(derivedType, TheModel, TheParameters) as BaseDataModule;
            }
            return null;
        }

        public static Dictionary<string, Type> CreateTypeMap()
        {
            var typeMap =
                new Dictionary<string, Type>(
                    StringComparer.OrdinalIgnoreCase);

            var currAssembly = Assembly.GetExecutingAssembly();

            var baseType = typeof(BaseDataModule);

            foreach (var type in currAssembly.GetTypes())
            {
                if (!type.IsClass || type.IsAbstract ||
                    !type.IsSubclassOf(baseType))
                {
                    continue;
                }

                if (Activator.CreateInstance(type) is BaseDataModule derivedObject)
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
            var Names = new List<string>();

            var currAssembly = Assembly.GetExecutingAssembly();

            var baseType = typeof(BaseDataModule);

            foreach (var type in currAssembly.GetTypes())
            {
                if (!type.IsClass || type.IsAbstract ||
                    !type.IsSubclassOf(baseType))
                {
                    continue;
                }

                if (Activator.CreateInstance(type) is BaseDataModule derivedObject)
                {
                    Names.Add(derivedObject.GetTypeName());
                }
            }

            return Names;
        }

        public void CheckForPlotsDirectory()
        {
            var s_Dir = Path.Combine(Model.WorkDirectory, "Plots");
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
                Model.LogMessage("Saving current work environment...", ModuleName, StepNumber);
                Model.RCalls.Run(string.Format("save.image('{0}')\n", Model.RWorkEnvironment), ModuleName, StepNumber);
            }
            else
            {
                Model.LogMessage("Saving current work environment...", ModuleName, StepNumber);
                var s_SaveFileName = Path.Combine(Model.WorkDirectory, "Results.RData").Replace("\\", "/");
                Model.RCalls.Run(string.Format("save.image('{0}')\n", s_SaveFileName), ModuleName, StepNumber);
            }
        }

        /// <summary>
        /// Generic method for modules to report the parameters they use with default values
        /// </summary>
        /// <returns>Parameters used by module and default values</returns>
        public virtual Dictionary<string, string> GetParametersTemplate()
        {
            return null;
        }

        #endregion
    }
}
