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
using System.Linq;
using System.Text;

namespace Cyclops.DataModules
{
    public class Histogram : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "Histogram",
            m_BarColor = "cornflowerblue",
            m_Main = "", m_XLabel = "", m_YLabel = "";
        /// <summary>
        /// Required parameters to run Aggregate
        /// </summary>
        private enum RequiredParameters
        {
            TableName, PlotFileName, DataColumns
        }

        private string m_HistogramType = "standard";
        #endregion

        #region Properties
        public string HistogramType
        {
            get { return m_HistogramType; }
            set { m_HistogramType = value; }
        }

        public string BarColor
        {
            get { return m_BarColor; }
            set { m_BarColor = value; }
        }

        public string PlotFileName { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an Histogram Module
        /// </summary>
        public Histogram()
        {
            ModuleName = m_ModuleName;
        }

        /// <summary>
        /// Histogram module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public Histogram(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Model = CyclopsModel;
        }

        /// <summary>
        /// Histogram module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public Histogram(CyclopsModel CyclopsModel,
            Dictionary<string, string> ExportParameters)
        {
            ModuleName = m_ModuleName;
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
            bool b_Successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                if (CheckParameters())
                    b_Successful = HistogramFunction();
            }

            return b_Successful;
        }

        /// <summary>
        /// Checks the parameters to ensure that all required keys are present
        /// </summary>
        /// <returns>True, if all required keys are included in the
        /// Parameters</returns>
        public override bool CheckParameters()
        {
            bool b_Successful = true;

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogWarning("Required Field Missing: " + s,
                        ModuleName, StepNumber);
                    b_Successful = false;
                    return b_Successful;
                }
            }
            
            if (!Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.TableName.ToString()]))
            {
                Model.LogError("R Environment does not contain the " +
                    "specified input table: " +
                    Parameters[RequiredParameters.TableName.ToString()],
                    ModuleName, StepNumber);
                b_Successful = false;
            }

            if (Parameters.ContainsKey("HistogramType"))
                HistogramType = Parameters["HistogramType"];
            if (Parameters.ContainsKey("BarColor"))
                BarColor = Parameters["BarColor"];

            if (Parameters.ContainsKey("BackgroundColor"))
                BackgroundColor = Parameters["BackgroundColor"];
            if (Parameters.ContainsKey("Height"))
                Height = Convert.ToInt32(Parameters["Height"]);
            if (Parameters.ContainsKey("Width"))
                Width = Convert.ToInt32(Parameters["Width"]);

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

            if (Directory.Exists(Model.WorkDirectory))
            {
                string s_PlotDirectory = Path.Combine(
                    Model.WorkDirectory, "Plots").Replace("\\", "/");
                if (!Directory.Exists(s_PlotDirectory))
                    Directory.CreateDirectory(s_PlotDirectory);
                PlotFileName = Path.Combine(s_PlotDirectory,
                    Parameters[RequiredParameters.PlotFileName.ToString()]).Replace("\\", "/");
            }            

            return b_Successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool HistogramFunction()
        {
            bool b_Successful = true;

            string Command = "";

            switch (HistogramType.ToLower())
            {
                case "standard":
                    Command = string.Format("plot_hist(Data={0}, " +
                         "file=\"{1}\", Data.Columns='{2}', " +
                         "IMGwidth={3}, " +
                         "IMGheight={4}, FNTsize={5}, colF=\"{6}\", colB=\"{7}\")",
                         Parameters[RequiredParameters.TableName.ToString()],
                         PlotFileName,
                         Parameters[RequiredParameters.DataColumns.ToString()],
                         Width,
                         Height,
                         FontSize,
                         BarColor,
                         BackgroundColor);
                    break;
            }

            try
            {
                b_Successful = Model.RCalls.Run(Command,
                    ModuleName, StepNumber);
            }
            catch (Exception exc)
            {
                Model.LogError("Exception encountered while performing " +
                    "Histogram:\n" + exc.ToString(), ModuleName, StepNumber);
                SaveCurrentREnvironment();
                b_Successful = false;
            }

            return b_Successful;
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
        #endregion
    }
}

