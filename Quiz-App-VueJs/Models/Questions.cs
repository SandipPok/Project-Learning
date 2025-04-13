namespace Quiz_App_VueJs.Models
{
    internal class Questions
    {
        public string? Id { get; set; }
        public string? SubTopicId { get; set; }
        public string? Text { get; set; }
        public List<string> Options { get; set; } = new();
        public int CorrectAnswer { get; set; }
        public List<string> Facts { get; set; } = new();
        public List<string> Examples { get; set; } = new();
    }
}
