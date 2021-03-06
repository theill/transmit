/*
   Monday, March 22, 20102:44:13 PM
   User: 
   Server: THEILL-PC\
   Database: transmit_development
   Application: 
*/

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Packages
	DROP CONSTRAINT DF_Packages_Scanned
GO
ALTER TABLE dbo.Packages
	DROP CONSTRAINT DF_Packages_CreatedAt
GO
CREATE TABLE dbo.Tmp_Packages
	(
	ID int NOT NULL IDENTITY (1, 1),
	SenderMail nvarchar(128) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	Message text COLLATE Danish_Norwegian_CI_AS NULL,
	Code nvarchar(64) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	Scanned bit NOT NULL,
	Status char(1) NOT NULL,
	ExpiresAt datetime NULL,
	CreatedAt datetime NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_Packages SET (LOCK_ESCALATION = TABLE)
GO
DECLARE @v sql_variant 
SET @v = N'O = Open, E = Expired'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'dbo', N'TABLE', N'Tmp_Packages', N'COLUMN', N'Status'
GO
ALTER TABLE dbo.Tmp_Packages ADD CONSTRAINT
	DF_Packages_Scanned DEFAULT ((0)) FOR Scanned
GO
ALTER TABLE dbo.Tmp_Packages ADD CONSTRAINT
	DF_Packages_Status DEFAULT 'O' FOR Status
GO
ALTER TABLE dbo.Tmp_Packages ADD CONSTRAINT
	DF_Packages_CreatedAt DEFAULT (getdate()) FOR CreatedAt
GO
SET IDENTITY_INSERT dbo.Tmp_Packages ON
GO
IF EXISTS(SELECT * FROM dbo.Packages)
	 EXEC('INSERT INTO dbo.Tmp_Packages (ID, SenderMail, Message, Code, Scanned, ExpiresAt, CreatedAt)
		SELECT ID, SenderMail, Message, Code, Scanned, ExpiresAt, CreatedAt FROM dbo.Packages WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_Packages OFF
GO
ALTER TABLE dbo.Files
	DROP CONSTRAINT FK_Files_Packages
GO
ALTER TABLE dbo.Transfers
	DROP CONSTRAINT FK_Transfers_Transfers
GO
DROP TABLE dbo.Packages
GO
EXECUTE sp_rename N'dbo.Tmp_Packages', N'Packages', 'OBJECT' 
GO
ALTER TABLE dbo.Packages ADD CONSTRAINT
	PK_Packages PRIMARY KEY CLUSTERED 
	(
	ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.Packages ADD CONSTRAINT
	IX_Packages UNIQUE NONCLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Transfers ADD CONSTRAINT
	FK_Transfers_Transfers FOREIGN KEY
	(
	PackageID
	) REFERENCES dbo.Packages
	(
	ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.Transfers SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Files ADD CONSTRAINT
	FK_Files_Packages FOREIGN KEY
	(
	PackageID
	) REFERENCES dbo.Packages
	(
	ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.Files SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
