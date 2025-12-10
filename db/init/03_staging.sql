-- ============================================
-- 03_staging.sql
-- Harvard API staging: raw + projection
-- ============================================


CREATE SCHEMA IF NOT EXISTS staging;

 
CREATE TABLE IF NOT EXISTS staging.harvard_objects (
  id              BIGINT PRIMARY KEY,   -- Harvard object id
  title           TEXT,
  dated           TEXT,
  classification  TEXT,
  technique       TEXT,
  medium          TEXT,
  culture         TEXT,
  period          TEXT,
  department      TEXT,
  people          JSONB,            
  primaryimageurl TEXT,
  url             TEXT,
  raw             JSONB NOT NULL,       
  fetched_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);


CREATE INDEX IF NOT EXISTS harvard_objects_class_idx
  ON staging.harvard_objects (classification);

CREATE INDEX IF NOT EXISTS harvard_objects_raw_gin_idx
  ON staging.harvard_objects
  USING GIN (raw);



-- --------------------------------------------
-- Удобное представление только для картин
-- (paintings) + имя художника из people
-- --------------------------------------------
CREATE OR REPLACE VIEW staging.harvard_paintings_v AS
SELECT
  o.id              AS harvard_id,
  o.title,
  o.dated,
  o.classification,
  o.technique,
  o.medium,
  o.culture,
  o.period,
  o.department,
  o.primaryimageurl,
  o.url,
  -- первое имя художника из массива people
  COALESCE(
    (
      SELECT p ->> 'name'
      FROM jsonb_path_query(o.people, '$[*]') AS p
      WHERE p ? 'name'
      LIMIT 1
    ),
    NULL
  ) AS artist_name,
  o.raw
FROM staging.harvard_objects o
WHERE o.classification ILIKE 'Painting%';