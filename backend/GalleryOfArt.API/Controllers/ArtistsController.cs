using GalleryOfART.Application.Services;
using GalleryOfART.Application.Dto;
using GalleryOfART.Persistence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GalleryOfART.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArtistsController : ControllerBase
    {
        private readonly IArtistService _artistService;

        public ArtistsController(IArtistService artistService)
        {
            _artistService = artistService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArtistDto>>> Get()
        {
            var artists = await _artistService.GetAllAsync();
            return Ok(artists);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ArtistDto>> GetById(Guid id)
        {
            var artist = await _artistService.GetByIdAsync(id);
            if (artist == null)
            {
                return NotFound();
            }
            return Ok(artist);
        }
    }
}