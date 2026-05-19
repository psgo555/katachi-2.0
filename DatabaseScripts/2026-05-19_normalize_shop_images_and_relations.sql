/*
    Normalize shop image columns and order item option relations.
    Target database: katachidb.

    Final rule:
    - ProductOptionValues.ImageUrl is the only shop image column.
    - Products.ImageUrl is removed after its value is copied into ProductOptionValues.
    - OrderItems.ImageUrl is removed.
    - OrderItems keeps ProductId for product relation.
    - OrderItemOptionValues records selected option values for each order item.

    Run this once after backing up the database.
*/

USE katachidb;
GO

IF @@TRANCOUNT > 0
BEGIN
    ROLLBACK TRANSACTION;
END;
GO

SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

IF COL_LENGTH('dbo.ProductOptionValues', 'ImageUrl') IS NULL
BEGIN
    ALTER TABLE dbo.ProductOptionValues ADD ImageUrl nvarchar(500) NULL;
END;

IF COL_LENGTH('dbo.Products', 'ImageUrl') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
        UPDATE pov
        SET ImageUrl = p.ImageUrl
        FROM dbo.ProductOptionValues pov
        INNER JOIN dbo.ProductOptions po ON po.Id = pov.ProductOptionId
        INNER JOIN dbo.Products p ON p.Id = po.ProductId
        WHERE (pov.ImageUrl IS NULL OR pov.ImageUrl = '''')
          AND p.ImageUrl IS NOT NULL
          AND p.ImageUrl <> '''';
    ';
END;

IF COL_LENGTH('dbo.OrderItems', 'ProductId') IS NULL
BEGIN
    ALTER TABLE dbo.OrderItems ADD ProductId int NULL;
END;

EXEC sp_executesql N'
    UPDATE oi
    SET ProductId = p.Id
    FROM dbo.OrderItems oi
    INNER JOIN dbo.Products p ON p.ProductCode = oi.ProductCode
    WHERE oi.ProductId IS NULL;
';

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_OrderItems_Products'
      AND parent_object_id = OBJECT_ID('dbo.OrderItems')
)
BEGIN
    ALTER TABLE dbo.OrderItems
    ADD CONSTRAINT FK_OrderItems_Products
    FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id);
END;

IF OBJECT_ID('dbo.OrderItemOptionValues') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItemOptionValues
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderItemOptionValues PRIMARY KEY,
        OrderItemId int NOT NULL,
        ProductOptionValueId int NOT NULL,
        CONSTRAINT FK_OrderItemOptionValues_OrderItems
            FOREIGN KEY (OrderItemId) REFERENCES dbo.OrderItems(Id) ON DELETE CASCADE,
        CONSTRAINT FK_OrderItemOptionValues_ProductOptionValues
            FOREIGN KEY (ProductOptionValueId) REFERENCES dbo.ProductOptionValues(Id)
    );
END;

EXEC sp_executesql N'
    INSERT INTO dbo.OrderItemOptionValues (OrderItemId, ProductOptionValueId)
    SELECT oi.Id, pov.Id
    FROM dbo.OrderItems oi
    INNER JOIN dbo.Products p ON p.ProductCode = oi.ProductCode
    INNER JOIN dbo.ProductOptions po ON po.ProductId = p.Id
    INNER JOIN dbo.ProductOptionValues pov ON pov.ProductOptionId = po.Id
    WHERE oi.OptionText LIKE ''%'' + po.Name + N''：'' + pov.Text + ''%''
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.OrderItemOptionValues existing
          WHERE existing.OrderItemId = oi.Id
            AND existing.ProductOptionValueId = pov.Id
      );
';

EXEC sp_executesql N'
    INSERT INTO dbo.OrderItemOptionValues (OrderItemId, ProductOptionValueId)
    SELECT oi.Id, MIN(pov.Id)
    FROM dbo.OrderItems oi
    INNER JOIN dbo.Products p ON p.ProductCode = oi.ProductCode
    INNER JOIN dbo.ProductOptions po ON po.ProductId = p.Id
    INNER JOIN dbo.ProductOptionValues pov ON pov.ProductOptionId = po.Id
    WHERE (pov.ImageUrl IS NOT NULL AND pov.ImageUrl <> '''')
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.OrderItemOptionValues existing
          WHERE existing.OrderItemId = oi.Id
      )
    GROUP BY oi.Id;
';

IF COL_LENGTH('dbo.OrderItems', 'ImageUrl') IS NOT NULL
BEGIN
    ALTER TABLE dbo.OrderItems DROP COLUMN ImageUrl;
END;

IF COL_LENGTH('dbo.Products', 'ImageUrl') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Products DROP COLUMN ImageUrl;
END;

IF COL_LENGTH('dbo.Orders', 'user_id') IS NOT NULL
   AND OBJECT_ID('dbo.users') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = 'FK_Orders_Users'
          AND parent_object_id = OBJECT_ID('dbo.Orders')
   )
BEGIN
    ALTER TABLE dbo.Orders
    ADD CONSTRAINT FK_Orders_Users
    FOREIGN KEY (user_id) REFERENCES dbo.users(user_id);
END;

COMMIT TRANSACTION;
GO
