# Renting de vehículos — Arquitectura Hexagonal + DDD (.NET Framework 4.8 / WPF)

Aplicación de escritorio para gestionar la flota de una empresa de renting: alta de vehículos,
listado, alquiler y devolución.

## Reglas de negocio

| Regla | Dónde vive | Por qué ahí |
|---|---|---|
| La flota no admite vehículos de más de 5 años | `VehicleAggregate.Create(...)` → `VehicleTooOldException` | Es una invariante de un único agregado |
| Una persona no puede tener más de 1 vehículo alquilado a la vez | `RentVehicleHandler` → `CustomerAlreadyHasActiveRentalException` | Abarca el conjunto de alquileres del cliente: necesita consultar el repositorio, no cabe dentro del agregado |

Ambas están además reforzadas en la base de datos mediante restricciones e índices únicos.

## Arranque rápido

1. Abrir `Hexagonal DDD.slnx` en Visual Studio 2022.
2. Restaurar paquetes NuGet (`Herramientas ▸ Administrador de paquetes NuGet ▸ Restaurar`).
3. `HexagonalDDD.Presentation` como proyecto de inicio y F5.

**No hace falta ninguna base de datos.** Por defecto la aplicación usa repositorios en memoria.

## Arquitectura

```
Presentation (WPF, MVVM)  ──► Application (Command/Query + Handler)  ──► Domain (agregados + puertos)
                                                                              ▲
                                          Infrastructure (adaptadores) ───────┘
```

El **dominio no depende de nada**. Define los puertos (`IVehicleRepository`, `IRentalRepository`,
`IClock`) y la infraestructura los implementa. El único punto que decide qué implementación concreta
se enchufa es `AddInfrastructure(...)`, invocado desde el composition root (`Bootstrapper.BuildHost()`).

| Proyecto | Contenido |
|---|---|
| `HexagonalDDD.Domain` | `VehicleAggregate`, `RentalAggregate`, excepciones de negocio, puertos |
| `HexagonalDDD.Applicaion` | Casos de uso (Command/Query + Handler) y DTOs |
| `HexagonalDDD.Infraestructure` | Adaptadores InMemory / Oracle / SQL Server, `SystemClock`, DI |
| `HexagonalDDD.Presentation` | WPF: `App.xaml`, `MainWindow`, `MainViewModel` |
| `HexagonalDDD.Unit.Test` | MSTest — caso de uso aislado con dobles escritos a mano |
| `HexagonalDDD.Infraestructure.Test` | MSTest — contenedor de DI real + validación de entrada |
| `HexagonalDDD.Functional.Test` | NUnit — ciclo de negocio completo sin levantar el host |

> Nota: el nombre `HexagonalDDD.Applicaion` (sin la `t`) viene de la plantilla original y se ha
> mantenido a propósito para no alterarla.

## Persistencia: enfoque database-first

**El esquema de base de datos es la fuente de verdad.** No hay ORM, ni migraciones generadas desde
código, ni model-first: las tablas se crean ejecutando los scripts de `database/`, y los repositorios
son ADO.NET puro (`OracleCommand` / `SqlCommand` con SQL escrito a mano) que mapean contra esas tablas
ya existentes. **La aplicación nunca crea ni modifica la estructura de la base de datos en tiempo de
ejecución**: si el esquema no está creado, falla al conectar.

El orden de trabajo es siempre: *primero la base de datos, después el código que la consume.*

### Mapeo tabla ▸ agregado

| Tabla | Columna | Tipo Oracle | Tipo SQL Server | Propiedad del agregado |
|---|---|---|---|---|
| `VEHICLES` | `ID` | `RAW(16)` | `UNIQUEIDENTIFIER` | `VehicleAggregate.Id` |
| | `PLATE_NUMBER` | `VARCHAR2(20)` | `NVARCHAR(20)` | `PlateNumber` |
| | `BRAND` / `MODEL` | `VARCHAR2(50)` | `NVARCHAR(50)` | `Brand` / `Model` |
| | `MANUFACTURE_DATE` | `DATE` | `DATE` | `ManufactureDate` |
| | `STATUS` | `NUMBER(1)` | `TINYINT` | `VehicleStatus` (0 = Available, 1 = Rented) |
| `RENTALS` | `ID`, `VEHICLE_ID` | `RAW(16)` | `UNIQUEIDENTIFIER` | `Id`, `VehicleId` |
| | `CUSTOMER_DOCUMENT_ID` | `VARCHAR2(20)` | `NVARCHAR(20)` | `CustomerDocumentId` |
| | `START_DATE` / `END_DATE` | `DATE` | `DATETIME2(3)` | `StartDate` / `EndDate` (nullable) |
| | `STATUS` | `NUMBER(1)` | `TINYINT` | `RentalStatus` (0 = Active, 1 = Returned) |

Los `Guid` se guardan como `RAW(16)` en Oracle (`Guid.ToByteArray()` ↔ `new Guid(byte[])`).

### Modificar la estructura

`database/oracle/02-alter-schema.sql` y `database/sqlserver/02-alter-schema.sql` son migraciones de
ejemplo sobre una base de datos ya creada: añaden una columna de auditoría con `DEFAULT`, amplían el
tamaño de `PLATE_NUMBER` y documentan las tablas. Al llevar `DEFAULT`, el `INSERT` existente de los
repositorios sigue siendo válido sin tocar una línea de C#.

## Elegir el proveedor de persistencia

En `Hexagonal DDD/App.config`:

```xml
<add key="PersistenceProvider" value="InMemory" />   <!-- InMemory | Oracle | SqlServer -->
```

| Valor | Requisitos |
|---|---|
| `InMemory` (por defecto) | Ninguno |
| `Oracle` | Cadena `OracleDb` + esquema creado |
| `SqlServer` | Cadena `SqlServerDb` + esquema creado |

### Levantar Oracle

```bash
docker compose --profile oracle up -d
```

Crea el usuario `RENTING` y ejecuta solo los scripts de `database/oracle` la primera vez.
Conexión: `localhost:1521/XEPDB1`.

### Levantar SQL Server

```bash
docker compose --profile sqlserver up -d
```

El servicio `sqlserver-init` espera a que la instancia acepte conexiones y aplica los scripts.
Termina en estado `exited (0)`: es lo esperado.

Alternativamente, sin Docker: abrir `database/sqlserver/01-create-schema.sql` y luego
`02-alter-schema.sql` en SQL Server Management Studio contra cualquier instancia local y ejecutarlos.
Después ajustar la cadena `SqlServerDb` del `App.config`.

## Tests

Los tres proyectos se ejecutan desde el **Explorador de pruebas** de Visual Studio y **ninguno
necesita base de datos**.

| Tipo | Proyecto | Qué cubre |
|---|---|---|
| Unitario | `HexagonalDDD.Unit.Test` | `RentVehicleHandler` contra dobles escritos a mano y reloj fijo. Sin BD, sin contenedor de DI |
| Infraestructura | `HexagonalDDD.Infraestructure.Test` | Monta el contenedor real (`Bootstrapper.BuildHost()`) sin abrir la ventana, y valida la recepción de datos de `CreateVehicleHandler` |
| Funcional | `HexagonalDDD.Functional.Test` | Ciclo completo alta ▸ alquiler ▸ intento rechazado ▸ devolución ▸ nuevo alquiler, atravesando todas las capas sin levantar el host |

Se usa un `IClock` inyectado en lugar de `DateTime.Now` para que la regla de los 5 años sea
comprobable con una fecha fija y los tests no caduquen con el tiempo.
