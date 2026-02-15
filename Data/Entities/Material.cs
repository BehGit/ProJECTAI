namespace ProjectAI.Data.Entities
{
    public class Material
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public Item? Item { get; set; }
        public string? Name { get; set; }
    }
}
