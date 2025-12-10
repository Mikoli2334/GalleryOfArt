using System;
using System.Collections.Generic;

namespace GalleryOfART.Persistence.Models;

public partial class collection_item
{
    public Guid collection_id { get; set; }

    public Guid artwork_id { get; set; }

    public int sort_index { get; set; }

    public virtual artwork artwork { get; set; } = null!;

    public virtual collection collection { get; set; } = null!;
}
