using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CustomLogger
{
    /// <summary>
    /// Generic logger
    /// </summary>
    public class CLogger
    {
        private string m_path;
        private string m_prefix;
        private string m_datepattern;
        private string m_ext;
        private string m_logfile;
        private bool m_enabled;
        private int m_maxfiles;
        private eLogLevel m_loglevel;

        /// <summary>
        /// Log level. Type of log message
        /// </summary>
        public enum eLogLevel
        {
            INFO = 0,
            DEBUG = 1,
            WARN = 2,
            ERROR = 3,
            FATAL = 4,
            ALL = 5
        }

        /// <summary>
        /// Generic logger 
        /// </summary>
        /// <param name="path">directory path where log file will be written</param>
        /// <param name="namepattern">log file pattern. Must be {prefix}_{datepattern}</param>
        /// <param name="limit">Max number of files to exist in the directory, if 0 no cleanup done</param>
        /// <param name="enabled">If logging enabled for writing into file</param>
        /// <param name="level">default log level</param>
        public CLogger(string path, string namepattern, int limit, bool enabled, eLogLevel level)
        {
            m_enabled = enabled;
            m_loglevel = level;
            m_path = path;
            m_maxfiles = limit;

            //Get the prefix without the date component
            var tokens = namepattern.Split('_');
            m_prefix = "Log";
            m_datepattern = "";
            m_ext = ".log";
            if (tokens.Length > 1)
            {
                m_prefix = tokens[0];
                m_datepattern = DateTime.Now.ToString(tokens[1]);
            }
            m_logfile = m_path + "\\" + m_prefix + "_" + m_datepattern + m_ext;

            if (m_maxfiles > 0)
                cleanUp(m_path, m_prefix, m_maxfiles, m_datepattern);
        }

        

        #region Public methods to write logs

        /// <summary>
        /// Write log for info only
        /// </summary>
        /// <param name="s">log info</param>
        public void Info(string s)
        {
            log(eLogLevel.INFO, s);
        }

        /// <summary>
        /// Write log for warning only
        /// </summary>
        /// <param name="s">log info</param>
        public void Warn(string s)
        {
            log(eLogLevel.WARN, s);
        }

        /// <summary>
        /// Write log for debugging only
        /// </summary>
        /// <param name="s">log info</param>
        public void Debug (string s)
        {
            log(eLogLevel.DEBUG, s);
        }

        /// <summary>
        /// Write Fatal log
        /// </summary>
        /// <param name="s">log info</param>
        public void Fatal (string s)
        {
            log(eLogLevel.FATAL, s);
        }

        /// <summary>
        /// Write Error log
        /// </summary>
        /// <param name="s">log info</param>
        public void Error(string s)
        {
            log(eLogLevel.ERROR, s);
        }

        /// <summary>
        /// Enable writing of logs
        /// </summary>
        /// <param name="enabled"></param>
        public void setEnabled(bool enabled)
        {
            m_enabled = enabled;
        }

        /// <summary>
        /// Set level of logs that can be written.
        /// If higher the level, the more logs are written 
        /// </summary>
        /// <param name="level"></param>
        public void setLevel(eLogLevel level)
        {
            m_loglevel = level;
        }

        #endregion Public methods to write logs

        /// <summary>
        /// Write log on file with timestamp
        /// </summary>
        /// <param name="level">level of log</param>
        /// <param name="s">log message</param>
        private void log(eLogLevel level, string s)
        {
            try
            {
                if (m_enabled)
                {
                    //ignore levels levels
                    if (level <= m_loglevel)
                    {
                        string line = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + level.ToString() + "\t- " + s + Environment.NewLine;
                        File.AppendAllText(m_logfile, line);
                    }
                }
            }
            catch(Exception)
            {
                //ignore

            }
        }

        /// <summary>
        /// Clean up logs based on limit max files.
        /// Best effort. If it fails, ignore
        /// </summary>
        /// <param name="logDirectory">directory where all logs are</param>
        /// <param name="logPrefix">common prefix logname of all logs with date pattern</param>
        /// <param name="limit">max num of files that can exist</param>
        /// <param name="datePattern">datepattern</param>
        private void cleanUp(string logDirectory, string logPrefix, int limit, string datePattern)
        {

            try
            {
                var dirInfo = new DirectoryInfo(logDirectory);
                if (!dirInfo.Exists)
                    return;

                //Get the prefix without the date component
                var tokens = logPrefix.Split('_');
                string prefix = ".log";
                if (tokens.Length > 1)
                    prefix = tokens[0];

                var fileInfos = dirInfo.GetFiles(prefix + "*");
                if (fileInfos.Length == 0)
                    return;

                // Sort by creation-time descending 
                Array.Sort(fileInfos, delegate(FileInfo f1, FileInfo f2)
                {
                    return f2.CreationTime.CompareTo(f1.CreationTime);
                });


                //delete if more than the limit
                for (int i = fileInfos.Length - 1; i >= limit; i--)
                {
                    fileInfos[i].Delete();
                }
            }
            catch(Exception)
            {
                //ignore
            }
        }
    }
}
