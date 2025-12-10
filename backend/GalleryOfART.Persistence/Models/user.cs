using System;
using System.Collections.Generic;

namespace GalleryOfART.Persistence.Models;

public partial class user
{
    public Guid id { get; set; }

    public string? external_id { get; set; }

    public string? email { get; set; }

    public string display_name { get; set; } = null!;

    public string? avatar_url { get; set; }

    public string role { get; set; } = null!;

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<artwork> artworks { get; set; } = new List<artwork>();

    public virtual ICollection<collection> collections { get; set; } = new List<collection>();

    public virtual ICollection<media_file> media_files { get; set; } = new List<media_file>();

    public virtual ICollection<post> posts { get; set; } = new List<post>();
}
