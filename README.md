# Solucion_MediCita

ID : admin@medicita.com
Pass : 123456


SQL SCRIPT 

/**************************************************************************
 * SCRIPT DE BASE DE DATOS - PROYECTO MEDICITA WEB
 * Curso: Desarrollo de Servicios Web I
 * Tecnologías: SQL Server, ADO.NET
 **************************************************************************/

USE master;
GO

-- 1. Eliminar la BD si ya existe para crearla limpia
IF EXISTS(SELECT * FROM sys.databases WHERE name = 'BD_MediCita')
BEGIN
    DROP DATABASE BD_MediCita;
END
GO

CREATE DATABASE BD_MediCita;
GO

USE BD_MediCita;
GO

/**************************************************************************
 * CREACIÓN DE TABLAS
 **************************************************************************/

-- Tabla Roles (Seguridad)
CREATE TABLE tb_Roles (
    IdRol INT PRIMARY KEY IDENTITY(1,1),
    NombreRol VARCHAR(50) NOT NULL
);

-- Tabla Usuarios (Login y Perfiles)
CREATE TABLE tb_Usuarios (
    IdUsuario INT PRIMARY KEY IDENTITY(1,1),
    NombreCompleto VARCHAR(100) NOT NULL,
    Correo VARCHAR(100) UNIQUE NOT NULL,
    Clave VARCHAR(100) NOT NULL,
    IdRol INT REFERENCES tb_Roles(IdRol)
);

-- Tabla Especialidades (Maestra para Citas)
CREATE TABLE tb_Especialidades (
    IdEspecialidad INT PRIMARY KEY IDENTITY(1,1),
    NombreEspec VARCHAR(50) NOT NULL,
    Descripcion VARCHAR(200)
);

-- Tabla Medicamentos (Maestra para Farmacia)
CREATE TABLE tb_Medicamentos (
    IdMedicamento INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(100) NOT NULL,
    Laboratorio VARCHAR(50),
    Precio DECIMAL(10,2) NOT NULL,
    Stock INT NOT NULL
);

-- Tabla Medicos (Relación con Usuarios y Especialidades)
CREATE TABLE tb_Medicos (
    IdMedico INT PRIMARY KEY IDENTITY(1,1),
    IdUsuario INT UNIQUE REFERENCES tb_Usuarios(IdUsuario),
    IdEspecialidad INT REFERENCES tb_Especialidades(IdEspecialidad),
    CMP VARCHAR(20) NOT NULL, -- Código Colegio Médico
    Telefono VARCHAR(15)
);

-- Tabla Citas (Transaccional Módulo Citas)
CREATE TABLE tb_Citas (
    IdCita INT PRIMARY KEY IDENTITY(1,1),
    IdPaciente INT REFERENCES tb_Usuarios(IdUsuario),
    IdMedico INT REFERENCES tb_Medicos(IdMedico),
    FechaCita DATETIME NOT NULL,
    Estado CHAR(1) DEFAULT 'P' -- P: Pendiente, A: Atendida, C: Cancelada
);

-- Tabla Ventas Cabecera (Transaccional Módulo Farmacia)
CREATE TABLE tb_Ventas (
    IdVenta INT PRIMARY KEY IDENTITY(1,1),
    IdPaciente INT REFERENCES tb_Usuarios(IdUsuario),
    FechaVenta DATETIME DEFAULT GETDATE(),
    Total DECIMAL(10,2)
);

-- Tabla Ventas Detalle (Transaccional Módulo Farmacia)
CREATE TABLE tb_DetalleVenta (
    IdDetalle INT PRIMARY KEY IDENTITY(1,1),
    IdVenta INT REFERENCES tb_Ventas(IdVenta),
    IdMedicamento INT REFERENCES tb_Medicamentos(IdMedicamento),
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(10,2) NOT NULL,
    SubTotal DECIMAL(10,2) NOT NULL
);
GO

/**************************************************************************
 * DATOS DE PRUEBA (SEMILLA)
 **************************************************************************/

-- Roles
INSERT INTO tb_Roles (NombreRol) VALUES ('Administrador'), ('Medico'), ('Paciente');

-- Usuarios (Clave: 123456)
INSERT INTO tb_Usuarios (NombreCompleto, Correo, Clave, IdRol) 
VALUES ('Administrador Principal', 'admin@medicita.com', '123456', 1);

INSERT INTO tb_Usuarios (NombreCompleto, Correo, Clave, IdRol) 
VALUES ('Dr. Juan Perez', 'doctor@medicita.com', '123456', 2);

INSERT INTO tb_Usuarios (NombreCompleto, Correo, Clave, IdRol) 
VALUES ('Paciente Prueba', 'paciente@gmail.com', '123456', 3);

-- Especialidades
INSERT INTO tb_Especialidades (NombreEspec, Descripcion) 
VALUES ('Medicina General', 'Atención primaria y chequeos'), 
       ('Pediatría', 'Atención a niños y adolescentes'), 
       ('Cardiología', 'Enfermedades del corazón');

-- Medicamentos
INSERT INTO tb_Medicamentos (Nombre, Laboratorio, Precio, Stock) VALUES 
('Paracetamol 500mg', 'Genfar', 1.50, 100),
('Ibuprofeno 400mg', 'Bayer', 2.80, 50),
('Amoxicilina 500mg', 'Genfar', 3.50, 80),
('Panadol Antigripal', 'GSK', 5.00, 200),
('Vick Vaporub', 'P&G', 12.50, 30);

-- Medicos (Asignamos al usuario 2 como médico)
INSERT INTO tb_Medicos (IdUsuario, IdEspecialidad, CMP, Telefono)
VALUES (2, 1, 'CMP-998877', '999888777');
GO

/**************************************************************************
 * PROCEDIMIENTOS ALMACENADOS
 **************************************************************************/

-- =============================================
-- 1. SEGURIDAD (LOGIN)
-- =============================================
CREATE PROCEDURE usp_ValidarUsuario
    @Correo VARCHAR(100),
    @Clave VARCHAR(100)
AS
BEGIN
    SELECT u.IdUsuario, u.NombreCompleto, u.Correo, u.Clave, u.IdRol, r.NombreRol
    FROM tb_Usuarios u
    INNER JOIN tb_Roles r ON u.IdRol = r.IdRol
    WHERE u.Correo = @Correo AND u.Clave = @Clave
END;
GO

-- =============================================
-- 2. MANTENIMIENTO FARMACIA (CRUD)
-- =============================================

-- Listar
CREATE PROCEDURE usp_ListarMedicamentos
AS
BEGIN
    SELECT IdMedicamento, Nombre, Laboratorio, Precio, Stock FROM tb_Medicamentos
END;
GO

-- Obtener por ID
CREATE PROCEDURE usp_ObtenerMedicamento
    @IdMedicamento INT
AS
BEGIN
    SELECT IdMedicamento, Nombre, Laboratorio, Precio, Stock 
    FROM tb_Medicamentos WHERE IdMedicamento = @IdMedicamento
END;
GO

-- Registrar
CREATE PROCEDURE usp_RegistrarMedicamento
    @Nombre VARCHAR(100),
    @Laboratorio VARCHAR(50),
    @Precio DECIMAL(10,2),
    @Stock INT
AS
BEGIN
    INSERT INTO tb_Medicamentos(Nombre, Laboratorio, Precio, Stock)
    VALUES (@Nombre, @Laboratorio, @Precio, @Stock)
END;
GO

-- Editar
CREATE PROCEDURE usp_EditarMedicamento
    @IdMedicamento INT,
    @Nombre VARCHAR(100),
    @Laboratorio VARCHAR(50),
    @Precio DECIMAL(10,2),
    @Stock INT
AS
BEGIN
    UPDATE tb_Medicamentos
    SET Nombre = @Nombre,
        Laboratorio = @Laboratorio,
        Precio = @Precio,
        Stock = @Stock
    WHERE IdMedicamento = @IdMedicamento
END;
GO

-- Eliminar
CREATE PROCEDURE usp_EliminarMedicamento
    @IdMedicamento INT
AS
BEGIN
    DELETE FROM tb_Medicamentos WHERE IdMedicamento = @IdMedicamento
END;
GO

-- =============================================
-- 3. PROCESO TRANSACCIONAL (CARRITO DE COMPRAS)
-- =============================================

-- Registrar Cabecera
CREATE PROCEDURE usp_RegistrarVenta
    @IdUsuario INT,
    @Total DECIMAL(10,2)
AS
BEGIN
    INSERT INTO tb_Ventas(IdPaciente, Total)
    VALUES (@IdUsuario, @Total);
    
    -- Retorna el ID autogenerado
    SELECT SCOPE_IDENTITY(); 
END;
GO

-- Registrar Detalle y Descontar Stock
CREATE PROCEDURE usp_RegistrarDetalle
    @IdVenta INT,
    @IdMedicamento INT,
    @Cantidad INT,
    @Precio DECIMAL(10,2),
    @SubTotal DECIMAL(10,2)
AS
BEGIN
    -- A. Insertar el detalle
    INSERT INTO tb_DetalleVenta(IdVenta, IdMedicamento, Cantidad, PrecioUnitario, SubTotal)
    VALUES (@IdVenta, @IdMedicamento, @Cantidad, @Precio, @SubTotal);

    -- B. Descontar el Stock del medicamento (Lógica de Negocio)
    UPDATE tb_Medicamentos
    SET Stock = Stock - @Cantidad
    WHERE IdMedicamento = @IdMedicamento;
END;
GO

-- =============================================
-- 4. PROCESO DE CITAS MÉDICAS
-- =============================================

-- Listar Médicos por Especialidad
CREATE PROCEDURE usp_ListarMedicosPorEspecialidad
    @IdEspecialidad INT
AS
BEGIN
    SELECT m.IdMedico, u.NombreCompleto, e.NombreEspec as Especialidad, m.CMP
    FROM tb_Medicos m
    INNER JOIN tb_Usuarios u ON m.IdUsuario = u.IdUsuario
    INNER JOIN tb_Especialidades e ON m.IdEspecialidad = e.IdEspecialidad
    WHERE m.IdEspecialidad = @IdEspecialidad
END;
GO

-- Registrar Cita
CREATE PROCEDURE usp_RegistrarCita
    @IdPaciente INT,
    @IdMedico INT,
    @FechaCita DATETIME
AS
BEGIN
    INSERT INTO tb_Citas(IdPaciente, IdMedico, FechaCita, Estado)
    VALUES (@IdPaciente, @IdMedico, @FechaCita, 'P')
END;
GO

-- =============================================
-- 5. REPORTES (PAGINACIÓN Y FILTROS)
-- =============================================

-- Reporte de Citas por Usuario
CREATE PROCEDURE usp_ListarCitasPorUsuario
    @IdPaciente INT
AS
BEGIN
    SELECT 
        c.IdCita,
        c.FechaCita,
        u.NombreCompleto AS NombreMedico,
        e.NombreEspec AS NombreEspecialidad,
        c.Estado
    FROM tb_Citas c
    INNER JOIN tb_Medicos m ON c.IdMedico = m.IdMedico
    INNER JOIN tb_Usuarios u ON m.IdUsuario = u.IdUsuario
    INNER JOIN tb_Especialidades e ON m.IdEspecialidad = e.IdEspecialidad
    WHERE c.IdPaciente = @IdPaciente
    ORDER BY c.FechaCita DESC
END;
GO
