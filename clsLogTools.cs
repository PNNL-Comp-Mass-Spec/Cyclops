using System;
using System.Collections.Generic;
using log4net;
using log4net.Appender;

//'This assembly attribute tells Log4Net where to find the config file
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Logging.config", Watch = true)]

namespace Cyclops
{
    public class clsLogTools
    {
        #region "Enums"
        public enum LogLevels
        {
            DEBUG = 5,
            INFO = 4,
            WARN = 3,
            ERROR = 2,
            FATAL = 1
        }

        public enum LoggerTypes
        {
            LogFile,
            LogDb,
            LogSystem
        }
        #endregion

        #region "Module variables"
        private static ILog m_FileLogger = LogManager.GetLogger("FileLogger");
        private static readonly ILog m_DbLogger = LogManager.GetLogger("DbLogger");
        private static readonly ILog m_SysLogger = LogManager.GetLogger("SysLogger");
        #endregion
        private static string m_MostRecentErrorMessage = string.Empty;

        #region "Properties"
         ///<summary>
         ///Tells calling program file debug status
         ///</summary>
         ///<returns>TRUE if debug level enabled for file logger; FALSE otherwise</returns>
         ///<remarks></remarks>
        public static bool FileLogDebugEnabled
        {
            get { return m_FileLogger.IsDebugEnabled; }
        }

        public static string MostRecentErrorMessage
        {
            get { return m_MostRecentErrorMessage; }
        }
        #endregion

        public static void WriteLog(LoggerTypes LoggerType, LogLevels LogLevel, string InpMsg)
        {
            ILog MyLogger = default(ILog);

            //Establish which logger will be used
            switch (LoggerType)
            {
                case LoggerTypes.LogDb:
                    // Note that the Logging.config should have the DbLogger logging to both the database and the rolling file
                    MyLogger = m_DbLogger;
                    break;
                case LoggerTypes.LogFile:
                    MyLogger = m_FileLogger;
                    break;
                case LoggerTypes.LogSystem:
                    MyLogger = m_SysLogger;
                    break;
                default:
                    throw new Exception("Invalid logger type specified");
            }

            //Send the log message
            switch (LogLevel)
            {
                case LogLevels.DEBUG:
                    if (MyLogger.IsDebugEnabled)
                        MyLogger.Debug(InpMsg);
                    break;
                case LogLevels.ERROR:
                    if (MyLogger.IsErrorEnabled)
                        MyLogger.Error(InpMsg);
                    break;
                case LogLevels.FATAL:
                    if (MyLogger.IsFatalEnabled)
                        MyLogger.Fatal(InpMsg);
                    break;
                case LogLevels.INFO:
                    if (MyLogger.IsInfoEnabled)
                        MyLogger.Info(InpMsg);
                    break;
                case LogLevels.WARN:
                    if (MyLogger.IsWarnEnabled)
                        MyLogger.Warn(InpMsg);
                    break;
                default:
                    throw new Exception("Invalid log level specified");
            }

            if (LogLevel <= LogLevels.ERROR)
            {
                m_MostRecentErrorMessage = InpMsg;
            }
        }


        public static void WriteLog(LoggerTypes LoggerType, LogLevels LogLevel, string InpMsg, Exception Ex)
        {
            ILog MyLogger = default(ILog);

            //Establish which logger will be used
            switch (LoggerType)
            {
                case LoggerTypes.LogDb:
                    MyLogger = m_DbLogger;
                    break;
                case LoggerTypes.LogFile:
                    MyLogger = m_FileLogger;
                    break;
                case LoggerTypes.LogSystem:
                    MyLogger = m_SysLogger;
                    break;
                default:
                    throw new Exception("Invalid logger type specified");
            }

            //Send the log message
            switch (LogLevel)
            {
                case LogLevels.DEBUG:
                    if (MyLogger.IsDebugEnabled)
                        MyLogger.Debug(InpMsg, Ex);
                    break;
                case LogLevels.ERROR:
                    if (MyLogger.IsErrorEnabled)
                        MyLogger.Error(InpMsg, Ex);
                    break;
                case LogLevels.FATAL:
                    if (MyLogger.IsFatalEnabled)
                        MyLogger.Fatal(InpMsg, Ex);
                    break;
                case LogLevels.INFO:
                    if (MyLogger.IsInfoEnabled)
                        MyLogger.Info(InpMsg, Ex);
                    break;
                case LogLevels.WARN:
                    if (MyLogger.IsWarnEnabled)
                        MyLogger.Warn(InpMsg, Ex);
                    break;
                default:
                    throw new Exception("Invalid log level specified");
            }
        }

        public static void ChangeLogFileName(string FileName)
        {
            //Get a list of appenders
            List<log4net.Appender.IAppender> AppendList = FindAppenders("RollingFileAppender");
            if (AppendList == null)
            {
                WriteLog(LoggerTypes.LogSystem, LogLevels.WARN, "Unable to change file name. No appender found");
                return;
            }

            m_FileLogger = LogManager.GetLogger("FileLogger");
            foreach (log4net.Appender.IAppender SelectedAppender in AppendList)
            {
                //Convert the IAppender object to a RollingFileAppender
                log4net.Appender.RollingFileAppender AppenderToChange = SelectedAppender as log4net.Appender.RollingFileAppender;
                if (AppenderToChange == null)
                {
                    WriteLog(LoggerTypes.LogSystem, LogLevels.ERROR, "Unable to convert appender");
                    return;
                }
                //Change the file name and activate change
                AppenderToChange.File = FileName;
                AppenderToChange.ActivateOptions();
            }
        }

        private static List<log4net.Appender.IAppender> FindAppenders(string AppendName)
        {

            //Get a list of the current loggers
            ILog[] LoggerList = LogManager.GetCurrentLoggers();
            if (LoggerList.GetLength(0) < 1)
                return null;

            //Create a List of appenders matching the criteria for each logger
            List<log4net.Appender.IAppender> RetList = new List<log4net.Appender.IAppender>();
            foreach (ILog TestLogger in LoggerList)
            {
                foreach (log4net.Appender.IAppender TestAppender in TestLogger.Logger.Repository.GetAppenders())
                {
                    if (TestAppender.Name == AppendName)
                        RetList.Add(TestAppender);
                }
            }

            //Return the list of appenders, if any found
            if (RetList.Count > 0)
            {
                return RetList;
            }
            else
            {
                return null;
            }
        }

        public static void SetFileLogLevel(int InpLevel)
        {
            Type LogLevelEnumType = typeof(LogLevels);

            //Verify input level is a valid log level
            if (!Enum.IsDefined(LogLevelEnumType, InpLevel))
            {
                WriteLog(LoggerTypes.LogFile, LogLevels.ERROR, "Invalid value specified for level: " + InpLevel.ToString());
                return;
            }

            //Convert input integer into the associated enum
            LogLevels Lvl = (LogLevels)Enum.Parse(LogLevelEnumType, InpLevel.ToString());
            SetFileLogLevel(Lvl);

        }

        public static void SetFileLogLevel(LogLevels InpLevel)
        {
            log4net.Repository.Hierarchy.Logger LogRepo = (log4net.Repository.Hierarchy.Logger)m_FileLogger.Logger;

            switch (InpLevel)
            {
                case LogLevels.DEBUG:
                    LogRepo.Level = LogRepo.Hierarchy.LevelMap["DEBUG"];
                    break;
                case LogLevels.ERROR:
                    LogRepo.Level = LogRepo.Hierarchy.LevelMap["ERROR"];
                    break;
                case LogLevels.FATAL:
                    LogRepo.Level = LogRepo.Hierarchy.LevelMap["FATAL"];
                    break;
                case LogLevels.INFO:
                    LogRepo.Level = LogRepo.Hierarchy.LevelMap["INFO"];
                    break;
                case LogLevels.WARN:
                    LogRepo.Level = LogRepo.Hierarchy.LevelMap["WARN"];
                    break;
            }
        }

    }
}
