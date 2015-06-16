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
