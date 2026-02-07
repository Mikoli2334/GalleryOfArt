import React, { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import "./GalleryPage.css";

export default function GalleryPage() {
  const navigate = useNavigate();

  const apiBase = useMemo(() => {
    const raw = import.meta.env.VITE_API_BASE_URL || "";
    return raw.replace(/\/+$/, "");
  }, []);

  const [artworks, setArtworks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!apiBase) {
      setError("VITE_API_BASE_URL is not set");
      setLoading(false);
      return;
    }

    let cancelled = false;

    async function load() {
      try {
        setLoading(true);
        setError("");

        const res = await fetch(`${apiBase}/api/Artworks`);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);

        const data = await res.json();
        if (!cancelled) setArtworks(Array.isArray(data) ? data : []);
      } catch (e) {
        if (!cancelled) setError("Failed to load artworks");
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, [apiBase]);

  if (loading) return <p className="center">Loading artworks…</p>;
  if (error) return <p className="center error">{error}</p>;

  return (
    <div className="page">
      <h1 className="title">Art Gallery</h1>

      <div className="grid">
        {artworks.map((a) => (
          <button
            key={a.id}
            className="card"
            type="button"
            onClick={() => navigate(`/artworks/${a.id}`)}
          >
            <div className="imgWrap">
              <img
                className="img"
                src={`${apiBase}/api/Artworks/image/${a.id}`}
                alt={a.title || "Artwork"}
                loading="lazy"
              />
            </div>

            <div className="cardTitle">{a.title || "Untitled"}</div>
          </button>
        ))}
      </div>
    </div>
  );
}