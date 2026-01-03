
using GalleryOfART.Application.Dto;

namespace GalleryOfART.Application.Services
{
    public interface IArtworkService
{
    Task<IEnumerable<ArtworkDto>> GetAllAsync();
    Task<IEnumerable<ArtworkDto>> GetByArtistIdAsync(Guid artistId);
    Task<ArtworkDto?> GetByIdAsync(Guid id);

    Task<string?> GetImageUrlAsync(Guid id);
}
}