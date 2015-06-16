CREATE TABLE [dbo].[OrderDispatch](
	[id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY UNIQUE,
	[Order_id] [int] NOT NULL,
	[DispatchTime] [varchar](50) NOT NULL,
	[DispatchPlace] [varchar](200) NOT NULL,
	[DispatchContact] [varchar](50) NULL,
	[DispatchType] [varchar] (1) NULL,
)