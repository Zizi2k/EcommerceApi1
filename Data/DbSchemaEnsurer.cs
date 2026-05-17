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
    }
}
