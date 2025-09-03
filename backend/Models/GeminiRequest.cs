namespace Backend.Models
{
    public class GeminiRequest
    {
        public List<Message> Messages { get; set; } = new();
    }

    public class Message
    {
        public string Role { get; set; } = "";
        public string Text { get; set; } = "";
    }
}
