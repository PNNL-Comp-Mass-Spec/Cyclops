using System;
using System.Collections.Generic;
using log4net;

//This assembly attribute tells Log4Net where to find the config file

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Logging.config", Watch = true)]

namespace Cyclops
{
    public class LogTools
    {

        //*********************************************************************************************************
        // Class for handling logging via Log4Net
        //*********************************************************************************************************

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
        private static readonly ILog m_FileLogger = LogManager.GetLogger("ToolFileLogger");
        private static readonly ILog m_DbLogger = LogManager.GetLogger("DbLogger");
        private static readonly ILog m_SysLogger = LogManager.GetLogger("SysLogger");
        #endregion
        private static string m_MostRecentErrorMessage = string.Empty;

        #region "Properties"
        /// <summary>
        /// Tells calling program file debug status
        /// </summary>
        /// <returns>TRUE if debug level enabled for file logger; FALSE otherwise</returns>
        /// <remarks></remarks>
        public static bool FileLogDebugEnabled
        {
            get { return m_FileLogger.IsDebugEnabled; }
        }

        public static string MostRecentErrorMessage
        {
            get { return m_MostRecentErrorMessage; }
        }
        #endregion

        #region "Methods"
        /// <summary>
        /// Writes a message to the logging system
        /// </summary>
        /// <param name="LoggerType">Type of logger to use</param>
        /// <param name="LogLevel">Level of log reporting</param>
        /// <param name="InpMsg">Message to be logged</param>
        /// <remarks></remarks>

        public static void WriteLog(LoggerTypes LoggerType, LogLevels LogLevel, string InpMsg)
        {
            ILog MyLogger;

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

        /// <summary>
        /// Overload to write a message and exception to the logging system
        /// </summary>
        /// <param name="LoggerType">Type of logger to use</param>
        /// <param name="LogLevel">Level of log reporting</param>
        /// <param name="InpMsg">Message to be logged</param>
        /// <param name="Ex">Exception to be logged</param>
        /// <remarks></remarks>

        public static void WriteLog(LoggerTypes LoggerType, LogLevels LogLevel, string InpMsg, Exception Ex)
        {
            ILog MyLogger;

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

        /// <summary>
        /// Changes the base log file name
        /// </summary>
        /// <param name="FileName">Log file base name and path (relative to program folder)</param>
        /// <remarks></remarks>

        public static void ChangeLogFileName(string FileName)
        {
            //Get a list of appenders
            var AppendList = FindAppenders("RollingFileAppender");
            if (AppendList == null)
            {
                WriteLog(LoggerTypes.LogSystem, LogLevels.WARN, "Unable to change file name. No appender found");
                return;
            }

            foreach (var SelectedAppender in AppendList)
            {
                //Convert the IAppender object to a RollingFileAppender
                var AppenderToChange = SelectedAppender as log4net.Appender.RollingFileAppender;
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

        /// <summary>
        /// Gets the specified appender
        /// </summary>
        /// <param name="AppendName">Name of appender to find</param>
        /// <returns>List(IAppender) objects if found; NOTHING otherwise</returns>
        /// <remarks></remarks>
        private static List<log4net.Appender.IAppender> FindAppenders(string AppendName)
        {

            //Get a list of the current loggers
            var LoggerList = LogManager.GetCurrentLoggers();
            if (LoggerList.GetLength(0) < 1)
                return null;

            //Create a List of appenders matching the criteria for each logger
            var RetList = new List<log4net.Appender.IAppender>();
            foreach (var TestLogger in LoggerList)
            {
                foreach (var TestAppender in TestLogger.Logger.Repository.GetAppenders())
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

        /// <summary>
        /// Sets the file logging level via an integer value (Overloaded)
        /// </summary>
        /// <param name="InpLevel">Integer corresponding to level (1-5, 5 being most verbose</param>
        /// <remarks></remarks>

        public static void SetFileLogLevel(int InpLevel)
        {
            var LogLevelEnumType = typeof(LogLevels);

            //Verify input level is a valid log level
            if (!Enum.IsDefined(LogLevelEnumType, InpLevel))
            {
                WriteLog(LoggerTypes.LogFile, LogLevels.ERROR, "Invalid value specified for level: " + InpLevel.ToString());
                return;
            }

            //Convert input integer into the associated enum
            var Lvl = (LogLevels)Enum.Parse(LogLevelEnumType, InpLevel.ToString());
            SetFileLogLevel(Lvl);

        }
        //%date [%thread] %-5level %logger [%property{NDC}] - %message%newline"

        /// <summary>
        /// Sets file logging level based on enumeration (Overloaded)
        /// </summary>
        /// <param name="InpLevel">LogLevels value defining level (Debug is most verbose)</param>
        /// <remarks></remarks>

        public static void SetFileLogLevel(LogLevels InpLevel)
        {
            var LogRepo = (log4net.Repository.Hierarchy.Logger)m_FileLogger.Logger;

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
        #endregion

    }
}
