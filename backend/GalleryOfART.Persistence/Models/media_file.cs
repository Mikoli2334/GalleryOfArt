using System;
using System.Collections.Generic;

namespace GalleryOfART.Persistence.Models;

public partial class media_file
{
    public Guid id { get; set; }

    public string bucket { get; set; } = null!;

    public string object_key { get; set; } = null!;

    public string mime_type { get; set; } = null!;

    public int? width { get; set; }

    public int? height { get; set; }

    public long? size_bytes { get; set; }

    public string? sha256 { get; set; }

    public Guid? uploaded_by { get; set; }

    public DateTime created_at { get; set; }

    public virtual ICollection<artwork> artworks { get; set; } = new List<artwork>();

    public virtual ICollection<collection> collections { get; set; } = new List<collection>();

    public virtual ICollection<post> posts { get; set; } = new List<post>();

    public virtual user? uploaded_byNavigation { get; set; }
}
