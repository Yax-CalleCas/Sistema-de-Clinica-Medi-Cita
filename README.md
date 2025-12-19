 Correo = "admin@medicita.com",
 Nombre = "Jack Cibertec",
 Clave = "admin123",

 # MediCita

MediCita es una aplicación web integral para la gestión de una clínica médica digital, desarrollada bajo el patrón ASP.NET Core MVC con Razor Pages, Bootstrap 5 y SQL Server.

La plataforma permite la administración de citas médicas en tiempo real con lógicas de horarios inteligentes, una farmacia online con carrito de compras persistente, historiales clínicos y reportes avanzados, bajo un sistema de seguridad basado en roles.

## Características Principales

### Gestión de Accesos y Roles
- Administrador: Gestión de usuarios, personal médico, catálogo de medicamentos y generación de reportes.
- Médico: Visualización y control de citas asignadas y agenda diaria.
- Paciente: Reserva de citas, adquisición de medicamentos y consulta de historial personal.

### Sistema de Citas Inteligente
- Generación automática de disponibilidad: Mediante HorarioService, se proyecta la agenda de los médicos por los próximos 7 días tras su registro.
- Filtros de disponibilidad: Los pacientes visualizan turnos basados en el día seleccionado, excluyendo horas pasadas y horarios previamente reservados.
- Configuración de jornada: Soporte técnico para exclusión de pausas de almuerzo y bloqueos de seguridad en tiempo real.

### Farmacia Online
- Catálogo dinámico: Gestión de productos por categorías, imágenes, descripciones y sistema de promociones (formato porcentual).
- Carrito persistente: Almacenamiento de la selección de productos directamente en la base de datos para continuidad de sesión.
- Procesamiento de ventas: Registro transaccional con cálculo automático de subtotal, IGV y total, utilizando Table-Valued Parameters (TVP) para optimizar operaciones complejas.

### Seguridad y Diseño
- Protección de datos: Implementación de hashing SHA256 para el resguardo de credenciales.
- Inicialización del sistema: Configuración automática del administrador principal mediante SeedService al ejecutar la aplicación por primera vez.
- Interfaz de usuario: Diseño responsive con navegación dinámica según el rol del usuario y notificaciones interactivas.

## Tecnologías Utilizadas

- Backend: ASP.NET Core MVC (.NET 8)
- Persistencia de datos: ADO.NET con Stored Procedures
- Base de Datos: SQL Server
- Frontend: Razor Pages, Bootstrap 5, SweetAlert2 y jQuery

## Requisitos del Entorno

- Visual Studio 2022 con carga de trabajo ASP.NET y desarrollo web.
- .NET 8.0 SDK.
- SQL Server (LocalDB, Express o versión completa).
- Git.

## Instalación y Configuración

1. Clonar el repositorio:
   git clone https://github.com/TU_USUARIO/MediCita.git

2. Configurar la base de datos:
   Ejecutar los scripts de base de datos y procedimientos almacenados en su instancia de SQL Server.

3. Actualizar la cadena de conexión:
   Modificar el archivo appsettings.json con los parámetros correspondientes a su servidor local.

4. Compilar y ejecutar:
   Iniciar el proyecto desde Visual Studio o mediante la línea de comandos con dotnet run.
