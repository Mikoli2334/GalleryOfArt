using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalleryOfART.Application.Dto;
using GalleryOfART.Persistence.Db;
using Microsoft.EntityFrameworkCore;

namespace GalleryOfART.Application.Services
{
    public class ArtistService : IArtistService
    {
        private readonly GalleryDbContext _context;

        public ArtistService(GalleryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ArtistDto>> GetAllAsync()
        {
            var artists = await _context.artists
            .Select(a => new ArtistDto
            {
                Id = a.id,
                FullName = a.full_name,
                BirthYear = a.birth_year,
                DeathYear = a.death_year,
                ArtworkCount = a.artworks.Count
            })
            .ToListAsync();
            return artists;
        }
        public async Task<ArtistDto?> GetByIdAsync(Guid id)
        {
                var artist = await _context.artists
                    .Where(a => a.id == id)
                    .Select(a => new ArtistDto
                    {
                        Id = a.id,
                        FullName = a.full_name,
                        BirthYear = a.birth_year,
                        DeathYear = a.death_year,
                        ArtworkCount = a.artworks.Count
                    })
                    .FirstOrDefaultAsync();

                return artist;
            }

    }
}