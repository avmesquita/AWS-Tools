DECLARE @path VARCHAR(256) 

SET @path = 'C:\TEMPO\database_backup.bak'

BACKUP DATABASE YOUR-DATABASE TO DISK = @path WITH FORMAT;  
