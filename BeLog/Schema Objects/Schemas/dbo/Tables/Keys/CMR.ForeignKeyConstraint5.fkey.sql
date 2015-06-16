ALTER TABLE [dbo].[CMR]
	ADD CONSTRAINT [FK_CMR_OrderItem] 
	FOREIGN KEY (TransportID)
	REFERENCES Transport (ID)	

