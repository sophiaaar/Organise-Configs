using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gurock.TestRail;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrganiseConfigs
{
    class MainClass
    {

        public struct Config
        {
            public string ConfigName;
            public string ConfigID;
            public string ConfigGroupID;
        }

        private static readonly IConfigReader _configReader = new ConfigReader();

        public static JArray GetConfigs(APIClient client, string projectID)
        {
            return (JArray)client.SendGet("get_configs/" + projectID);
        }

        public static APIClient ConnectToTestrail()
        {
            APIClient client = new APIClient("http://qatestrail.hq.unity3d.com");
            client.User = _configReader.TestRailUser;
            client.Password = _configReader.TestRailPass;
            return client;
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            APIClient client = ConnectToTestrail();

            JArray configs = GetConfigs(client, "2");
            string json = CreateConfigJson(configs);


            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;

            try
            {
                ostrm = new FileStream("Configs.csv", FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open Cases.csv for writing");
                Console.WriteLine(e.Message);
                return;
            }
            Console.SetOut(writer);

            List<Config> configsFromJson = CreateListsOfGroups(configs);
            string csv = CreateCsvOfConfigs(configsFromJson);
            Console.WriteLine(csv);

            Console.SetOut(oldOut);
            writer.Close();
            ostrm.Close();
        }

        public static string CreateConfigJson(JArray configs)
        {
            string json = JsonConvert.SerializeObject(configs);
            //Console.WriteLine(json);
            return json;
        }

        public static List<Config> CreateListsOfGroups(JArray configs)
        {
            List<string> listOfGroupIds = new List<string>();
            List<Config> groupsWithConfigs = new List<Config>();
            //JArray groups = 
            for (int i = 0; i < configs.Count; i++)
            {
                JObject group = configs[i].ToObject<JObject>();
                string groupId = group.Property("id").Value.ToString();
                listOfGroupIds.Add(groupId);

                JArray configsInGroup = (JArray)group.Property("configs").First;
                for (int j = 0; j < configsInGroup.Count; j++)
                {
                    JObject config = configsInGroup[j].ToObject<JObject>();
                    string name = config.Property("name").Value.ToString();
                    string configID = config.Property("id").Value.ToString();
                    string groupID = config.Property("group_id").Value.ToString();

                    Config currentConfig;
                    currentConfig.ConfigName = name;
                    currentConfig.ConfigID = configID;
                    currentConfig.ConfigGroupID = groupID;

                    groupsWithConfigs.Add(currentConfig);
                }
            }
            return groupsWithConfigs;
        }

        public static string CreateCsvOfConfigs(List<Config> configs)
        {
            StringBuilder csv = new StringBuilder();

            for (int i = 0; i < configs.Count; i++)
            {
                string line = string.Format("{0},{1},{2},{3},", configs[i].ConfigGroupID, configs[i].ConfigID, configs[i].ConfigName, "\n");
                csv.Append(line);
            }
            return csv.ToString();
        }
    }
}
