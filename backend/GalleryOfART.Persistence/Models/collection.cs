using System;
using System.Collections.Generic;

namespace GalleryOfART.Persistence.Models;

public partial class collection
{
    public Guid id { get; set; }

    public string slug { get; set; } = null!;

    public string title { get; set; } = null!;

    public string? description_md { get; set; }

    public Guid? cover_media_id { get; set; }

    public Guid? created_by { get; set; }

    public DateTime created_at { get; set; }

    public virtual ICollection<collection_item> collection_items { get; set; } = new List<collection_item>();

    public virtual media_file? cover_media { get; set; }

    public virtual user? created_byNavigation { get; set; }
}
