ALTER TABLE [dbo].[Invoice]
	ADD CONSTRAINT [FK_Invoice_CompanyToForeignKey] 
	FOREIGN KEY (CompanyToID)
	REFERENCES Company (ID)