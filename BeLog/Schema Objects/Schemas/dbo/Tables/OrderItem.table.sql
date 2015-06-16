CREATE TABLE [dbo].[OrderItem]
(
	ID int IDENTITY(21,1)PRIMARY KEY CLUSTERED,
	CMRID  int NULL,
	MarkAndNum varchar(50) DEFAULT (''),
	NumOfPackages int NOT NULL,
	PackagingMethod varchar(30) DEFAULT (''),
	GoodsNature varchar(100) DEFAULT (''),
	StartNumber varchar(20) DEFAULT (''),
	[Weight] decimal(12,2) NOT NULL,
	Volume decimal(12,2) NOT NULL
)
