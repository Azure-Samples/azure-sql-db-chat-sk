CREATE SEQUENCE [web].[global_id] 
 AS [int]
 START WITH 1
 INCREMENT BY 1
GO

CREATE TABLE [web].[sessions]
(
    [id] INT DEFAULT (NEXT VALUE FOR [web].[global_id]) NOT NULL,
    [title] NVARCHAR (200) NOT NULL,
    [abstract] NVARCHAR (MAX) NOT NULL,
    [external_id] VARCHAR (100) COLLATE Latin1_General_100_BIN2 NOT NULL,
    [details] JSON NULL,
    [require_embeddings_update] BIT DEFAULT ((0)) NOT NULL,

    PRIMARY KEY CLUSTERED ([id] ASC),
    UNIQUE NONCLUSTERED ([title] ASC)
);
GO

ALTER TABLE [web].[sessions] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = OFF);
GO

CREATE TABLE [web].[sessions_details_embeddings]
(
    id INT NOT NULL,
    [session_id] INT NOT NULL,
    details_vector_text3 VECTOR(1536) NOT NULL
)
GO
CREATE CLUSTERED INDEX [ixc] ON [web].[sessions_details_embeddings]([session_id] ASC)
GO
CREATE NONCLUSTERED INDEX [ix__review_id] ON [web].[sessions_details_embeddings] ([session_id] ASC, [id] ASC)
GO
ALTER TABLE [web].[sessions_details_embeddings] ADD CONSTRAINT pk__sessions_details_embeddings PRIMARY KEY NONCLUSTERED ([id] ASC)
GO

CREATE TABLE [web].[sessions_abstracts_embeddings]
(
    id INT NOT NULL,
    [session_id] INT NOT NULL,
    [abstract_vector_text3] vector(1536) not null
)
GO
CREATE CLUSTERED INDEX [ixc] ON [web].[sessions_abstracts_embeddings]([session_id] ASC)
GO
CREATE NONCLUSTERED INDEX [ix__review_id] ON [web].[sessions_abstracts_embeddings] ([session_id] ASC, [id] ASC)
GO
ALTER TABLE [web].[sessions_abstracts_embeddings] ADD CONSTRAINT pk__sessions_abstracts_embeddings PRIMARY KEY NONCLUSTERED ([id] ASC)
GO
