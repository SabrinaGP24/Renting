-- =============================================================================
-- Renting - clientes (migracion 03). Equivalente SQL Server de
-- database/oracle/03-add-customers.sql.
--
-- "Que coche tiene alquilado cada cliente" NO se guarda como columna: se calcula
-- en la vista V_ACTIVE_RENTALS a partir de RENTALS, para no tener el mismo dato
-- en dos sitios.
-- =============================================================================

USE RENTING;
GO

IF OBJECT_ID('dbo.CUSTOMERS', 'U') IS NULL
CREATE TABLE dbo.CUSTOMERS (
    DOCUMENT_ID    NVARCHAR(20)  NOT NULL,
    FIRST_NAME     NVARCHAR(50)  NOT NULL,
    LAST_NAME      NVARCHAR(80)  NOT NULL,
    PHONE          NVARCHAR(20)  NULL,
    REGISTERED_AT  DATETIME2(3)  NOT NULL,
    CONSTRAINT PK_CUSTOMERS PRIMARY KEY (DOCUMENT_ID)
);
GO

-- Integridad referencial: ya no se puede alquilar a un cliente inexistente.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RENTALS_CUSTOMER')
    ALTER TABLE dbo.RENTALS
        ADD CONSTRAINT FK_RENTALS_CUSTOMER
        FOREIGN KEY (CUSTOMER_DOCUMENT_ID) REFERENCES dbo.CUSTOMERS (DOCUMENT_ID);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RENTALS_CUSTOMER')
    CREATE INDEX IX_RENTALS_CUSTOMER ON dbo.RENTALS (CUSTOMER_DOCUMENT_ID);
GO

CREATE OR ALTER VIEW dbo.V_ACTIVE_RENTALS AS
SELECT c.DOCUMENT_ID,
       c.FIRST_NAME,
       c.LAST_NAME,
       c.PHONE,
       v.PLATE_NUMBER,
       v.BRAND,
       v.MODEL,
       r.START_DATE
  FROM dbo.RENTALS   r
  JOIN dbo.CUSTOMERS c ON c.DOCUMENT_ID = r.CUSTOMER_DOCUMENT_ID
  JOIN dbo.VEHICLES  v ON v.ID          = r.VEHICLE_ID
 WHERE r.STATUS = 0;
GO
