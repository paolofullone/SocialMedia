/* Change to the SocialMedia database */
USE SocialMedia;
GO

/* Create user */
IF NOT EXISTS(SELECT *
FROM sys.server_principals
WHERE name = 'SMUser')
BEGIN
	CREATE LOGIN SMUser WITH PASSWORD=N'SmPA$$06500', DEFAULT_DATABASE=SocialMedia
END


IF NOT EXISTS(SELECT *
FROM sys.database_principals
WHERE name = 'SMUser')
BEGIN
	EXEC sp_adduser 'SMUser', 'SMUser', 'db_owner';
END
