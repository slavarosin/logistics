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
	  cm.Name as Consignee,
	  cm.Address as ConsigneeAddress,
	  cm1.Address as ConsignorAddress,
	  cm.Phone as ConsigneePhone,
	  cm1.Phone as ConsigorPhone,
	  BFUser.FirstName as FName,
	  BFUser.LastName as LNamme,
	  BFUser.PhoneNumber as PNumber,
	  SUM(OI.NumOfPackages) as TotalNumOfPackages,
	  SUM(OI.Volume) as TotalVolume,
	  SUM(OI.Weight) as TotalWeight
	  FROM Manifest MAN right join CMR cmr on MAN.id = cmr.ManifestId inner join Company cm on cm.ID = cmr.ConsigneeID inner join 
	  Company cm1 on cm1.ID = cmr.SenderID inner join BFUser on MAN.CreatedBy = BFUser.ID inner join OrderItem OI on OI.CMRID = cmr.ID
	   WHERE MAN.id = @ManifestID
	  GROUP BY  
	  MAN.Departure,
	  MAN.Arrival, 
	  MAN.ManifestDate,
	  MAN.ManifesName,
	  cmr.SenderID,
	  cmr.ConsigneeID,
	  cm1.Name,
	  cm.Name,
	  cm.Address,
	  cm1.Address,
	  cm.Phone,
	  cm1.Phone,
	  BFUser.FirstName,
	  BFUser.LastName,
	  BFUser.PhoneNumber
	 

GO


