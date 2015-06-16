ALTER TABLE [dbo].[Invoice]
	ADD CONSTRAINT [FK_Invoice_UserIDForeignKey] 
	FOREIGN KEY (GeneratedByUserID)
	REFERENCES BFUser (ID)

