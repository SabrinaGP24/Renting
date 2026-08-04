-- =============================================================================
-- Renting - esquema Oracle (enfoque database-first)
--
-- Formas de ejecutarlo:
--
--   a) Docker: la imagen gvenzl/oracle-xe lo ejecuta sola la primera vez que
--      arranca el contenedor, desde /container-entrypoint-initdb.d. Esa
--      ejecucion se hace como SYS y conectada a la raiz del CDB (XE), de ahi
--      el cambio de contenedor a XEPDB1.
--
--   b) Oracle local ya instalado: desde SQL Developer o sqlplus, cambiando la
--      linea DEFINE de abajo al nombre del esquema destino.
-- =============================================================================

SET VERIFY OFF

-- ---------------------------------------------------------------------------
-- Esquema destino. docker-compose.yml crea el usuario RENTING (APP_USER), por
-- eso es el valor por defecto. Para instalarlo sobre otro esquema es la UNICA
-- linea que hay que tocar.
-- ---------------------------------------------------------------------------
DEFINE app_schema = RENTING

-- Necesario solo cuando el script se lanza como SYS desde la raiz del CDB.
-- Si ya estas conectado a XEPDB1 (por ejemplo desde SQL Developer), esta
-- sentencia falla sin consecuencias y el script continua.
WHENEVER SQLERROR CONTINUE
ALTER SESSION SET CONTAINER = XEPDB1;
WHENEVER SQLERROR EXIT FAILURE

-- -----------------------------------------------------------------------------
-- VEHICLES: agregado VehicleAggregate
--   ID     -> Guid almacenado como RAW(16)
--   STATUS -> 0 = Available, 1 = Rented (enum VehicleStatus)
-- -----------------------------------------------------------------------------
CREATE TABLE &app_schema..VEHICLES (
    ID                RAW(16)       NOT NULL,
    PLATE_NUMBER      VARCHAR2(15)  NOT NULL,
    BRAND             VARCHAR2(50)  NOT NULL,
    MODEL             VARCHAR2(50)  NOT NULL,
    MANUFACTURE_DATE  DATE          NOT NULL,
    STATUS            NUMBER(1)     NOT NULL,
    CONSTRAINT PK_VEHICLES        PRIMARY KEY (ID),
    CONSTRAINT UK_VEHICLES_PLATE  UNIQUE (PLATE_NUMBER),
    CONSTRAINT CK_VEHICLES_STATUS CHECK (STATUS IN (0, 1))
);

-- -----------------------------------------------------------------------------
-- RENTALS: agregado RentalAggregate
--   STATUS -> 0 = Active, 1 = Returned (enum RentalStatus)
-- -----------------------------------------------------------------------------
CREATE TABLE &app_schema..RENTALS (
    ID                    RAW(16)       NOT NULL,
    VEHICLE_ID            RAW(16)       NOT NULL,
    CUSTOMER_DOCUMENT_ID  VARCHAR2(20)  NOT NULL,
    START_DATE            DATE          NOT NULL,
    END_DATE              DATE,
    STATUS                NUMBER(1)     NOT NULL,
    CONSTRAINT PK_RENTALS         PRIMARY KEY (ID),
    CONSTRAINT FK_RENTALS_VEHICLE FOREIGN KEY (VEHICLE_ID) REFERENCES &app_schema..VEHICLES (ID),
    CONSTRAINT CK_RENTALS_STATUS  CHECK (STATUS IN (0, 1)),
    CONSTRAINT CK_RENTALS_DATES   CHECK (END_DATE IS NULL OR END_DATE >= START_DATE)
);

-- -----------------------------------------------------------------------------
-- Las dos reglas de negocio, ademas de en el dominio, garantizadas por la base
-- de datos mediante indices unicos funcionales: la expresion CASE solo indexa
-- las filas con STATUS = 0 (alquiler activo), de forma que los alquileres ya
-- devueltos no compiten por la unicidad.
-- -----------------------------------------------------------------------------

-- Una misma persona no puede tener mas de un alquiler activo a la vez.
CREATE UNIQUE INDEX &app_schema..UX_RENTALS_ACTIVE_CUSTOMER
    ON &app_schema..RENTALS (CASE WHEN STATUS = 0 THEN CUSTOMER_DOCUMENT_ID END);

-- Un mismo vehiculo no puede estar alquilado dos veces a la vez.
CREATE UNIQUE INDEX &app_schema..UX_RENTALS_ACTIVE_VEHICLE
    ON &app_schema..RENTALS (CASE WHEN STATUS = 0 THEN VEHICLE_ID END);

-- Apoya la consulta de vehiculos disponibles.
CREATE INDEX &app_schema..IX_VEHICLES_STATUS ON &app_schema..VEHICLES (STATUS);

EXIT;
