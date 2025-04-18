using System.Text.Json.Serialization;

namespace Quiz_App_VueJs.Models
{
    internal class Topics
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        [JsonPropertyName("category")]
        public string? Category { get; set; }
    }
}
