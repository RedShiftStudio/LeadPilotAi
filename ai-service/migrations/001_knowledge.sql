CREATE EXTENSION IF NOT EXISTS vector;
CREATE TABLE IF NOT EXISTS knowledge_documents(id uuid PRIMARY KEY, organization_id uuid NOT NULL, filename text NOT NULL, created_at timestamptz NOT NULL DEFAULT now());
CREATE INDEX IF NOT EXISTS knowledge_documents_org_idx ON knowledge_documents(organization_id);
CREATE TABLE IF NOT EXISTS knowledge_chunks(id uuid PRIMARY KEY, document_id uuid NOT NULL REFERENCES knowledge_documents(id) ON DELETE CASCADE, organization_id uuid NOT NULL, body text NOT NULL, embedding vector(1536));
CREATE INDEX IF NOT EXISTS knowledge_chunks_org_idx ON knowledge_chunks(organization_id);
