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