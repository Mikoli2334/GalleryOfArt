using GalleryOfART.Application.Dto;
using GalleryOfART.Persistence.Db;
using Microsoft.EntityFrameworkCore;

namespace GalleryOfART.Application.Services
{
    public class ArtworkService : IArtworkService
    {
        private readonly GalleryDbContext _context;
        
        public ArtworkService(GalleryDbContext context)
        {
            _context=context;
        }

        public async Task<IEnumerable<ArtworkDto>> GetAllAsync()
        {
            var artwork=await _context.artworks
            .Select(a=>new ArtworkDto
            {
                Id=a.id,
                Title=a.title,
                YearCreated=a.year_created,
                ImageUrl=a.harvard_image,
                Artist=a.artist==null?null:new ArtistDto
                {
                    Id=a.artist.id,
                    FullName=a.artist.full_name,
                    BirthYear=a.artist.birth_year,
                    DeathYear=a.artist.death_year,
                    ArtworkCount=a.artist.artworks.Count
                }
            }
            ).ToListAsync();
            return artwork;
        }
        public async Task<IEnumerable<ArtworkDto>> GetByArtistIdAsync(Guid artistId)
        {
            var artwork=await _context.artworks
            .Where(a=>a.artist_id==artistId)
            .Select(a=>new ArtworkDto
            {
                Id=a.id,
                Title=a.title,
                YearCreated=a.year_created,
                ImageUrl=a.harvard_image,
                Artist=a.artist==null?null:new ArtistDto
                {
                    Id=a.artist.id,
                    FullName=a.artist.full_name,
                    BirthYear=a.artist.birth_year,
                    DeathYear=a.artist.death_year,
                    ArtworkCount=a.artist.artworks.Count
                }
            }
            ).ToListAsync();
            return artwork;
        }
        public async Task<string?> GetImageUrlAsync(Guid id)
        {
            var ImageUrl=await _context.artworks
            .Where(a=>a.id==id)
            .Select(a=>a.harvard_image)
            .FirstOrDefaultAsync();
            return ImageUrl;
        }

    }
}