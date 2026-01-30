import React from "react";
import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import "./ArtworkDetailsPage.css";

export default function ArtworkDetailsPage() {
  const { id } = useParams();
  const [artwork, setArtwork] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  useEffect(() => {
    setLoading(true);
    setError(false);

    fetch(`http://localhost:5010/api/Artworks/${id}`)
      .then((res) => {
        if (!res.ok) throw new Error();
        return res.json();
      })
      .then((data) => {
        setArtwork(data);
        setLoading(false);
      })
      .catch(() => {
        setError(true);
        setLoading(false);
      });
  }, [id]);

  if (loading) return <p className="center">Loading…</p>;
  if (error || !artwork)
    return (
      <div className="page">
        <Link to="/" className="back">
          ← Back
        </Link>
        <p className="error">Failed to load artwork</p>
      </div>
    );

  const artist = artwork.artist?.fullName;

  return (
    <div className="page">
      <Link to="/" className="back">
        ← Back to gallery
      </Link>

      <div className="layout">
        <div className="left">
          <img
            className="bigImg"
            src={`/api/Artworks/image/${artwork.id}`}
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
            <span>{artist ?? "—"}</span>
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