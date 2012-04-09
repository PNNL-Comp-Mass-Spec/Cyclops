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
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

using log4net;
using RDotNet;

namespace Cyclops.DataModules
{
    public class clsIDPicker : clsBaseDataModule
    {
        #region Members
        private string s_RInstance, s_Current_R_Statement = "", s_DbName = "",
            s_Directory = "", s_VersionIDPicker = "", s_FastaFileName = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        private List<string> l_IDPickerCommands = new List<string>();
        private string s_FastaFilePath = ""; // The full path to the fasta file.
        #endregion

        #region Constructors
        /// <summary>
        /// Module that runs IDPicker
        /// </summary>
        public clsIDPicker()
        {
            ModuleName = "IDPicker Module";
        }
        /// <summary>
        /// Module that runs IDPicker
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsIDPicker(string InstanceOfR)
        {
            ModuleName = "IDPicker Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Module that runs IDPicker
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsIDPicker(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "IDPicker Module";
            Model = TheCyclopsModel;
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        /// STEP (remember to error check between steps):
        /// 1. Build the IDPicker Directory in the working directory
        /// 2. Unzip the contents from the IDPicker.zip folder (on gigasax),
        ///    to the IDPicker Directory
        /// 3. Copy the Database fasta file from C:/DMS_OrgDB to the IDPicker directory
        /// 4. Copy the .pepXML file over to IDPicker Directory
        /// 5. Build the Assembly File
        /// 6. Run IDPicker

        public override void PerformOperation()
        {
            traceLog.Info("Performing IDPicker Analysis...");

            dsp.GetParameters(ModuleName, Parameters);

            if (dsp.RunAnalysis)
            {
                traceLog.Info("IDPicker: Run analysis requested and proceeding...");
                RunIDPicker();
            }
            else
            {
                traceLog.Info("IDPicker: Run analysis was NOT REQUESTED and will not be performed.");
            }
        }

        private void RunIDPicker()
        {
            s_Directory = dsp.WorkDirectory + "/IDPickerAnalysis";
            bool b_ContinueRunning = true;
        }

        /// <summary>
        /// Checks if the IDPicker directory already exists in the working
        /// directory. If so, it deletes all files in the directory, otherwise
        /// it creates the directory
        /// </summary>
        /// <returns></returns>
        private bool CreateIDPickerDirectory()
        {
            bool b_Return = false;

            if (!Directory.Exists(s_Directory))
            {
                Directory.CreateDirectory(s_Directory);
                traceLog.Info("IDPicker: Temporary directory created.");
                b_Return = true;
            }
            else
            {
                // delete any files in the directory
                string[] s_Files = Directory.GetFiles(s_Directory);
                foreach (string s in s_Files)
                {
                    File.Delete(s);
                }
                traceLog.Info("IDPicker: Existing IDPicker directory has been cleared for use.");
                return true;
            }

            return b_Return;
        }

        /// <summary>
        /// Extracts the contents from IDPickerSoftware.zip (the path to this file
        /// is supplied by dsp), into the ProteinProphet directory
        /// </summary>
        /// <returns></returns>
        private bool ExtractIDPickerSoftware()
        {
            bool b_Return = false;

            clsZipCompression.ExtractZipFile(dsp.FilePath, dsp.Password, s_Directory);

            // check that the major files were extracted
            // there are also dlls and manifest files that are required,
            // but we don't check every single file
            string[] s_Files2Check = new string[] {
                Path.Combine(s_Directory, "idpQonvert.exe"),
                Path.Combine(s_Directory, "idpReport.exe"),
                Path.Combine(s_Directory, "idpAssemble.exe"),
                Path.Combine(s_Directory, "SoftwareVersion.txt")
            };

            foreach (string s in s_Files2Check)
            {
                if (!File.Exists(s))
                {
                    Model.SuccessRunningPipeline = false;
                    traceLog.Error("ERROR IDPicker class: " +
                        s + " was NOT extracted.");
                    b_Return = false;
                }
            }

            return b_Return;
        }

        /// <summary>
        /// Reads in SoftwareVersion.txt file from the files extracted from
        /// zip archive. Authenticates software name and version. Logs Version
        /// identifier.
        /// </summary>
        /// <returns></returns>
        public bool ReadSoftwareVersion()
        {
            bool b_Return = false;

            try
            {
                StreamReader sr = new StreamReader(
                    Path.Combine(s_Directory, "SoftwareVersion.txt"));

                string s_Line = sr.ReadLine(); // get the name of the software
                if (s_Line.Equals("IDPicker"))
                {
                    // Get the version id
                    s_Line = sr.ReadLine();
                    if (s_Line.StartsWith("Version: "))
                    {
                        s_VersionIDPicker = s_Line.Substring(9);
                        traceLog.Info("Running IDPicker version: " +
                            s_VersionIDPicker);
                        b_Return = true;
                    }
                    else
                    {
                        traceLog.Error("ERROR IDPicker: Failed to " +
                            "authenticate software version: " +
                            s_Line);
                    }
                }
                else
                {
                    traceLog.Error("ERROR IDPicker reading " +
                        "SoftwareVersion.txt, failed to authenticate " +
                        "name of software. Name given: " + s_Line);
                }

                sr.Close();
            }
            catch (IOException ioe)
            {
                traceLog.Error("ERROR IDPicker attempting to read " +
                    "SoftwareVersion.txt file: " + ioe.ToString());
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR IDPicker attempting to read " +
                    "SoftwareVersion.txt file: " + exc.ToString());
            }

            return b_Return;
        }

        /// <summary>
        /// Copies the database fasta file used in the search over to ProteinProphet
        /// directory
        /// </summary>
        /// <returns></returns>
        private bool CopyDatabaseOver()
        {
            bool b_Return = false;
            string[] s_FastaFiles = Directory.GetFiles(dsp.DatabasePath,
                "*.fasta");

            string s_Destination = "";

            try
            {
                // Copy the files over from local directory
                foreach (string s in s_FastaFiles)
                {
                    s_DbName = Path.Combine(s_Directory,
                        Path.GetFileName(s));
                    File.Copy(s, s_DbName);
                    s_FastaFileName = Path.GetFileName(s);
                }

                // Now clean out the local directory
                string[] s_Files2Delete = Directory.GetFiles(dsp.DatabasePath);
                foreach (string s in s_Files2Delete)
                {
                    File.Delete(s);
                }

                // test if the function completed successfully
                if (File.Exists(s_DbName))
                {
                    b_Return = true;
                }
                else
                {
                    traceLog.Error("IDPicker: ERROR, Fasta file was not copied: " +
                        s_Destination);
                }
            }
            catch (IOException ioe)
            {
                traceLog.Error("IDPicker: IOError copying fasta file over to temporary directory: " +
                    ioe.ToString());
            }
            catch (Exception exc)
            {
                traceLog.Error("IDPicker: Error copying fasta file over to temporary directory: " +
                    exc.ToString());
            }

            if (b_Return)
                traceLog.Info("ProteinProphet: Fasta file has been copied over to temporary directory.");

            return b_Return;
        }

        /// <summary>
        /// Copies over the pepXML file. 
        /// This function is still under development.
        /// </summary>
        /// <returns></returns>
        private bool CopyOverPepXMLFile()
        {
            bool b_Return = false;

            return b_Return;
        }

        private bool BuildTheAssemblyFile()
        {
            bool b_Return = false;

            try
            {
                StreamWriter sw = new StreamWriter(
                    Path.Combine(s_Directory, "Assembly.txt"));

                string[] s_PepXMLFiles = Directory.GetFiles(s_Directory,
                    "*.pepXML");

                int i_Log = Convert.ToInt16(Math.Ceiling(Math.Log10(
                    Convert.ToDouble(s_PepXMLFiles.Length))));

                string s_IntFormat = "0:";
                for (int i = 0; i < i_Log; i++)
                {
                    s_IntFormat += "0";
                }

                int i_Counter = 0;
                foreach (string s in s_PepXMLFiles)
                {
                    sw.WriteLine(
                        string.Format("PNNL/ds{" + s_IntFormat + "}\t" +
                        Path.GetFileNameWithoutExtension(s) + ".idpXML",
                        i_Counter++));
                }

                sw.Close();
            }
            catch (IOException ioe)
            {
                traceLog.Error("ERROR IDPicker writing out assembly file: " +
                    ioe.ToString());
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR IDPicker writing out assembly file: " +
                    exc.ToString());
            }

            return b_Return;
        }

        private bool BuildTheCrunchFile()
        {
            bool b_Return = false;

            string s_OutputFileName = Path.Combine(s_Directory,
                "crunch-one.bat");

            string s_Convert2PepXML = string.Format("PeptidesListToPepXML.exe {0}",
                dsp.InputFileName);
            l_IDPickerCommands.Add(s_Convert2PepXML);

            // construct the idpqonvert statement
            string s_Qonvert = string.Format("idpqonvert {0}" +
                "-ProteinDatabase {1} {2}{3}{4}{5}{6}",
                !string.IsNullOrEmpty(dsp.MaxFDR) ? "-MaxFDR " + dsp.MaxFDR + " " : "",
                s_FastaFileName,
                !string.IsNullOrEmpty(dsp.SearchScoreWeights) ? "-SearchScoreWeights \"" + dsp.SearchScoreWeights + "\" " : "",
                !string.IsNullOrEmpty(dsp.OptimizeScoreWeights) ? "-OptimizeScoreWeights " + dsp.OptimizeScoreWeights + " " : "",
                !string.IsNullOrEmpty(dsp.NormalizedSearchScores) ? "-NormalizedSearchScores \"" + dsp.NormalizedSearchScores + "\" " : "",
                !string.IsNullOrEmpty(dsp.DecoyPrefix) ? "-DecoyPrefix \"" + dsp.DecoyPrefix + "\" " : "",
                "-dump *.pepXML");
            l_IDPickerCommands.Add(s_Qonvert);

            string s_AssembleCommand = string.Format(
                "idpassembl Assemble.xml {0}-Assembl.txt",
                !string.IsNullOrEmpty(dsp.MaxFDR) ? "-MaxFDR " + dsp.MaxFDR + " " : ""
                );
            l_IDPickerCommands.Add(s_AssembleCommand);

            string s_ReportCommand = string.Format(
                "ipdreport report Assemble.xml " +
                "{0}{1}{2}{3}{4}{5}{6}{7}{8}",
                !string.IsNullOrEmpty(dsp.MaxFDR) ? "-MaxFDR " + dsp.MaxFDR + " " : "",
                !string.IsNullOrEmpty(dsp.MinDistinctPeptides) ? "-MinDistinctPeptides " + dsp.MinDistinctPeptides + " " : "",
                !string.IsNullOrEmpty(dsp.MinAdditionalPeptides) ? "-MinAdditionalPeptides " + dsp.MinAdditionalPeptides + " " : "",
                !string.IsNullOrEmpty(dsp.OutputTextReport) ? "-OutputTextReport " + dsp.OutputTextReport + " " : "",
                !string.IsNullOrEmpty(dsp.ModsAreDistinctByDefault) ? "-ModsAreDistinctByDefault " + dsp.ModsAreDistinctByDefault + " " : "",
                !string.IsNullOrEmpty(dsp.MaxAmbiguousIds) ? "-MaxAmbiguousIds " + dsp.MaxAmbiguousIds + " " : "",
                !string.IsNullOrEmpty(dsp.MinSpectraPerProtein) ? "-MinSpectraPerProtein " + dsp.MinSpectraPerProtein + " " : "",
                !string.IsNullOrEmpty(dsp.QuantitativeMethod) ? "-QuantitationMethod " + dsp.QuantitativeMethod + " " : "",
                !string.IsNullOrEmpty(dsp.RawSourcePath) ? "-RawSourcePath " + dsp.RawSourcePath + " " : ""
                );
            l_IDPickerCommands.Add(s_ReportCommand);

            // construct the assembly statement

            try
            {
                StreamWriter sw = new StreamWriter(s_OutputFileName);

                foreach (string s in l_IDPickerCommands)
                {
                    sw.WriteLine(s);
                    traceLog.Info("IDPicker Executing: " + s);
                    //ExecuteCommandSync(s);
                }

                sw.Close();

                FileInfo fi = new FileInfo(s_OutputFileName);
                if (File.Exists(s_OutputFileName) &
                    fi.Length > 0)
                {
                    b_Return = true;
                }
            }
            catch (IOException ioe)
            {
                traceLog.Error("ERROR IDPicker writing out crunch-one file: " +
                    ioe.ToString());
            }
            catch (Exception exc)
            {
                traceLog.Error("ERROR IDPicker writing out crunch-one file: " +
                    exc.ToString());
            }

            return b_Return;
        }

        /// <summary>
        /// Runs the bat one file.
        /// </summary>
        /// <returns></returns>
        private bool RunProteinProphetThruCrunchOneFile()
        {
            bool b_Return = false;
            string s_ErrorInfo, s_OutputInfo;

            string s_BatFile = s_Directory + "/crunch-one.bat",
                s_ErrorFile = s_Directory + "/errorFile.txt",
                s_OutputInfoFile = s_Directory + "/outputInfoFile.txt";
            if (File.Exists(s_BatFile))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = s_BatFile;
                proc.StartInfo.WorkingDirectory = s_Directory;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;

                proc.Start();
                proc.WaitForExit();
                s_ErrorInfo = proc.StandardError.ReadToEnd();
                proc.WaitForExit();
                s_OutputInfo = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                StreamWriter sw_Error = new StreamWriter(s_ErrorFile);
                sw_Error.WriteLine(s_ErrorInfo);
                sw_Error.Close();
                StreamWriter sw_Output = new StreamWriter(s_OutputInfoFile);
                sw_Output.WriteLine(s_OutputInfo);
                sw_Output.Close();
            }
            return b_Return;
        }

        /// <summary>
        /// Executes a shell command synchronously.
        /// </summary>
        /// <param name="command">string command</param>
        /// <returns>string, as output of the command.</returns>
        private void ExecuteCommandSync(object command)
        {
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);
                traceLog.Info("IDPICKER EXECUTING: " + command);

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                // Display the command output.
                Console.WriteLine(result);
            }
            catch (Exception objException)
            {
                // Log the exception
                traceLog.Error("ERROR IN IDPICKER: " + objException.ToString());
            }
        }
        #endregion
    }
}
