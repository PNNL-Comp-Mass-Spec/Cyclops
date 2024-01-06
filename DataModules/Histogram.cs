/* Written by Joseph N. Brown
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
using System.IO;

namespace Cyclops.DataModules
{
    public class Histogram : BaseDataModule
    {
        private readonly string m_ModuleName = "Histogram";
        private readonly string m_Description = "";

        /// <summary>
        /// Required parameters to run Aggregate
        /// </summary>
        private enum RequiredParameters
        {
            TableName, PlotFileName, DataColumns
        }

        public string HistogramType { get; set; } = "standard";

        public string BarColor { get; set; } = "cornflowerblue";

        public string PlotFileName { get; set; }

        /// <summary>
        /// Generic constructor creating an Histogram Module
        /// </summary>
        public Histogram()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// Histogram module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public Histogram(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// Histogram module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public Histogram(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
            Parameters = ExportParameters;
        }

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
                {
                    successful = HistogramFunction();
                }
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

            if (!Model.RCalls.ContainsObject(Parameters[nameof(RequiredParameters.TableName)]))
            {
                Model.LogError("R Environment does not contain the specified input table: " +
                    Parameters[nameof(RequiredParameters.TableName)],
                    ModuleName, StepNumber);
                successful = false;
            }

            if (Parameters.TryGetValue("HistogramType", out var histogramType))
            {
                HistogramType = histogramType;
            }

            if (Parameters.TryGetValue("BarColor", out var barColor))
            {
                BarColor = barColor;
            }

            if (Parameters.TryGetValue("BackgroundColor", out var backgroundColor))
            {
                BackgroundColor = backgroundColor;
            }

            if (Parameters.TryGetValue("Height", out var height))
            {
                Height = Convert.ToInt32(height);
            }

            if (Parameters.TryGetValue("Width", out var width))
            {
                Width = Convert.ToInt32(width);
            }

            /*
            if (Parameters.ContainsKey("Main"))
            {
                if (!string.IsNullOrEmpty(Parameters["Main"]))
                    m_Main = Parameters["Main"];
            }
            if (Parameters.ContainsKey("XLabel"))
            {
                if (!string.IsNullOrEmpty(Parameters["XLabel"]))
                    m_XLabel = Parameters["XLabel"];
            }
            if (Parameters.ContainsKey("YLabel"))
            {
                if (!string.IsNullOrEmpty(Parameters["YLabel"]))
                    m_YLabel = Parameters["YLabel"];
            }
            */

            if (Directory.Exists(Model.WorkDirectory))
            {
                var plotDirectory = Path.Combine(Model.WorkDirectory, "Plots");
                if (!Directory.Exists(plotDirectory))
                {
                    Directory.CreateDirectory(plotDirectory);
                }

                var plotFilePath = Path.Combine(plotDirectory, Parameters[nameof(RequiredParameters.PlotFileName)]);
                PlotFileName = GenericRCalls.ConvertToRCompatiblePath(plotFilePath);
            }

            return successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool HistogramFunction()
        {
            bool successful;

            var rcmd = "";

            switch (HistogramType.ToLower())
            {
                case "standard":
                    rcmd = string.Format("plot_hist(Data={0}, " +
                         "file=\"{1}\", Data.Columns='{2}', " +
                         "IMGwidth={3}, " +
                         "IMGheight={4}, FNTsize={5}, colF=\"{6}\", colB=\"{7}\")",
                         Parameters[nameof(RequiredParameters.TableName)],
                         PlotFileName,
                         Parameters[nameof(RequiredParameters.DataColumns)],
                         Width,
                         Height,
                         FontSize,
                         BarColor,
                         BackgroundColor);
                    break;
            }

            try
            {
                successful = Model.RCalls.Run(rcmd, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while performing " +
                    "Histogram:\n" + ex, ModuleName, StepNumber);
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
    }
}
