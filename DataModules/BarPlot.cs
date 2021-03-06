﻿/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/
 * -----------------------------------------------------
 *
 * Licensed under the 2-Clause BSD License; you may not use this
 * file except in compliance with the License.  You may obtain
 * a copy of the License at https://opensource.org/licenses/BSD-2-Clause
 * -----------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Cyclops.DataModules
{
    public class BarPlot : BaseDataModule
    {
        #region Members
        private readonly string m_ModuleName = "BarPlot";
        private readonly string m_Description = "";

        /// <summary>
        /// Required parameters to run Aggregate
        /// </summary>
        private enum RequiredParameters
        {
            TableName, PlotFileName, DataColumns,
        }

        #endregion

        #region Properties
        public string BarColor { get; set; } = "cornflowerblue";

        public string Log => LogBase == null ? "FALSE" : "TRUE";

        public double? LogBase { get; set; }

        public string Names { get; set; }

        public string PlotFileName { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an BarPlot Module
        /// </summary>
        public BarPlot()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// BarPlot module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public BarPlot(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// BarPlot module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public BarPlot(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
            Parameters = ExportParameters;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override bool PerformOperation()
        {
            var successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                if (CheckParameters())
                    successful = BarPlotFunction();
            }

            return successful;
        }

        /// <summary>
        /// Retrieves a dictionary of all parameters used by the module,
        /// and the corresponding default values
        /// </summary>
        /// <returns>Parameters used by module</returns>
        public override Dictionary<string, string> GetParametersTemplate()
        {
            var paramDictionary = new Dictionary<string, string>();

            foreach (var s in Enum.GetNames(typeof(RequiredParameters)))
            {
                paramDictionary.Add(s, "");
            }

            return paramDictionary;
        }

        /// <summary>
        /// Checks the parameters to ensure that all required keys are present
        /// </summary>
        /// <returns>True, if all required keys are included in the
        /// Parameters</returns>
        public override bool CheckParameters()
        {
            var successful = true;

            foreach (var s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogWarning("Required Field Missing: " + s, ModuleName, StepNumber);
                    return false;
                }
            }

            if (!Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.TableName.ToString()]))
            {
                Model.LogError("R Environment does not contain the " +
                    "specified input table: " +
                    Parameters[RequiredParameters.TableName.ToString()],
                    ModuleName, StepNumber);
                successful = false;
            }

            #region General Plot Parameters
            if (Parameters.ContainsKey("BackgroundColor"))
                BackgroundColor = Parameters["BackgroundColor"];
            if (Parameters.ContainsKey("Height"))
                Height = Convert.ToInt32(Parameters["Height"]);
            if (Parameters.ContainsKey("Width"))
                Width = Convert.ToInt32(Parameters["Width"]);
            if (Parameters.ContainsKey("FontSize"))
                FontSize = Convert.ToInt32(Parameters["FontSize"]);
            if (Parameters.ContainsKey("Resolution"))
                Resolution = Convert.ToInt32(Parameters["Resolution"]);
            if (Parameters.ContainsKey("Main"))
                Main = Parameters["Main"];
            if (Parameters.ContainsKey("XLabel"))
                XLabel = Parameters["XLabel"];
            if (Parameters.ContainsKey("YLabel"))
                YLabel = Parameters["YLabel"];
            #endregion

            #region Plot-specific Parameters
            if (Parameters.ContainsKey("BarColor"))
                BarColor = Parameters["BarColor"];
            if (Parameters.ContainsKey("LogBase"))
                LogBase = Convert.ToDouble(Parameters["LogBase"]);
            if (Parameters.ContainsKey("Names"))
                Names = Parameters["Names"];
            #endregion

            if (Directory.Exists(Model.WorkDirectory) && successful)
            {
                var plotDirectory = Path.Combine(Model.WorkDirectory, "Plots").Replace("\\", "/");

                if (!Directory.Exists(plotDirectory))
                    Directory.CreateDirectory(plotDirectory);

                var plotFilePath = Path.Combine(plotDirectory, Parameters[RequiredParameters.PlotFileName.ToString()]);
                PlotFileName = GenericRCalls.ConvertToRCompatiblePath(plotFilePath);
            }

            return successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool BarPlotFunction()
        {
            bool successful;

            var rCmd = "";

            if (Parameters.ContainsKey("Mode"))
            {
                switch (Parameters["Mode"])
                {
                    case "iterator":
                        var tmpTable = GetTemporaryTableName("tmpBarPlot_");
                        rCmd += string.Format("{0} <- " +
                            "data.frame(Cleavage=c(\"Tryptic\", " +
                            "\"Partial\", \"NonTryptic\"), " +
                            "Frequency=c(sum({1}$Tryptic), " +
                            "sum({1}$PartTryptic), " +
                            "sum({1}$NonTryptic)))\n\n",
                            tmpTable,
                            Parameters[RequiredParameters.TableName.ToString()]);

                        Parameters[RequiredParameters.TableName.ToString()] = tmpTable;
                        break;
                }
            }

            rCmd += string.Format("plotBars(" +
                    "x={0}, Data.Column=\"{1}\", " +
                    "file=\"{2}\", " +
                    "bkground=\"{3}\", " +
                    "takeLog={4}, " +
                    "base={5}, " +
                    "names.arg=\"{6}\", " +
                    "xLab=\"{7}\", " +
                    "yLab=\"{8}\", " +
                    "title=\"{9}\", " +
                    "col={10}, " +
                    // ReSharper disable StringLiteralTypo
                    "IMGwidth={11}, " +
                    "IMGheight={12}, " +
                    "FNTsize={13}, " +
                    // ReSharper restore StringLiteralTypo
                    "res={14})\n",
                    Parameters[RequiredParameters.TableName.ToString()],    // 0
                    Parameters[RequiredParameters.DataColumns.ToString()],  // 1
                    PlotFileName,                                           // 2
                    BackgroundColor,                                        // 3
                    Log,                                                    // 4
                    LogBase,                                                // 5
                    Names,                                                  // 6
                    XLabel,                                                 // 7
                    YLabel,                                                 // 8
                    Main,                                                   // 9
                    BarColor,                                               // 10
                    Width,                                                  // 11
                    Height,                                                 // 12
                    FontSize,                                               // 13
                    Resolution                                              // 14
                    );

            if (Parameters.ContainsKey("Mode"))
            {
                if (Parameters["Mode"].Equals("iterator"))
                {
                    rCmd += string.Format(
                        "rm({0})\n",
                        Parameters[RequiredParameters.TableName.ToString()]);
                }
            }

            try
            {
                successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while creating a BarPlot:\n" + ex);
                SaveCurrentREnvironment();
                successful = false;
            }



            return successful;
        }

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

        /// <summary>
        /// Retrieves the Type Description for automatically
        /// registering the module assembly
        /// </summary>
        /// <returns>Module's Description</returns>
        protected override string GetTypeDescription()
        {
            return Description;
        }
        #endregion
    }
}

