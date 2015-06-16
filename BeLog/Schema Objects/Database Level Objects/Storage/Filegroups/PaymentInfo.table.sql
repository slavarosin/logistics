CREATE TABLE [dbo].[PaymentInfo]
(
	ID int IDENTITY(80,1)PRIMARY KEY CLUSTERED, 
	CompanyID int NULL,
	CarriageCharges decimal(10,2) NOT NULL,
	Deductions decimal(10,2) NOT NULL,
	Saldo decimal(10,2) NOT NULL,
	Supplements decimal(10,2) NOT NULL,
	OtherCharges decimal(10,2) NOT NULL,
	Currency varchar(3) NOT NULL
)
