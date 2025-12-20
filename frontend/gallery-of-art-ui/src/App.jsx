import { useEffect, useState } from "react";
import "./App.css";

function App() {
  const [artworks, setArtworks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  useEffect(() => {
    fetch("/api/Artworks")
      .then(res => {
        if (!res.ok) throw new Error();
        return res.json();
      })
      .then(data => {
        setArtworks(data);
        setLoading(false);
      })
      .catch(() => {
        setError(true);
        setLoading(false);
      });
  }, []);

  if (loading) return <p style={{ textAlign: "center" }}>Loading artworks…</p>;
  if (error) return <p style={{ textAlign: "center", color: "red" }}>Failed to load artworks</p>;

  return (
    <div style={{ padding: "2rem" }}>
      <h1 style={{ textAlign: "center" }}>Art Gallery</h1>

      <div
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(auto-fill, minmax(250px, 1fr))",
          gap: "20px",
          marginTop: "2rem",
        }}
      >
        {artworks.map(a => (
          <div key={a.id} style={{ textAlign: "center" }}>
            <img
              src={`/api/Artworks/image/${a.id}`}
              alt={a.title}
              style={{ width: "100%", borderRadius: "8px" }}
            />
            <h3>{a.title || "Untitled"}</h3>
            {a.artist && <p>{a.artist.fullName}</p>}
          </div>
        ))}
      </div>
    </div>
  );
}

export default App;