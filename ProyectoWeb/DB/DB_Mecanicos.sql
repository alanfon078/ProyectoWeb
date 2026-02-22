CREATE DATABASE TallerDB;
GO
USE TallerDB;
GO

CREATE TABLE Clientes (
    IdCliente INT IDENTITY(1,1) PRIMARY KEY,
    RFC VARCHAR(15) NOT NULL UNIQUE,
    Nombre VARCHAR(100) NOT NULL
);

CREATE TABLE Vehiculos (
    IdVehiculo INT IDENTITY(1,1) PRIMARY KEY,
    Clave VARCHAR(20) NOT NULL UNIQUE,
    Descripcion VARCHAR(255) NOT NULL,
    IdCliente INT NOT NULL,
    FOREIGN KEY (IdCliente) REFERENCES Clientes(IdCliente)
);

CREATE TABLE Servicios (
    IdServicio INT PRIMARY KEY, 
    Descripcion VARCHAR(255) NOT NULL,
    Precio DECIMAL(10,2) NOT NULL
);

CREATE TABLE OrdenesServicio (
    Folio INT IDENTITY(1,1) PRIMARY KEY, 
    Fecha DATETIME DEFAULT GETDATE(), 
    IdCliente INT NOT NULL,
    IdVehiculo INT NOT NULL,
    Subtotal DECIMAL(10,2) NOT NULL,
    IVA DECIMAL(10,2) NOT NULL,
    Total DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (IdCliente) REFERENCES Clientes(IdCliente),
    FOREIGN KEY (IdVehiculo) REFERENCES Vehiculos(IdVehiculo)
);

CREATE TABLE DetallesOrden (
    IdDetalle INT IDENTITY(1,1) PRIMARY KEY,
    Folio INT NOT NULL,
    IdServicio INT NOT NULL,
    Cantidad INT NOT NULL,
    Precio DECIMAL(10,2) NOT NULL,
    Importe AS (Cantidad * Precio) PERSISTED, 
    FOREIGN KEY (Folio) REFERENCES OrdenesServicio(Folio),
    FOREIGN KEY (IdServicio) REFERENCES Servicios(IdServicio)
);

-- 1. Crear el inicio de sesión (Login)
USE master
GO
CREATE LOGIN AlanAdmin WITH PASSWORD = 'Contrasenia1.', 
DEFAULT_DATABASE = master, 
CHECK_EXPIRATION = OFF, 
CHECK_POLICY = ON; 
GO

-- 2. Crear el usuario en la base de datos específica
USE TallerDB
GO
CREATE USER AlanAdmin FOR LOGIN AlanAdmin;
GO

-- 3. Asignar roles necesarios
ALTER SERVER ROLE [sysadmin] ADD MEMBER [AlanAdmin];
GO



-- Poblando Clientes
INSERT INTO Clientes (RFC, Nombre) VALUES 
('GOMP850101H12', 'Pedro Gómez Martínez'),
('SARM920515K34', 'María Sánchez Rodríguez'),
('LOPA701020TY8', 'Alejandro López Pérez'),
('FERJ880304LL1', 'Juan Fernández García'),
('GORM950812GH5', 'Marta González Ruiz');

-- Poblando Vehículos (Asociados a los IdCliente 1 al 5)
INSERT INTO Vehiculos (Clave, Descripcion, IdCliente) VALUES 
('TSU-2015-01', 'Nissan Tsuru 2015 Blanco', 1),
('HTZ-2022-05', 'Honda CR-V 2022 Gris', 2),
('F150-2018-02', 'Ford F-150 2018 Roja', 3),
('VW-JET-2020', 'VW Jetta 2020 Azul', 4),
('TOY-COR-2021', 'Toyota Corolla 2021 Plata', 5);

INSERT INTO Servicios (IdServicio, Descripcion, Precio) VALUES 
(101, 'Cambio de Aceite y Filtro', 850.00),
(102, 'Afinación Mayor', 2500.00),
(103, 'Revisión de Frenos', 450.00),
(104, 'Alineación y Balanceo', 600.00),
(105, 'Lavado de Motor', 350.00);

-- Insertando 5 Órdenes de Servicio
INSERT INTO OrdenesServicio (IdCliente, IdVehiculo, Subtotal, IVA, Total) VALUES 
(1, 1, 850.00, 136.00, 986.00),
(2, 2, 2500.00, 400.00, 2900.00),
(3, 3, 1300.00, 208.00, 1508.00),
(4, 4, 600.00, 96.00, 696.00),
(5, 5, 1200.00, 192.00, 1392.00);

-- Insertando detalles para cada Folio generado
INSERT INTO DetallesOrden (Folio, IdServicio, Cantidad, Precio) VALUES 
(1, 101, 1, 850.00),   -- Para la orden 1: Cambio de aceite
(2, 102, 1, 2500.00),  -- Para la orden 2: Afinación
(3, 101, 1, 850.00),   -- Para la orden 3: Aceite...
(3, 103, 1, 450.00),   -- ...y frenos
(4, 104, 1, 600.00),   -- Para la orden 4: Alineación
(5, 101, 1, 850.00),   -- Para la orden 5: Aceite...
(5, 105, 1, 350.00);   -- ...y lavado
