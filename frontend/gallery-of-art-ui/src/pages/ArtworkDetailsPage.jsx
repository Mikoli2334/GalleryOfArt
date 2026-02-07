import React, { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import "./ArtworkDetailsPage.css";

export default function ArtworkDetailsPage() {
  const { id } = useParams();

  const apiBase = useMemo(() => {
    const raw = import.meta.env.VITE_API_BASE_URL || "";
    return raw.replace(/\/+$/, ""); // убираем хвостовые /
  }, []);

  const [artwork, setArtwork] = useState(null);
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

        const res = await fetch(`${apiBase}/api/Artworks/${id}`);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);

        const data = await res.json();
        if (!cancelled) setArtwork(data);
      } catch (e) {
        if (!cancelled) setError("Failed to load artwork");
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, [apiBase, id]);

  if (loading) return <p className="center">Loading…</p>;

  if (error || !artwork) {
    return (
      <div className="page">
        <Link to="/" className="back">← Back</Link>
        <p className="error">{error || "Failed to load artwork"}</p>
      </div>
    );
  }

  const artistName = artwork.artist?.fullName ?? "—";

  return (
    <div className="page">
      <Link to="/" className="back">← Back to gallery</Link>

      <div className="layout">
        <div className="left">
          <img
            className="bigImg"
            src={`${apiBase}/api/Artworks/image/${artwork.id}`}
            alt={artwork.title || "Artwork"}
          />
        </div>

        <div className="right">
          <h2 className="h2">{artwork.title || "Untitled"}</h2>

          <div className="metaRow">
            <span className="label">Year</span>
            <span>{artwork.yearCreated ?? "—"}</span>
          </div>

          <div className="metaRow">
            <span className="label">Artist</span>
            <span>{artistName}</span>
          </div>

          {artwork.artist && (
            <>
              <div className="metaRow">
                <span className="label">Birth</span>
                <span>{artwork.artist.birthYear ?? "—"}</span>
              </div>

              <div className="metaRow">
                <span className="label">Death</span>
                <span>{artwork.artist.deathYear ?? "—"}</span>
              </div>

              <div className="metaRow">
                <span className="label">Artist artworks</span>
                <span>{artwork.artist.artworkCount ?? "—"}</span>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
}