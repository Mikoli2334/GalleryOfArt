const API_BASE = "http://localhost:5212/api";

document.addEventListener("DOMContentLoaded", () => {
  loadArtworks();
});

async function loadArtworks() {
  const gallery = document.getElementById("gallery");
  const emptyState = document.getElementById("gallery-empty");
  const errorState = document.getElementById("gallery-error");

  gallery.innerHTML = "Loading artworks...";
  emptyState.classList.add("hidden");
  errorState.classList.add("hidden");

  try {
    const response = await fetch(`${API_BASE}/Artworks`);
    if (!response.ok) throw new Error();

    const artworks = await response.json();
    if (!artworks.length) {
      gallery.innerHTML = "";
      emptyState.classList.remove("hidden");
      return;
    }

    gallery.innerHTML = "";

    artworks.forEach(a => {
      const card = document.createElement("div");
      card.className = "artwork-card";

      const imgWrapper = document.createElement("div");
      imgWrapper.className = "artwork-image-wrapper";

      const img = document.createElement("img");
      img.className = "artwork-image";
      img.src = `${API_BASE}/Artworks/image/${a.id}`;
      img.alt = a.title || "Artwork";

      imgWrapper.appendChild(img);
      card.appendChild(imgWrapper);

      const title = document.createElement("div");
      title.className = "artwork-title";
      title.textContent = a.title || "Untitled";
      card.appendChild(title);

      const meta = document.createElement("div");
      meta.className = "artwork-meta";

      const artist = a.artist ? a.artist.fullName : null;
      const year = a.yearCreated;

      let details = [];
      if (year) details.push(`Year: ${year}`);
      if (artist) details.push(`Artist: ${artist}`);

      meta.textContent = details.join(" • ") || "No details available";
      card.appendChild(meta);

      gallery.appendChild(card);
    });
  } catch (err) {
    gallery.innerHTML = "";
    errorState.classList.remove("hidden");
  }
}