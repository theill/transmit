/*
   Friday, March 12, 20101:19:00 PM
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
ALTER TABLE dbo.Settings
	DROP CONSTRAINT DF_Settings_CreatedAt
GO
CREATE TABLE dbo.Tmp_Settings
	(
	ID int NOT NULL IDENTITY (1, 1),
	Login nvarchar(64) COLLATE Danish_Norwegian_CI_AS NULL,
	CompanyName nvarchar(255) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	CompanyLogo nvarchar(255) NULL,
	InternalUrl nvarchar(255) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	ExternalUrl nvarchar(255) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	UploadUrl nvarchar(255) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	StorageLocation nvarchar(255) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	RestrictSettingsToGroup nvarchar(128) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	LdapFilterName nvarchar(1024) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	LdapFilterMail nvarchar(1024) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	LdapFilterLogin nvarchar(1024) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	LdapSizeLimit int NOT NULL,
	MailSecure bit NOT NULL,
	MailReplyTo nvarchar(128) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	UploadSizeLimit int NOT NULL,
	ShareMailSubject nvarchar(128) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	ShareMailBodyPlain text COLLATE Danish_Norwegian_CI_AS NOT NULL,
	ShareMailBodyHtml text COLLATE Danish_Norwegian_CI_AS NOT NULL,
	ShareDefaultMessage nvarchar(1024) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	RequestMailSubject nvarchar(128) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	RequestMailBodyPlain text COLLATE Danish_Norwegian_CI_AS NOT NULL,
	RequestMailBodyHtml text COLLATE Danish_Norwegian_CI_AS NOT NULL,
	RequestDefaultMessage nvarchar(1024) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	UploadMailSubject nvarchar(128) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	UploadMailBodyPlain text COLLATE Danish_Norwegian_CI_AS NOT NULL,
	UploadMailBodyHtml text COLLATE Danish_Norwegian_CI_AS NOT NULL,
	UploadDefaultMessage nvarchar(1024) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	CreatedAt datetime NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_Settings SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_Settings ADD CONSTRAINT
	DF_Settings_CreatedAt DEFAULT (getdate()) FOR CreatedAt
GO
SET IDENTITY_INSERT dbo.Tmp_Settings ON
GO
IF EXISTS(SELECT * FROM dbo.Settings)
	 EXEC('INSERT INTO dbo.Tmp_Settings (ID, Login, CompanyName, InternalUrl, ExternalUrl, UploadUrl, StorageLocation, RestrictSettingsToGroup, LdapFilterName, LdapFilterMail, LdapFilterLogin, LdapSizeLimit, MailSecure, MailReplyTo, UploadSizeLimit, ShareMailSubject, ShareMailBodyPlain, ShareMailBodyHtml, ShareDefaultMessage, RequestMailSubject, RequestMailBodyPlain, RequestMailBodyHtml, RequestDefaultMessage, UploadMailSubject, UploadMailBodyPlain, UploadMailBodyHtml, UploadDefaultMessage, CreatedAt)
		SELECT ID, Login, CompanyName, InternalUrl, ExternalUrl, UploadUrl, StorageLocation, RestrictSettingsToGroup, LdapFilterName, LdapFilterMail, LdapFilterLogin, LdapSizeLimit, MailSecure, MailReplyTo, UploadSizeLimit, ShareMailSubject, ShareMailBodyPlain, ShareMailBodyHtml, ShareDefaultMessage, RequestMailSubject, RequestMailBodyPlain, RequestMailBodyHtml, RequestDefaultMessage, UploadMailSubject, UploadMailBodyPlain, UploadMailBodyHtml, UploadDefaultMessage, CreatedAt FROM dbo.Settings WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_Settings OFF
GO
DROP TABLE dbo.Settings
GO
EXECUTE sp_rename N'dbo.Tmp_Settings', N'Settings', 'OBJECT' 
GO
ALTER TABLE dbo.Settings ADD CONSTRAINT
	PK_Settings PRIMARY KEY CLUSTERED 
	(
	ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
