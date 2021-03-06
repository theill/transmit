/****** Object:  Table [dbo].[Files]    Script Date: 01/06/2010 16:47:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Files](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PackageID] [int] NOT NULL,
	[FileHash] [nvarchar](512) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[FileSize] [bigint] NOT NULL,
	[DownloadCount] [int] NOT NULL CONSTRAINT [DF_Files_DownloadCount]  DEFAULT ((0)),
	[CreatedAt] [datetime] NOT NULL CONSTRAINT [DF_Files_CreatedAt]  DEFAULT (getdate()),
 CONSTRAINT [PK_Files] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[Files]  WITH CHECK ADD  CONSTRAINT [FK_Files_Packages] FOREIGN KEY([PackageID])
REFERENCES [dbo].[Packages] ([ID])
GO
ALTER TABLE [dbo].[Files] CHECK CONSTRAINT [FK_Files_Packages]

/****** Object:  Table [dbo].[Invitations]    Script Date: 01/06/2010 16:47:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Invitations](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SenderMail] [nvarchar](128) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[SenderDisplayName] [nvarchar](128) COLLATE Danish_Norwegian_CI_AS NULL,
	[Message] [text] COLLATE Danish_Norwegian_CI_AS NULL,
	[RecipientMail] [nvarchar](128) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[RecipientDisplayName] [nvarchar](128) COLLATE Danish_Norwegian_CI_AS NULL,
	[Code] [nvarchar](64) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[CreatedAt] [datetime] NOT NULL CONSTRAINT [DF_Invitations_CreatedAt]  DEFAULT (getdate()),
 CONSTRAINT [PK_Invitations] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


/****** Object:  Table [dbo].[Packages]    Script Date: 01/06/2010 16:49:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Packages](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SenderMail] [nvarchar](128) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[Message] [text] COLLATE Danish_Norwegian_CI_AS NULL,
	[Code] [nvarchar](64) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[ExpiresAt] [datetime] NULL,
	[CreatedAt] [datetime] NOT NULL CONSTRAINT [DF_Packages_CreatedAt]  DEFAULT (getdate()),
 CONSTRAINT [PK_Packages] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

/****** Object:  Table [dbo].[Transfers]    Script Date: 01/06/2010 16:50:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Transfers](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PackageID] [int] NOT NULL,
	[RecipientMail] [nvarchar](128) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_Transfers] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[Transfers]  WITH CHECK ADD  CONSTRAINT [FK_Transfers_Transfers] FOREIGN KEY([PackageID])
REFERENCES [dbo].[Packages] ([ID])
GO
ALTER TABLE [dbo].[Transfers] CHECK CONSTRAINT [FK_Transfers_Transfers]

CREATE TABLE [dbo].[Settings](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Login] [nvarchar](64) COLLATE Danish_Norwegian_CI_AS NULL,
	[CompanyName] [nvarchar](255) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[InternalUrl] [nvarchar](255) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[ExternalUrl] [nvarchar](255) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[UploadUrl] [nvarchar](255) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[StorageLocation] [nvarchar](255) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[RestrictSettingsToGroup] [nvarchar](128) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[LdapFilterName] [nvarchar](1024) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[LdapFilterMail] [nvarchar](1024) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[LdapFilterLogin] [nvarchar](1024) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[LdapSizeLimit] [int] NOT NULL,
	[MailSecure] [bit] NOT NULL,
	[MailReplyTo] [nvarchar](128) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[UploadSizeLimit] [int] NOT NULL,
	[ShareMailSubject] [nvarchar](128) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[ShareMailBodyPlain] [text] COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[ShareMailBodyHtml] [text] COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[ShareDefaultMessage] [nvarchar](1024) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[RequestMailSubject] [nvarchar](128) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[RequestMailBodyPlain] [text] COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[RequestMailBodyHtml] [text] COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[RequestDefaultMessage] [nvarchar](1024) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[UploadMailSubject] [nvarchar](128) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[UploadMailBodyPlain] [text] COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[UploadMailBodyHtml] [text] COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[UploadDefaultMessage] [nvarchar](1024) COLLATE Danish_Norwegian_CI_AS NOT NULL,
	[CreatedAt] [datetime] NOT NULL CONSTRAINT [DF_Settings_CreatedAt]  DEFAULT (getdate()),
 CONSTRAINT [PK_Settings] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO