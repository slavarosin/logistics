ALTER TABLE [dbo].[CMR]
	ADD CONSTRAINT [FK_CMR_ConsingeePaymentInfo] 
	FOREIGN KEY (ConsigneePaymentID)
	REFERENCES PaymentInfo (ID)	

