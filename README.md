# Sistema de Gestión de Préstamos de Pañol Escolar

Aplicación de escritorio hecha en **C# Windows Forms (.NET 8)** y **SQL Server**. Está pensada como una base sencilla y entendible para el trabajo integrador, opción A.

## Funciones incluidas
- Alta y consulta de herramientas con control de stock y estado.
- Alta y consulta de alumnos.
- Registro de préstamos: valida alumno, herramienta disponible y stock suficiente.
- Devolución: cierra el préstamo y reintegra automáticamente las unidades al stock.
- Registro y consulta de daños informados.
- Listados en grillas para consultar la información.

## Requisitos
- Windows.
- Visual Studio 2022 con la carga de trabajo **Desarrollo de escritorio de .NET**.
- .NET 8 SDK.
- SQL Server Express o SQL Server.
- SQL Server Management Studio (SSMS), recomendado para ejecutar el script.

## Instalación
1. Abrí `BaseDeDatos.sql` en SQL Server Management Studio.
2. Ejecutá el script completo con **F5**. Se crea la base `PanolEscolar`, sus tablas y algunos datos de ejemplo.
3. Abrí `SistemaPanol.sln` en Visual Studio.
4. En `SistemaPanol/Database.cs`, revisá `ConnectionString`.
   - Para SQL Server Express normalmente: `Server=.\SQLEXPRESS;Database=PanolEscolar;Trusted_Connection=True;TrustServerCertificate=True;`
   - Para una instancia local predeterminada: `Server=localhost;Database=PanolEscolar;Trusted_Connection=True;TrustServerCertificate=True;`
5. En Visual Studio, restaurá los paquetes NuGet y ejecutá con **F5**.

## Uso rápido
- **Herramientas e inventario:** completá nombre, cantidad inicial y estado para agregar una herramienta. El ID aparece en la grilla.
- **Alumnos:** cargá apellido y nombre y curso. El ID aparece en la grilla.
- **Préstamos y devoluciones:** ingresá ID de alumno, ID de herramienta y cantidad. Seleccioná un préstamo pendiente en la grilla para devolverlo.
- **Daños e historial:** ingresá el ID del préstamo y describí el daño para dejar registro.

## Modelo de datos
- `Alumnos`: personas habilitadas para retirar herramientas.
- `Herramientas`: inventario y unidades disponibles.
- `Prestamos`: relación entre alumno y herramienta, cantidad y fechas.
- `Danios`: reportes de daños vinculados a un préstamo.

## Decisiones y límites de esta versión
- La conexión usa autenticación integrada de Windows.
- No incluye usuarios/contraseñas ni sanciones de alumnos, porque la consigna de opción A no las exige expresamente.
- Es una versión educativa inicial: antes de usarla en un pañol real conviene agregar permisos, auditoría y pruebas de concurrencia.
