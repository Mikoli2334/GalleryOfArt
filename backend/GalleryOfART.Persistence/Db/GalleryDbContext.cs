using System;
using System.Collections.Generic;
using GalleryOfART.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace GalleryOfART.Persistence.Db;

public partial class GalleryDbContext : DbContext
{
    public GalleryDbContext(DbContextOptions<GalleryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<artist> artists { get; set; }

    public virtual DbSet<artwork> artworks { get; set; }

    public virtual DbSet<collection> collections { get; set; }

    public virtual DbSet<collection_item> collection_items { get; set; }

    public virtual DbSet<media_file> media_files { get; set; }

    public virtual DbSet<post> posts { get; set; }

    public virtual DbSet<tag> tags { get; set; }

    public virtual DbSet<user> users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("ai", "job_status_t", new[] { "queued", "running", "succeeded", "failed" })
            .HasPostgresEnum("content", "condition_t", new[] { "unknown", "excellent", "good", "fair", "poor" })
            .HasPostgresEnum("social", "target_t", new[] { "artwork", "post" })
            .HasPostgresExtension("citext")
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("uuid-ossp")
            .HasPostgresExtension("vector");

        modelBuilder.Entity<artist>(entity =>
        {
            entity.HasKey(e => e.id).HasName("artists_pkey");

            entity.ToTable("artists", "common");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.updated_at).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<artwork>(entity =>
        {
            entity.HasKey(e => e.id).HasName("artworks_pkey");

            entity.ToTable("artworks", "content");

            entity.HasIndex(e => e.harvard_id, "artworks_harvard_idx");

            entity.HasIndex(e => e.slug, "artworks_slug_key").IsUnique();

            entity.HasIndex(e => e.harvard_id, "uq_artworks_harvard").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.dimensions_cm).HasColumnType("jsonb");
            entity.Property(e => e.is_published).HasDefaultValue(false);
            entity.Property(e => e.updated_at).HasDefaultValueSql("now()");

            entity.HasOne(d => d.artist).WithMany(p => p.artworks)
                .HasForeignKey(d => d.artist_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("artworks_artist_id_fkey");

            entity.HasOne(d => d.cover_media).WithMany(p => p.artworks)
                .HasForeignKey(d => d.cover_media_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("artworks_cover_media_id_fkey");

            entity.HasOne(d => d.created_byNavigation).WithMany(p => p.artworks)
                .HasForeignKey(d => d.created_by)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("artworks_created_by_fkey");

            entity.HasMany(d => d.tags).WithMany(p => p.artworks)
                .UsingEntity<Dictionary<string, object>>(
                    "artwork_tag",
                    r => r.HasOne<tag>().WithMany()
                        .HasForeignKey("tag_id")
                        .HasConstraintName("artwork_tags_tag_id_fkey"),
                    l => l.HasOne<artwork>().WithMany()
                        .HasForeignKey("artwork_id")
                        .HasConstraintName("artwork_tags_artwork_id_fkey"),
                    j =>
                    {
                        j.HasKey("artwork_id", "tag_id").HasName("artwork_tags_pkey");
                        j.ToTable("artwork_tags", "content");
                    });
        });

        modelBuilder.Entity<collection>(entity =>
        {
            entity.HasKey(e => e.id).HasName("collections_pkey");

            entity.ToTable("collections", "content");

            entity.HasIndex(e => e.slug, "collections_slug_key").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");

            entity.HasOne(d => d.cover_media).WithMany(p => p.collections)
                .HasForeignKey(d => d.cover_media_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("collections_cover_media_id_fkey");

            entity.HasOne(d => d.created_byNavigation).WithMany(p => p.collections)
                .HasForeignKey(d => d.created_by)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("collections_created_by_fkey");
        });

        modelBuilder.Entity<collection_item>(entity =>
        {
            entity.HasKey(e => new { e.collection_id, e.artwork_id }).HasName("collection_items_pkey");

            entity.ToTable("collection_items", "content");

            entity.Property(e => e.sort_index).HasDefaultValue(0);

            entity.HasOne(d => d.artwork).WithMany(p => p.collection_items)
                .HasForeignKey(d => d.artwork_id)
                .HasConstraintName("collection_items_artwork_id_fkey");

            entity.HasOne(d => d.collection).WithMany(p => p.collection_items)
                .HasForeignKey(d => d.collection_id)
                .HasConstraintName("collection_items_collection_id_fkey");
        });

        modelBuilder.Entity<media_file>(entity =>
        {
            entity.HasKey(e => e.id).HasName("media_files_pkey");

            entity.ToTable("media_files", "common");

            entity.HasIndex(e => new { e.bucket, e.object_key }, "media_files_bucket_object_key_key").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");

            entity.HasOne(d => d.uploaded_byNavigation).WithMany(p => p.media_files)
                .HasForeignKey(d => d.uploaded_by)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("media_files_uploaded_by_fkey");
        });

        modelBuilder.Entity<post>(entity =>
        {
            entity.HasKey(e => e.id).HasName("posts_pkey");

            entity.ToTable("posts", "content");

            entity.HasIndex(e => e.slug, "posts_slug_key").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.updated_at).HasDefaultValueSql("now()");

            entity.HasOne(d => d.author).WithMany(p => p.posts)
                .HasForeignKey(d => d.author_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("posts_author_id_fkey");

            entity.HasOne(d => d.cover_media).WithMany(p => p.posts)
                .HasForeignKey(d => d.cover_media_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("posts_cover_media_id_fkey");
        });

        modelBuilder.Entity<tag>(entity =>
        {
            entity.HasKey(e => e.id).HasName("tags_pkey");

            entity.ToTable("tags", "content");

            entity.HasIndex(e => e.slug, "tags_slug_key").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.id).HasName("users_pkey");

            entity.ToTable("users", "common");

            entity.HasIndex(e => e.email, "users_email_key").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.email).HasColumnType("citext");
            entity.Property(e => e.role).HasDefaultValueSql("'user'::text");
            entity.Property(e => e.updated_at).HasDefaultValueSql("now()");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
