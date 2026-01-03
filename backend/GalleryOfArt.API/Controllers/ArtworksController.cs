using GalleryOfART.Application.Services;
using GalleryOfART.Application.Dto;
using GalleryOfART.Persistence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GalleryOfART.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArtworksController : ControllerBase{

        private readonly IArtworkService _artworkService;

        public ArtworksController(IArtworkService artworkService)
        {
            _artworkService=artworkService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArtworkDto>>> Get()
        {
            var artworks=await _artworkService.GetAllAsync();
            return Ok(artworks);

        }

        [HttpGet("by-artist/{artistId}")]
        public async Task<ActionResult<IEnumerable<ArtworkDto>>> GetByArtistId(Guid artistId)
        {
            var artworks=await _artworkService.GetByArtistIdAsync(artistId);
            return Ok(artworks);
        }
        [HttpGet("image-url/{id}")]
        public async Task<ActionResult<string?>> GetImageUrl(Guid id)
        {
            var imageUrl=await _artworkService.GetImageUrlAsync(id);
            if(string.IsNullOrEmpty(imageUrl))
            {
                return NotFound();
            }
            return Ok(imageUrl);
        }
        [HttpGet("image/{id}")]
        public async Task<IActionResult> GetImage(Guid id)
        {
            var imageUrl=await _artworkService.GetImageUrlAsync(id);
            if (string.IsNullOrEmpty(imageUrl))
            {
                return NotFound();

            }
            return Redirect(imageUrl);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ArtworkDto>> GetById(Guid id)
        {
            var artwork = await _artworkService.GetByIdAsync(id);
            if (artwork == null) return NotFound();
            return Ok(artwork);
        }
        

    }
}