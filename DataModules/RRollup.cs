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

namespace Cyclops.DataModules
{
    public class RRollup : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "RRollup";
        private string m_Description = "";
        private string m_MinimumPresence = "1";
        private string m_Mode = "median";
        private string m_ProteinInfo_ProteinColumn = "1";
        private string m_ProteinInfo_PeptideColumn = "2";
        private string m_MinimumOverlap = "2";
        private string m_OneHitWonders = "FALSE";
        private string m_GrubbsPValue = "0.05";
        private string m_GminPCount = "5";
        private string m_Center = "FALSE";

        /// <summary>
        /// Required parameters to run RRollup Module
        /// </summary>
        private enum RequiredParameters
        {
            NewTableName, InputTableName, ProteinInfoTable
        }

        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an RRollup Module
        /// </summary>
        public RRollup()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// RRollup module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public RRollup(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// RRollup module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public RRollup(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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
            bool b_Successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running RRollup", ModuleName, StepNumber);

                if (CheckParameters())
                    b_Successful = RRollupFunction();
            }

            return b_Successful;
        }

        /// <summary>
        /// Retrieves a dictionary of all parameters used by the module,
        /// and the corresponding default values
        /// </summary>
        /// <returns>Parameters used by module</returns>
        public override Dictionary<string, string> GetParametersTemplate()
        {
            Dictionary<string, string> d_Parameters = new Dictionary<string, string>();

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                d_Parameters.Add(s, "");
            }

            return d_Parameters;
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
                    Model.LogWarning("Required Field Missing: " + s, ModuleName, StepNumber);
                    b_Successful = false;
                    return b_Successful;
                }
            }

            if (!Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.InputTableName.ToString()]))
            {
                Model.LogWarning("WARNING in RRollup: The R environment does " +
                    "not contain the input table, " +
                    Parameters[RequiredParameters.InputTableName.ToString()]);
                b_Successful = false;
            }

            if (Parameters.ContainsKey("MinPresence"))
                m_MinimumPresence = Parameters["MinPresence"];
            if (Parameters.ContainsKey("ProteinInfo_ProteinCol"))
                m_ProteinInfo_ProteinColumn = Parameters["ProteinInfo_ProteinCol"];
            if (Parameters.ContainsKey("Mode"))
                m_Mode = Parameters["Mode"];
            if (Parameters.ContainsKey("MinOverlap"))
                m_MinimumOverlap = Parameters["MinOverlap"];
            if (Parameters.ContainsKey("OneHitWonders"))
                m_OneHitWonders = Parameters["OneHitWonders"];
            if (Parameters.ContainsKey("GpValue"))
                m_GrubbsPValue = Parameters["GpValue"];
            if (Parameters.ContainsKey("GminPCount"))
                m_GminPCount = Parameters["GminPCount"];
            if (Parameters.ContainsKey("Center"))
                m_Center = Parameters["Center"];


            return b_Successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool RRollupFunction()
        {
            bool b_Successful = true;

            string rCmd = string.Format("{0} <- RRollup.proteins(" +
                    "Data={1}, ProtInfo={2}, minPresence={3}, Mode=\"{4}\", " +
                    "protInfo_ProtCol={5}, protInfo_PepCol={6}, minOverlap={7}, " +
                    "oneHitWonders={8}, gpvalue={9}, gminPCount={10}, center={11})",
                    Parameters[RequiredParameters.NewTableName.ToString()],     // 0
                    Parameters[RequiredParameters.InputTableName.ToString()],   // 1
                    Parameters[RequiredParameters.ProteinInfoTable.ToString()], // 2
                    m_MinimumPresence,                                          // 3
                    m_Mode,                                                     // 4
                    m_ProteinInfo_ProteinColumn,                                // 5
                    m_ProteinInfo_PeptideColumn,                                // 6
                    m_MinimumOverlap,                                           // 7
                    m_OneHitWonders,                                            // 8
                    m_GrubbsPValue,                                             // 9
                    m_GminPCount,                                               // 10
                    m_Center);                                                  // 11


            try
            {
                b_Successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while running " +
                    "'RRollupFunction': " + ex.ToString(), ModuleName,
                    StepNumber);
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
