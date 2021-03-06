﻿CREATE TABLE [dbo].[Invoice]
(
    ID int IDENTITY(1,1) UNIQUE PRIMARY KEY NOT NULL,
	InvDate DateTime Default getDate() NOT NULL,
	CompanyToID int NOT NULL,
    GeneratedByUserID int NOT NULL, 
    ContactPhoneNumber varchar(20) NULL,
	OrderNr int NULL,
	RemarkUrgent bit Default(0) NOT NULL,
	RemarkForReview bit Default(0) NOT NULL,
	RemarkASAP bit Default(0) NOT NULL,
	RemarkComment bit Default(0) NOT NULL,
    Cruise varchar(100) Default('') NOT NULL,
	CargoDescription varchar(100) Default('') NOT NULL,
	LoadingTime varchar(50)  Default('') NOT NULL,
	LoadingPlace varchar(200) Default('') NOT NULL,
	DispatchTime varchar(50)  Default('') NOT NULL,
    DispatchPlace varchar(200) Default('') NOT NULL,
	DispatchContact varchar(50)  Default('') NOT NULL,
	PriceInfo varchar(200) Default('')  NOT NULL,
	PaymentTerms varchar(100) Default('') NOT NULL,
	TransportInfo varchar(100) Default('') NOT NULL,
	UserNotes text Default('') NOT NULL,
	UserCreated varchar(50) NULL
)
