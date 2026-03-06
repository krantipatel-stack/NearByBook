namespace NearByBook.DTO
{
    public class CreateBookDto
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public string Condition { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
    }
}
