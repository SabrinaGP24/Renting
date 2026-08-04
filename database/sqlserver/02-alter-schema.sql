-- =============================================================================
-- Renting - evolucion del esquema SQL Server (migracion 02)
--
-- Equivalente a database/oracle/02-alter-schema.sql: modifica la estructura de
-- una base de datos ya creada sin romper el codigo existente.
-- =============================================================================

USE RENTING;
GO

-- Auditoria: cuando se dio de alta el vehiculo en la flota.
IF COL_LENGTH('dbo.VEHICLES', 'REGISTERED_AT') IS NULL
    ALTER TABLE dbo.VEHICLES
        ADD REGISTERED_AT DATETIME2(3) NOT NULL CONSTRAINT DF_VEHICLES_REGISTERED_AT DEFAULT SYSUTCDATETIME();
GO

-- Se amplia el tamano de la matricula para admitir formatos extranjeros.
-- SQL Server no permite alterar una columna mientras un indice depende de ella,
-- asi que hay que retirar la restriccion UNIQUE y volver a crearla despues.
IF COL_LENGTH('dbo.VEHICLES', 'PLATE_NUMBER') < 40   -- NVARCHAR: 2 bytes por caracter
BEGIN
    IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UK_VEHICLES_PLATE')
        ALTER TABLE dbo.VEHICLES DROP CONSTRAINT UK_VEHICLES_PLATE;

    ALTER TABLE dbo.VEHICLES ALTER COLUMN PLATE_NUMBER NVARCHAR(20) NOT NULL;

    ALTER TABLE dbo.VEHICLES ADD CONSTRAINT UK_VEHICLES_PLATE UNIQUE (PLATE_NUMBER);
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
     @value = N'0 = Disponible, 1 = Alquilado',
     @level0type = N'SCHEMA', @level0name = N'dbo',
     @level1type = N'TABLE',  @level1name = N'VEHICLES',
     @level2type = N'COLUMN', @level2name = N'STATUS';
GO
