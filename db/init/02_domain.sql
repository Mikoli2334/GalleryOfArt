-- =============================
--       COMMON SCHEMA
-- =============================

CREATE TABLE IF NOT EXISTS common.users (
  id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  external_id  TEXT,
  email        CITEXT UNIQUE,
  display_name TEXT NOT NULL,
  avatar_url   TEXT,
  role         TEXT NOT NULL DEFAULT 'user',
  created_at   TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at   TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS common.artists (
  id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  full_name    TEXT NOT NULL,
  pseudonym    TEXT,
  birth_year   INT,
  death_year   INT,
  country      TEXT,
  bio_md       TEXT,
  created_at   TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at   TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_artists_fullname_birth
  ON common.artists (full_name, COALESCE(birth_year, -1));

CREATE TABLE IF NOT EXISTS common.media_files (
  id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  bucket       TEXT NOT NULL,
  object_key   TEXT NOT NULL,
  mime_type    TEXT NOT NULL,
  width        INT,
  height       INT,
  size_bytes   BIGINT,
  sha256       TEXT,
  uploaded_by  UUID REFERENCES common.users(id) ON DELETE SET NULL,
  created_at   TIMESTAMPTZ NOT NULL DEFAULT now(),
  UNIQUE (bucket, object_key)
);



-- =============================
--       CONTENT SCHEMA
-- =============================

DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_type t
    JOIN pg_namespace n ON n.oid = t.typnamespace
    WHERE t.typname='condition_t' AND n.nspname='content'
  ) THEN
    CREATE TYPE content.condition_t AS ENUM
      ('unknown','excellent','good','fair','poor');
  END IF;
END$$;

CREATE TABLE IF NOT EXISTS content.artworks (
  id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),

  slug           TEXT NOT NULL UNIQUE,
  harvard_id     INT,                                        
  harvard_image  TEXT,                                       

  title          TEXT NOT NULL,
  artist_id      UUID REFERENCES common.artists(id) ON DELETE SET NULL,

  year_created_raw TEXT,                                     
  year_created     INT,                                      

  materials      TEXT,
  technique      TEXT,
  dimensions_cm  JSONB,                                     
  description_md TEXT,

  cover_media_id UUID REFERENCES common.media_files(id) ON DELETE SET NULL,
  source_url     TEXT,

  is_published   BOOLEAN NOT NULL DEFAULT FALSE,
  condition      content.condition_t NOT NULL DEFAULT 'unknown',

  created_by     UUID REFERENCES common.users(id) ON DELETE SET NULL,
  created_at     TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at     TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS artworks_harvard_idx
  ON content.artworks (harvard_id);



CREATE TABLE IF NOT EXISTS content.tags (
  id    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  slug  TEXT NOT NULL UNIQUE,
  title TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS content.artwork_tags (
  artwork_id UUID NOT NULL REFERENCES content.artworks(id) ON DELETE CASCADE,
  tag_id     UUID NOT NULL REFERENCES content.tags(id) ON DELETE CASCADE,
  PRIMARY KEY (artwork_id, tag_id)
);

CREATE TABLE IF NOT EXISTS content.collections (
  id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  slug           TEXT NOT NULL UNIQUE,
  title          TEXT NOT NULL,
  description_md TEXT,
  cover_media_id UUID REFERENCES common.media_files(id) ON DELETE SET NULL,
  created_by     UUID REFERENCES common.users(id) ON DELETE SET NULL,
  created_at     TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS content.collection_items (
  collection_id UUID NOT NULL REFERENCES content.collections(id) ON DELETE CASCADE,
  artwork_id    UUID NOT NULL REFERENCES content.artworks(id)    ON DELETE CASCADE,
  sort_index    INT NOT NULL DEFAULT 0,
  PRIMARY KEY (collection_id, artwork_id)
);

CREATE TABLE IF NOT EXISTS content.posts (
  id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  slug           TEXT NOT NULL UNIQUE,
  title          TEXT NOT NULL,
  body_md        TEXT NOT NULL,
  cover_media_id UUID REFERENCES common.media_files(id) ON DELETE SET NULL,
  author_id      UUID REFERENCES common.users(id) ON DELETE SET NULL,
  published_at   TIMESTAMPTZ,
  created_at     TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at     TIMESTAMPTZ NOT NULL DEFAULT now()
);



-- =============================
--     SOCIAL SCHEMA
-- =============================

DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_type t JOIN pg_namespace n ON n.oid=t.typnamespace
    WHERE t.typname='target_t' AND n.nspname='social'
  ) THEN
    CREATE TYPE social.target_t AS ENUM ('artwork','post');
  END IF;
END$$;

CREATE TABLE IF NOT EXISTS social.comments (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  target_type social.target_t NOT NULL,
  target_id   UUID NOT NULL,
  parent_id   UUID REFERENCES social.comments(id) ON DELETE CASCADE,
  author_id   UUID REFERENCES common.users(id) ON DELETE SET NULL,
  body_md     TEXT NOT NULL,
  body_plain  TEXT,
  is_public   BOOLEAN NOT NULL DEFAULT TRUE,
  created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS comments_target_idx
  ON social.comments (target_type, target_id);

CREATE INDEX IF NOT EXISTS comments_search_idx
  ON social.comments USING GIN (to_tsvector('simple', coalesce(body_plain,'')));

CREATE TABLE IF NOT EXISTS social.reactions (
  user_id     UUID NOT NULL REFERENCES common.users(id) ON DELETE CASCADE,
  target_type social.target_t NOT NULL,
  target_id   UUID NOT NULL,
  reaction    TEXT NOT NULL,
  created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
  PRIMARY KEY (user_id, target_type, target_id, reaction)
);

CREATE TABLE IF NOT EXISTS social.favourites (
  user_id     UUID NOT NULL REFERENCES common.users(id) ON DELETE CASCADE,
  target_type social.target_t NOT NULL,
  target_id   UUID NOT NULL,
  created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
  PRIMARY KEY (user_id, target_type, target_id)
);



-- =============================
--         AI SCHEMA
-- =============================

DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_type t JOIN pg_namespace n ON n.oid=t.typnamespace
    WHERE t.typname='job_status_t' AND n.nspname='ai'
  ) THEN
    CREATE TYPE ai.job_status_t AS ENUM ('queued','running','succeeded','failed');
  END IF;
END$$;

CREATE TABLE IF NOT EXISTS ai.styles (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  code        TEXT NOT NULL UNIQUE,
  title       TEXT NOT NULL,
  description TEXT
);

CREATE TABLE IF NOT EXISTS ai.models (
  id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name       TEXT NOT NULL,
  version    TEXT NOT NULL,
  task       TEXT NOT NULL,
  params     JSONB,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  UNIQUE (name, version)
);

CREATE TABLE IF NOT EXISTS ai.artwork_embeddings (
  artwork_id UUID PRIMARY KEY REFERENCES content.artworks(id) ON DELETE CASCADE,
  model_id   UUID NOT NULL REFERENCES ai.models(id),
  vector     VECTOR(768) NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ai_emb_vec_idx
  ON ai.artwork_embeddings USING ivfflat (vector vector_cosine_ops) WITH (lists=200);

CREATE TABLE IF NOT EXISTS ai.artwork_style_predictions (
  id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  artwork_id UUID NOT NULL REFERENCES content.artworks(id) ON DELETE CASCADE,
  model_id   UUID NOT NULL REFERENCES ai.models(id),
  style_id   UUID NOT NULL REFERENCES ai.styles(id),
  confidence REAL NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  UNIQUE (artwork_id, model_id, style_id)
);

CREATE TABLE IF NOT EXISTS ai.inference_jobs (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  task        TEXT NOT NULL,
  payload     JSONB NOT NULL,
  model_id    UUID REFERENCES ai.models(id),
  status      ai.job_status_t NOT NULL DEFAULT 'queued',
  error       TEXT,
  created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
  started_at  TIMESTAMPTZ,
  finished_at TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ai_jobs_idx
  ON ai.inference_jobs (status, created_at);


-- =============================
--       PRICING SCHEMA
-- =============================

CREATE TABLE IF NOT EXISTS pricing.models (
  id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name       TEXT NOT NULL,
  version    TEXT NOT NULL,
  approach   TEXT NOT NULL,
  params     JSONB,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  UNIQUE (name, version)
);

CREATE TABLE IF NOT EXISTS pricing.valuations (
  id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  artwork_id    UUID NOT NULL REFERENCES content.artworks(id) ON DELETE CASCADE,
  model_id      UUID NOT NULL REFERENCES pricing.models(id),
  valuation_usd NUMERIC(18,2) NOT NULL,
  currency      CHAR(3) NOT NULL DEFAULT 'USD',
  ci_low_usd    NUMERIC(18,2),
  ci_high_usd   NUMERIC(18,2),
  features_used JSONB,
  created_at    TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS pricing_val_idx
  ON pricing.valuations (artwork_id, created_at DESC);

CREATE MATERIALIZED VIEW IF NOT EXISTS pricing.latest_valuation AS
SELECT DISTINCT ON (artwork_id)
  artwork_id, id AS valuation_id, valuation_usd, currency, created_at
FROM pricing.valuations
ORDER BY artwork_id, created_at DESC;



-- =============================
--         INFRA SCHEMA
-- =============================

CREATE TABLE IF NOT EXISTS infra.outbox (
  id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  aggregate    TEXT NOT NULL,
  aggregate_id UUID NOT NULL,
  event_type   TEXT NOT NULL,
  payload      JSONB NOT NULL,
  created_at   TIMESTAMPTZ NOT NULL DEFAULT now(),
  published_at TIMESTAMPTZ,
  attempts     INT NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS outbox_pub_idx
  ON infra.outbox (published_at);

CREATE INDEX IF NOT EXISTS outbox_aggr_idx
  ON infra.outbox (aggregate, aggregate_id);

CREATE TABLE IF NOT EXISTS infra.audit_log (
  id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id      UUID REFERENCES common.users(id) ON DELETE SET NULL,
  action       TEXT NOT NULL,
  target_type  TEXT,
  target_id    UUID,
  meta         JSONB,
  created_at   TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS audit_target_idx
  ON infra.audit_log (target_type, target_id, created_at);

CREATE INDEX IF NOT EXISTS audit_user_idx
  ON infra.audit_log (user_id, created_at);

CREATE TABLE IF NOT EXISTS infra.api_keys (
  id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  owner_user_id UUID REFERENCES common.users(id) ON DELETE SET NULL,
  name          TEXT NOT NULL,
  key_hash      TEXT NOT NULL,
  scopes        TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[],
  rate_limit_rps INT,
  is_active     BOOLEAN NOT NULL DEFAULT TRUE,
  created_at    TIMESTAMPTZ NOT NULL DEFAULT now(),
  revoked_at    TIMESTAMPTZ,
  UNIQUE (key_hash)
);

CREATE TABLE IF NOT EXISTS infra.idempotency (
  idempotency_key TEXT PRIMARY KEY,
  requester_id    UUID,
  request_fingerprint TEXT,
  response_code   INT,
  response_body   JSONB,
  created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
  expires_at      TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS idemp_exp_idx
  ON infra.idempotency (expires_at);

CREATE TABLE IF NOT EXISTS infra.webhooks (
  id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  owner_user_id UUID REFERENCES common.users(id) ON DELETE SET NULL,
  url           TEXT NOT NULL,
  secret        TEXT,
  events        TEXT[] NOT NULL,
  is_active     BOOLEAN NOT NULL DEFAULT TRUE,
  created_at    TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS infra.webhook_deliveries (
  id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  webhook_id   UUID NOT NULL REFERENCES infra.webhooks(id) ON DELETE CASCADE,
  outbox_id    UUID NOT NULL REFERENCES infra.outbox(id)     ON DELETE CASCADE,
  status       TEXT NOT NULL,
  http_code    INT,
  error        TEXT,
  attempt      INT NOT NULL DEFAULT 0,
  created_at   TIMESTAMPTZ NOT NULL DEFAULT now(),
  delivered_at TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS wh_deliv_idx
  ON infra.webhook_deliveries (status, created_at);