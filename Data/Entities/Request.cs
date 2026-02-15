namespace ProjectAI.Data.Entities
{
    public class Request
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string AiRawResponse { get; set; } = string.Empty;// JSON от первого AI вызова
        public DateTime CreatedAt { get; set; } 
        public ICollection<Item>? Items { get; set; } 
    }
}
