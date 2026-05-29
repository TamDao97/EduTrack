IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529085737_InitCore'
)
BEGIN
    CREATE TABLE [ConfigJsons] (
        [Id] uniqueidentifier NOT NULL,
        [JsonOrg] nvarchar(max) NULL,
        [JsonSupperAdmin] nvarchar(max) NULL,
        [CreatedUserId] uniqueidentifier NULL,
        [CreatedUserName] nvarchar(max) NULL,
        [DateCreated] datetime2 NULL,
        [ModifyUserId] uniqueidentifier NULL,
        [ModifyUserName] nvarchar(max) NULL,
        [DateModify] datetime2 NULL,
        [DeletedUserId] uniqueidentifier NULL,
        [DeletedUserName] nvarchar(max) NULL,
        [DateDeleted] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [Order] int NULL,
        CONSTRAINT [PK_ConfigJsons] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529085737_InitCore'
)
BEGIN
    CREATE TABLE [Files] (
        [Id] uniqueidentifier NOT NULL,
        [OriginalName] nvarchar(max) NOT NULL,
        [FileName] nvarchar(max) NOT NULL,
        [FilePath] nvarchar(max) NOT NULL,
        [FileType] nvarchar(max) NOT NULL,
        [FileSize] bigint NOT NULL,
        [UploadedBy] uniqueidentifier NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Files] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529085737_InitCore'
)
BEGIN
    CREATE TABLE [Pages] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Url] nvarchar(max) NULL,
        [Icon] nvarchar(max) NULL,
        [IsActive] bit NULL,
        [IsTab] bit NULL,
        [IsHomePage] bit NULL,
        [IdParent] uniqueidentifier NULL,
        [PermissionCode] nvarchar(max) NULL,
        [Order] int NULL,
        [CreatedUserId] uniqueidentifier NULL,
        [CreatedUserName] nvarchar(max) NULL,
        [DateCreated] datetime2 NULL,
        [ModifyUserId] uniqueidentifier NULL,
        [ModifyUserName] nvarchar(max) NULL,
        [DateModify] datetime2 NULL,
        [DeletedUserId] uniqueidentifier NULL,
        [DeletedUserName] nvarchar(max) NULL,
        [DateDeleted] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Pages] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529085737_InitCore'
)
BEGIN
    CREATE TABLE [Permissions] (
        [Id] uniqueidentifier NOT NULL,
        [ModuleCode] nvarchar(max) NOT NULL,
        [ModuleDescription] nvarchar(max) NOT NULL,
        [ModuleOrder] int NOT NULL,
        [PermissionCode] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [RoleCodes] nvarchar(max) NULL,
        [CreatedUserId] uniqueidentifier NULL,
        [CreatedUserName] nvarchar(max) NULL,
        [DateCreated] datetime2 NULL,
        [ModifyUserId] uniqueidentifier NULL,
        [ModifyUserName] nvarchar(max) NULL,
        [DateModify] datetime2 NULL,
        [DeletedUserId] uniqueidentifier NULL,
        [DeletedUserName] nvarchar(max) NULL,
        [DateDeleted] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [Order] int NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529085737_InitCore'
)
BEGIN
    CREATE TABLE [RolePermissions] (
        [Id] uniqueidentifier NOT NULL,
        [IdRole] uniqueidentifier NOT NULL,
        [IdPermission] uniqueidentifier NOT NULL,
        [CreatedUserId] uniqueidentifier NULL,
        [CreatedUserName] nvarchar(max) NULL,
        [DateCreated] datetime2 NULL,
        [ModifyUserId] uniqueidentifier NULL,
        [ModifyUserName] nvarchar(max) NULL,
        [DateModify] datetime2 NULL,
        [DeletedUserId] uniqueidentifier NULL,
        [DeletedUserName] nvarchar(max) NULL,
        [DateDeleted] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [Order] int NULL,
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529085737_InitCore'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [CreatedUserId] uniqueidentifier NULL,
        [CreatedUserName] nvarchar(max) NULL,
        [DateCreated] datetime2 NULL,
        [ModifyUserId] uniqueidentifier NULL,
        [ModifyUserName] nvarchar(max) NULL,
        [DateModify] datetime2 NULL,
        [DeletedUserId] uniqueidentifier NULL,
        [DeletedUserName] nvarchar(max) NULL,
        [DateDeleted] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [Order] int NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529085737_InitCore'
)
BEGIN
    CREATE TABLE [UserRoles] (
        [Id] uniqueidentifier NOT NULL,
        [IdUser] uniqueidentifier NOT NULL,
        [IdRole] uniqueidentifier NOT NULL,
        [CreatedUserId] uniqueidentifier NULL,
        [CreatedUserName] nvarchar(max) NULL,
        [DateCreated] datetime2 NULL,
        [ModifyUserId] uniqueidentifier NULL,
        [ModifyUserName] nvarchar(max) NULL,
        [DateModify] datetime2 NULL,
        [DeletedUserId] uniqueidentifier NULL,
        [DeletedUserName] nvarchar(max) NULL,
        [DateDeleted] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [Order] int NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529085737_InitCore'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [UserName] nvarchar(max) NULL,
        [DisplayName] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NULL,
        [PasswordHash] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [Gender] int NULL,
        [DateLockout] datetimeoffset NULL,
        [IsLockout] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        [IsAdmin] bit NOT NULL,
        [IsSuper] bit NOT NULL,
        [CreatedUserId] uniqueidentifier NULL,
        [CreatedUserName] nvarchar(max) NULL,
        [DateCreated] datetime2 NULL,
        [ModifyUserId] uniqueidentifier NULL,
        [ModifyUserName] nvarchar(max) NULL,
        [DateModify] datetime2 NULL,
        [DeletedUserId] uniqueidentifier NULL,
        [DeletedUserName] nvarchar(max) NULL,
        [DateDeleted] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [Order] int NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529085737_InitCore'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529085737_InitCore', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529101138_AddTutorDomain'
)
BEGIN
    CREATE TABLE [Lessons] (
        [Id] uniqueidentifier NOT NULL,
        [IdTutor] uniqueidentifier NOT NULL,
        [IdStudent] uniqueidentifier NOT NULL,
        [ScheduledDate] datetime2 NOT NULL,
        [StartTime] time NOT NULL,
        [EndTime] time NOT NULL,
        [Location] nvarchar(500) NULL,
        [Status] int NOT NULL,
        [ChargeAmount] decimal(18,0) NOT NULL,
        [DoneAt] datetime2 NULL,
        [Notes] nvarchar(max) NULL,
        [IdTuitionPeriod] uniqueidentifier NULL,
        [CreatedUserId] uniqueidentifier NULL,
        [CreatedUserName] nvarchar(max) NULL,
        [DateCreated] datetime2 NULL,
        [ModifyUserId] uniqueidentifier NULL,
        [ModifyUserName] nvarchar(max) NULL,
        [DateModify] datetime2 NULL,
        [DeletedUserId] uniqueidentifier NULL,
        [DeletedUserName] nvarchar(max) NULL,
        [DateDeleted] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [Order] int NULL,
        CONSTRAINT [PK_Lessons] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529101138_AddTutorDomain'
)
BEGIN
    CREATE TABLE [Parents] (
        [Id] uniqueidentifier NOT NULL,
        [IdTutor] uniqueidentifier NOT NULL,
        [FullName] nvarchar(200) NOT NULL,
        [Phone] nvarchar(20) NOT NULL,
        [Email] nvarchar(100) NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedUserId] uniqueidentifier NULL,
        [CreatedUserName] nvarchar(max) NULL,
        [DateCreated] datetime2 NULL,
        [ModifyUserId] uniqueidentifier NULL,
        [ModifyUserName] nvarchar(max) NULL,
        [DateModify] datetime2 NULL,
        [DeletedUserId] uniqueidentifier NULL,
        [DeletedUserName] nvarchar(max) NULL,
        [DateDeleted] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [Order] int NULL,
        CONSTRAINT [PK_Parents] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529101138_AddTutorDomain'
)
BEGIN
    CREATE TABLE [Students] (
        [Id] uniqueidentifier NOT NULL,
        [IdTutor] uniqueidentifier NOT NULL,
        [IdParent] uniqueidentifier NOT NULL,
        [FullName] nvarchar(200) NOT NULL,
        [DateBirth] datetime2 NULL,
        [Grade] nvarchar(50) NULL,
        [Subject] nvarchar(100) NULL,
        [PerLessonRate] decimal(18,0) NOT NULL,
        [AvatarFileId] uniqueidentifier NULL,
        [Status] int NOT NULL,
        [StartedAt] datetime2 NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedUserId] uniqueidentifier NULL,
        [CreatedUserName] nvarchar(max) NULL,
        [DateCreated] datetime2 NULL,
        [ModifyUserId] uniqueidentifier NULL,
        [ModifyUserName] nvarchar(max) NULL,
        [DateModify] datetime2 NULL,
        [DeletedUserId] uniqueidentifier NULL,
        [DeletedUserName] nvarchar(max) NULL,
        [DateDeleted] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [Order] int NULL,
        CONSTRAINT [PK_Students] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529101138_AddTutorDomain'
)
BEGIN
    CREATE TABLE [TuitionPeriods] (
        [Id] uniqueidentifier NOT NULL,
        [IdTutor] uniqueidentifier NOT NULL,
        [IdStudent] uniqueidentifier NOT NULL,
        [PeriodMonth] int NOT NULL,
        [PeriodYear] int NOT NULL,
        [ClosedAt] datetime2 NULL,
        [TotalLessons] int NOT NULL,
        [TotalAmount] decimal(18,0) NOT NULL,
        [Adjustment] decimal(18,0) NOT NULL,
        [FinalAmount] decimal(18,0) NOT NULL,
        [PaidAmount] decimal(18,0) NOT NULL,
        [Status] int NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedUserId] uniqueidentifier NULL,
        [CreatedUserName] nvarchar(max) NULL,
        [DateCreated] datetime2 NULL,
        [ModifyUserId] uniqueidentifier NULL,
        [ModifyUserName] nvarchar(max) NULL,
        [DateModify] datetime2 NULL,
        [DeletedUserId] uniqueidentifier NULL,
        [DeletedUserName] nvarchar(max) NULL,
        [DateDeleted] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [Order] int NULL,
        CONSTRAINT [PK_TuitionPeriods] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529101138_AddTutorDomain'
)
BEGIN
    CREATE TABLE [TutorProfiles] (
        [Id] uniqueidentifier NOT NULL,
        [IdUser] uniqueidentifier NOT NULL,
        [BankName] nvarchar(100) NULL,
        [BankAccountNumber] nvarchar(50) NULL,
        [BankAccountHolder] nvarchar(200) NULL,
        [Subjects] nvarchar(500) NULL,
        [Bio] nvarchar(max) NULL,
        [CreatedUserId] uniqueidentifier NULL,
        [CreatedUserName] nvarchar(max) NULL,
        [DateCreated] datetime2 NULL,
        [ModifyUserId] uniqueidentifier NULL,
        [ModifyUserName] nvarchar(max) NULL,
        [DateModify] datetime2 NULL,
        [DeletedUserId] uniqueidentifier NULL,
        [DeletedUserName] nvarchar(max) NULL,
        [DateDeleted] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [Order] int NULL,
        CONSTRAINT [PK_TutorProfiles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529101138_AddTutorDomain'
)
BEGIN
    CREATE INDEX [IX_Lessons_IdStudent_ScheduledDate] ON [Lessons] ([IdStudent], [ScheduledDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529101138_AddTutorDomain'
)
BEGIN
    CREATE INDEX [IX_Lessons_IdTuitionPeriod] ON [Lessons] ([IdTuitionPeriod]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529101138_AddTutorDomain'
)
BEGIN
    CREATE INDEX [IX_Lessons_IdTutor_ScheduledDate] ON [Lessons] ([IdTutor], [ScheduledDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529101138_AddTutorDomain'
)
BEGIN
    CREATE INDEX [IX_Parents_IdTutor_Phone] ON [Parents] ([IdTutor], [Phone]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529101138_AddTutorDomain'
)
BEGIN
    CREATE INDEX [IX_Students_IdParent] ON [Students] ([IdParent]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529101138_AddTutorDomain'
)
BEGIN
    CREATE INDEX [IX_Students_IdTutor_Status] ON [Students] ([IdTutor], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529101138_AddTutorDomain'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TuitionPeriods_IdStudent_PeriodYear_PeriodMonth] ON [TuitionPeriods] ([IdStudent], [PeriodYear], [PeriodMonth]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529101138_AddTutorDomain'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TutorProfiles_IdUser] ON [TutorProfiles] ([IdUser]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529101138_AddTutorDomain'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529101138_AddTutorDomain', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529103435_AddNotification'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] uniqueidentifier NOT NULL,
        [IdTutor] uniqueidentifier NOT NULL,
        [IdStudent] uniqueidentifier NULL,
        [Type] int NOT NULL,
        [RefId] uniqueidentifier NULL,
        [Title] nvarchar(200) NOT NULL,
        [BodyText] nvarchar(max) NOT NULL,
        [ZaloDeepLink] nvarchar(max) NULL,
        [ParentPhone] nvarchar(20) NULL,
        [StudentFullName] nvarchar(200) NULL,
        [Status] int NOT NULL,
        [ScheduledAt] datetime2 NOT NULL,
        [SentAt] datetime2 NULL,
        [CreatedUserId] uniqueidentifier NULL,
        [CreatedUserName] nvarchar(max) NULL,
        [DateCreated] datetime2 NULL,
        [ModifyUserId] uniqueidentifier NULL,
        [ModifyUserName] nvarchar(max) NULL,
        [DateModify] datetime2 NULL,
        [DeletedUserId] uniqueidentifier NULL,
        [DeletedUserName] nvarchar(max) NULL,
        [DateDeleted] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [Order] int NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529103435_AddNotification'
)
BEGIN
    CREATE INDEX [IX_Notifications_IdTutor_Status_ScheduledAt] ON [Notifications] ([IdTutor], [Status], [ScheduledAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529103435_AddNotification'
)
BEGIN
    CREATE INDEX [IX_Notifications_RefId] ON [Notifications] ([RefId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529103435_AddNotification'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529103435_AddNotification', N'8.0.8');
END;
GO

COMMIT;
GO

