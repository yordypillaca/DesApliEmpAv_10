USE master;
GO

IF DB_ID(N'HotelDB') IS NOT NULL
BEGIN
    ALTER DATABASE HotelDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE HotelDB;
END
GO

CREATE DATABASE HotelDB;
GO

USE HotelDB;
GO

/* ----------------------------- TABLAS ----------------------------------- */
CREATE TABLE TipoHabitacion (
    IdTipoHabitacion INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_TipoHabitacion PRIMARY KEY,
    Nombre           NVARCHAR(50)  NOT NULL
        CONSTRAINT UQ_TipoHabitacion_Nombre UNIQUE,
    Capacidad        INT           NOT NULL
        CONSTRAINT CK_TipoHabitacion_Capacidad CHECK (Capacidad > 0),
    PrecioNoche      DECIMAL(8,2)  NOT NULL
        CONSTRAINT CK_TipoHabitacion_Precio CHECK (PrecioNoche > 0),
    Activo           BIT           NOT NULL
        CONSTRAINT DF_TipoHabitacion_Activo DEFAULT 1
);
GO

CREATE TABLE Habitacion (
    IdHabitacion     INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Habitacion PRIMARY KEY,
    Numero           NVARCHAR(10)  NOT NULL
        CONSTRAINT UQ_Habitacion_Numero UNIQUE,
    Piso             INT           NOT NULL
        CONSTRAINT CK_Habitacion_Piso CHECK (Piso >= 1),
    IdTipoHabitacion INT           NOT NULL
        CONSTRAINT FK_Habitacion_TipoHabitacion
        REFERENCES TipoHabitacion (IdTipoHabitacion),
    Activo           BIT           NOT NULL
        CONSTRAINT DF_Habitacion_Activo DEFAULT 1
);
GO

CREATE TABLE Huesped (
    IdHuesped        INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Huesped PRIMARY KEY,
    Dni              CHAR(8)       NOT NULL
        CONSTRAINT UQ_Huesped_Dni UNIQUE,
    Nombres          NVARCHAR(100) NOT NULL,
    Apellidos        NVARCHAR(100) NOT NULL,
    Correo           NVARCHAR(120) NULL,
    Telefono         NVARCHAR(15)  NULL,
    Activo           BIT           NOT NULL
        CONSTRAINT DF_Huesped_Activo DEFAULT 1
);
GO

CREATE TABLE Reserva (
    IdReserva        INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Reserva PRIMARY KEY,
    IdHuesped        INT           NOT NULL
        CONSTRAINT FK_Reserva_Huesped REFERENCES Huesped (IdHuesped),
    IdHabitacion     INT           NOT NULL
        CONSTRAINT FK_Reserva_Habitacion REFERENCES Habitacion (IdHabitacion),
    FechaIngreso     DATE          NOT NULL,
    FechaSalida      DATE          NOT NULL,
    Estado           NVARCHAR(15)  NOT NULL
        CONSTRAINT DF_Reserva_Estado DEFAULT N'Pendiente'
        CONSTRAINT CK_Reserva_Estado
            CHECK (Estado IN (N'Pendiente', N'Confirmada', N'Cancelada', N'Finalizada')),
    FechaRegistro    DATETIME2(0)  NOT NULL
        CONSTRAINT DF_Reserva_FechaRegistro DEFAULT SYSDATETIME(),
    CONSTRAINT CK_Reserva_Fechas CHECK (FechaSalida > FechaIngreso)
);
GO

CREATE INDEX IX_Reserva_Habitacion_Fechas
    ON Reserva (IdHabitacion, FechaIngreso, FechaSalida);
GO

/* ------------------------- DATOS DE PRUEBA ------------------------------ */
INSERT INTO TipoHabitacion (Nombre, Capacidad, PrecioNoche) VALUES
    (N'Simple',      1, 120.00),
    (N'Doble',       2, 180.00),
    (N'Matrimonial', 2, 200.00),
    (N'Suite',       4, 350.00);
GO

INSERT INTO Habitacion (Numero, Piso, IdTipoHabitacion, Activo) VALUES
    (N'101', 1, 1, 1),
    (N'102', 1, 2, 1),
    (N'103', 1, 3, 1),
    (N'201', 2, 1, 1),
    (N'202', 2, 2, 1),
    (N'203', 2, 3, 1),
    (N'204', 2, 2, 0),   -- fuera de servicio (Activo = 0)
    (N'301', 3, 4, 1),
    (N'302', 3, 4, 1);
GO

INSERT INTO Huesped (Dni, Nombres, Apellidos, Correo, Telefono, Activo) VALUES
    ('70123456', N'Carlos', N'Ramírez Soto',   N'carlos.ramirez@correo.com', N'987654321', 1),
    ('45879632', N'María',  N'Torres Vega',    N'maria.torres@correo.com',   N'986123456', 1),
    ('71234890', N'Luis',   N'Quispe Mamani',  NULL,                         N'999888777', 1),
    ('08765432', N'Ana',    N'Flores Chávez',  N'ana.flores@correo.com',     NULL,         1),
    ('72345619', N'Jorge',  N'Salazar Ríos',   N'jorge.salazar@correo.com',  N'955444333', 1),
    ('43219876', N'Rosa',   N'Huamán Castro',  N'rosa.huaman@correo.com',    N'944333222', 1),
    ('40000001', N'Pedro',  N'Villanueva Paz', NULL,                         NULL,         0);  -- huésped dado de baja
GO

-- Las fechas son relativas a la fecha de ejecución del script, así siempre hay
-- reservas próximas con las que probar la disponibilidad.
DECLARE @hoy DATE = CAST(GETDATE() AS DATE);

INSERT INTO Reserva (IdHuesped, IdHabitacion, FechaIngreso, FechaSalida, Estado) VALUES
    (1, 1, DATEADD(DAY,   1, @hoy), DATEADD(DAY,   4, @hoy), N'Confirmada'),  -- hab. 101
    (2, 2, DATEADD(DAY,   2, @hoy), DATEADD(DAY,   5, @hoy), N'Pendiente'),   -- hab. 102
    (3, 3, DATEADD(DAY,   3, @hoy), DATEADD(DAY,   6, @hoy), N'Confirmada'),  -- hab. 103
    (4, 8, DATEADD(DAY,   5, @hoy), DATEADD(DAY,   9, @hoy), N'Confirmada'),  -- hab. 301
    (5, 1, DATEADD(DAY,   4, @hoy), DATEADD(DAY,   6, @hoy), N'Cancelada'),   -- hab. 101 (cancelada: no bloquea)
    (6, 4, DATEADD(DAY,  -5, @hoy), DATEADD(DAY,  -2, @hoy), N'Finalizada'),  -- hab. 201
    (1, 5, DATEADD(DAY,  10, @hoy), DATEADD(DAY,  12, @hoy), N'Pendiente'),   -- hab. 202
    (2, 8, DATEADD(DAY,  12, @hoy), DATEADD(DAY,  15, @hoy), N'Confirmada'),  -- hab. 301
    (3, 2, DATEADD(DAY, -10, @hoy), DATEADD(DAY,  -8, @hoy), N'Finalizada'),  -- hab. 102
    (6, 1, DATEADD(DAY,   8, @hoy), DATEADD(DAY,  10, @hoy), N'Confirmada');  -- hab. 101
GO

/* -------------------------- VERIFICACIÓN -------------------------------- */
SELECT 'TipoHabitacion' AS Tabla, COUNT(*) AS Filas FROM TipoHabitacion
UNION ALL SELECT 'Habitacion', COUNT(*) FROM Habitacion
UNION ALL SELECT 'Huesped',    COUNT(*) FROM Huesped
UNION ALL SELECT 'Reserva',    COUNT(*) FROM Reserva;
GO
