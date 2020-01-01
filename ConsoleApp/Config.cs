using Newtonsoft.Json;
using System;
using System.IO;

namespace ConsoleApp {
    public class Config {
        [JsonProperty("useRemoteDriver")]
        public bool UseRemoteDriver { get; set; }

        [JsonProperty("RemoteDriverIp")]
        public string RemoteDriverIp { get; set; }

        [JsonProperty("localDriverPath")]
        public string LocalDriverPath { get; set; }

        [JsonProperty("runHeadless")]
        public bool RunHeadless { get; set; }

        [JsonProperty("loadImages")]
        public bool LoadImages { get; set; }

        [JsonProperty("secondsToWaitForElements")]
        public int SecondsToWaitForElements { get; set; }

        [JsonProperty("driverImplicitWait")]
        public int DriverImplicitWait { get; set; }

        [JsonProperty("driverCache")]
        public int DriverCache { get; set; }


        public static Config FromJsonFile(string path) {
            var jsonString = File.ReadAllText("config.json");
            jsonString = jsonString.Replace(Environment.NewLine, "");
            var config = FromJsonString(jsonString);
            return config;
        }

        public static Config FromJsonString(string jsonString) {
            var config = JsonConvert.DeserializeObject<Config>(jsonString);
            return config;
        }

        public string ToJson(Formatting format = Formatting.Indented) {
            var jsonText = JsonConvert.SerializeObject(this, format);
            return jsonText;
        }
    }
}
