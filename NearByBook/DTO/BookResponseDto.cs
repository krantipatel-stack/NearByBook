namespace NearByBook.DTO
{
    public class BookResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public string Condition { get; set; }
        public decimal Price { get; set; }
        public string City { get; set; }
        public string ImageUrl { get; set; }
    }
}
