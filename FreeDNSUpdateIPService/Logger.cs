using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class Logger
    {
        private int numTabs = 0;
        private string logDir;
        private string progName;

        public Logger(string dir, string prog)
        {
            logDir = dir;
            progName = prog;
        }

        public void WriteLog(string text)
        {
            string logPath = logDir + progName + "_" + String.Format("{0:yyyy-MM-dd}", DateTime.Now) + ".txt";
            StreamWriter log = new StreamWriter(logPath, true);

            StringBuilder tabs = new StringBuilder("");
            for (int i = 0; i <= numTabs; i++)
                tabs.Append("\t");

            log.WriteLine(String.Format("{0:yyyy-MM-dd hh:mm:ss} {1}{2}", DateTime.Now, tabs, text));

            log.Close();
        }

        public void WriteFuncNameLog(string text)
        {
            string logPath = logDir + progName + "_" + String.Format("{0:yyyy-MM-dd}", DateTime.Now) + ".txt";
            StreamWriter log = new StreamWriter(logPath, true);

            StringBuilder tabs = new StringBuilder("");
            for (int i = 0; i <= numTabs; i++)
                tabs.Append("\t");

            log.WriteLine(String.Format("{0:yyyy-MM-dd hh:mm:ss}\t{1}", DateTime.Now, text));

            log.Close();
        }

        public void LogFunc(string funcName, bool enter)
        {
            if (enter)
            {
                WriteLog("Entering: " + funcName);
                numTabs++;
            }
            else
            {
                numTabs--;
                WriteLog("Leaving:  " + funcName);
            }
        }

        public void CleanLogs()
        {
            LogFunc("CleanLogs", true);
            try
            {
                string[] files = Directory.GetFiles(logDir);

                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.LastAccessTime < DateTime.Now.AddMonths(-3))
                    {
                        WriteLog("Deleting: " + file);
                        fi.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog("***ERROR***: " + ex.ToString());
            }
            LogFunc("CleanLogs", false);
        }

        public void WriteAlert(string eventText)
        {
            LogFunc("WriteAlert", true);
            string source = progName;
            string eventLog = "Application";

            if (!EventLog.SourceExists(source))
                EventLog.CreateEventSource(source, eventLog);

            EventLog.WriteEntry(source, eventText, EventLogEntryType.Warning);
            LogFunc("WriteAlert", false);
        }
    }
}
