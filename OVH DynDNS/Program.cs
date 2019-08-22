using Newtonsoft.Json;
using Ovh.Api;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace OVH_DynDNS
{
    class Program
    {
        static int Main(string[] args)
        {
            // Variables.
            string publicIp = "";
            string configPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\OVH DynDNS\\";
            string configFile = configPath + "config.json";
            string logFile = configPath + "ovh_dyndns.log";
            StringBuilder logs = new StringBuilder();

            // Check log size.
            if (File.Exists(logFile) && new FileInfo(logFile).Length >= 1048576) // 1 Mo.
            {
                File.Delete(logFile);
            }

            logs.Append("===================\n");
            logs.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss\n\n"));

            // Get public IP.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ipify.org/");
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.UserAgent = "OVH DynDNS";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                publicIp = reader.ReadToEnd();
                logs.Append("Current public IP is: " + publicIp + ".\n");
            }

            // Create AppData directory.
            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
                logs.Append("Directory created: " + configPath + ".\n");
            }

            // Create config file.
            if (!File.Exists(configFile))
            {
                OvhConfig newConfig = new OvhConfig
                {
                    LastPublicIp = publicIp
                };

                File.WriteAllText(configFile, JsonConvert.SerializeObject(newConfig));
                logs.Append("File created: " + configFile + ".\n");
            }

            // Get config.
            OvhConfig config = JsonConvert.DeserializeObject<OvhConfig>(File.ReadAllText(configFile));
            logs.Append("Last public IP is: " + config.LastPublicIp + ".\n");

            // Check config.
            if (
                !string.IsNullOrEmpty(config.OvhRegion) &&
                !string.IsNullOrEmpty(config.ApplicationKey) &&
                !string.IsNullOrEmpty(config.ApplicationSecret) &&
                !string.IsNullOrEmpty(config.ConsumerKey) &&
                !string.IsNullOrEmpty(config.ZoneName)
            )
            {
                // Change IP for all records of
                // the DNS zone if needed.
                if (publicIp != config.LastPublicIp)
                {
                    logs.Append("Current IP is different from last IP, apply changes.\n");
                    Client client = new Client(config.OvhRegion, config.ApplicationKey, config.ApplicationSecret, config.ConsumerKey);
                    int[] recordsIds = client.Get<int[]>("/domain/zone/" + config.ZoneName + "/dynHost/record");

                    foreach (int recordId in recordsIds)
                    {
                        OvhRecord record = client.Get<OvhRecord>("/domain/zone/" + config.ZoneName + "/dynHost/record/" + recordId);
                        record.Ip = publicIp;
                        client.Put("/domain/zone/" + config.ZoneName + "/dynHost/record/" + recordId, "{\"ip\": \"" + record.Ip + "\"}");
                        logs.Append("Subdomain changed: " + record.SubDomain + ".\n");
                    }

                    // Refresh DNS zone.
                    client.Post("/domain/zone/" + config.ZoneName + "/refresh", "");
                    logs.Append("Configuration updated.\n");

                    // Update config.
                    config.LastPublicIp = publicIp;
                    File.WriteAllText(configFile, JsonConvert.SerializeObject(config));

                    // Send SMS.
                    if (
                        !string.IsNullOrEmpty(config.SmsUser) &&
                        !string.IsNullOrEmpty(config.SmsPass)
                    )
                    {
                        WebRequest.Create("https://smsapi.free-mobile.fr/sendmsg?user=" + config.SmsUser + "&pass=" + config.SmsPass + "&msg=Public%20IP%20change%20detected%21%0ANew%20IP%20is%20" + publicIp).GetResponse();
                    }

                    // Send Telegram notification.
                    if (!string.IsNullOrEmpty(config.TelegramAccessToken))
                    {
                        WebRequest.Create("https://api.telegram.org/bot" + config.TelegramAccessToken + "/sendMessage?chat_id=" + config.TelegramChatId + "&text=Public%20IP%20change%20detected%21%0ANew%20IP%20is%20" + publicIp).GetResponse();
                    }
                }
                else
                {
                    logs.Append("Current IP is similar to last IP, no changes needed.\n");
                }
            }
            else
            {
                logs.Append("WARNING! Config not found!\n");
                logs.Append("Please add your config in " + configFile + "!\n");
            }

            logs.Append("===================\n\n\n");
            File.AppendAllText(logFile, logs.ToString());
            logs.Clear();

            return 0;
        }
    }

    public class OvhRecord
    {
        public string Ip { get; set; }
        public string SubDomain { get; set; }
    }

    public class OvhConfig
    {
        public string OvhRegion { get; set; }
        public string ApplicationKey { get; set; }
        public string ApplicationSecret { get; set; }
        public string ConsumerKey { get; set; }
        public string ZoneName { get; set; }
        public string SmsUser { get; set; }
        public string SmsPass { get; set; }
        public string TelegramAccessToken { get; set; }
        public string TelegramChatId { get; set; }
        public string LastPublicIp { get; set; }
    }
}
