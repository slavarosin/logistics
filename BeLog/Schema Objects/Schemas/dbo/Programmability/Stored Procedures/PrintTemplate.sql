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


