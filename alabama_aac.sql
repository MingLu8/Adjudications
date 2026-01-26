CREATE TABLE [dbo].[aac] (
    [id]         UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()), 
    [state]      NVARCHAR(2)     NOT NULL, 
    [ndc]        NVARCHAR(20)     NOT NULL, 
    
    [price]      DECIMAL(18, 4)   NOT NULL, 
    
    [effective_date] DATE     NOT NULL DEFAULT (GETDATE()),
    [created_date] DATETIME       NOT NULL DEFAULT (GETDATE()),
    [updated_date] DATETIME       NOT NULL DEFAULT (GETDATE()),
    
    [is_active]   BIT              NOT NULL DEFAULT (1),

    CONSTRAINT [PK_aac] PRIMARY KEY NONCLUSTERED ([id])
);

CREATE CLUSTERED INDEX [IX_aac_CreatedDate] ON [dbo].[aac] ([created_date]);