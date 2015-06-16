CREATE TABLE [dbo].[Transport]
(
	ID int IDENTITY(41,1)PRIMARY KEY CLUSTERED,
	RegisterNumber varchar(20) DEFAULT ('') NOT NULL,
	Model varchar(50) DEFAULT ('') NOT NULL,
)
