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
    public class RMD : BaseDataModule
    {
        #region Members
        private string m_ModuleName = "RMD";
        private string m_Description = "";
        
        /// <summary>
        /// Required parameters to run RMD Module
        /// </summary>
        private enum RequiredParameters
        {
            NewTableName, InputTableName, OutlierTableName,
            FactorTable, BioRep, ConsolidateFactor
        }
        
        #endregion

        #region Properties

        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an RMD Module
        /// </summary>
        public RMD()
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
        }

        /// <summary>
        /// RMD module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public RMD(CyclopsModel CyclopsModel)
        {
            ModuleName = m_ModuleName;
            Description = m_Description;
            Model = CyclopsModel;
        }

        /// <summary>
        /// RMD module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        /// <param name="ExportParameters">Export Parameters</param>
        public RMD(CyclopsModel CyclopsModel, Dictionary<string, string> ExportParameters)
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

                Model.LogMessage("Running " + ModuleName, ModuleName, StepNumber);

                if (CheckParameters())
                    Model.PipelineCurrentlySuccessful = RMDFunction();
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
                Model.LogError("Unable to find the input table, " +
                    Parameters[RequiredParameters.InputTableName.ToString()] +
                    ", in the R environment!", ModuleName, StepNumber);
                b_Successful = false;
            }

            if (!Model.RCalls.ContainsObject(
                Parameters[RequiredParameters.FactorTable.ToString()]))
            {
                Model.LogError("Unable to find the input table, " +
                    Parameters[RequiredParameters.InputTableName.ToString()] +
                    ", in the R environment!", ModuleName, StepNumber);
                b_Successful = false;
            }

            if (b_Successful)
            {
                if (!Model.RCalls.TableContainsColumn(
                    Parameters[RequiredParameters.FactorTable.ToString()],
                    Parameters[RequiredParameters.BioRep.ToString()]))
                {
                    Model.LogError("Factor table does not contain BioRep:\n" +
                        "FactorTable: " + Parameters[RequiredParameters.FactorTable.ToString()] +
                        "BioRep: " + Parameters[RequiredParameters.BioRep.ToString()],
                        ModuleName, StepNumber);
                    b_Successful = false;
                }
            }

            if (b_Successful)
            {
                if (!Model.RCalls.TableContainsColumn(
                    Parameters[RequiredParameters.FactorTable.ToString()],
                    Parameters[RequiredParameters.BioRep.ToString()]))
                {
                    Model.LogError("Factor table does not contain ConsolidateFactor:\n" +
                        "FactorTable: " + Parameters[RequiredParameters.FactorTable.ToString()] +
                        "BioRep: " + Parameters[RequiredParameters.ConsolidateFactor.ToString()],
                        ModuleName, StepNumber);
                    b_Successful = false;
                }
            }

            return b_Successful;
        }

        /// <summary>
        /// Runs the RMD Function
        /// </summary>
        /// <returns>True, if the RMD function completes successfully</returns>
        public bool RMDFunction()
        {
            bool b_Successful = true;

            string s_TmpTable = GetTemporaryTableName("tmpRMD_");

            string Command = string.Format(
                "{0} <- DetectOutliers(" +
                "data={1}, " +
                "class=as.numeric({2}${3}), " +
                "techreps=as.numeric({2}${4}))\n" +
                "{5} <- {1}[,{0}$Keep_runs]\n",
                s_TmpTable,
                Parameters[RequiredParameters.InputTableName.ToString()],
                Parameters[RequiredParameters.FactorTable.ToString()],
                Parameters[RequiredParameters.BioRep.ToString()],
                Parameters[RequiredParameters.ConsolidateFactor.ToString()],
                Parameters[RequiredParameters.NewTableName.ToString()]);

            try
            {
                b_Successful = Model.RCalls.Run(Command, ModuleName, StepNumber);
            }
            catch (Exception ex)
            {
                Model.LogError("Exception encountered while running " +
                    "RMD Analysis: " + ex.ToString(), ModuleName,
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
