-- =============================================================================
-- Renting - evolucion del esquema Oracle (migracion 02)
--
-- Modifica la estructura de una base de datos ya creada sin romper el codigo
-- existente: la columna nueva lleva DEFAULT, de modo que los INSERT actuales de
-- OracleVehicleRepository siguen siendo validos sin tocar una linea de C#.
-- =============================================================================

SET VERIFY OFF

-- Mismo esquema destino que en 01-create-schema.sql.
DEFINE app_schema = RENTING

WHENEVER SQLERROR CONTINUE
ALTER SESSION SET CONTAINER = XEPDB1;
WHENEVER SQLERROR EXIT FAILURE

-- Auditoria: cuando se dio de alta el vehiculo en la flota.
ALTER TABLE &app_schema..VEHICLES
    ADD (REGISTERED_AT TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL);

-- La matricula se guarda siempre normalizada en mayusculas desde el dominio;
-- se amplia el tamano para admitir formatos de matricula extranjeros.
-- Oracle permite ampliar la columna con la restriccion UNIQUE ya creada.
ALTER TABLE &app_schema..VEHICLES
    MODIFY (PLATE_NUMBER VARCHAR2(20));

COMMENT ON TABLE  &app_schema..VEHICLES               IS 'Flota de vehiculos de la empresa de renting';
COMMENT ON COLUMN &app_schema..VEHICLES.STATUS        IS '0 = Disponible, 1 = Alquilado';
COMMENT ON COLUMN &app_schema..VEHICLES.REGISTERED_AT IS 'Fecha de alta del vehiculo en la flota';
COMMENT ON TABLE  &app_schema..RENTALS                IS 'Alquileres, activos e historicos';
COMMENT ON COLUMN &app_schema..RENTALS.STATUS         IS '0 = Activo, 1 = Devuelto';

EXIT;
