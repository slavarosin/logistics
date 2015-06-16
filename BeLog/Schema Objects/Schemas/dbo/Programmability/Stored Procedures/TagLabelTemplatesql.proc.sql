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

