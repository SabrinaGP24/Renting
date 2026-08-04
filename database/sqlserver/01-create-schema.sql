-- =============================================================================
-- Renting - esquema SQL Server (alternativa a Oracle)
--
-- Ejecutable tal cual desde SQL Server Management Studio contra cualquier
-- instancia local, o automaticamente por el servicio sqlserver-init de
-- docker-compose.yml.
-- =============================================================================

IF DB_ID('RENTING') IS NULL
    CREATE DATABASE RENTING;
GO

USE RENTING;
GO

-- -----------------------------------------------------------------------------
-- VEHICLES: agregado VehicleAggregate
--   STATUS -> 0 = Available, 1 = Rented (enum VehicleStatus)
-- -----------------------------------------------------------------------------
IF OBJECT_ID('dbo.VEHICLES', 'U') IS NULL
CREATE TABLE dbo.VEHICLES (
    ID                UNIQUEIDENTIFIER  NOT NULL,
    PLATE_NUMBER      NVARCHAR(15)      NOT NULL,
    BRAND             NVARCHAR(50)      NOT NULL,
    MODEL             NVARCHAR(50)      NOT NULL,
    MANUFACTURE_DATE  DATE              NOT NULL,
    STATUS            TINYINT           NOT NULL,
    CONSTRAINT PK_VEHICLES        PRIMARY KEY (ID),
    CONSTRAINT UK_VEHICLES_PLATE  UNIQUE (PLATE_NUMBER),
    CONSTRAINT CK_VEHICLES_STATUS CHECK (STATUS IN (0, 1))
);
GO

-- -----------------------------------------------------------------------------
-- RENTALS: agregado RentalAggregate
--   STATUS -> 0 = Active, 1 = Returned (enum RentalStatus)
-- -----------------------------------------------------------------------------
IF OBJECT_ID('dbo.RENTALS', 'U') IS NULL
CREATE TABLE dbo.RENTALS (
    ID                    UNIQUEIDENTIFIER  NOT NULL,
    VEHICLE_ID            UNIQUEIDENTIFIER  NOT NULL,
    CUSTOMER_DOCUMENT_ID  NVARCHAR(20)      NOT NULL,
    START_DATE            DATETIME2(3)      NOT NULL,
    END_DATE              DATETIME2(3)      NULL,
    STATUS                TINYINT           NOT NULL,
    CONSTRAINT PK_RENTALS         PRIMARY KEY (ID),
    CONSTRAINT FK_RENTALS_VEHICLE FOREIGN KEY (VEHICLE_ID) REFERENCES dbo.VEHICLES (ID),
    CONSTRAINT CK_RENTALS_STATUS  CHECK (STATUS IN (0, 1)),
    CONSTRAINT CK_RENTALS_DATES   CHECK (END_DATE IS NULL OR END_DATE >= START_DATE)
);
GO

-- -----------------------------------------------------------------------------
-- Las dos reglas de negocio garantizadas tambien por la base de datos, mediante
-- indices unicos filtrados sobre las filas de alquileres activos (STATUS = 0).
-- -----------------------------------------------------------------------------

-- Una misma persona no puede tener mas de un alquiler activo a la vez.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_RENTALS_ACTIVE_CUSTOMER')
    CREATE UNIQUE INDEX UX_RENTALS_ACTIVE_CUSTOMER
        ON dbo.RENTALS (CUSTOMER_DOCUMENT_ID) WHERE STATUS = 0;
GO

-- Un mismo vehiculo no puede estar alquilado dos veces a la vez.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_RENTALS_ACTIVE_VEHICLE')
    CREATE UNIQUE INDEX UX_RENTALS_ACTIVE_VEHICLE
        ON dbo.RENTALS (VEHICLE_ID) WHERE STATUS = 0;
GO

-- Apoya la consulta de vehiculos disponibles.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_VEHICLES_STATUS')
    CREATE INDEX IX_VEHICLES_STATUS ON dbo.VEHICLES (STATUS);
GO
