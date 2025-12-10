namespace GalleryOfART.Domain.Entities
{
    public class Artwork
    {
        public Guid Id { get; set; }
        public string Title { get; set; }=null!;
        public int? YearCreated { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }   
        public Guid? ArtistId { get; set; }
        public Artist? Artist { get; set; }
        
    }
}