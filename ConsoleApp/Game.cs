using Newtonsoft.Json;

namespace ConsoleApp {
    public class Game {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("coverPath")]
        public string CoverPath { get; set; }

        [JsonProperty("price")]
        public double? Price { get; set; }

        [JsonProperty("salvePrice")]
        public double? SalePrice { get; set; }
    }
}
