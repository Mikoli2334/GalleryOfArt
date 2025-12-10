using System;
using System.Collections.Generic;

namespace GalleryOfART.Persistence.Models;

public partial class artist
{
    public Guid id { get; set; }

    public string full_name { get; set; } = null!;

    public string? pseudonym { get; set; }

    public int? birth_year { get; set; }

    public int? death_year { get; set; }

    public string? country { get; set; }

    public string? bio_md { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<artwork> artworks { get; set; } = new List<artwork>();
}
