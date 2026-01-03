import { Routes, Route } from "react-router-dom";
import GalleryPage from "./pages/GalleryPage";
import ArtworkDetailsPage from "./pages/ArtworkDetailsPage";

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<GalleryPage />} />
      <Route path="/artworks/:id" element={<ArtworkDetailsPage />} />
    </Routes>
  );
}