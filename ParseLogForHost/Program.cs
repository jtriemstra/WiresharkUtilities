using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace ParseLogForHost
{
    class Program
    {
        static void Main(string[] args)
        {
            String strLogPath = @"C:\TestProjects\WiresharkUtilities\ParseLogForHost\Wireshark.csv";
            TextReader objTextReader = File.OpenText(strLogPath);
            CsvReader objCsvReader = new CsvReader(objTextReader);
            Dictionary<String, String> hshIpToCompanyName = new Dictionary<string, string>();
            WebClient objWebClient = new WebClient();
            String strArinPrefix = "http://whois.arin.net/rest/ip/";
            Dictionary<String, List<String>> hshCompanyNameToIps = new Dictionary<string, List<string>>();

            while (objCsvReader.Read())
            {
                var objRecord = objCsvReader.GetRecord<dynamic>();
                String strDestination = objRecord.Destination;
                if (!strDestination.StartsWith("192.168") && !hshIpToCompanyName.ContainsKey(strDestination))
                {
                    String strArinResponse = objWebClient.DownloadString(strArinPrefix + strDestination + ".json");
                    var objArinResponse = JsonConvert.DeserializeObject<dynamic>(strArinResponse);

                    hshIpToCompanyName.Add(strDestination, objArinResponse.net.name["$"].ToString());
                }
            }

            foreach (String strIp in hshIpToCompanyName.Keys)
            {
                String strCompany = hshIpToCompanyName[strIp];
                if (!hshCompanyNameToIps.ContainsKey(strCompany))
                {
                    hshCompanyNameToIps[strCompany] = new List<string>();
                }
                hshCompanyNameToIps[strCompany].Add(strIp);
            }

            foreach (String strCompany in hshCompanyNameToIps.Keys)
            {
                System.Diagnostics.Debug.WriteLine(strCompany);
                foreach (String strIp in hshCompanyNameToIps[strCompany])
                {
                    System.Diagnostics.Debug.WriteLine(strIp);
                }
            }
        }
    }
}
