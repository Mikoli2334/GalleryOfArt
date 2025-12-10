    
     namespace GalleryOfART.Application.Dto
{
    public class ArtworkDto
    {
        public Guid Id{get;set;}
        public string Title{get;set;}=string.Empty;
        public int? YearCreated{get;set;}

        public string? ImageUrl{get;set;}
        public ArtistDto? Artist{get;set;}


        
    }
}