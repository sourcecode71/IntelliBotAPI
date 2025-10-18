public class ChatRequest
{
    public string Message { get; set; }
    public string? ConversationId { get; set; }
    public AiModel Model { get; set; }
    public double Temperature { get; set; }
    public int MaxTokens { get; set; }
    public List<MessageRequest> PreviousMessages { get; set; }
    public string? UserId { get; set; } // Added property to fix CS1061  
}
