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
using System.Linq;
using System.Text;

using RDotNet;
using log4net;

namespace Cyclops.DataModules
{
    /// <summary>
    /// Transforms datasets in R using log transformations and scaling operations
    /// Parameters include:
    /// - "newTableName"    - name of the new table to be generated.
    /// - "inputTableName"  - name of the table in R workspace
    /// - "logBase"         - Base to perform log transformation
    /// - "scale"           - value to multiply by
    /// - "add"             - value to add by
    /// </summary>
    public class clsTransformModule : clsBaseDataModule
    {
        #region Variables
        private string s_RInstance, s_Current_R_Statement = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");
        #endregion

        #region Contructors
        /// <summary>
        /// Module to scale and transform datasets
        /// </summary>
        public clsTransformModule()
        {
            ModuleName = "Transform Module";
        }
        /// <summary>
        /// Module to scale and transform datasets
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsTransformModule(string InstanceOfR)
        {
            ModuleName = "Transform Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Module to scale and transform datasets
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsTransformModule(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "Transform Module";
            Model = TheCyclopsModel;
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        public override void PerformOperation()
        {
            traceLog.Info("Transforming Datasets...");

            TransformData();

            RunChildModules();
        }

        /// <summary>
        /// Checks the dictionary to ensure all the necessary parameters are present
        /// </summary>
        /// <returns>True if all necessary parameters are present</returns>
        protected bool CheckPassedParameters()
        {
            bool b_2Pass = true;

            // NECESSARY PARAMETERS
            if (!dsp.HasNewTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Transform class: 'newTableName': \"" +
                    dsp.NewTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }
            if (!dsp.HasInputTableName)
            {
                Model.SuccessRunningPipeline = false;
                traceLog.Error("ERROR: Transform class: 'inputTableName': \"" +
                    dsp.InputTableName + "\", was not found in the passed parameters");
                b_2Pass = false;
            }

            return b_2Pass;
        }

        private void TransformData()
        {
            dsp.GetParameters(ModuleName, Parameters);

            if (CheckPassedParameters())
            {
                REngine engine = REngine.GetInstanceFromID(s_RInstance);
                string s_RStatement = "";
                
                if (dsp.HasLogBase)
                {
                    s_RStatement = string.Format(
                        "{0} <- log((data.matrix({1})+{2})*{3},{4})",
                        dsp.NewTableName,
                        dsp.InputTableName,
                        dsp.Add,
                        dsp.Scale,
                        dsp.LogBase);
                }
                else
                {
                    s_RStatement = string.Format(
                        "{0} <- ({1}+{2})*{3}",
                        dsp.NewTableName,
                        dsp.InputTableName,
                        dsp.Add,
                        dsp.Scale);
                }

                try
                {
                    traceLog.Info("Transforming Datasets: " + s_RStatement);

                    if (dsp.Set_0_to_NA)
                    {
                        s_RStatement += string.Format("\n{0}[{0}==-Inf] <- NA",
                            dsp.NewTableName);
                    }

                    s_Current_R_Statement = s_RStatement;
                    engine.EagerEvaluate(s_RStatement);

                    if (dsp.AutoScale)
                    {
                        double? d_Min = clsGenericRCalls.GetMinimumValue(s_RInstance,
                            dsp.NewTableName);
                        if (d_Min < 0 | d_Min == null)
                        {
                            // Autoscale the data.
                            s_RStatement = "{0} <- jnb_AutoScale({0})";
                            traceLog.Info("Autoscaling Transformed Datasets: " +
                                s_RStatement);
                            engine.EagerEvaluate(s_RStatement);
                        }
                    }
                }
                catch (Exception exc)
                {
                    traceLog.Error("Error transforming datasets: " + exc.ToString());
                    Model.SuccessRunningPipeline = false;
                }
            }
        }

        /// <summary>
        /// Unit Test for Transforming data
        /// </summary>
        /// <returns>Information regarding the result of the UnitTest</returns>
        public clsTestResult TestTransform()
        {
            dsp.GetParameters(ModuleName, Parameters);
            clsTestResult result = new clsTestResult(true, "");
            result.Module = ModuleName;

            try
            {
                if (!CheckPassedParameters())
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR TRANSFORMING DATA: Not all required parameters were passed in!";
                    return result;
                }

                TransformData();

                // Confirm by testing if the new table exists within the environment
                if (!clsGenericRCalls.ContainsObject(s_RInstance, dsp.NewTableName))
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR TRANSFORMING: After TRANSFORMING " +
                        dsp.InputTableName + 
                        ", the new table name could not be found within the R workspace";
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }

                System.Data.DataTable dt = clsGenericRCalls.GetDataTable(s_RInstance, dsp.NewTableName);
                if (dt.Columns.Count != 40)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR TRANSFORMING DATA: After transforming the table, " +
                        "the new table was supposed to have 40 columns, and instead has " +
                        dt.Columns.Count;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
                if (dt.Rows.Count != 94)
                {
                    result.IsSuccessful = false;
                    result.Message = "ERROR TRANSFORMING DATA: After transforming the table, " +
                        "the new table was supposed to have 94 rows, and instead has " +
                        dt.Rows.Count;
                    result.R_Statement = s_Current_R_Statement;
                    return result;
                }
            }
            catch (Exception exc)
            {
                result.IsSuccessful = false;
                result.Message = "ERROR IMPORTING COLUMN METADATATABE FROM SQLITE: " + dsp.InputFileName + "\n\n" + exc.ToString();
                result.R_Statement = s_Current_R_Statement;
            }

            return result;
        }
        #endregion
    }
}
