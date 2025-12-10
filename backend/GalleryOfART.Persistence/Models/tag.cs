using System;
using System.Collections.Generic;

namespace GalleryOfART.Persistence.Models;

public partial class tag
{
    public Guid id { get; set; }

    public string slug { get; set; } = null!;

    public string title { get; set; } = null!;

    public virtual ICollection<artwork> artworks { get; set; } = new List<artwork>();
}
