using System.Text.Json.Serialization;

namespace Quiz_App_VueJs.Models
{
    internal class Questions
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("subtopicId")]
        public string? SubTopicId { get; set; }
        [JsonPropertyName("text")]
        public string? Text { get; set; }
        [JsonPropertyName("options")]
        public List<string> Options { get; set; } = new();
        [JsonPropertyName("correctAnswer")]
        public int CorrectAnswer { get; set; }
        [JsonPropertyName("selectedAnswer")]
        public int? SelectedAnswer { get; set; }
        [JsonPropertyName("facts")]
        public List<string> Facts { get; set; } = new();
        [JsonPropertyName("examples")]
        public List<string> Examples { get; set; } = new();
    }
}
