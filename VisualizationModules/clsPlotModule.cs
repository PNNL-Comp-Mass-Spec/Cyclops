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
using System.IO;
using System.Threading;

using RDotNet;

namespace Cyclops
{
    public class clsPlotModule : clsBaseVisualizationModule
    {
        public enum PlotType { Histogram, ScatterPlot, BoxPlot, CorrelationHeatmap, Heatmap };
        private int plotType;

        private string s_RInstance;

        #region Constructors
        public clsPlotModule()
        {
            ModuleName = "Plot Module";
        }
        public clsPlotModule(string InstanceOfR)
        {
            ModuleName = "Plot Module";
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Members

        #endregion

        #region Properties

        #endregion

        #region Functions
        /// <summary>
        /// Sets the DataType that the object is going to pull the data from
        /// </summary>
        /// <param name="DataType">ImportDataType</param>
        public void SetDataType(PlotType ThePlotType)
        {
            plotType = (int)ThePlotType;
            switch(plotType)
            {
                case (int)PlotType.BoxPlot:
                    break;
                case (int)PlotType.CorrelationHeatmap:
                    break;
                case (int)PlotType.Heatmap:
                    break;
                case (int)PlotType.Histogram:
                    break;
                case (int)PlotType.ScatterPlot:
                    break;
            }
        }

        /// <summary>
        ///  Runs module
        /// </summary>
        public override void PerformOperation()
        {
            CreatePlotsFolder();


        }

        /// <summary>
        /// Creates a directory to store image files
        /// </summary>
        public void CreatePlotsFolder()
        {
            if (Parameters.ContainsKey("createPlotsFolder"))
            {
                string s_Create = Parameters["createPlotsFolder"].ToString();
                if (s_Create.Equals("true"))
                {
                    if (Parameters.ContainsKey("workDir"))
                    {
                        string s_WorkDir = Parameters["workDir"].ToString();
                        s_WorkDir += "/Plots";
                        s_WorkDir = s_WorkDir.Replace('\\', '/');
                        if (!Directory.Exists(s_WorkDir))
                        {
                            Directory.CreateDirectory(s_WorkDir);
                        }
                    }
                    else
                    {
                        if (!Directory.Exists("Plots"))
                            Directory.CreateDirectory("Plots");
                    }
                }
            }
        }
        #endregion
    }
}
