@ECHO OFF
@ECHO.
@ECHO Please, wait! Generating database backup
osql -E -S YOUR-HOST-SERVER -i ".\backup.sql"
@ECHO.
@ECHO.
@ECHO Backup done. 
aws-send2s3.exe
@ECHO Backup sent to S3. 