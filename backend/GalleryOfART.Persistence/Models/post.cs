using System;
using System.Collections.Generic;

namespace GalleryOfART.Persistence.Models;

public partial class post
{
    public Guid id { get; set; }

    public string slug { get; set; } = null!;

    public string title { get; set; } = null!;

    public string body_md { get; set; } = null!;

    public Guid? cover_media_id { get; set; }

    public Guid? author_id { get; set; }

    public DateTime? published_at { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual user? author { get; set; }

    public virtual media_file? cover_media { get; set; }
}
