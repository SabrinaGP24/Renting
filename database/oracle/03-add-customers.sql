-- =============================================================================
-- Renting - clientes (migracion 03)
--
-- Convierte al cliente en una entidad de verdad. Hasta ahora RENTALS guardaba un
-- documento suelto, sin nombre y sin garantia de que existiera.
--
-- Importante: NO se guarda aqui "que coche tiene alquilado". Ese dato ya vive en
-- RENTALS (la fila con STATUS = 0) y duplicarlo crearia dos fuentes de verdad que
-- se pueden desincronizar. Para consultarlo comodamente esta la vista
-- V_ACTIVE_RENTALS del final, que lo calcula.
-- =============================================================================

SET VERIFY OFF

DEFINE app_schema = RENTING

WHENEVER SQLERROR CONTINUE
ALTER SESSION SET CONTAINER = XEPDB1;
WHENEVER SQLERROR EXIT FAILURE

-- -----------------------------------------------------------------------------
-- CUSTOMERS: agregado CustomerAggregate.
-- La clave primaria es el documento identificativo (clave natural), por eso no
-- hay id artificial y RENTALS puede referenciarlo directamente.
-- -----------------------------------------------------------------------------
CREATE TABLE &app_schema..CUSTOMERS (
    DOCUMENT_ID    VARCHAR2(20)  NOT NULL,
    FIRST_NAME     VARCHAR2(50)  NOT NULL,
    LAST_NAME      VARCHAR2(80)  NOT NULL,
    PHONE          VARCHAR2(20),
    REGISTERED_AT  DATE          NOT NULL,
    CONSTRAINT PK_CUSTOMERS PRIMARY KEY (DOCUMENT_ID)
);

-- Integridad referencial: ya no se puede alquilar a un cliente inexistente.
ALTER TABLE &app_schema..RENTALS
    ADD CONSTRAINT FK_RENTALS_CUSTOMER
    FOREIGN KEY (CUSTOMER_DOCUMENT_ID) REFERENCES &app_schema..CUSTOMERS (DOCUMENT_ID);

-- Evita un bloqueo de tabla completa en CUSTOMERS al borrar filas de RENTALS:
-- Oracle no indexa automaticamente las columnas de clave ajena.
CREATE INDEX &app_schema..IX_RENTALS_CUSTOMER
    ON &app_schema..RENTALS (CUSTOMER_DOCUMENT_ID);

-- -----------------------------------------------------------------------------
-- Quien tiene que coche, calculado. Sustituye a la columna "coche actual" que
-- habria que mantener a mano en CUSTOMERS.
-- -----------------------------------------------------------------------------
CREATE OR REPLACE VIEW &app_schema..V_ACTIVE_RENTALS AS
SELECT c.DOCUMENT_ID,
       c.FIRST_NAME,
       c.LAST_NAME,
       c.PHONE,
       v.PLATE_NUMBER,
       v.BRAND,
       v.MODEL,
       r.START_DATE
  FROM &app_schema..RENTALS   r
  JOIN &app_schema..CUSTOMERS c ON c.DOCUMENT_ID = r.CUSTOMER_DOCUMENT_ID
  JOIN &app_schema..VEHICLES  v ON v.ID          = r.VEHICLE_ID
 WHERE r.STATUS = 0;

COMMENT ON TABLE  &app_schema..CUSTOMERS    IS 'Clientes de la empresa de renting';
COMMENT ON COLUMN &app_schema..CUSTOMERS.DOCUMENT_ID IS 'DNI/NIE normalizado en mayusculas; clave natural';

EXIT;
