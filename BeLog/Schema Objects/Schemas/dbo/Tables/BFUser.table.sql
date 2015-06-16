CREATE TABLE [dbo].[BFUser]
(	
	ID int IDENTITY(1,1) NOT NULL PRIMARY KEY UNIQUE,
	[LoginUserName] varchar(50) DEFAULT ('') NOT NULL,
	[LoginPassword] varchar(50) DEFAULT ('') NOT NULL,
    [ApplicationRights] varchar(200) DEFAULT ('') NOT NULL,	
	[Abreviation] varchar(10),
	[FirstName] varchar(50) DEFAULT ('') NOT NULL,
	[LastName] varchar(50) DEFAULT ('') NOT NULL,
	[PhoneNumber] varchar(50) DEFAULT ('') NOT NULL

)
