namespace ProjectAI.Data.Entities
{
    public class Item
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public Request? Request { get; set; }
        public string? Name { get; set; }
        public bool IsUserCorrected { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<Material>? Materials { get; set; }
    }
}
