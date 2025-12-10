namespace GalleryOfART.Domain.Entities
{
    public class Artist
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }= null!;
        public int? BirthYear { get; set; }
        public int? DeathYear { get; set; }
        public string? Country { get; set; }
        public ICollection<Artwork> Artworks {get; set;}=new List<Artwork>();
    }
}