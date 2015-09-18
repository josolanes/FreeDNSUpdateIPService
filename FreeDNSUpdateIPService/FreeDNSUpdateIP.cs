using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Timers;
using Tools;

namespace FreeDNSUpdateIPService
{
    public partial class FreeDNSUpdateIP : ServiceBase
    {
        private static string logDir;
        private static Logger log;

        Timer timer = new Timer();

        Dictionary<string, string> websites = new Dictionary<string, string>();
        private int checkIntervalMinutes = 15;

        public FreeDNSUpdateIP()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ReadConfig();

            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
                log.LogFunc("OnStart", true);
                log.WriteLog("Creating: " + logDir);
            }
            else
                log.LogFunc("OnStart", true);

            log.CleanLogs();

            PrintConfigValues();
            
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
            timer.Start();

            log.LogFunc("OnStart", false);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateIPs();
        }

        protected override void OnStop()
        {
            log.WriteLog("Stopping service...");
            timer.Stop();
            timer.Dispose();
        }

        private void ReadConfig()
        {
            foreach (string key in ConfigurationManager.AppSettings)
            {
                switch(key)
                {
                    case "logPath":
                        logDir = ConfigurationManager.AppSettings[key];
                        log = new Logger(logDir, "FreeDNSUpdateIPService");
                        break;
                    case "checkInterval_minutes":
                        if (!Int32.TryParse(ConfigurationManager.AppSettings["checkInterval_minutes"], out checkIntervalMinutes) || checkIntervalMinutes == 0)
                        {
                            log.WriteAlert("Invalid value for 'checkInterval_minutes', defaulting to 15 minutes");
                            checkIntervalMinutes = 15;
                        }

                        timer.Interval = checkIntervalMinutes * 60 * 1000;
                        break;
                    default:
                        websites.Add(key, ConfigurationManager.AppSettings[key]);
                        break;
                }
            }
        }

        private void PrintConfigValues()
        {
            log.LogFunc("PrintConfigValues()", true);

            log.WriteLog("logPath = " + logDir);
            log.WriteLog("checkInterval_Minutes = " + checkIntervalMinutes.ToString());
            log.WriteLog("Websites and their update addresses:");
            foreach (KeyValuePair<string, string> website in websites)
                log.WriteLog(String.Format("\t{0}: {1}", website.Key, website.Value));

            log.LogFunc("PrintConfigValues()", false);
        }

        private void UpdateIPs()
        {
            log.LogFunc("UpdateIPs", true);
            try
            {
                foreach (KeyValuePair<string, string> website in websites)
                {
                    log.WriteLog(website.Key + ":");
                    Request request = new Request(website.Value);
                    string response = request.GetResponse();
                    log.WriteLog("\t" + response);

                    if (response.Contains("Updated"))
                        log.WriteAlert(response);
                }
            }
            catch (Exception ex)
            {
                log.WriteLog("***ERROR***: " + ex.ToString());
            }
            log.LogFunc("UpdateIPs", false);
        }
    }
}
