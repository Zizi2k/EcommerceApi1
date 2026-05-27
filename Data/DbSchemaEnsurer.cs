using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Data
{
    /// <summary>Bổ sung cột Orders nếu migration chưa chạy đủ (tránh lỗi Invalid column name khi checkout).</summary>
    public static class DbSchemaEnsurer
    {
        public static void EnsureOrderCustomerColumns(ApplicationDbContext context)
        {
            context.Database.ExecuteSqlRaw(@"
IF COL_LENGTH('Orders', 'CustomerName') IS NULL
    ALTER TABLE [Orders] ADD [CustomerName] nvarchar(max) NULL;
IF COL_LENGTH('Orders', 'CustomerPhone') IS NULL
    ALTER TABLE [Orders] ADD [CustomerPhone] nvarchar(max) NULL;
IF COL_LENGTH('Orders', 'ShippingAddress') IS NULL
    ALTER TABLE [Orders] ADD [ShippingAddress] nvarchar(max) NULL;
IF COL_LENGTH('Orders', 'PhoneVerified') IS NULL
    ALTER TABLE [Orders] ADD [PhoneVerified] bit NOT NULL CONSTRAINT [DF_Orders_PhoneVerified] DEFAULT 0;
IF COL_LENGTH('Orders', 'AccountUsername') IS NULL
    ALTER TABLE [Orders] ADD [AccountUsername] nvarchar(128) NULL;
");

            context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderItems_ProductId' AND object_id = OBJECT_ID('OrderItems'))
    CREATE INDEX [IX_OrderItems_ProductId] ON [OrderItems]([ProductId]);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_OrderItems_Products_ProductId')
    ALTER TABLE [OrderItems] ADD CONSTRAINT [FK_OrderItems_Products_ProductId]
        FOREIGN KEY ([ProductId]) REFERENCES [Products]([Id]) ON DELETE NO ACTION;
");
        }

        public static void EnsureUsersForGoogleLogin(ApplicationDbContext context)
        {
            // App hiện chạy demo user từ file; Google login cần bảng Users để lưu Email/Name/Avatar/GoogleId.
            context.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[Users]', N'U') IS NULL
BEGIN
    CREATE TABLE [Users](
        [Id] int IDENTITY(1,1) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [FullName] nvarchar(256) NOT NULL,
        [PasswordHash] nvarchar(512) NOT NULL CONSTRAINT [DF_Users_PasswordHash] DEFAULT 'GOOGLE_OAUTH',
        [Role] nvarchar(32) NOT NULL CONSTRAINT [DF_Users_Role] DEFAULT 'Customer',
        [AuthProvider] nvarchar(32) NOT NULL CONSTRAINT [DF_Users_AuthProvider] DEFAULT 'Local',
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_Users_CreatedAt] DEFAULT SYSUTCDATETIME(),
        [GoogleSub] nvarchar(128) NULL,
        [Name] nvarchar(256) NOT NULL CONSTRAINT [DF_Users_Name] DEFAULT '',
        [AvatarUrl] nvarchar(2048) NULL,
        [GoogleId] nvarchar(128) NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF COL_LENGTH('Users', 'Email') IS NULL
    ALTER TABLE [Users] ADD [Email] nvarchar(256) NOT NULL CONSTRAINT [DF_Users_Email] DEFAULT '';
IF COL_LENGTH('Users', 'Name') IS NULL
    ALTER TABLE [Users] ADD [Name] nvarchar(256) NOT NULL CONSTRAINT [DF_Users_Name] DEFAULT '';
IF COL_LENGTH('Users', 'AvatarUrl') IS NULL
    ALTER TABLE [Users] ADD [AvatarUrl] nvarchar(2048) NULL;
IF COL_LENGTH('Users', 'GoogleId') IS NULL
    ALTER TABLE [Users] ADD [GoogleId] nvarchar(128) NULL;
IF COL_LENGTH('Users', 'Role') IS NULL
    ALTER TABLE [Users] ADD [Role] nvarchar(32) NOT NULL CONSTRAINT [DF_Users_Role2] DEFAULT 'Customer';

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('Users'))
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users]([Email]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_GoogleId' AND object_id = OBJECT_ID('Users'))
    CREATE INDEX [IX_Users_GoogleId] ON [Users]([GoogleId]);
IF COL_LENGTH('Users', 'BackgroundUrl') IS NULL
    ALTER TABLE [Users] ADD [BackgroundUrl] nvarchar(2048) NULL;
");
        }

        public static void EnsureOrderAdminReviewColumns(ApplicationDbContext context)
        {
            context.Database.ExecuteSqlRaw(@"
IF COL_LENGTH('Orders', 'AdminRating') IS NULL
    ALTER TABLE [Orders] ADD [AdminRating] int NULL;
IF COL_LENGTH('Orders', 'AdminReviewNote') IS NULL
    ALTER TABLE [Orders] ADD [AdminReviewNote] nvarchar(2000) NULL;
IF COL_LENGTH('Orders', 'AdminReviewedAtUtc') IS NULL
    ALTER TABLE [Orders] ADD [AdminReviewedAtUtc] datetime2 NULL;
IF COL_LENGTH('Orders', 'CustomerRating') IS NULL
    ALTER TABLE [Orders] ADD [CustomerRating] int NULL;
IF COL_LENGTH('Orders', 'CustomerReviewNote') IS NULL
    ALTER TABLE [Orders] ADD [CustomerReviewNote] nvarchar(2000) NULL;
IF COL_LENGTH('Orders', 'CustomerReviewedAtUtc') IS NULL
    ALTER TABLE [Orders] ADD [CustomerReviewedAtUtc] datetime2 NULL;
IF COL_LENGTH('Orders', 'CancelReason') IS NULL
    ALTER TABLE [Orders] ADD [CancelReason] nvarchar(256) NULL;
IF COL_LENGTH('Orders', 'CancelNote') IS NULL
    ALTER TABLE [Orders] ADD [CancelNote] nvarchar(2000) NULL;
IF COL_LENGTH('Orders', 'CancelRequestedAtUtc') IS NULL
    ALTER TABLE [Orders] ADD [CancelRequestedAtUtc] datetime2 NULL;
");

            context.Database.ExecuteSqlRaw(@"
UPDATE [Orders] SET [Status] = N'Delivered' WHERE [Status] IN (N'Completed', N'completed', N'HoanThanh', N'Hoàn thành');
UPDATE [Orders] SET [Status] = N'Preparing'
WHERE [Status] NOT IN (N'Preparing', N'Delivering', N'Delivered', N'Cancelled');
");
        }

        public static void EnsurePromotionalProductsTable(ApplicationDbContext context)
        {
            context.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[PromotionalProducts]', N'U') IS NULL
BEGIN
    CREATE TABLE [PromotionalProducts](
        [Id] int IDENTITY(1,1) NOT NULL,
        [ProductId] int NOT NULL,
        [Headline] nvarchar(256) NULL,
        [Subtitle] nvarchar(512) NULL,
        [BadgeText] nvarchar(64) NULL,
        [PromoPrice] decimal(18,2) NULL,
        [SortOrder] int NOT NULL CONSTRAINT [DF_PromotionalProducts_SortOrder] DEFAULT 0,
        [IsActive] bit NOT NULL CONSTRAINT [DF_PromotionalProducts_IsActive] DEFAULT 1,
        [IsFlashSale] bit NOT NULL CONSTRAINT [DF_PromotionalProducts_IsFlashSale] DEFAULT 0,
        [FlashSaleType] nvarchar(24) NOT NULL CONSTRAINT [DF_PromotionalProducts_FlashSaleType] DEFAULT N'None',
        [DailySlotKey] nvarchar(24) NULL,
        [EventStartUtc] datetime2 NULL,
        [EventEndUtc] datetime2 NULL,
        [CreatedAtUtc] datetime2 NOT NULL CONSTRAINT [DF_PromotionalProducts_CreatedAtUtc] DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [PK_PromotionalProducts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PromotionalProducts_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_PromotionalProducts_ProductId] ON [PromotionalProducts]([ProductId]);
    CREATE INDEX [IX_PromotionalProducts_IsActive_SortOrder] ON [PromotionalProducts]([IsActive], [SortOrder]);
END;

IF COL_LENGTH('PromotionalProducts', 'IsFlashSale') IS NULL
    ALTER TABLE [PromotionalProducts] ADD [IsFlashSale] bit NOT NULL CONSTRAINT [DF_PromotionalProducts_IsFlashSale2] DEFAULT 0;
IF COL_LENGTH('PromotionalProducts', 'FlashSaleType') IS NULL
    ALTER TABLE [PromotionalProducts] ADD [FlashSaleType] nvarchar(24) NOT NULL CONSTRAINT [DF_PromotionalProducts_FlashSaleType2] DEFAULT N'None';
IF COL_LENGTH('PromotionalProducts', 'DailySlotKey') IS NULL
    ALTER TABLE [PromotionalProducts] ADD [DailySlotKey] nvarchar(24) NULL;
IF COL_LENGTH('PromotionalProducts', 'DailyStartMinute') IS NULL
    ALTER TABLE [PromotionalProducts] ADD [DailyStartMinute] int NULL;
IF COL_LENGTH('PromotionalProducts', 'DailyEndMinute') IS NULL
    ALTER TABLE [PromotionalProducts] ADD [DailyEndMinute] int NULL;
IF COL_LENGTH('PromotionalProducts', 'EventStartUtc') IS NULL
    ALTER TABLE [PromotionalProducts] ADD [EventStartUtc] datetime2 NULL;
IF COL_LENGTH('PromotionalProducts', 'EventEndUtc') IS NULL
    ALTER TABLE [PromotionalProducts] ADD [EventEndUtc] datetime2 NULL;
");

            context.Database.ExecuteSqlRaw(@"
UPDATE [PromotionalProducts]
SET [DailyStartMinute] =
    CASE [DailySlotKey]
        WHEN N'MORNING' THEN 540
        WHEN N'NOON' THEN 840
        WHEN N'EVENING' THEN 1200
        ELSE [DailyStartMinute]
    END,
    [DailyEndMinute] =
    CASE [DailySlotKey]
        WHEN N'MORNING' THEN 720
        WHEN N'NOON' THEN 1020
        WHEN N'EVENING' THEN 1380
        ELSE [DailyEndMinute]
    END
WHERE [FlashSaleType] = N'DailySlot'
AND ([DailyStartMinute] IS NULL OR [DailyEndMinute] IS NULL);
");
        }

        public static void EnsureNotificationsTable(ApplicationDbContext context)
        {
            context.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[Notifications]', N'U') IS NULL
BEGIN
    CREATE TABLE [Notifications](
        [Id] int IDENTITY(1,1) NOT NULL,
        [UserId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(2000) NOT NULL,
        [Type] nvarchar(64) NOT NULL,
        [LinkUrl] nvarchar(512) NULL,
        [RelatedOrderId] int NULL,
        [IsRead] bit NOT NULL CONSTRAINT [DF_Notifications_IsRead] DEFAULT 0,
        [CreatedAtUtc] datetime2 NOT NULL CONSTRAINT [DF_Notifications_CreatedAtUtc] DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_Notifications_UserId_CreatedAtUtc] ON [Notifications]([UserId], [CreatedAtUtc] DESC);
    CREATE INDEX [IX_Notifications_UserId_IsRead] ON [Notifications]([UserId], [IsRead]);
END;
");
        }

        public static void EnsureProductReviewsTable(ApplicationDbContext context)
        {
            context.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[ProductReviews]', N'U') IS NULL
BEGIN
    CREATE TABLE [ProductReviews](
        [Id] int IDENTITY(1,1) NOT NULL,
        [ProductId] int NOT NULL,
        [OrderId] int NOT NULL,
        [UserId] int NOT NULL,
        [ReviewerName] nvarchar(256) NOT NULL,
        [Rating] int NOT NULL,
        [Note] nvarchar(2000) NULL,
        [CreatedAtUtc] datetime2 NOT NULL CONSTRAINT [DF_ProductReviews_CreatedAtUtc] DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [PK_ProductReviews] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [UX_ProductReviews_Order_Product] ON [ProductReviews]([OrderId], [ProductId]);
    CREATE INDEX [IX_ProductReviews_ProductId] ON [ProductReviews]([ProductId], [CreatedAtUtc] DESC);
END;
");

            context.Database.ExecuteSqlRaw(@"
INSERT INTO [ProductReviews] ([ProductId], [OrderId], [UserId], [ReviewerName], [Rating], [Note], [CreatedAtUtc])
SELECT oi.[ProductId], o.[Id], o.[UserId],
    COALESCE(NULLIF(LTRIM(RTRIM(o.[CustomerName])), N''), NULLIF(LTRIM(RTRIM(o.[AccountUsername])), N''), N'Khách'),
    o.[CustomerRating], o.[CustomerReviewNote], COALESCE(o.[CustomerReviewedAtUtc], SYSUTCDATETIME())
FROM [Orders] o
INNER JOIN [OrderItems] oi ON oi.[OrderId] = o.[Id]
WHERE o.[CustomerRating] IS NOT NULL AND o.[CustomerRating] BETWEEN 1 AND 5
AND NOT EXISTS (
    SELECT 1 FROM [ProductReviews] pr
    WHERE pr.[OrderId] = o.[Id] AND pr.[ProductId] = oi.[ProductId]
);
");
        }

        public static void EnsureCostAndProfitColumns(ApplicationDbContext context)
        {
            context.Database.ExecuteSqlRaw(@"
IF COL_LENGTH('Products', 'CostPrice') IS NULL
    ALTER TABLE [Products] ADD [CostPrice] decimal(18,2) NOT NULL CONSTRAINT [DF_Products_CostPrice] DEFAULT 0;

IF COL_LENGTH('OrderItems', 'UnitCost') IS NULL
    ALTER TABLE [OrderItems] ADD [UnitCost] decimal(18,2) NOT NULL CONSTRAINT [DF_OrderItems_UnitCost] DEFAULT 0;
");

            context.Database.ExecuteSqlRaw(@"
UPDATE oi
SET oi.[UnitCost] =
    CASE
        WHEN p.[CostPrice] > 0 THEN p.[CostPrice]
        ELSE ROUND(oi.[UnitPrice] * 0.7, 2)
    END
FROM [OrderItems] oi
INNER JOIN [Products] p ON p.[Id] = oi.[ProductId]
WHERE oi.[UnitCost] <= 0;
");
        }
    }
}
