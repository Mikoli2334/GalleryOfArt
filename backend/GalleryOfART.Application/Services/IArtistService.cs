using System.Collections.Generic;
using System.Threading.Tasks;
using GalleryOfART.Application.Dto;

namespace GalleryOfART.Application.Services
{
public interface IArtistService
{
    Task<IEnumerable<ArtistDto>> GetAllAsync();
    Task<ArtistDto?> GetByIdAsync(Guid id);
}
}