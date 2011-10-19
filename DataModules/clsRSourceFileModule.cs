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
        /// <summary>
        ///  Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            // Load ALL the files in the directory
            if (!Parameters.ContainsKey("rSourceCodeDirectory"))
            {
                if (Parameters.ContainsKey("workDir"))
                {
                    GetDirectoriesAndLoadRSourceFiles(
                        Path.Combine(Parameters["workDir"].ToString(), "R_Scripts"),
                        s_RInstance);                        
                }
                else
                {
                    GetDirectoriesAndLoadRSourceFiles("R_Scripts", s_RInstance);
                }
            }
            else // Load the specified files
            {
                string[] s_Files = Parameters["rSourceCodeDirectory"].ToString().Split(';');
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

        /// <summary>
        /// Iterative function that parses through directories, runs all R source files in the 
        /// directory, then iterates through the subdirectories
        /// </summary>
        /// <param name="MyDirectory">Path to Directory</param>
        /// <param name="RInstance">Instance of R Workspace</param>
        protected void GetDirectoriesAndLoadRSourceFiles(string MyDirectory, string RInstance)
        {
            foreach (string s in Directory.GetFiles(MyDirectory))
            {
                if (Path.GetExtension(s).Equals(".R"))
                    LoadRSourceFile(s, RInstance);
            }
            foreach (string s in Directory.GetDirectories(MyDirectory))
            {
                GetDirectoriesAndLoadRSourceFiles(s, RInstance);
            }
        }

        /// <summary>
        /// Loads a R source file (*.R) into the workspace environment
        /// </summary>
        /// <param name="MyFile">Name of the source file to load</param>
        /// <param name="RInstance">Instance of R Workspace</param>
        protected void LoadRSourceFile(string MyFile, string RInstance)
        {
            REngine engine = REngine.GetInstanceFromID(s_RInstance);

            try
            {
                /// Visual Studios adds a 3 character format to the
                /// beginning of each ".R" file, so just need to
                /// clean it up before reading it.
                if (Parameters.ContainsKey("removeFirstCharacters"))
                {
                    if (Parameters["removeFirstCharacters"].ToString().Equals("true"))
                    {
                        StreamReader sr = new StreamReader(MyFile);
                        string s_Content = sr.ReadToEnd();
                        sr.Close();
                        //s_Content = s_Content.Remove(0, 2);
                        s_Content.Replace("ï»¿", "");
                        StreamWriter sw = new StreamWriter(MyFile);
                        sw.Write(s_Content);
                        sw.Close();
                    }
                }

                string s_Command = string.Format("source(\"{0}\")",
                    MyFile.Replace("\\", "/"));
                engine.EagerEvaluate(s_Command);
            }
            catch (ParseException pe)
            {
                Console.WriteLine("Encountered a Parse Excpetion: " + pe.ToString());
            }
        }
        #endregion
    }
}
