
INSERT INTO [dbo].[User] (UserGuid, Email, IsVatPayer, IsActive, Deleted, InvalidLoginAttempts, CreatedUTC) VALUES (NEWID(), N'jra@gmail.com', 0, 1, 0, 0, GETUTCDATE());

INSERT INTO [dbo].[UserRole] (Name, RoleTypeId, IsActive, HasEditableRights, CreatedUTC) VALUES (N'Administrators', 1, 1, 0, GETUTCDATE());
INSERT INTO [dbo].[UserRole] (Name, RoleTypeId, IsActive, HasEditableRights, CreatedUTC) VALUES (N'Moderators', 5, 1, 1, GETUTCDATE());
INSERT INTO [dbo].[UserRole] (Name, RoleTypeId, IsActive, HasEditableRights, CreatedUTC) VALUES (N'FullReadOnlyAccess', 10, 1, 0, GETUTCDATE());
INSERT INTO [dbo].[UserRole] (Name, RoleTypeId, IsActive, HasEditableRights, CreatedUTC) VALUES (N'RegisteredUsers', 20, 1, 0, GETUTCDATE());
INSERT INTO [dbo].[UserRole] (Name, RoleTypeId, IsActive, HasEditableRights, CreatedUTC) VALUES (N'Visitors', 50, 1, 0, GETUTCDATE());

INSERT INTO [dbo].[UserRoleMap] (UserId, UserRoleId) VALUES (1, 1);

INSERT INTO [dbo].[UserPassword] (UserId, PasswordHash, PasswordSalt, HashTypeId, CreatedUTC) 
VALUES (1, N'kWrIvfkpM7se09EiXQub78O/JWSndo9yqsHX8+qzOZ0=', N't7raDvoJf1a6FKaIiWVFDg==', 50, GETUTCDATE());
