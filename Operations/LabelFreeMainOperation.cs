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

namespace Cyclops.Operations
{
	public class LabelFreeMainOperation : BaseOperationModule
	{
		#region Enums
		/// <summary>
		/// Log2: Simple Log2 transformation, RRollup
		/// Log2LR: Log2 transformation, Linear Regression, RRollup
		/// Log2CT: Log2 transformation, Central Tendency, RRollup
		/// </summary>
		public enum LbfTypes { Log2, Log2LR, Log2CT, Log2All, AnovaPractice, MainAnovaPractice, HtmlPractice };

		/// <summary>
		/// Required parameters to run SpectralCountMainOperation Module
		/// </summary>
		private enum RequiredParameters
		{
			Type
		}
		#endregion

		#region Members

		private string m_LabelFreeTableName = "T_LabelFreeLog2PipelineOperation";

		private const string m_ModuleName = "LabelFreeMainOperation";

		private Dictionary<LbfTypes, string> m_LabelFreeTableNames;

		#endregion

		#region Properties

		#endregion

		#region Constructors
		public LabelFreeMainOperation()
		{
			ModuleName = m_ModuleName;
			Initialize();
		}

		public LabelFreeMainOperation(CyclopsModel CyclopsModel)
		{
			ModuleName = m_ModuleName;
			Model = CyclopsModel;
			Initialize();
		}

		public LabelFreeMainOperation(CyclopsModel CyclopsModel,
			Dictionary<string, string> OperationParameters)
		{
			ModuleName = m_ModuleName;
			Model = CyclopsModel;
			Parameters = OperationParameters;
			Initialize();
		}
		#endregion

		#region Methods

		private void Initialize()
		{
			m_LabelFreeTableNames = new Dictionary<LbfTypes, string>
			{
				{LbfTypes.Log2,              "T_LabelFreeLog2PipelineOperation"},
				{LbfTypes.Log2LR,            "T_LabelFreeLog2_LR_PipelineOperation"},
				{LbfTypes.Log2CT,            "T_LabelFreeLog2_CT_PipelineOperation"},
				{LbfTypes.Log2All,           "T_LabelFreeLog2_All_PipelineOperation"},
				{LbfTypes.AnovaPractice,     "T_LabelFree_AnovaPractice"},
				{LbfTypes.MainAnovaPractice, "T_LabelFree_MainAnovaPractice"},
				{LbfTypes.HtmlPractice,      "T_LabelFree_HtmlPractice"}
			};
		}

		/// <summary>
		/// Runs module and then child modules
		/// </summary>
		public override bool PerformOperation()
		{
			bool b_Successful = true;

			if (Model.PipelineCurrentlySuccessful)
			{
				Model.CurrentStepNumber = StepNumber;

				Model.LogMessage("Running " + ModuleName,
						ModuleName, StepNumber);

				if (CheckParameters())
					b_Successful =
						LabelFreeMainOperationFunction();
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

			if (Parameters.ContainsKey("DatabaseFileName"))
			{
				OperationsDatabasePath = Parameters["DatabaseFileName"];
			}

			return b_Successful;
		}

		/// <summary>
		/// Main Method to run the Spectral Count Operation
		/// </summary>
		/// <returns>True, if the operation completes successfully</returns>
		public bool LabelFreeMainOperationFunction()
		{
			bool b_Successful = true;

			SetTypes();

			b_Successful = ConstructModules();

			return b_Successful;
		}

		/// <summary>
		/// Sets the type of Spectral Count Operation, and sets the SQLite table
		/// to use to run the operation.
		/// </summary>
		public void SetTypes()
		{
			switch (Parameters[RequiredParameters.Type.ToString()].ToLower())
			{
				case "log2":
					m_LabelFreeTableName =
						m_LabelFreeTableNames[LbfTypes.Log2];
					break;
				case "log2lr":
					m_LabelFreeTableName =
						m_LabelFreeTableNames[LbfTypes.Log2LR];
					break;
				case "log2ct":
					m_LabelFreeTableName =
						m_LabelFreeTableNames[LbfTypes.Log2CT];
					break;
				case "log2all":
					m_LabelFreeTableName =
						m_LabelFreeTableNames[LbfTypes.Log2All];
					break;
				case "htmlpractice":
					m_LabelFreeTableName =
						m_LabelFreeTableNames[LbfTypes.HtmlPractice];
					break;
				case "anovapractice":
					m_LabelFreeTableName =
						m_LabelFreeTableNames[LbfTypes.AnovaPractice];
					break;
				case "mainanovapractice":
					m_LabelFreeTableName =
						m_LabelFreeTableNames[LbfTypes.MainAnovaPractice];
					break;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool ConstructModules()
		{
			bool b_Successful = true;

			try
			{
				WorkflowHandler wfh = new WorkflowHandler(Model);
				wfh.InputWorkflowFileName = OperationsDatabasePath;
				wfh.WorkflowTableName = m_LabelFreeTableName;
				b_Successful = wfh.ReadSQLiteWorkflow();

				if (b_Successful)
					Model.ModuleLoader = wfh;
			}
			catch (Exception ex)
			{
				Model.LogError("Exception encounterd while running 'ConstructModules' " +
					"for the LabelFree Operation:\n" +
					ex.ToString(), ModuleName, StepNumber);
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
