/*
Deployment script for BeLogDB
*/

GO
SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;

SET NUMERIC_ROUNDABORT OFF;


GO
:setvar DatabaseName "BeLogDB"
:setvar DefaultDataPath "c:\Program Files\Microsoft SQL Server\MSSQL10.SQLEXPRESS\MSSQL\DATA\"
:setvar DefaultLogPath "c:\Program Files\Microsoft SQL Server\MSSQL10.SQLEXPRESS\MSSQL\DATA\"

GO
USE [master]

GO
:on error exit
GO
IF (DB_ID(N'$(DatabaseName)') IS NOT NULL
    AND DATABASEPROPERTYEX(N'$(DatabaseName)','Status') <> N'ONLINE')
BEGIN
    RAISERROR(N'The state of the target database, %s, is not set to ONLINE. To deploy to this database, its state must be set to ONLINE.', 16, 127,N'$(DatabaseName)') WITH NOWAIT
    RETURN
END

GO
IF (DB_ID(N'$(DatabaseName)') IS NOT NULL) 
BEGIN
    ALTER DATABASE [$(DatabaseName)]
    SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [$(DatabaseName)];
END

GO
PRINT N'Creating $(DatabaseName)...'
GO
CREATE DATABASE [$(DatabaseName)]
    ON 
    PRIMARY(NAME = [BeLogDB], FILENAME = N'$(DefaultDataPath)BeLogDB.mdf')
    LOG ON (NAME = [BeLogDB_log], FILENAME = N'$(DefaultLogPath)BeLogDB_log.ldf') COLLATE Estonian_CI_AS
GO
EXECUTE sp_dbcmptlevel [$(DatabaseName)], 100;


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'$(DatabaseName)')
    BEGIN
        ALTER DATABASE [$(DatabaseName)]
            SET ANSI_NULLS ON,
                ANSI_PADDING ON,
                ANSI_WARNINGS ON,
                ARITHABORT ON,
                CONCAT_NULL_YIELDS_NULL ON,
                NUMERIC_ROUNDABORT OFF,
                QUOTED_IDENTIFIER ON,
                ANSI_NULL_DEFAULT ON,
                CURSOR_DEFAULT LOCAL,
                RECOVERY FULL,
                CURSOR_CLOSE_ON_COMMIT OFF,
                AUTO_CREATE_STATISTICS ON,
                AUTO_SHRINK ON,
                AUTO_UPDATE_STATISTICS ON,
                RECURSIVE_TRIGGERS OFF 
            WITH ROLLBACK IMMEDIATE;
        ALTER DATABASE [$(DatabaseName)]
            SET AUTO_CLOSE OFF 
            WITH ROLLBACK IMMEDIATE;
    END


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'$(DatabaseName)')
    BEGIN
        ALTER DATABASE [$(DatabaseName)]
            SET ALLOW_SNAPSHOT_ISOLATION OFF;
    END


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'$(DatabaseName)')
    BEGIN
        ALTER DATABASE [$(DatabaseName)]
            SET READ_COMMITTED_SNAPSHOT OFF;
    END


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'$(DatabaseName)')
    BEGIN
        ALTER DATABASE [$(DatabaseName)]
            SET AUTO_UPDATE_STATISTICS_ASYNC OFF,
                PAGE_VERIFY NONE,
                DATE_CORRELATION_OPTIMIZATION OFF,
                DISABLE_BROKER,
                PARAMETERIZATION SIMPLE,
                SUPPLEMENTAL_LOGGING OFF 
            WITH ROLLBACK IMMEDIATE;
    END


GO
IF IS_SRVROLEMEMBER(N'sysadmin') = 1
    BEGIN
        IF EXISTS (SELECT 1
                   FROM   [master].[dbo].[sysdatabases]
                   WHERE  [name] = N'$(DatabaseName)')
            BEGIN
                EXECUTE sp_executesql N'ALTER DATABASE [$(DatabaseName)]
    SET TRUSTWORTHY OFF,
        DB_CHAINING OFF 
    WITH ROLLBACK IMMEDIATE';
            END
    END
ELSE
    BEGIN
        PRINT N'The database settings cannot be modified. You must be a SysAdmin to apply these settings.';
    END


GO
IF IS_SRVROLEMEMBER(N'sysadmin') = 1
    BEGIN
        IF EXISTS (SELECT 1
                   FROM   [master].[dbo].[sysdatabases]
                   WHERE  [name] = N'$(DatabaseName)')
            BEGIN
                EXECUTE sp_executesql N'ALTER DATABASE [$(DatabaseName)]
    SET HONOR_BROKER_PRIORITY OFF 
    WITH ROLLBACK IMMEDIATE';
            END
    END
ELSE
    BEGIN
        PRINT N'The database settings cannot be modified. You must be a SysAdmin to apply these settings.';
    END


GO
USE [$(DatabaseName)]

GO
IF fulltextserviceproperty(N'IsFulltextInstalled') = 1
    EXECUTE sp_fulltext_database 'enable';


GO
/*
 Pre-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be executed before the build script.	
 Use SQLCMD syntax to include a file in the pre-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the pre-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

GO
PRINT N'Creating [dbo].[BFUser]...';


GO
CREATE TABLE [dbo].[BFUser] (
    [ID]                INT           IDENTITY (1, 1) NOT NULL,
    [LoginUserName]     VARCHAR (50)  NOT NULL,
    [LoginPassword]     VARCHAR (50)  NOT NULL,
    [ApplicationRights] VARCHAR (200) NOT NULL,
    [Abreviation]       VARCHAR (10)  NULL,
    [FirstName]         VARCHAR (50)  NOT NULL,
    [LastName]          VARCHAR (50)  NOT NULL,
    [PhoneNumber]       VARCHAR (50)  NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF),
    UNIQUE NONCLUSTERED ([ID] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating [dbo].[CMR]...';


GO
CREATE TABLE [dbo].[CMR] (
    [ID]                 INT           IDENTITY (1000000, 1) NOT NULL,
    [SenderID]           INT           NULL,
    [ConsigneeID]        INT           NULL,
    [DeliveryPlace]      VARCHAR (100) NOT NULL,
    [DeliveryCountry]    VARCHAR (100) NOT NULL,
    [TakingPlace]        VARCHAR (100) NOT NULL,
    [TakingCountry]      VARCHAR (100) NOT NULL,
    [AttachedDocuments]  VARCHAR (500) NOT NULL,
    [CarrierID]          INT           NULL,
    [NextCarrierID]      INT           NULL,
    [ClassValue]         VARCHAR (10)  NOT NULL,
    [Num]                INT           NOT NULL,
    [Letter]             VARCHAR (2)   NOT NULL,
    [ADR]                VARCHAR (100) NOT NULL,
    [SendInstruction]    VARCHAR (500) NOT NULL,
    [SenderPaymentID]    INT           NOT NULL,
    [ConsigneePaymentID] INT           NOT NULL,
    [SpecialAgreements]  VARCHAR (500) NOT NULL,
    [PaymentInstruction] VARCHAR (30)  NOT NULL,
    [CMRValid]           BIT           NOT NULL,
    [EstablieshedIn]     VARCHAR (50)  NOT NULL,
    [EstablieshedDate]   DATETIME      NOT NULL,
    [TimeOfDep]          DATETIME      NOT NULL,
    [TransportID]        INT           NOT NULL,
    [TransportID2]       INT           NULL,
    [UserCreated]        VARCHAR (50)  NULL,
    [ManifestId]         INT           NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF),
    UNIQUE NONCLUSTERED ([ID] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating [dbo].[Company]...';


GO
CREATE TABLE [dbo].[Company] (
    [ID]            INT           IDENTITY (106, 1) NOT NULL,
    [Name]          VARCHAR (50)  NOT NULL,
    [Address]       VARCHAR (100) NOT NULL,
    [Country]       VARCHAR (2)   NOT NULL,
    [CompanyType]   INT           NOT NULL,
    [ContactPerson] VARCHAR (50)  NULL,
    [Fax]           VARCHAR (50)  NULL,
    [Phone]         VARCHAR (50)  NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating [dbo].[Country]...';


GO
CREATE TABLE [dbo].[Country] (
    [CountryCode] VARCHAR (2)  NOT NULL,
    [CountryName] VARCHAR (50) NULL,
    PRIMARY KEY CLUSTERED ([CountryCode] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating [dbo].[Invoice]...';


GO
CREATE TABLE [dbo].[Invoice] (
    [ID]                 INT           IDENTITY (1, 1) NOT NULL,
    [InvDate]            DATETIME      NOT NULL,
    [CompanyToID]        INT           NOT NULL,
    [GeneratedByUserID]  INT           NOT NULL,
    [ContactPhoneNumber] VARCHAR (20)  NULL,
    [OrderNr]            INT           NULL,
    [RemarkUrgent]       BIT           NOT NULL,
    [RemarkForReview]    BIT           NOT NULL,
    [RemarkASAP]         BIT           NOT NULL,
    [RemarkComment]      BIT           NOT NULL,
    [Cruise]             VARCHAR (100) NOT NULL,
    [CargoDescription]   VARCHAR (100) NOT NULL,
    [LoadingTime]        VARCHAR (50)  NOT NULL,
    [LoadingPlace]       VARCHAR (200) NOT NULL,
    [DispatchTime]       VARCHAR (50)  NOT NULL,
    [DispatchPlace]      VARCHAR (200) NOT NULL,
    [DispatchContact]    VARCHAR (50)  NOT NULL,
    [PriceInfo]          VARCHAR (200) NOT NULL,
    [PaymentTerms]       VARCHAR (100) NOT NULL,
    [TransportInfo]      VARCHAR (100) NOT NULL,
    [UserNotes]          TEXT          NOT NULL,
    [UserCreated]        VARCHAR (50)  NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF),
    UNIQUE NONCLUSTERED ([ID] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating [dbo].[Manifest]...';


GO
CREATE TABLE [dbo].[Manifest] (
    [id]           INT           IDENTITY (1, 1) NOT NULL,
    [Departure]    VARCHAR (50)  NULL,
    [Arrival]      VARCHAR (50)  NULL,
    [ManifestDate] DATETIME      NULL,
    [ManifesName]  VARCHAR (100) NULL,
    CONSTRAINT [PK_TransportManifest] PRIMARY KEY CLUSTERED ([id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF) ON [PRIMARY]
) ON [PRIMARY];


GO
PRINT N'Creating [dbo].[OrderDispatch]...';


GO
CREATE TABLE [dbo].[OrderDispatch] (
    [id]              INT           IDENTITY (1, 1) NOT NULL,
    [Order_id]        INT           NOT NULL,
    [DispatchTime]    VARCHAR (50)  NOT NULL,
    [DispatchPlace]   VARCHAR (200) NOT NULL,
    [DispatchContact] VARCHAR (50)  NULL,
    [DispatchType]    VARCHAR (1)   NULL,
    PRIMARY KEY CLUSTERED ([id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF),
    UNIQUE NONCLUSTERED ([id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating [dbo].[OrderItem]...';


GO
CREATE TABLE [dbo].[OrderItem] (
    [ID]              INT             IDENTITY (21, 1) NOT NULL,
    [CMRID]           INT             NULL,
    [MarkAndNum]      VARCHAR (50)    NULL,
    [NumOfPackages]   INT             NOT NULL,
    [PackagingMethod] VARCHAR (30)    NULL,
    [GoodsNature]     VARCHAR (100)   NULL,
    [StartNumber]     VARCHAR (20)    NULL,
    [Weight]          DECIMAL (12, 2) NOT NULL,
    [Volume]          DECIMAL (12, 2) NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating [dbo].[PaymentInfo]...';


GO
CREATE TABLE [dbo].[PaymentInfo] (
    [ID]              INT             IDENTITY (80, 1) NOT NULL,
    [CompanyID]       INT             NULL,
    [CarriageCharges] DECIMAL (10, 2) NOT NULL,
    [Deductions]      DECIMAL (10, 2) NOT NULL,
    [Saldo]           DECIMAL (10, 2) NOT NULL,
    [Supplements]     DECIMAL (10, 2) NOT NULL,
    [OtherCharges]    DECIMAL (10, 2) NOT NULL,
    [Currency]        VARCHAR (3)     NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating [dbo].[Transport]...';


GO
CREATE TABLE [dbo].[Transport] (
    [ID]             INT          IDENTITY (41, 1) NOT NULL,
    [RegisterNumber] VARCHAR (20) NOT NULL,
    [Model]          VARCHAR (50) NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating On column: LoginUserName...';


GO
ALTER TABLE [dbo].[BFUser]
    ADD DEFAULT ('') FOR [LoginUserName];


GO
PRINT N'Creating On column: LoginPassword...';


GO
ALTER TABLE [dbo].[BFUser]
    ADD DEFAULT ('') FOR [LoginPassword];


GO
PRINT N'Creating On column: ApplicationRights...';


GO
ALTER TABLE [dbo].[BFUser]
    ADD DEFAULT ('') FOR [ApplicationRights];


GO
PRINT N'Creating On column: FirstName...';


GO
ALTER TABLE [dbo].[BFUser]
    ADD DEFAULT ('') FOR [FirstName];


GO
PRINT N'Creating On column: LastName...';


GO
ALTER TABLE [dbo].[BFUser]
    ADD DEFAULT ('') FOR [LastName];


GO
PRINT N'Creating On column: PhoneNumber...';


GO
ALTER TABLE [dbo].[BFUser]
    ADD DEFAULT ('') FOR [PhoneNumber];


GO
PRINT N'Creating On column: Letter...';


GO
ALTER TABLE [dbo].[CMR]
    ADD DEFAULT ('') FOR [Letter];


GO
PRINT N'Creating On column: ADR...';


GO
ALTER TABLE [dbo].[CMR]
    ADD DEFAULT ('') FOR [ADR];


GO
PRINT N'Creating On column: Name...';


GO
ALTER TABLE [dbo].[Company]
    ADD DEFAULT ('') FOR [Name];


GO
PRINT N'Creating On column: Address...';


GO
ALTER TABLE [dbo].[Company]
    ADD DEFAULT ('') FOR [Address];


GO
PRINT N'Creating On column: Country...';


GO
ALTER TABLE [dbo].[Company]
    ADD DEFAULT ('') FOR [Country];


GO
PRINT N'Creating On column: CompanyType...';


GO
ALTER TABLE [dbo].[Company]
    ADD DEFAULT (0) FOR [CompanyType];


GO
PRINT N'Creating On column: ContactPerson...';


GO
ALTER TABLE [dbo].[Company]
    ADD DEFAULT ('') FOR [ContactPerson];


GO
PRINT N'Creating On column: Fax...';


GO
ALTER TABLE [dbo].[Company]
    ADD DEFAULT ('') FOR [Fax];


GO
PRINT N'Creating On column: Phone...';


GO
ALTER TABLE [dbo].[Company]
    ADD DEFAULT ('') FOR [Phone];


GO
PRINT N'Creating On column: CountryCode...';


GO
ALTER TABLE [dbo].[Country]
    ADD DEFAULT ('') FOR [CountryCode];


GO
PRINT N'Creating On column: CountryName...';


GO
ALTER TABLE [dbo].[Country]
    ADD DEFAULT ('') FOR [CountryName];


GO
PRINT N'Creating On column: InvDate...';


GO
ALTER TABLE [dbo].[Invoice]
    ADD DEFAULT getDate() FOR [InvDate];


GO
PRINT N'Creating On column: RemarkUrgent...';


GO
ALTER TABLE [dbo].[Invoice]
    ADD DEFAULT (0) FOR [RemarkUrgent];


GO
PRINT N'Creating On column: RemarkForReview...';


GO
ALTER TABLE [dbo].[Invoice]
    ADD DEFAULT (0) FOR [RemarkForReview];


GO
PRINT N'Creating On column: RemarkASAP...';


GO
ALTER TABLE [dbo].[Invoice]
    ADD DEFAULT (0) FOR [RemarkASAP];


GO
PRINT N'Creating On column: RemarkComment...';


GO
ALTER TABLE [dbo].[Invoice]
    ADD DEFAULT (0) FOR [RemarkComment];


GO
PRINT N'Creating On column: Cruise...';


GO
ALTER TABLE [dbo].[Invoice]
    ADD DEFAULT ('') FOR [Cruise];


GO
PRINT N'Creating On column: CargoDescription...';


GO
ALTER TABLE [dbo].[Invoice]
    ADD DEFAULT ('') FOR [CargoDescription];


GO
PRINT N'Creating On column: LoadingTime...';


GO
ALTER TABLE [dbo].[Invoice]
    ADD DEFAULT ('') FOR [LoadingTime];


GO
PRINT N'Creating On column: LoadingPlace...';


GO
ALTER TABLE [dbo].[Invoice]
    ADD DEFAULT ('') FOR [LoadingPlace];


GO
PRINT N'Creating On column: DispatchTime...';


GO
ALTER TABLE [dbo].[Invoice]
    ADD DEFAULT ('') FOR [DispatchTime];


GO
PRINT N'Creating On column: DispatchPlace...';


GO
ALTER TABLE [dbo].[Invoice]
    ADD DEFAULT ('') FOR [DispatchPlace];


GO
PRINT N'Creating On column: DispatchContact...';


GO
ALTER TABLE [dbo].[Invoice]
    ADD DEFAULT ('') FOR [DispatchContact];


GO
PRINT N'Creating On column: PriceInfo...';


GO
ALTER TABLE [dbo].[Invoice]
    ADD DEFAULT ('') FOR [PriceInfo];


GO
PRINT N'Creating On column: PaymentTerms...';


GO
ALTER TABLE [dbo].[Invoice]
    ADD DEFAULT ('') FOR [PaymentTerms];


GO
PRINT N'Creating On column: TransportInfo...';


GO
ALTER TABLE [dbo].[Invoice]
    ADD DEFAULT ('') FOR [TransportInfo];


GO
PRINT N'Creating On column: UserNotes...';


GO
ALTER TABLE [dbo].[Invoice]
    ADD DEFAULT ('') FOR [UserNotes];


GO
PRINT N'Creating On column: MarkAndNum...';


GO
ALTER TABLE [dbo].[OrderItem]
    ADD DEFAULT ('') FOR [MarkAndNum];


GO
PRINT N'Creating On column: PackagingMethod...';


GO
ALTER TABLE [dbo].[OrderItem]
    ADD DEFAULT ('') FOR [PackagingMethod];


GO
PRINT N'Creating On column: GoodsNature...';


GO
ALTER TABLE [dbo].[OrderItem]
    ADD DEFAULT ('') FOR [GoodsNature];


GO
PRINT N'Creating On column: StartNumber...';


GO
ALTER TABLE [dbo].[OrderItem]
    ADD DEFAULT ('') FOR [StartNumber];


GO
PRINT N'Creating On column: RegisterNumber...';


GO
ALTER TABLE [dbo].[Transport]
    ADD DEFAULT ('') FOR [RegisterNumber];


GO
PRINT N'Creating On column: Model...';


GO
ALTER TABLE [dbo].[Transport]
    ADD DEFAULT ('') FOR [Model];


GO
PRINT N'Creating FK_CMR_ConsingeePaymentInfo...';


GO
ALTER TABLE [dbo].[CMR] WITH NOCHECK
    ADD CONSTRAINT [FK_CMR_ConsingeePaymentInfo] FOREIGN KEY ([ConsigneePaymentID]) REFERENCES [dbo].[PaymentInfo] ([ID]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating FK_CMR_OrderItem...';


GO
ALTER TABLE [dbo].[CMR] WITH NOCHECK
    ADD CONSTRAINT [FK_CMR_OrderItem] FOREIGN KEY ([TransportID]) REFERENCES [dbo].[Transport] ([ID]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating FK_CMR_SenderPaymentInfo...';


GO
ALTER TABLE [dbo].[CMR] WITH NOCHECK
    ADD CONSTRAINT [FK_CMR_SenderPaymentInfo] FOREIGN KEY ([SenderPaymentID]) REFERENCES [dbo].[PaymentInfo] ([ID]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating FK_Invoice_CompanyToForeignKey...';


GO
ALTER TABLE [dbo].[Invoice] WITH NOCHECK
    ADD CONSTRAINT [FK_Invoice_CompanyToForeignKey] FOREIGN KEY ([CompanyToID]) REFERENCES [dbo].[Company] ([ID]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating FK_Invoice_UserIDForeignKey...';


GO
ALTER TABLE [dbo].[Invoice] WITH NOCHECK
    ADD CONSTRAINT [FK_Invoice_UserIDForeignKey] FOREIGN KEY ([GeneratedByUserID]) REFERENCES [dbo].[BFUser] ([ID]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating FK_OrderItem_CMR...';


GO
ALTER TABLE [dbo].[OrderItem] WITH NOCHECK
    ADD CONSTRAINT [FK_OrderItem_CMR] FOREIGN KEY ([CMRID]) REFERENCES [dbo].[CMR] ([ID]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating [dbo].[PrintInvoice]...';


GO
CREATE PROCEDURE [dbo].[PrintInvoice](@InvoiceID int )
AS
	   SELECT 
	  Invoice.InvDate as InvoiceDate,
	  Invoice.OrderNr as OrderNumber,
	  Invoice.RemarkASAP as RemarkASAP,
	  Invoice.RemarkForReview as RemarkReview,
	  Invoice.RemarkUrgent as RemarkUrgent,
	  Invoice.RemarkComment as RemarkComment,
	  Invoice.Cruise as Cruise,
	  Invoice.CargoDescription as CargoDescription,
	  Invoice.LoadingTime as LoadingTime,
	  Invoice.LoadingPlace as LoadingPlace,
	  Invoice.DispatchTime as DispatchTime,
	  Invoice.DispatchPlace as DispatchPlace,
	  Invoice.DispatchContact as DispatchContact,
	  Invoice.PriceInfo as PriceInfo,
	  Invoice.PaymentTerms as PaymentTerms,
	  Invoice.TransportInfo as TransportInfo,
	  Invoice.UserNotes as UserNotes,
	  SUBSTRING(Invoice.UserCreated,1,1) as UserCreated,
	  SC.FirstName ContactFName, 
	  SC.LastName  ContactLName,
	  SC.PhoneNumber ContactPhoneNum,	
	  CC.Name CompanyToName,
	  CC.[Address]  CompanyToAddress,
	  CC.ContactPerson CompanyToContact,
	  CC.Fax  CompanyToFax,
	  CC.Phone  CompanyToPhone,
	  od.DispatchContact SecDispatchContact,
	  od.DispatchPlace SecDispatchPlace,
	  od.DispatchTime SecDispatchTime,
	  od.DispatchType SecDispatchType	
	  FROM Invoice
	   INNER JOIN BFUser SC ON Invoice.GeneratedByUserID = SC.ID 
	   		 INNER JOIN Company CC ON Invoice.CompanyToID = CC.ID
	   		   INNER JOIN  OrderDispatch od ON Invoice.ID = od.Order_id
									    WHERE Invoice.ID =  @InvoiceID
GO
PRINT N'Creating [dbo].[PrintManifest]...';


GO
CREATE PROCEDURE [dbo].[PrintManifest](@ManifestID int )
AS
	  SELECT 
	  
	  MAN.Departure,
	  MAN.Arrival, 
	  MAN.ManifestDate,
	  MAN.ManifesName,
	  cmr.SenderID,
	  cmr.ConsigneeID,
	  cm1.Name as Consignor,
	  cm.Name as Consignee
	  FROM Manifest MAN right join CMR cmr on MAN.id = cmr.ManifestId inner join Company cm on cm.ID = cmr.ConsigneeID inner join 
	  Company cm1 on cm1.ID = cmr.SenderID 
	  WHERE MAN.id = @ManifestID
GO
PRINT N'Creating [dbo].[TagLabelTemplate]...';


GO
CREATE PROCEDURE  TagLabelTemplate(@CMRID int )
AS


CREATE Table #TagLabels (ConsigneeName varchar(50), ConsigneeAddress varchar(100),TotalNumOfPckges int)
DECLARE @totalNumOfPackages int 
DECLARE @i int
DECLARE @ConsName varchar(50)
DECLARE @ConsAddress varchar(150)

SELECT @ConsName =  CC.[Name]  from CMR  INNER JOIN Company CC ON CMR.ConsigneeID = CC.ID where CMR.ID = @CMRID
SELECT @ConsAddress =  CC.[Address] +' ' + Coucc.CountryName from CMR  INNER JOIN Company CC ON CMR.ConsigneeID = CC.ID
												INNER JOIN Country Coucc  ON  Coucc.CountryCode = CC.Country where CMR.ID =@CMRID

SELECT @i = 0
SELECT @totalNumOfPackages =SUM(OrderItem.NumOfPackages) from OrderItem Where OrderItem.CMRID = @CMRID


while @i<@totalNumOfPackages
BEGIN
	set @i = @i + 1
	PRINT @i
INSERT INTO #TagLabels VALUES (@ConsName, @ConsAddress,@totalNumOfPackages)
END

SELECT * FROM #TagLabels
GO
PRINT N'Creating [dbo].[getOrderItem1]...';


GO
CREATE FUNCTION getOrderItem1 ( @cmrid int )
RETURNS @OrderTable TABLE
(
ID int,
CMRID int,
MarkAndNum varchar(50) ,
NumOfPackages int ,
PackagingMethod varchar(30),
GoodsNature varchar(100) ,
StartNumber varchar(20) ,
[Weight] decimal(12, 2),
Volume decimal(12, 2)
)
AS
BEGIN

IF (SELECT COUNT(*) FROM OrderItem where CMRID = @cmrid) > 0
	BEGIN
		INSERT @OrderTable select top 1 * from (
		select top 1 * from (
			select top 1 *
				from dbo.OrderItem
				where CMRID = @cmrid
				order by dbo.OrderItem.ID asc
		) as newtbl order by newtbl.ID desc
	) as newtbl2 order by newtbl2.ID desc
	END;
ELSE
	BEGIN
	INSERT @OrderTable
           (ID
           ,CMRID
           ,MarkAndNum
           ,NumOfPackages
           ,PackagingMethod
           ,GoodsNature
           ,StartNumber
           ,[Weight]
           ,Volume)
     VALUES
           (1
           ,@cmrid
           ,''
           ,NULL
           ,''
           ,''
           ,''
           ,NULL
           ,NULL)
	END;
RETURN
END
GO
PRINT N'Creating [dbo].[getOrderItem2]...';


GO
CREATE FUNCTION getOrderItem2 ( @cmrid int )
RETURNS @OrderTable TABLE
(
ID int,
CMRID int,
MarkAndNum varchar(50) ,
NumOfPackages int ,
PackagingMethod varchar(30),
GoodsNature varchar(100) ,
StartNumber varchar(20) ,
[Weight] decimal(12, 2),
Volume decimal(12, 2)
)
AS
BEGIN

IF (SELECT COUNT(*) FROM OrderItem where CMRID = @cmrid) > 1
	BEGIN
		INSERT @OrderTable select top 1 * from (
		select top 1 * from (
			select top 2 *
				from dbo.OrderItem
				where CMRID = @cmrid
				order by dbo.OrderItem.ID asc
		) as newtbl order by newtbl.ID desc
	) as newtbl2 order by newtbl2.ID desc
	END;
ELSE
	BEGIN
	INSERT @OrderTable
           (ID
           ,CMRID
           ,MarkAndNum
           ,NumOfPackages
           ,PackagingMethod
           ,GoodsNature
           ,StartNumber
           ,[Weight]
           ,Volume)
     VALUES
           (1
           ,@cmrid
           ,''
           ,NULL
           ,''
           ,''
           ,''
           ,NULL
           ,NULL)
	END;
RETURN
END
GO
PRINT N'Creating [dbo].[getOrderItem3]...';


GO
CREATE FUNCTION getOrderItem3 ( @cmrid int )
RETURNS @OrderTable TABLE
(
ID int,
CMRID int,
MarkAndNum varchar(50) ,
NumOfPackages int ,
PackagingMethod varchar(30),
GoodsNature varchar(100) ,
StartNumber varchar(20) ,
[Weight] decimal(12, 2),
Volume decimal(12, 2)
)
AS
BEGIN

IF (SELECT COUNT(*) FROM OrderItem where CMRID = @cmrid) > 2
	BEGIN
		INSERT @OrderTable select top 1 * from (
		select top 1 * from (
			select top 3 *
				from dbo.OrderItem
				where CMRID = @cmrid
				order by dbo.OrderItem.ID asc
		) as newtbl order by newtbl.ID desc
	) as newtbl2 order by newtbl2.ID desc
	END;
ELSE
	BEGIN
	INSERT @OrderTable
           (ID
           ,CMRID
           ,MarkAndNum
           ,NumOfPackages
           ,PackagingMethod
           ,GoodsNature
           ,StartNumber
           ,[Weight]
           ,Volume)
     VALUES
           (1
           ,@cmrid
           ,''
           ,NULL
           ,''
           ,''
           ,''
           ,NULL
           ,NULL)
	END;
RETURN
END
GO
PRINT N'Creating [dbo].[getOrderItem4]...';


GO
CREATE FUNCTION getOrderItem4 ( @cmrid int )
RETURNS @OrderTable TABLE
(
ID int,
CMRID int,
MarkAndNum varchar(50) ,
NumOfPackages int ,
PackagingMethod varchar(30),
GoodsNature varchar(100) ,
StartNumber varchar(20) ,
[Weight] decimal(12, 2),
Volume decimal(12, 2)
)
AS
BEGIN

IF (SELECT COUNT(*) FROM OrderItem where CMRID = @cmrid) > 3
	BEGIN
		INSERT @OrderTable select top 1 * from (
		select top 1 * from (
			select top 4 *
				from dbo.OrderItem
				where CMRID = @cmrid
				order by dbo.OrderItem.ID asc
		) as newtbl order by newtbl.ID desc
	) as newtbl2 order by newtbl2.ID desc
	END;
ELSE
	BEGIN
	INSERT @OrderTable
           (ID
           ,CMRID
           ,MarkAndNum
           ,NumOfPackages
           ,PackagingMethod
           ,GoodsNature
           ,StartNumber
           ,[Weight]
           ,Volume)
     VALUES
           (1
           ,@cmrid
           ,''
           ,NULL
           ,''
           ,''
           ,''
           ,NULL
           ,NULL)
	END;
RETURN
END
GO
PRINT N'Creating [dbo].[getOrderItem5]...';


GO
CREATE FUNCTION getOrderItem5 ( @cmrid int )
RETURNS @OrderTable TABLE
(
ID int,
CMRID int,
MarkAndNum varchar(50) ,
NumOfPackages int ,
PackagingMethod varchar(30),
GoodsNature varchar(100) ,
StartNumber varchar(20) ,
[Weight] decimal(12, 2),
Volume decimal(12, 2)
)
AS
BEGIN

IF (SELECT COUNT(*) FROM OrderItem where CMRID = @cmrid) > 4
	BEGIN
		INSERT @OrderTable select top 1 * from (
		select top 1 * from (
			select top 5 *
				from dbo.OrderItem
				where CMRID = @cmrid
				order by dbo.OrderItem.ID asc
		) as newtbl order by newtbl.ID desc
	) as newtbl2 order by newtbl2.ID desc
	END;
ELSE
	BEGIN
	INSERT @OrderTable
           (ID
           ,CMRID
           ,MarkAndNum
           ,NumOfPackages
           ,PackagingMethod
           ,GoodsNature
           ,StartNumber
           ,[Weight]
           ,Volume)
     VALUES
           (1
           ,@cmrid
           ,''
           ,NULL
           ,''
           ,''
           ,''
           ,NULL
           ,NULL)
	END;
RETURN
END
GO
PRINT N'Creating [dbo].[getOrderItem6]...';


GO
CREATE FUNCTION getOrderItem6 ( @cmrid int )
RETURNS @OrderTable TABLE
(
ID int,
CMRID int,
MarkAndNum varchar(50) ,
NumOfPackages int ,
PackagingMethod varchar(30),
GoodsNature varchar(100) ,
StartNumber varchar(20) ,
[Weight] decimal(12, 2),
Volume decimal(12, 2)
)
AS
BEGIN

IF (SELECT COUNT(*) FROM OrderItem where CMRID = @cmrid) > 5
	BEGIN
		INSERT @OrderTable select top 1 * from (
		select top 1 * from (
			select top 6 *
				from dbo.OrderItem
				where CMRID = @cmrid
				order by dbo.OrderItem.ID asc
		) as newtbl order by newtbl.ID desc
	) as newtbl2 order by newtbl2.ID desc
	END;
ELSE
	BEGIN
	INSERT @OrderTable
           (ID
           ,CMRID
           ,MarkAndNum
           ,NumOfPackages
           ,PackagingMethod
           ,GoodsNature
           ,StartNumber
           ,[Weight]
           ,Volume)
     VALUES
           (1
           ,@cmrid
           ,''
           ,NULL
           ,''
           ,''
           ,''
           ,NULL
           ,NULL)
	END;
RETURN
END
GO
PRINT N'Creating [dbo].[PrintTemplate]...';


GO
CREATE PROCEDURE  PrintTemplate(@CMRID int )
AS
SELECT 
	CMR.ID, 
	(select MarkAndNum from dbo.getOrderItem1(@CMRID)) as MarkAndNum1,
	(select NumOfPackages from dbo.getOrderItem1(@CMRID)) as NumOfPackages1,
	(select GoodsNature from dbo.getOrderItem1(@CMRID)) as GoodsNature1,
	(select StartNumber from dbo.getOrderItem1(@CMRID)) as StartNumber1,
	(select Volume from dbo.getOrderItem1(@CMRID)) as Volume1,
	(select [Weight] from dbo.getOrderItem1(@CMRID)) as Weight1,
	(select PackagingMethod from dbo.getOrderItem1(@CMRID)) as PackagingMethod1,
	
	(select MarkAndNum from getOrderItem2(@CMRID)) as MarkAndNum2,
	(select NumOfPackages from getOrderItem2(@CMRID)) as NumOfPackages2,
	(select GoodsNature from getOrderItem2(@CMRID)) as GoodsNature2,
	(select StartNumber from getOrderItem2(@CMRID)) as StartNumber2,
	(select Volume from getOrderItem2(@CMRID)) as Volume2,
	(select [Weight] from getOrderItem2(@CMRID)) as Weight2,
	(select PackagingMethod from getOrderItem2(@CMRID)) as PackagingMethod2,
	
	(select MarkAndNum from getOrderItem3(@CMRID)) as MarkAndNum3,
	(select NumOfPackages from getOrderItem3(@CMRID)) as NumOfPackages3,
	(select GoodsNature from getOrderItem3(@CMRID)) as GoodsNature3,
	(select StartNumber from getOrderItem3(@CMRID)) as StartNumber3,
	(select Volume from getOrderItem3(@CMRID)) as Volume3,
	(select [Weight] from getOrderItem3(@CMRID)) as Weight3,
	(select PackagingMethod from getOrderItem3(@CMRID)) as PackagingMethod3,
	
	(select MarkAndNum from getOrderItem4(@CMRID)) as MarkAndNum4,
	(select NumOfPackages from getOrderItem4(@CMRID)) as NumOfPackages4,
	(select GoodsNature from getOrderItem4(@CMRID)) as GoodsNature4,
	(select StartNumber from getOrderItem4(@CMRID)) as StartNumber4,
	(select Volume from getOrderItem4(@CMRID)) as Volume4,
	(select [Weight] from getOrderItem4(@CMRID)) as Weight4,
	(select PackagingMethod from getOrderItem4(@CMRID)) as PackagingMethod4,
	
	(select MarkAndNum from getOrderItem5(@CMRID)) as MarkAndNum5,
	(select NumOfPackages from getOrderItem5(@CMRID)) as NumOfPackages5,
	(select GoodsNature from getOrderItem5(@CMRID)) as GoodsNature5,
	(select StartNumber from getOrderItem5(@CMRID)) as StartNumber5,
	(select Volume from getOrderItem5(@CMRID)) as Volume5,
	(select [Weight] from getOrderItem5(@CMRID)) as Weight5,
	(select PackagingMethod from getOrderItem5(@CMRID)) as PackagingMethod5,
	
	(select MarkAndNum from getOrderItem6(@CMRID)) as MarkAndNum6,
	(select NumOfPackages from getOrderItem6(@CMRID)) as NumOfPackages6,
	(select GoodsNature from getOrderItem6(@CMRID)) as GoodsNature6,
	(select StartNumber from getOrderItem6(@CMRID)) as StartNumber6,
	(select Volume from getOrderItem6(@CMRID)) as Volume6,
	(select [Weight] from getOrderItem6(@CMRID)) as Weight6,
	(select PackagingMethod from getOrderItem6(@CMRID)) as PackagingMethod6,
	
	SC.Name as SenderName,
			SC.[Address] as SenderAddress,
			couSC.CountryName as SenderCountry,
			CC.[Name] as ConsigneeName,  
			CC.[Address] as ConsigneeAddress,
			couCC.CountryName as ConsigneeCountry,
            CMR.DeliveryPlace as DeliveryPlace,
			delCountry.CountryName as DeliveryCountry,
			CMR.TakingPlace as TakingPlace,
  			takCountry.CountryName as TakingCountry,
			CMR.AttachedDocuments as AttachedDocuments,
			CMR.ClassValue as ClassValue,
			CMR.Num as NUM,
			CMR.Letter as Letter,
			CMR.ADR as ADR,
			CMR.SpecialAgreements as SpecialAgreements,
			CMR.EstablieshedIn as EstablieshedIn,
			CMR.TimeOfDep as TimeOfDeparture,
			CMR.EstablieshedDate as EstablieshedDate,
			CMR.SendInstruction as SendInstruction,
			CMR.PaymentInstruction as PaymentInstruction,
			CMR.CMRValid as  CMRValid,
			UPPER(SUBSTRING(CMR.UserCreated,1,1)) as UserCreated,
			SendP.CarriageCharges as SenderCarriageCharges,
			SendP.Deductions as SenderDeductions, 
			SendP.Currency as Currency,
			SendP.OtherCharges as SenderOtherCharges,
			SendP.Saldo as SenderSaldo,
			SendP.Supplements as SenderSupplements,
			ConsP.CarriageCharges as ConsigneeCarriageCharges,
			ConsP.Deductions as ConsigneeDeductions, 
			ConsP.OtherCharges as ConsigneeOtherCharges,
			ConsP.Saldo as ConsigneeSaldo,
			ConsP.Supplements as ConsigneeSupplements,
			TR.Model as TransportModel,
			TR.RegisterNumber as TransportRegNumber
from CMR
 INNER JOIN Company SC ON CMR.SenderID = SC.ID 
	     INNER JOIN Country couSC ON SC.Country = couSC.CountryCode
	   		 INNER JOIN Company CC ON CMR.ConsigneeID = CC.ID
		  		 INNER JOIN Country couCC ON CC.Country = couCC.CountryCode
	     			
								  INNER JOIN PaymentInfo ConsP ON CMR.ConsigneePaymentID = ConsP.ID
								    INNER JOIN PaymentInfo SendP ON CMR.SenderPaymentID = SendP.ID
									   INNER JOIN Country delCountry ON CMR.DeliveryCountry = delCountry.CountryCode
									    INNER JOIN Country takCountry ON CMR.TakingCountry = takCountry.CountryCode
								  	   INNER JOIN Transport TR on CMR.TransportID = TR.ID
									    
 WHERE CMR.ID =  @CMRID
GO
-- Refactoring step to update target server with deployed transaction logs
CREATE TABLE  [dbo].[__RefactorLog] (OperationKey UNIQUEIDENTIFIER NOT NULL PRIMARY KEY)
GO
sp_addextendedproperty N'microsoft_database_tools_support', N'refactoring log', N'schema', N'dbo', N'table', N'__RefactorLog'
GO
INSERT INTO [dbo].[__RefactorLog] (OperationKey) values ('64d3ebb2-ca1f-4ccc-814b-b7897cbd24e8')

GO

GO
/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
INSERT INTO Country VALUES ('CS','Serbia ja Montenegro')
	   INSERT INTO Country VALUES ('LV','Läti')
	   INSERT INTO Country VALUES ('LY','Liibüa')
	   INSERT INTO Country VALUES ('MA','Maroko')
	   INSERT INTO Country VALUES ('MD','Moldova')
	   INSERT INTO Country VALUES ('MG','Madagaskar')
	   INSERT INTO Country VALUES ('MH','Marshalli saared')
	   INSERT INTO Country VALUES ('MK','Makedoonia')
	   INSERT INTO Country VALUES ('ML','Mali')
	   INSERT INTO Country VALUES ('MN','Mongoolia')
	   INSERT INTO Country VALUES ('MO','Macau')
	   INSERT INTO Country VALUES ('MP','Põhja-Mariaanid')
	   INSERT INTO Country VALUES ('MQ','Martinique')
	   INSERT INTO Country VALUES ('MS','Montserrat')
	   INSERT INTO Country VALUES ('MT','Malta')
	   INSERT INTO Country VALUES ('MU','Mauritius')
	   INSERT INTO Country VALUES ('MV','Maldiivid')
	   INSERT INTO Country VALUES ('MW','Malawi')
	   INSERT INTO Country VALUES ('MX','Mehhiko')
	   INSERT INTO Country VALUES ('MY','Malaisia')
	   INSERT INTO Country VALUES ('MZ','Mosambiik')
	   INSERT INTO Country VALUES ('NA','Namiibia')
	   INSERT INTO Country VALUES ('NC','Uus-Kaledoonia')
	   INSERT INTO Country VALUES ('NF','Norfolki saared')
	   INSERT INTO Country VALUES ('NG','Nigeeria')
	   INSERT INTO Country VALUES ('NI','Nikaraagua')
	   INSERT INTO Country VALUES ('NL','Holland')
	   INSERT INTO Country VALUES ('NO','Norra')
	   INSERT INTO Country VALUES ('NP','Nepaal')
	   INSERT INTO Country VALUES ('NR','Nauru')
	   INSERT INTO Country VALUES ('NZ','Uus-Meremaa')
	   INSERT INTO Country VALUES ('OM','Omaan')
	   INSERT INTO Country VALUES ('PA','Panama')
	   INSERT INTO Country VALUES ('PE','Peruu')
	   INSERT INTO Country VALUES ('PF','Prantsuse Polüneesia')
	   INSERT INTO Country VALUES ('PG','Paapua Uus-Guinea')
	   INSERT INTO Country VALUES ('PH','Filipiinid')
	   INSERT INTO Country VALUES ('PL','Poola')
	   INSERT INTO Country VALUES ('PM','St. Pierre ja Miquelon')
	   INSERT INTO Country VALUES ('PN','Pitcairni saar')
	   INSERT INTO Country VALUES ('PR','Puerto Rico')
	   INSERT INTO Country VALUES ('PT','Portugal')
	   INSERT INTO Country VALUES ('PW','Palau')
	   INSERT INTO Country VALUES ('PY','Paraguay')
	   INSERT INTO Country VALUES ('QA','Qatar')
	   INSERT INTO Country VALUES ('RE','Reunion')
	   INSERT INTO Country VALUES ('LU','Luksemburg')
	   INSERT INTO Country VALUES ('RO','Rumeenia')
	   INSERT INTO Country VALUES ('RW','Rwanda')
	   INSERT INTO Country VALUES ('SA','Saudi Araabia')
	   INSERT INTO Country VALUES ('SB','Saalomoni saared')
	   INSERT INTO Country VALUES ('SC','Seišellid')
	   INSERT INTO Country VALUES ('SD','Sudaan')
	   INSERT INTO Country VALUES ('SE','Rootsi')
	   INSERT INTO Country VALUES ('SG','Singapur')
	   INSERT INTO Country VALUES ('SH','St. Helena')
	   INSERT INTO Country VALUES ('SI','Sloveenia')
	   INSERT INTO Country VALUES ('AD','Andorra')
	   INSERT INTO Country VALUES ('AE','Araabia Ühendemiraadid')
	   INSERT INTO Country VALUES ('AF','Afganistaan')
	   INSERT INTO Country VALUES ('AG','Antigua ja Barbuda')
	   INSERT INTO Country VALUES ('AI','Anguilla')
	   INSERT INTO Country VALUES ('AL','Albaania')
	   INSERT INTO Country VALUES ('AN','Hollandi Antillid')
	   INSERT INTO Country VALUES ('AO','Angoola')
	   INSERT INTO Country VALUES ('AR','Argentiina')
	   INSERT INTO Country VALUES ('AS','Ameerika Samoa')
	   INSERT INTO Country VALUES ('AT','Austria')
	   INSERT INTO Country VALUES ('AU','Austraalia')
	   INSERT INTO Country VALUES ('AW','Aruuba')
	   INSERT INTO Country VALUES ('AZ','Aserbaidžaan')
	   INSERT INTO Country VALUES ('BA','Bosnia Hertsegoviina')
	   INSERT INTO Country VALUES ('BB','Barbados')
	   INSERT INTO Country VALUES ('BE','Belgia')
	   INSERT INTO Country VALUES ('BG','Bulgaaria')
	   INSERT INTO Country VALUES ('BH','Bahrain')
	   INSERT INTO Country VALUES ('BI','Burundi')
	   INSERT INTO Country VALUES ('BJ','Benin')
	   INSERT INTO Country VALUES ('BN','Brunei Darussalam')
	   INSERT INTO Country VALUES ('BR','Brasiilia')
	   INSERT INTO Country VALUES ('BS','Bahama')
	   INSERT INTO Country VALUES ('BT','Bhutan')
	   INSERT INTO Country VALUES ('BW','Botswaana')
	   INSERT INTO Country VALUES ('BZ','Belize')
	   INSERT INTO Country VALUES ('CA','Kanada')
	   INSERT INTO Country VALUES ('CC','Kookossaared')
	   INSERT INTO Country VALUES ('CH','Šveits')
	   INSERT INTO Country VALUES ('CI','Elevandiluurannik')
	   INSERT INTO Country VALUES ('CK','Cooki saared')
	   INSERT INTO Country VALUES ('CL','Tšiili')
	   INSERT INTO Country VALUES ('CM','Kamerun')
	   INSERT INTO Country VALUES ('CN','Hiina')
	   INSERT INTO Country VALUES ('CR','Costa Rica')
	   INSERT INTO Country VALUES ('CU','Kuuba')
	   INSERT INTO Country VALUES ('CX','Jõulusaar')
	   INSERT INTO Country VALUES ('CZ','Tšehhi Vabariik')
	   INSERT INTO Country VALUES ('DJ','Djibouti')
	   INSERT INTO Country VALUES ('DK','Taani')
	   INSERT INTO Country VALUES ('DM','Dominica')
	   INSERT INTO Country VALUES ('DO','Dominica Vabariik')
	   INSERT INTO Country VALUES ('DZ','Alžeeria')
	   INSERT INTO Country VALUES ('EC','Ecuador')
	   INSERT INTO Country VALUES ('EE','Eesti')
	   INSERT INTO Country VALUES ('EG','Egiptus')
	   INSERT INTO Country VALUES ('LC','Saint Lucia')
	   INSERT INTO Country VALUES ('LR','Libeeria')
	   INSERT INTO Country VALUES ('LS','Lesotho')
	   INSERT INTO Country VALUES ('LT','Leedu')
	   INSERT INTO Country VALUES ('BF','Burkina Faso')
	   INSERT INTO Country VALUES ('BM','Bermuuda')
	   INSERT INTO Country VALUES ('BO','Boliivia')
	   INSERT INTO Country VALUES ('ZZ','Merel')
	   INSERT INTO Country VALUES ('BY','Valgevene')
	   INSERT INTO Country VALUES ('CF','Kesk-Aafrika Vabariik')
	   INSERT INTO Country VALUES ('CO','Kolumbia')
	   INSERT INTO Country VALUES ('CV','Roheneemesaared')
	   INSERT INTO Country VALUES ('DE','Saksamaa')
	   INSERT INTO Country VALUES ('SK','Slovakkia')
	   INSERT INTO Country VALUES ('SN','Senegal')
	   INSERT INTO Country VALUES ('GU','Guam')
	   INSERT INTO Country VALUES ('GZ','Gaza sektor')
	   INSERT INTO Country VALUES ('HR','Horvaatia')
	   INSERT INTO Country VALUES ('IL','Iisrael')
	   INSERT INTO Country VALUES ('IR','Iraan')
	   INSERT INTO Country VALUES ('KH','Kambodža')
	   INSERT INTO Country VALUES ('KN','Saint Kitts - Nevis')
	   INSERT INTO Country VALUES ('KZ','Kasahstan')
	   INSERT INTO Country VALUES ('YT','Mayotte')
	   INSERT INTO Country VALUES ('LI','Liechtenstein')
	   INSERT INTO Country VALUES ('MC','Monaco')
	   INSERT INTO Country VALUES ('MR','Mauritaania')
	   INSERT INTO Country VALUES ('NE','Niger')
	   INSERT INTO Country VALUES ('NU','Niue')
	   INSERT INTO Country VALUES ('PK','Pakistan')
	   INSERT INTO Country VALUES ('RU','Vene Föderatsioon')
	   INSERT INTO Country VALUES ('AM','Armeenia')
	   INSERT INTO Country VALUES ('BD','Bangladesh')
	   INSERT INTO Country VALUES ('CG','Kongo')
	   INSERT INTO Country VALUES ('CY','Küpros')
	   INSERT INTO Country VALUES ('SJ','Svalbard ja Jan Mayen')
	   INSERT INTO Country VALUES ('SY','Süüria Araabia Vabariik')
	   INSERT INTO Country VALUES ('TZ','Tansaania Ühendatud Vabariik')
	   INSERT INTO Country VALUES ('ZM','Sambia')
	   INSERT INTO Country VALUES ('ER','Eritrea')
	   INSERT INTO Country VALUES ('GB','Suurbrittannia')
	   INSERT INTO Country VALUES ('IN','India')
	   INSERT INTO Country VALUES ('KG','Kõrgõzstan')
	   INSERT INTO Country VALUES ('LK','Sri Lanka')
	   INSERT INTO Country VALUES ('US','Ameerika Ühendriigid')
	   INSERT INTO Country VALUES ('XZ','Rahvusvaheline tsoon')
	   INSERT INTO Country VALUES ('SL','Sierra Leone')
	   INSERT INTO Country VALUES ('SM','San Marino')
	   INSERT INTO Country VALUES ('SO','Somaalia')
	   INSERT INTO Country VALUES ('SR','Suriname')
	   INSERT INTO Country VALUES ('ST','Sao Tome ja Principe')
	   INSERT INTO Country VALUES ('SV','El Salvador')
	   INSERT INTO Country VALUES ('SZ','Svaasimaa')
	   INSERT INTO Country VALUES ('TC','Turks ja Caicos')
	   INSERT INTO Country VALUES ('TD','Tšaad')
	   INSERT INTO Country VALUES ('TF','Prantsuse alad lõunas ja Antarktikas')
	   INSERT INTO Country VALUES ('TG','Togo')
	   INSERT INTO Country VALUES ('TH','Tai')
	   INSERT INTO Country VALUES ('TJ','Tadžikistan')
	   INSERT INTO Country VALUES ('TK','Tokelau')
	   INSERT INTO Country VALUES ('TM','Türkmenistan')
	   INSERT INTO Country VALUES ('TN','Tuneesia')
	   INSERT INTO Country VALUES ('TO','Tonga')
	   INSERT INTO Country VALUES ('TR','Türgi')
	   INSERT INTO Country VALUES ('TT','Trinidad ja Tobago')
	   INSERT INTO Country VALUES ('TV','Tuvalu')
	   INSERT INTO Country VALUES ('TW','Taiwan')
	   INSERT INTO Country VALUES ('UA','Ukraina')
	   INSERT INTO Country VALUES ('UG','Uganda')
	   INSERT INTO Country VALUES ('UM','Ameerika Ühendriikide hajasaared')
	   INSERT INTO Country VALUES ('UY','Uruguay')
	   INSERT INTO Country VALUES ('UZ','Usbekistan')
	   INSERT INTO Country VALUES ('VA','Vatikani Linnriik')
	   INSERT INTO Country VALUES ('VC','Saint Vincent ja Grenadiinid')
	   INSERT INTO Country VALUES ('VE','Venetsueela')
	   INSERT INTO Country VALUES ('VG','Briti Neitsisaared')
	   INSERT INTO Country VALUES ('VI','Ühendriikide Neitsisaared')
	   INSERT INTO Country VALUES ('VN','Vietnam')
	   INSERT INTO Country VALUES ('VU','Vanuatu')
	   INSERT INTO Country VALUES ('WE','Jordani Läänekallas')
	   INSERT INTO Country VALUES ('WF','Wallise ja Futuna saared')
	   INSERT INTO Country VALUES ('WS','Samoa')
	   INSERT INTO Country VALUES ('YE','Jeemen')
	   INSERT INTO Country VALUES ('ZA','Lõuna-Aafrika')
	   INSERT INTO Country VALUES ('ZR','Sair')
	   INSERT INTO Country VALUES ('ZW','Zimbabwe')
	   INSERT INTO Country VALUES ('GS','Lõuna-Gruusia ja Lõuna Võileiva saared')
	   INSERT INTO Country VALUES ('CD','Kongo Demokraatlik Vabariik')
	   INSERT INTO Country VALUES ('AQ','Antarktika')
	   INSERT INTO Country VALUES ('OT','Muu')
	   INSERT INTO Country VALUES ('TA','Tahiiti')
	   INSERT INTO Country VALUES ('MM','Myanmar (Former Burma)')
	   INSERT INTO Country VALUES ('TL','Ida-Timor')
	   INSERT INTO Country VALUES ('ES','Hispaania')
	   INSERT INTO Country VALUES ('ET','Etioopia')
	   INSERT INTO Country VALUES ('FI','Soome')
	   INSERT INTO Country VALUES ('FJ','Fidži')
	   INSERT INTO Country VALUES ('FK','Falklandi saared')
	   INSERT INTO Country VALUES ('FM','Mikroneesia')
	   INSERT INTO Country VALUES ('FO','Fääri saared')
	   INSERT INTO Country VALUES ('FR','Prantsusmaa')
	   INSERT INTO Country VALUES ('GA','Gabon')
	   INSERT INTO Country VALUES ('GD','Grenada')
	   INSERT INTO Country VALUES ('GE','Gruusia')
	   INSERT INTO Country VALUES ('GF','Prantsuse Guajaana')
	   INSERT INTO Country VALUES ('GH','Ghana')
	   INSERT INTO Country VALUES ('GI','Gibraltar')
	   INSERT INTO Country VALUES ('GL','Gröönimaa')
	   INSERT INTO Country VALUES ('GM','Gambia')
	   INSERT INTO Country VALUES ('GN','Guinea')
	   INSERT INTO Country VALUES ('GP','Guadeloupe')
	   INSERT INTO Country VALUES ('GQ','Ekvatoriaalne Guinea')
	   INSERT INTO Country VALUES ('GR','Kreeka')
	   INSERT INTO Country VALUES ('GT','Guatemala')
	   INSERT INTO Country VALUES ('GW','Guinea-Bissau')
	   INSERT INTO Country VALUES ('GY','Guyana')
	   INSERT INTO Country VALUES ('HK','Hong Kong')
	   INSERT INTO Country VALUES ('HM','Heardi ja Mcdonaldi saared')
	   INSERT INTO Country VALUES ('HN','Honduras')
	   INSERT INTO Country VALUES ('HT','Haiti')
	   INSERT INTO Country VALUES ('HU','Ungari')
	   INSERT INTO Country VALUES ('ID','Indoneesia')
	   INSERT INTO Country VALUES ('IE','Iirimaa')
	   INSERT INTO Country VALUES ('IO','India ookeani territoorium')
	   INSERT INTO Country VALUES ('IQ','Iraak')
	   INSERT INTO Country VALUES ('IS','Island')
	   INSERT INTO Country VALUES ('IT','Itaalia')
	   INSERT INTO Country VALUES ('JM','Jamaika')
	   INSERT INTO Country VALUES ('JO','Jordaania')
	   INSERT INTO Country VALUES ('JP','Jaapan')
	   INSERT INTO Country VALUES ('KE','Keenia')
	   INSERT INTO Country VALUES ('KI','Kiribati')
	   INSERT INTO Country VALUES ('KM','Comoros')
	   INSERT INTO Country VALUES ('KP','Korea Demokraatlik Rahvavabariik (Põhja-)')
	   INSERT INTO Country VALUES ('KR','Korea Vabariik (Lõuna-)')
	   INSERT INTO Country VALUES ('KW','Kuveit')
	   INSERT INTO Country VALUES ('KY','Caymani saared')
	   INSERT INTO Country VALUES ('LA','Lao Demokraatlik Rahvavabariik')
	   INSERT INTO Country VALUES ('LB','Liibanon')
	   INSERT INTO Country VALUES ('EH','Lääne-Sahaara')
	   INSERT INTO Country VALUES ('ME','Montenegro')
	   INSERT INTO Country VALUES ('','')
       
GO
PRINT N'Checking existing data against newly created constraints';


GO
USE [$(DatabaseName)];


GO
ALTER TABLE [dbo].[CMR] WITH CHECK CHECK CONSTRAINT [FK_CMR_ConsingeePaymentInfo];

ALTER TABLE [dbo].[CMR] WITH CHECK CHECK CONSTRAINT [FK_CMR_OrderItem];

ALTER TABLE [dbo].[CMR] WITH CHECK CHECK CONSTRAINT [FK_CMR_SenderPaymentInfo];

ALTER TABLE [dbo].[Invoice] WITH CHECK CHECK CONSTRAINT [FK_Invoice_CompanyToForeignKey];

ALTER TABLE [dbo].[Invoice] WITH CHECK CHECK CONSTRAINT [FK_Invoice_UserIDForeignKey];

ALTER TABLE [dbo].[OrderItem] WITH CHECK CHECK CONSTRAINT [FK_OrderItem_CMR];


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'$(DatabaseName)')
    BEGIN
        DECLARE @VarDecimalSupported AS BIT;
        SELECT @VarDecimalSupported = 0;
        IF ((ServerProperty(N'EngineEdition') = 3)
            AND (((@@microsoftversion / power(2, 24) = 9)
                  AND (@@microsoftversion & 0xffff >= 3024))
                 OR ((@@microsoftversion / power(2, 24) = 10)
                     AND (@@microsoftversion & 0xffff >= 1600))))
            SELECT @VarDecimalSupported = 1;
        IF (@VarDecimalSupported > 0)
            BEGIN
                EXECUTE sp_db_vardecimal_storage_format N'$(DatabaseName)', 'ON';
            END
    END


GO
