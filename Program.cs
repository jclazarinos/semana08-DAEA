using Microsoft.EntityFrameworkCore;
using Lab08_JeanLazarinos.Data;
using Lab08_JeanLazarinos.Repositories;
using Lab08_JeanLazarinos.Repositories.Interfaces;
using Lab08_JeanLazarinos.closedXml;
using System;

var builder = WebApplication.CreateBuilder(args);

// --- 1. REGISTRO DE SERVICIOS ---
// Aquí le dices a tu aplicación qué "herramientas" va a necesitar.

// Agrega los servicios necesarios para que los controladores de la API funcionen.
builder.Services.AddControllers();

// Agrega los servicios para generar la documentación de Swagger.
// Es el estándar para documentar y probar APIs basadas en controladores.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Lee la cadena de conexión desde 'appsettings.json'.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


Console.WriteLine("Iniciando la creación de la TABLA Excel...");

// 1. Crear una instancia de tu clase
var generador = new FirstExample();

// 2. Ejecutar el método que SÍ crea la tabla
generador.MyFourthExample(); 

Console.WriteLine("¡Archivo Excel con TABLA y ESTILOS!");
Console.WriteLine("Revisa la ruta: C:\\MET\\semana08\\Lab08-JeanLazarinos\\example.xlsx");

builder.Services.AddScoped<Lab08_JeanLazarinos.Services.IExcelService, Lab08_JeanLazarinos.Services.ExcelService>();

// Registra tu DbContext (StoreLdbContext) en el contenedor de servicios
// y le dice que debe usar PostgreSQL con la cadena de conexión obtenida.
// Esto permite que luego lo puedas "inyectar" en tus controladores.
builder.Services.AddDbContext<StoreLdbContext>(options =>
    options.UseNpgsql(connectionString));

// LÍNEA PARA REGISTRAR EL UNIT OF WORK
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);


var app = builder.Build();

// --- 2. CONFIGURACIÓN DEL PIPELINE HTTP ---
// Aquí defines el orden en que se procesarán las peticiones que lleguen a tu API.

// Habilita el middleware de Swagger solo si estás en el entorno de desarrollo.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Esto genera la página web interactiva para probar la API.
}

// Redirige automáticamente cualquier petición HTTP a HTTPS para mayor seguridad.
app.UseHttpsRedirection();

// Habilita el middleware de autorización.
app.UseAuthorization();

// Busca los atributos de ruta en tus clases de controlador y los configura
// para que la aplicación sepa qué hacer con una URL como '/api/Productos'.
app.MapControllers();

// Ejecuta la aplicación.
app.Run();