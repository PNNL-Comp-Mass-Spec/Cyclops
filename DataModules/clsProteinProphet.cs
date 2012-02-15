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
using System.IO;
using System.Data;
using System.Data.SQLite;

using log4net;
using RDotNet;

namespace Cyclops.DataModules
{
    public class clsProteinProphet : clsBaseDataModule
    {
        private string s_RInstance, s_Current_R_Statement = "", s_DbName = "",
            s_Directory = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        #region Constructors
        /// <summary>
        /// Module that run ProteinProphet
        /// </summary>
        public clsProteinProphet()
        {
            ModuleName = "ProteinProphet Module";
        }
        /// <summary>
        /// Module that run ProteinProphet
        /// </summary>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsProteinProphet(string InstanceOfR)
        {
            ModuleName = "ProteinProphet Module";
            s_RInstance = InstanceOfR;
        }
        /// <summary>
        /// Module that run ProteinProphet
        /// </summary>
        /// <param name="TheCyclopsModel">Instance of the CyclopsModel to report to</param>
        /// <param name="InstanceOfR">Instance of R workspace to call</param>
        public clsProteinProphet(clsCyclopsModel TheCyclopsModel, string InstanceOfR)
        {
            ModuleName = "ProteinProphet Module";
            Model = TheCyclopsModel;
            s_RInstance = InstanceOfR;
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        /// STEPS (remember to error check between steps):
        /// 1. Build the ProteinProphet Directory in the working directory
        /// 2. Unzip the contents from the ProteinProphet.zip folder (on gigasax),
        ///    to the ProteinProphet Directory
        /// 3. Copy the Database fasta file from gigasax to the ProteinProphet directory
        /// 4. Create the database.dat file in the ProteinProphet directory according
        ///    to the name of the fasta file
        /// 5. Grab the table containing peptides, proteins, and extended peptide seq
        /// 6. Format the table according to ProteinProphet standards
        /// 7. Check the table to for potential formatting errors
        /// 8. Save the table out as tab-delimited html file to the ProteinProphet directory
        /// 9. Construct the crunch.bat file to run the program
        /// 10. Run ProteinProphet
        

        /// <summary>
        /// Runs module and then child modules
        /// </summary>
        public override void PerformOperation()
        {
            traceLog.Info("Peforming ProteinProphet Analysis...");

            dsp.GetParameters(ModuleName, Parameters);
            s_Directory = dsp.WorkDirectory + "/ProteinProphet";

            bool b_ContinueRunning = true;

            b_ContinueRunning = CreateProteinProphetDirectory(); // Step 1
            if (!b_ContinueRunning) 
            {
                Model.SuccessRunningPipeline = false;
                // handle problem creating the ProteinProphet directory
                traceLog.Error("ERROR ProteinProphet class: " +
                    "unable to create the ProteinProphet folder " +
                    "in the working directory");
            }

            b_ContinueRunning = ExtractProteinProphetSoftware(); // Step 2
            if (!b_ContinueRunning) 
            {
                Model.SuccessRunningPipeline = false;
                // handle problem extracting the software
                traceLog.Error("ERROR ProteinProphet class: " +
                    "unable to extract all files in ProteinProphet software " +
                    "to the working directory");
            }

            b_ContinueRunning = CopyDatabaseOver(); // Step 3
            if (!b_ContinueRunning) 
            {
                Model.SuccessRunningPipeline = false;
                // handle problem copying the database over to working directory
                traceLog.Error("ERROR ProteinProphet class: " +
                    "database file was not copied over to working directory.\n" +
                    "Attempted to copy: " + s_DbName + "\n" +
                    "To: ProteinProphet/" + Path.GetFileName(s_DbName));
            }

            b_ContinueRunning = CreateDatabaseDatFile(); // Step 4
            if (!b_ContinueRunning) 
            {
                Model.SuccessRunningPipeline = false;
                // handle problem creating the database.dat file
                traceLog.Error("ERROR ProteinProphet class: " +
                    "database.dat file was not created properly.");
            }

            b_ContinueRunning = ConstructProteinProphetInputFile(); // Step 5 - 8
            if (!b_ContinueRunning) 
            {
                Model.SuccessRunningPipeline = false;
                // handle problem creating the database.dat file
                traceLog.Error("ERROR ProteinProphet class: " +
                    "error processing table from database.");
            }

            b_ContinueRunning = BuildTheCrunchFile(); // Step 9
            if (!b_ContinueRunning)
            {
                Model.SuccessRunningPipeline = false;
                // handle problem creating the database.dat file
                traceLog.Error("ERROR ProteinProphet class: " +
                    "error building and saving the crunch-one bat file.");
            }

            b_ContinueRunning = RunProteinProphetThruCrunchOneFile();

            RunChildModules();
        }
        
        /// <summary>
        /// Checks if the ProteinProphet directory already exists in the working
        /// directory. If so, it deletes all files in the directory, otherwise
        /// it creates the directory
        /// </summary>
        /// <returns></returns>
        private bool CreateProteinProphetDirectory()
        {
            bool b_Return = false;

            if (!Directory.Exists(s_Directory))
            {
                Directory.CreateDirectory(s_Directory);
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
                return true;
            }

            return b_Return;
        }

        /// <summary>
        /// Extracts the contents from ProteinProphet.zip (the path to this file
        /// is supplied by dsp), into the ProteinProphet directory
        /// </summary>
        /// <returns></returns>
        private bool ExtractProteinProphetSoftware()
        {
            bool b_Return = true;
            clsZipCompression.ExtractZipFile(dsp.FilePath, dsp.Password, s_Directory);

            // check that the files were extracted
            string[] s_Files2Check = new string[] {
                s_Directory + "/html_to_txt.pl",
                s_Directory + "/makedgn.exe",
                s_Directory + "/proph_to_access.exe",
                s_Directory + "/proph_to_access.pl",
                s_Directory + "/ProProphet.pl",
                s_Directory + "/PROPT.BAT",
                s_Directory + "/ProteinProphet_v1.7.dtd",
                s_Directory + "/txt_to_ppr.pl"
            };

            // test if the function completed successfully
            foreach (string s in s_Files2Check)
            {
                if (!File.Exists(s))
                {
                    Model.SuccessRunningPipeline = false;
                    traceLog.Error("ERROR ProteinProphet class: " +
                        s + " was NOT extracted.");
                    b_Return = false;
                }
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
            s_DbName = dsp.DatabasePath;
            string s_Destination = s_Directory + "/" + Path.GetFileName(s_DbName);

            File.Copy(s_DbName, s_Destination);

            // test if the function completed successfully
            if (File.Exists(s_Destination))
            {
                b_Return = true;
            }
            return b_Return;
        }

        /// <summary>
        /// Creates the database.dat file in the ProteinProphetDirectory
        /// </summary>
        /// <returns></returns>
        private bool CreateDatabaseDatFile()
        {
            bool b_Return = false;
            string s_DbFileName = s_Directory + "/database.dat";
            StreamWriter sw = new StreamWriter(s_DbFileName);
            sw.Write(Path.GetFileName(s_DbName));
            sw.Close();

            // test if the function completed successfully
            if (File.Exists(s_DbFileName))
            {
                b_Return = true;
            }
            return b_Return;
        }

        /// <summary>
        /// Builds the tab-delimited html file from the table of peptides/proteins in
        /// the SQLite database, formats the table to ProteinProphet standards, and
        /// saves it to the working directory.
        /// </summary>
        /// <returns></returns>
        private bool ConstructProteinProphetInputFile()
        {
            bool b_Return = false;
            string s_Command = "SELECT * FROM " +
                dsp.InputTableName;
            //string s_Connection = "Data Source=" + dsp.WorkDirectory + Path.DirectorySeparatorChar +
            //    dsp.InputFileName + ";Version=3;";
            //string s_Connection = @"Data Source=C:\DMS_WorkDir\Test\ResultsDatabase.db3; Version=3;";
            string s_Connection = dsp.WorkDirectory + Path.DirectorySeparatorChar +
                dsp.InputFileName;

            traceLog.Info("ProteinProphet class, COMMAND: " + s_Command);
            traceLog.Info("ProteinProphet class, CONNECTION: " + s_Connection);
            //if (File.Exists(@"C:\DMS_WorkDir\Test\ResultsDatabase.db3"))
            //    Console.WriteLine("The FILE EXISTS " + s_Connection);
            //else
            //    Console.WriteLine("File DOES NOT EXIST");

            //SQLiteConnection conn = new SQLiteConnection(s_Connection);
            //try
            //{
            //    conn.Open();
            //    conn.Close();
            //    Console.WriteLine("The database opened!");
            //}
            //catch (Exception exc)
            //{
            //    Console.WriteLine("Error: " + exc.ToString());
            //}

            //Console.ReadKey();

            DataTable dt_Tmp = clsSQLiteHandler.GetDataTable(s_Command, s_Connection);
            string s_OutputFile = s_Directory + "/ProteinProphetInputFile.html";
            // write out the input file
            StreamWriter sw = new StreamWriter(s_OutputFile);
            for (int i = 0; i < dt_Tmp.Rows.Count; i++)
            {
                int j = i + 1;
                sw.WriteLine(j + "\tfile" + i + "\t" + j + "\t" +
                    dt_Tmp.Rows[i]["Peptide"].ToString() + "\t2\t1\t1\t" +
                    dt_Tmp.Rows[i]["Protein"].ToString() + "\t" +
                    dt_Tmp.Rows[i]["PeptideEx"].ToString() + "\t" +
                    "1\t1\t1\t1\t1\t1\t1\t1");
            }
            sw.Close();
            
            FileInfo fi = new FileInfo(s_OutputFile);
            if (File.Exists(s_OutputFile) &
                fi.Length > 0)
            {
                b_Return = true;
            }

            return b_Return;
        }

        /// <summary>
        /// Builds the crunch-one.bat file for running ProteinProphet
        /// </summary>
        /// <returns></returns>
        private bool BuildTheCrunchFile()
        {
            bool b_Return = false;

            string s_OutputFile = s_Directory + "/crunch-one.bat";
            StreamWriter sw = new StreamWriter(s_OutputFile);
            //sw.WriteLine("cd " + s_Directory);
            sw.WriteLine("set FNAME=ProteinProphetInputFile");
            sw.WriteLine("set DATABASE=" + Path.GetFileName(s_DbName));
            sw.WriteLine(
                "txt_to_ppr.pl %FNAME%.html\n" +
                "makedgn *tmp2 %DATABASE%\n" +
                "ProProphet.pl *%FNAME%*.html %FNAME%-outputfile HTML\n" +
                "html_to_txt.pl %FNAME%-outputfile.htm %FNAME%-outputfile.txt\n" +
                "proph_to_access.pl %FNAME%-outputfile.txt");
            sw.Close();

            FileInfo fi = new FileInfo(s_OutputFile);
            if (File.Exists(s_OutputFile) &
                fi.Length > 0)
            {
                b_Return = true;
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
        #endregion
    }
}
