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
    public class BBM_QuasiTel : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "BBM_QuasiTel";
        private string m_Description = "";
        
        /// <summary>
        /// Required parameters to run BBM_and_QuasiTel Module
        /// </summary>
        private enum RequiredParameters
        { 
            NewTableName, InputTableName, FactorTable, Fixed_Effect, Theta
        }

        //private enum ParametersForBBMandQuasitel
        //{
        //    Fixed_Effect
        //}

        private string m_MergeColumn = "Alias"; // default value of MergeColumn
        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an BBM_and_QuasiTel Module
        /// </summary>
        public BBM_QuasiTel()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// BBM_and_QuasiTel module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public BBM_QuasiTel(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// BBM_and_QuasiTel module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public BBM_QuasiTel(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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
            bool successful = true;

            if (Model.PipelineCurrentlySuccessful)
            {
                Model.CurrentStepNumber = StepNumber;

                Model.LogMessage("Running " + ModuleName, ModuleName, StepNumber);

                if (CheckParameters())
                    successful = BBM_and_QuasiTelFunction();
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
            Dictionary<string, string> paramDictionary = new Dictionary<string, string>();

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
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
            bool successful = true;

            foreach (string s in Enum.GetNames(typeof(RequiredParameters)))
            {
                if (!Parameters.ContainsKey(s) && !string.IsNullOrEmpty(s))
                {
                    Model.LogWarning("Required Field Missing: " + s, ModuleName, StepNumber);
                    successful = false;
                    return successful;
                }
            }

            if (Parameters.ContainsKey("MergeColumn"))
                m_MergeColumn = Parameters["MergeColumn"];

            if (!Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.InputTableName.ToString()]))
            {
                Model.LogWarning("R Environment does not contain the " +
                    "specified input table: " +
                    Parameters[RequiredParameters.InputTableName.ToString()],
                    ModuleName, StepNumber);
                successful = false;
            }
            if (!Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.FactorTable.ToString()]))
            {
                Model.LogWarning("R Environment does not contain the " +
                    "specified factor table: " +
                    Parameters[RequiredParameters.FactorTable.ToString()],
                    ModuleName, StepNumber);
                successful = false;
            }


            return successful;
        }

        /// <summary>
        /// Performs the Beta-Binomial & QuasiTel Model
        /// </summary>
        /// <returns>True, if the function completes successfully</returns>
        public bool BBM_and_QuasiTelFunction()
        {
            bool successful = true;

            if (Parameters.ContainsKey(
                RequiredParameters.Fixed_Effect.ToString()))
            {
                string factorTable = Parameters[RequiredParameters.FactorTable.ToString()];
                string fixedEffect = Parameters[RequiredParameters.Fixed_Effect.ToString()];

                if (string.IsNullOrEmpty(factorTable))
                {
                      Model.LogWarning("FactorTable parameter is empty; skipping QuasiTel", ModuleName, StepNumber);
                    return true;
                }

                if (string.IsNullOrEmpty(fixedEffect))
                {
                    Model.LogWarning("FixedEffect parameter is empty; skipping QuasiTel", ModuleName, StepNumber);
                    return true;
                }


                if (!Model.RCalls.TableContainsColumn(factorTable, fixedEffect))
                {
                    Model.LogError(string.Format(
                        "Factor table ({0}) does not contain the specified " +
                        "column ({1})",
                        Parameters[RequiredParameters.FactorTable.ToString()],
                        Parameters[RequiredParameters.Fixed_Effect.ToString()]),
                        ModuleName, StepNumber);
                    return false;
                }


                // TODO : Make it work
                string tmpFactorTable = GetTemporaryTableName("T_BBMQuasiFactor_");
                string tmpInputTableName = GetTemporaryTableName("T_BBMQuasiInput_");
                string factorComplete = Parameters[RequiredParameters.FactorTable.ToString()] + "[,'" +
                        Parameters[RequiredParameters.Fixed_Effect.ToString()] + "']";

                try
                {
                    string rCmd = "";

                    if (Parameters.ContainsKey("removePeptideColumn"))
                    {
                        rCmd += string.Format(
                            "{0} <- data.matrix({1}[,2:ncol({1})])\n", 
                            tmpInputTableName,
                            Parameters[RequiredParameters.InputTableName.ToString()]);
                    }
                    else
                    {
                        rCmd += string.Format(
                            "{0} <- {1}\n",
                            tmpInputTableName,
                            Parameters[RequiredParameters.InputTableName.ToString()]);
                    }

                    successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);

                    List<string> factorList = Model.RCalls.GetColumnNames(tmpInputTableName, true);
                    int factorCount = Model.RCalls.GetLengthOfVector(factorComplete);
                    if (factorList.Count == factorCount && successful)
                    {
                        rCmd = string.Format(
                            "{0} <- jnb_BBM_and_QTel(" +
                            "tData={1}, " +
                            "colMetadata={2}, " +
                            "colFactor='{3}', " +
                            "theta={4}, " +
                            "sinkFileName='')\n" +
                            "rm({1})\n",
                            Parameters[RequiredParameters.NewTableName.ToString()],
                            tmpInputTableName,
                            Parameters[RequiredParameters.FactorTable.ToString()],
                            Parameters[RequiredParameters.Fixed_Effect.ToString()],
                            Parameters[RequiredParameters.Theta.ToString()]);

                        successful = Model.RCalls.Run(rCmd, ModuleName, StepNumber);
                    }
                    else
                    {
                        Model.LogError(string.Format(
                                "ERROR BBM_and_QuasiTel class: Dimensions of spectral count table ({0}) " +
                                "do not match the dimensions of your factor vector ({1})",
                                factorList.Count,
                                factorCount));
                        SaveCurrentREnvironment();
                        successful = false;
                    }
                }
                catch (Exception ex)
                {
                    Model.LogError("Exception encountered while performing " +
                        "BBM and QuasiTel analyses:\n" + ex.ToString(),
                        ModuleName, StepNumber);
                    SaveCurrentREnvironment();
                    successful = false;
                }
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
