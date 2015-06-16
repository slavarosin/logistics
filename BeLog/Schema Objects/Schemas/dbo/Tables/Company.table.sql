CREATE TABLE [dbo].[Company]
(
	ID int IDENTITY(106,1)PRIMARY KEY CLUSTERED , 
	Name varchar(50) DEFAULT ('') NOT NULL,
	[Address] varchar(100) DEFAULT ('') NOT NULL,
	Country varchar(2) DEFAULT ('') NOT NULL,
	CompanyType int Default(0) NOT NULL,
	ContactPerson varchar(50) DEFAULT('') NULL,
	Fax varchar(50) DEFAULT ('') NULL,
	Phone varchar(50) DEFAULT ('') NULL
)
