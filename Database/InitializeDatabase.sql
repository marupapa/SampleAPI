-- ユーザーテーブル作成
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20),
    PasswordHash NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    LastLoginAt DATETIME2,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0
);

-- インデックス作成
CREATE INDEX IX_Users_Username ON Users(Username) WHERE IsDeleted = 0;
CREATE INDEX IX_Users_Email ON Users(Email) WHERE IsDeleted = 0;
CREATE INDEX IX_Users_CreatedAt ON Users(CreatedAt);

GO

-- ユーザー作成ストアドプロシージャ
CREATE PROCEDURE sp_CreateUser
    @Username NVARCHAR(50),
    @Email NVARCHAR(255),
    @FullName NVARCHAR(100),
    @PhoneNumber NVARCHAR(20) = NULL,
    @PasswordHash NVARCHAR(255),
    @IsActive BIT = 1,
    @NewUserId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- ユーザー名の重複チェック
        IF EXISTS (SELECT 1 FROM Users WHERE Username = @Username AND IsDeleted = 0)
        BEGIN
            RAISERROR('Username already exists', 16, 1);
            RETURN;
        END
        
        -- メールアドレスの重複チェック
        IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email AND IsDeleted = 0)
        BEGIN
            RAISERROR('Email already exists', 16, 1);
            RETURN;
        END
        
        -- ユーザー挿入
        INSERT INTO Users (Username, Email, FullName, PhoneNumber, PasswordHash, IsActive, CreatedAt)
        VALUES (@Username, @Email, @FullName, @PhoneNumber, @PasswordHash, @IsActive, GETUTCDATE());
        
        SET @NewUserId = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- ユーザー更新ストアドプロシージャ
CREATE PROCEDURE sp_UpdateUser
    @Id INT,
    @Username NVARCHAR(50),
    @Email NVARCHAR(255),
    @FullName NVARCHAR(100),
    @PhoneNumber NVARCHAR(20) = NULL,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- ユーザーの存在チェック
        IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = @Id AND IsDeleted = 0)
        BEGIN
            RAISERROR('User not found', 16, 1);
            RETURN;
        END
        
        -- ユーザー名の重複チェック（自分以外）
        IF EXISTS (SELECT 1 FROM Users WHERE Username = @Username AND Id != @Id AND IsDeleted = 0)
        BEGIN
            RAISERROR('Username already exists', 16, 1);
            RETURN;
        END
        
        -- メールアドレスの重複チェック（自分以外）
        IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email AND Id != @Id AND IsDeleted = 0)
        BEGIN
            RAISERROR('Email already exists', 16, 1);
            RETURN;
        END
        
        -- ユーザー更新
        UPDATE Users
        SET 
            Username = @Username,
            Email = @Email,
            FullName = @FullName,
            PhoneNumber = @PhoneNumber,
            IsActive = @IsActive,
            UpdatedAt = GETUTCDATE()
        WHERE Id = @Id AND IsDeleted = 0;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- ユーザー削除ストアドプロシージャ（論理削除）
CREATE PROCEDURE sp_DeleteUser
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- ユーザーの存在チェック
        IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = @Id AND IsDeleted = 0)
        BEGIN
            RAISERROR('User not found', 16, 1);
            RETURN;
        END
        
        -- 論理削除
        UPDATE Users
        SET 
            IsDeleted = 1,
            IsActive = 0,
            UpdatedAt = GETUTCDATE()
        WHERE Id = @Id;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- サンプルデータ挿入
INSERT INTO Users (Username, Email, FullName, PhoneNumber, PasswordHash, IsActive)
VALUES 
    ('admin', 'admin@example.com', 'Administrator', '090-1234-5678', 'HASH_PASSWORD_HERE', 1),
    ('testuser', 'test@example.com', 'Test User', '090-8765-4321', 'HASH_PASSWORD_HERE', 1);
GO
