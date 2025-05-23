DROP TABLE IF EXISTS [dbo].[customers];
DROP TABLE IF EXISTS [dbo].[claims];
DROP TABLE IF EXISTS [dbo].[policies];
DROP TABLE IF EXISTS [dbo].[communication_history];
GO

CREATE TABLE [dbo].[customers]
(
    [id] INT NOT NULL,
    [first_name] NVARCHAR(100) NOT NULL,
    [last_name] NVARCHAR(100) NOT NULL,
    [address] NVARCHAR(100) NOT NULL,
    [city] NVARCHAR(100) NOT NULL,
    [state] NVARCHAR(100) NOT NULL,
    [zip] NVARCHAR(100) NOT NULL,
    [country] NVARCHAR(100) NOT NULL,
    [email] NVARCHAR(100) NOT NULL,
    [details] JSON NULL,

    PRIMARY KEY NONCLUSTERED ([id] ASC),
    UNIQUE NONCLUSTERED ([email] ASC)
)

CREATE TABLE [dbo].[claims]
(
    [id] INT NOT NULL,
    [customer_id] int NOT NULL,
    [claim_type] VARCHAR (100) NOT NULL,
    [claim_date] DATETIME2(0) NOT NULL,
    [details] NVARCHAR(MAX) NOT NULL,

    PRIMARY KEY NONCLUSTERED ([id] ASC)
);
GO

CREATE TABLE [dbo].[policies]
(
    [id] INT NOT NULL,
    [customer_id] int NOT NULL,
    [type] VARCHAR (100) NOT NULL,
    [premium] DECIMAL(9,4) NOT NULL,
    [payment_type] VARCHAR(50) NOT NULL,
    [start_date] DATE NOT NULL, 
    [duration] VARCHAR(50) NOT NULL,
    [payment_amount] DECIMAL(9,4) NOT NULL,
    [additional_notes] NVARCHAR(MAX) NULL,

    PRIMARY KEY NONCLUSTERED ([id] ASC)
);
GO

CREATE TABLE [dbo].[communication_history]
(
    [id] INT NOT NULL,
    [customer_id] int NOT NULL,
    [communication_type] VARCHAR (100) NOT NULL,
    [communication_date] DATETIME2(0) NOT NULL,
    [details] NVARCHAR(MAX) NOT NULL,
    [embedding] VECTOR(1536) NULL

    PRIMARY KEY NONCLUSTERED ([id] ASC)
);

