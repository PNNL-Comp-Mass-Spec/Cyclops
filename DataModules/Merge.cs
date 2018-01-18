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
    public class Merge : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "Merge";
        private string m_Description = "";
        
        /// <summary>
        /// Required parameters to run Merge Module
        /// </summary>
        private enum RequiredParameters
        {
            NewTableName, XTable, YTable, XLink, YLink, AllX, AllY
        }
        
        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an Merge Module
        /// </summary>
        public Merge()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// Merge module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public Merge(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// Merge module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public Merge(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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

                Model.LogMessage("Running Merge", ModuleName, StepNumber);

                if (CheckParameters())
                    Model.PipelineCurrentlySuccessful = MergeFunction();
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
                Parameters[RequiredParameters.XTable.ToString()]))
            {
                Model.LogError("ERROR R environment does not contain " +
                    "the X table: " + Parameters[
                    RequiredParameters.XTable.ToString()]);
                b_Successful = false;
            }
            if (!Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.YTable.ToString()]))
            {
                Model.LogError("ERROR R environment does not contain " +
                    "the Y table: " + Parameters[
                    RequiredParameters.YTable.ToString()]);
                b_Successful = false;
            }
            if (!Model.RCalls.TableContainsColumn(
                Parameters[RequiredParameters.XTable.ToString()],
                Parameters[RequiredParameters.XLink.ToString()]))
            {
                Model.LogError("ERROR The X table does not contain " +
                    "the column: " + Parameters[
                    RequiredParameters.XLink.ToString()]);
                b_Successful = false;
            }
            if (!Model.RCalls.TableContainsColumn(
                Parameters[RequiredParameters.YTable.ToString()],
                Parameters[RequiredParameters.YLink.ToString()]))
            {
                Model.LogError("ERROR The Y table does not contain " +
                    "the column: " + Parameters[
                    RequiredParameters.YLink.ToString()]);
                b_Successful = false;
            }

            return b_Successful;
        }

        /// <summary>
        /// Function
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool MergeFunction()
        {
            bool b_Successful = true;

            // Construct the R statement
            string rCmd = string.Format("{0} <- merge(x={1}," +
                "y={2}, by.x=\"{3}\", by.y=\"{4}\", all.x={5}, all.y={6})",
                Parameters[RequiredParameters.NewTableName.ToString()],
                Parameters[RequiredParameters.XTable.ToString()],
                Parameters[RequiredParameters.YTable.ToString()],
                Parameters[RequiredParameters.XLink.ToString()],
                Parameters[RequiredParameters.YLink.ToString()],
                Parameters[RequiredParameters.AllX.ToString()].ToUpper(),
                Parameters[RequiredParameters.AllY.ToString()].ToUpper());

            try
            {
                b_Successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while performing merge:\n" +
                    ex.ToString());
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
