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
        #region Members
        private string s_RInstance, s_DbName = "",
            s_Directory = "";
        private DataModules.clsDataModuleParameterHandler dsp =
            new DataModules.clsDataModuleParameterHandler();
        private static ILog traceLog = LogManager.GetLogger("TraceLog");

        private List<string> l_ProteinProphetCommands = new List<string>();
        private string s_FastaFilePath = ""; // The full path to the fasta file.
        #endregion

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
            if (Model.SuccessRunningPipeline)
            {
                Model.IncrementStep(ModuleName);

                traceLog.Info("Peforming ProteinProphet Analysis...");

                dsp.GetParameters(ModuleName, Parameters);

                traceLog.Info("Fasta file to be used in analysis: " + dsp.DatabasePath);

                if (dsp.RunAnalysis)
                {
                    traceLog.Info("ProteinProphet: Run analysis requested and proceeding...");
                    RunProteinProphet();
                }
                else
                {
                    traceLog.Info("ProteinProphet: Run analysis was NOT REQUESTED and will not be performed.");
                }

                RunChildModules();
            }
        }

        private void RunProteinProphet()
        {

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

            string s_ProteinProphetTempDatabase = Path.Combine(dsp.WorkDirectory,
                "ProteinProphetTempDB.db3");
            CreateTempSQLiteDatabase(s_ProteinProphetTempDatabase);

            string s_PeptideResultsFileFromProteinProphet =
                Path.Combine(dsp.WorkDirectory, "ProteinProphet",
                "ProteinProphetInputFile-outputfile.txt.pep");
            string s_ProteinResultsFileFromProteinProphet =
                Path.Combine(dsp.WorkDirectory, "ProteinProphet",
                "ProteinProphetInputFile-outputfile.txt.pro");


            clsSQLiteHandler.WriteTabDelimitedTextFileToSQLiteTable(s_ProteinProphetTempDatabase,
                s_PeptideResultsFileFromProteinProphet, "T_Peptides");
            clsSQLiteHandler.WriteTabDelimitedTextFileToSQLiteTable(s_ProteinProphetTempDatabase,
                s_ProteinResultsFileFromProteinProphet, "T_Proteins");

            clsSQLiteHandler.CreateIndex(s_ProteinProphetTempDatabase,
                "T_Peptides", "ITEM", null);
            clsSQLiteHandler.CreateIndex(s_ProteinProphetTempDatabase,
                "T_Proteins", "ITEM", null);

            string s_QueryProteinProphetTempDatabase =
                "SELECT p.ITEM AS ProteinGroup, r.ORF AS Protein, p.PEPTIDE As Peptide " +
                "FROM T_Peptides p INNER JOIN T_Proteins r ON p.ITEM = r.ITEM " +
                "GROUP BY p.ITEM, r.ORF, p.PEPTIDE";

            DataTable dt_ProteinProphetResults = clsSQLiteHandler.GetDataTable(
                s_QueryProteinProphetTempDatabase, s_ProteinProphetTempDatabase);

            clsSQLiteHandler.WriteDataTableToSQLiteTable(
                Path.Combine(dsp.WorkDirectory, dsp.InputFileName), dt_ProteinProphetResults,
                "T_ProteinProphetResultsTable");

            clsSQLiteHandler.CreateIndex(Path.Combine(dsp.WorkDirectory, dsp.InputFileName),
                "T_ProteinProphetResultsTable", "Protein", null);
            clsSQLiteHandler.CreateIndex(Path.Combine(dsp.WorkDirectory, dsp.InputFileName),
                "T_ProteinProphetResultsTable", "Peptide", null);

            File.Move(s_PeptideResultsFileFromProteinProphet,
                Path.Combine(dsp.WorkDirectory, "ProteinProphetResults_Peptides.txt"));
            File.Move(s_ProteinResultsFileFromProteinProphet,
                Path.Combine(dsp.WorkDirectory, "ProteinProphetResults_Proteins.txt"));

            clsMiscFunctions.SaveDataTable(dt_ProteinProphetResults,
                Path.Combine(dsp.WorkDirectory, "T_ProteinProphetResults.txt"));

            string s_CreateProteinProphetPeptideCountTable =
                "CREATE TABLE T_ProteinProphetPeptideCount AS " +
                "SELECT ProteinGroup, COUNT(DISTINCT Peptide) AS PeptideCount " +
                "FROM T_ProteinProphetResultsTable GROUP BY ProteinGroup";

            clsSQLiteHandler.RunNonQuery(s_CreateProteinProphetPeptideCountTable,
                Path.Combine(dsp.WorkDirectory, dsp.InputFileName));

            RemoveTempSQLiteDatabase(s_ProteinProphetTempDatabase);
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
                traceLog.Info("ProteinProphet: Temporary directory created.");
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
                traceLog.Info("ProteinProphet: Existing ProteinProphet directory has been cleared for use.");
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
                //s_Directory + "/proph_to_access.exe",
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

            if (b_Return)
                traceLog.Info("ProteinProphet: Zip file has been extracted successfully.");
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
                    traceLog.Error("ProteinProphet: ERROR, Fasta file was not copied: " + 
                        s_Destination);
                }
            }
            catch (IOException ioe)
            {
                traceLog.Error("ProteinProphet: IOError copying fasta file over to temporary directory: " +
                    ioe.ToString());
            }
            catch (Exception exc)
            {
                traceLog.Error("ProteinProphet: Error copying fasta file over to temporary directory: " +
                    exc.ToString());
            }

            if (b_Return)
                traceLog.Info("ProteinProphet: Fasta file has been copied over to temporary directory.");
                
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
            try
            {
                StreamWriter sw = new StreamWriter(s_DbFileName);
                sw.Write(Path.GetFileName(s_DbName));
                sw.Close();

                // test if the function completed successfully
                if (File.Exists(s_DbFileName))
                {
                    b_Return = true;
                }
            }
            catch (IOException ioe)
            {
                traceLog.Error("ProteinProphet: IOERROR Creating database.dat file: " + ioe.ToString());
            }
            catch (Exception exc)
            {
                traceLog.Error("ProteinProphet: ERROR Creating database.dat file: " + exc.ToString());
            }
            if (b_Return)
                traceLog.Info("ProteinProphet: database.dat file created successfully.");
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
            string s_Connection = dsp.WorkDirectory + Path.DirectorySeparatorChar +
                dsp.InputFileName;

            traceLog.Info("ProteinProphet class, COMMAND: " + s_Command);
            traceLog.Info("ProteinProphet class, CONNECTION: " + s_Connection);

            DataTable dt_Tmp = clsSQLiteHandler.GetDataTable(s_Command, s_Connection);
            string s_OutputFile = s_Directory + "/ProteinProphetInputFile.html";

            try
            {
                // write out the input file
                StreamWriter sw = new StreamWriter(s_OutputFile);
                for (int i = 0; i < dt_Tmp.Rows.Count; i++)
                {
                    string pep = dt_Tmp.Rows[i]["Peptide"].ToString();
                    pep = pep.Replace("*", "");
                    pep = pep.Replace("#", "");
                    string prot = dt_Tmp.Rows[i]["Protein"].ToString();
                    string pepex = dt_Tmp.Rows[i]["PeptideEx"].ToString();
                    pepex = pepex.Replace("*", "");
                    pepex = pepex.Replace("#", "");

                    int j = i + 1;
                    sw.WriteLine(j + "\tfile0\t" + j + "\t" +
                        pep + "\t2\t1\t1\t" +
                        prot + "\t" +
                        pepex + "\t" +
                        "1\t1\t1\t1\t1\t1\t1\t1");
                }
                sw.Close();
            }
            catch (IOException ioe)
            {
                traceLog.Error("ProteinProphet: IOERROR writing out input file: " +
                    ioe.ToString());
            }
            catch (Exception exc)
            {
                traceLog.Error("ProteinProphet: ERROR writing out input file: " +
                    exc.ToString());
            }
            
            FileInfo fi = new FileInfo(s_OutputFile);
            if (File.Exists(s_OutputFile) &
                fi.Length > 0)
            {
                traceLog.Info("ProteinProphet: Input file created successfully!");
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

            //l_ProteinProphetCommands.Add("set FNAME=ProteinProphetInputFile");
            //l_ProteinProphetCommands.Add("set DATABASE=" + Path.GetFileName(s_DbName));
            l_ProteinProphetCommands.Add("txt_to_ppr.pl ProteinProphetInputFile.html");
            l_ProteinProphetCommands.Add(string.Format("makedgn *tmp2 {0}", Path.GetFileName(s_DbName)));
            l_ProteinProphetCommands.Add("ProProphet.pl *ProteinProphetInputFile*.html ProteinProphetInputFile-outputfile HTML");
            l_ProteinProphetCommands.Add("html_to_txt.pl ProteinProphetInputFile-outputfile.htm ProteinProphetInputFile-outputfile.txt");
            l_ProteinProphetCommands.Add("proph_to_access.pl ProteinProphetInputFile-outputfile.txt");

            foreach (string s in l_ProteinProphetCommands)
            {
                sw.WriteLine(s);
                traceLog.Info("PROTEIN PROPHET EXECUTING: " + s);
                //ExecuteCommandSync(s);
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
                traceLog.Info("PROTEIN PROPHET EXECUTING: " + command);

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
                traceLog.Error("ERROR IN PROTEINPROPHET: " + objException.ToString());
            }
        }

        private bool CreateTempSQLiteDatabase(string FileName)
        {
            bool b_FileExists = false;
            SQLiteConnection.CreateFile(FileName);
            if (File.Exists(FileName))
                b_FileExists = true;
            return b_FileExists;
        }

        private bool RemoveTempSQLiteDatabase(string FileName)
        {
            bool b_FileExists = File.Exists(FileName);

            if (b_FileExists)
                File.Delete(FileName);

            b_FileExists = File.Exists(FileName);

            return b_FileExists;
        }
        #endregion
    }
}
