using System;
using System.Collections.Generic;

namespace GalleryOfART.Persistence.Models;

public partial class artwork
{
    public Guid id { get; set; }

    public string slug { get; set; } = null!;

    public int? harvard_id { get; set; }

    public string? harvard_image { get; set; }

    public string title { get; set; } = null!;

    public Guid? artist_id { get; set; }

    public string? year_created_raw { get; set; }

    public int? year_created { get; set; }

    public string? materials { get; set; }

    public string? technique { get; set; }

    public string? dimensions_cm { get; set; }

    public string? description_md { get; set; }

    public Guid? cover_media_id { get; set; }

    public string? source_url { get; set; }

    public bool is_published { get; set; }

    public Guid? created_by { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual artist? artist { get; set; }

    public virtual ICollection<collection_item> collection_items { get; set; } = new List<collection_item>();

    public virtual media_file? cover_media { get; set; }

    public virtual user? created_byNavigation { get; set; }

    public virtual ICollection<tag> tags { get; set; } = new List<tag>();
}
