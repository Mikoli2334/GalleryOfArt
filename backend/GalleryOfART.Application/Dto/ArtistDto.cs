
    namespace GalleryOfART.Application.Dto
{
    public class ArtistDto
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public int? BirthYear { get; set; }

        public int? DeathYear { get; set; }

        public int ArtworkCount { get; set; }
    }
}
