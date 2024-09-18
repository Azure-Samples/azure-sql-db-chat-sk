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

CREATE NONCLUSTERED INDEX [ix2]
    ON [web].[sessions_speakers]([speaker_id] ASC);
GO

ALTER TABLE [web].[sessions] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = OFF);
GO


