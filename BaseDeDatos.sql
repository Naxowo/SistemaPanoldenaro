-- Ejecutar en SQL Server Management Studio (SSMS).
IF DB_ID('PanolEscolar') IS NULL
    CREATE DATABASE PanolEscolar;
GO
USE PanolEscolar;
GO

IF OBJECT_ID('dbo.Alumnos', 'U') IS NULL
BEGIN
    CREATE TABLE Alumnos (
        AlumnoID INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Curso NVARCHAR(30) NOT NULL
    );
END;
GO

IF OBJECT_ID('dbo.Herramientas', 'U') IS NULL
BEGIN
    CREATE TABLE Herramientas (
        HerramientaID INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Stock INT NOT NULL CHECK (Stock >= 0),
        Estado NVARCHAR(30) NOT NULL DEFAULT 'Disponible'
            CHECK (Estado IN ('Disponible', 'En reparación', 'Dañada'))
    );
END;
GO

IF OBJECT_ID('dbo.Prestamos', 'U') IS NULL
BEGIN
    CREATE TABLE Prestamos (
        PrestamoID INT IDENTITY(1,1) PRIMARY KEY,
        AlumnoID INT NOT NULL FOREIGN KEY REFERENCES Alumnos(AlumnoID),
        HerramientaID INT NOT NULL FOREIGN KEY REFERENCES Herramientas(HerramientaID),
        Cantidad INT NOT NULL CHECK (Cantidad > 0),
        FechaPrestamo DATETIME NOT NULL,
        FechaDevolucion DATETIME NULL,
        Estado NVARCHAR(20) NOT NULL CHECK (Estado IN ('Pendiente', 'Devuelto'))
    );
END;
GO

IF OBJECT_ID('dbo.Danios', 'U') IS NULL
BEGIN
    CREATE TABLE Danios (
        DanioID INT IDENTITY(1,1) PRIMARY KEY,
        PrestamoID INT NOT NULL FOREIGN KEY REFERENCES Prestamos(PrestamoID),
        Detalle NVARCHAR(300) NOT NULL,
        Fecha DATETIME NOT NULL
    );
END;
GO

-- Datos de ejemplo (solo si las tablas están vacías).
IF NOT EXISTS (SELECT 1 FROM Alumnos)
    INSERT INTO Alumnos(Nombre,Curso) VALUES
    ('Juan Pérez','7°1°'), ('María Gómez','7°2°');
IF NOT EXISTS (SELECT 1 FROM Herramientas)
    INSERT INTO Herramientas(Nombre,Stock,Estado) VALUES
    ('Destornillador',10,'Disponible'),
    ('Pinza',8,'Disponible'),
    ('Taladro',2,'Disponible');
GO
