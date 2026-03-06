namespace NearByBook.Model

{
    public class Book
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public string Condition { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }

        // 🔥 Foreign Key
        public int SellerId { get; set; }

        public User Seller { get; set; }   // Navigation Property

        public bool IsApproved { get; set; } = false;
        public bool IsSold { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
