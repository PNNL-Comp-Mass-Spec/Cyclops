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
using System.Text;
using System.IO;

using RDotNet;

namespace Cyclops
{
    public class clsRSourceFileModule : clsBaseDataModule
    {
        protected string s_RInstance;

        #region Constructors
        public clsRSourceFileModule()
        {
            ModuleName = "R Source File Module - Loading R Functions";
        }
        public clsRSourceFileModule(string InstanceOfR)
        {
            ModuleName = "R Source File Module - Loading R Functions";
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Functions
        public override void PerformOperation()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            // Load ALL the files in the directory
            if (Parameters.Count < 1)
            {
                string[] s_Files = Directory.GetFiles("R_Scripts");
                foreach (string s in s_Files)
                {
                    try
                    {
                        //StreamReader sr = new StreamReader(s);
                        //string s_Content = sr.ReadToEnd();
                        //sr.Close();
                        //s_Content = s_Content.Remove(0, 2);
                        //StreamWriter sw = new StreamWriter(s);
                        //sw.Write(s_Content);
                        //sw.Close();
                        string s_Command = string.Format("source(\"{0}/{1}\")",
                            Directory.GetCurrentDirectory().Replace('\\', '/'), s.Replace('\\', '/'));
                        engine.EagerEvaluate(s_Command);
                    }
                    catch (ParseException pe)
                    {
                        Console.WriteLine("Encountered a Parse Excpetion: " + pe.ToString());
                    }
                }
            }
            else // Load the specified files
            {
                string[] s_Files = Parameters["files"].ToString().Split(';');
                foreach (string s in s_Files)
                {
                    try
                    {
                        string s_Command = string.Format("source(\"{0}/{1}\")",
                            Directory.GetCurrentDirectory().Replace('\\', '/'), s.Replace('\\', '/'));
                        engine.EagerEvaluate(s_Command);
                    }
                    catch (ParseException pe)
                    {
                        Console.WriteLine("Encountered a Parse Excpetion: " + pe.ToString());
                    }
                }
            }
            //string s_Command = string.Format("source(\"{0}\")",
            //                    Parameters["PATH"].ToString().Replace('\\', '/'));
            //engine.EagerEvaluate(s_Command);

            RunChildModules();
        }
        #endregion
    }
}
