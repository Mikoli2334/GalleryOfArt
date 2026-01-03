import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import "./GalleryPage.css";

export default function GalleryPage() {
  const [artworks, setArtworks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    fetch("/api/Artworks")
      .then((res) => {
        if (!res.ok) throw new Error();
        return res.json();
      })
      .then((data) => {
        setArtworks(data);
        setLoading(false);
      })
      .catch(() => {
        setError(true);
        setLoading(false);
      });
  }, []);

  if (loading) return <p className="center">Loading artworks…</p>;
  if (error) return <p className="center error">Failed to load artworks</p>;

  return (
    <div className="page">
      <h1 className="title">Art Gallery</h1>

      <div className="grid">
        {artworks.map((a) => (
          <button
            key={a.id}
            className="card"
            onClick={() => navigate(`/artworks/${a.id}`)}
            type="button"
          >
            <div className="imgWrap">
              <img
                className="img"
                src={`/api/Artworks/image/${a.id}`}
                alt={a.title || "Artwork"}
              />
            </div>

            <div className="cardTitle">{a.title || "Untitled"}</div>
          </button>
        ))}
      </div>
    </div>
  );
}