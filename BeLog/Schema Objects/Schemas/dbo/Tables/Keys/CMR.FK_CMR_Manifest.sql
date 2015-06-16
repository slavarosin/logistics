ALTER TABLE [dbo].[CMR]
	ADD CONSTRAINT [FK_CMR_Manifest] 
	FOREIGN KEY (ManifestId)	
	REFERENCES Manifest (id)	