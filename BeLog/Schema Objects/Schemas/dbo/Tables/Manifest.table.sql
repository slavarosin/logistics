CREATE TABLE [dbo].[Manifest](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Departure] [varchar](50) NULL,
	[Arrival] [varchar](50) NULL,
	[ManifestDate] [datetime] NULL,
	[ManifesName] [varchar](100) NULL,
	[CreatedBy] [int]  NULL
 CONSTRAINT [PK_TransportManifest] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

