ALTER TABLE [dbo].[CMR]
	ADD CONSTRAINT [FK_CMR_SenderPaymentInfo] 
	FOREIGN KEY (SenderPaymentID)
	REFERENCES PaymentInfo (ID)	

