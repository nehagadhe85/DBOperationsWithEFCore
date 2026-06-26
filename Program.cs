using Microsoft.EntityFrameworkCore;
using DBOperationWithEFCore.Data;
using DBOperationWithEFCore.Middleware;
using DBOperationWithEFCore.Services;
using DBOperationWithEFCore.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// 1. REGISTER SERVICES (Dependency Injection)
// ========================================

// EF Core: Register DbContext with SQL Server provider
// The connection string is read from appsettings.json
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("LibraryDb")
    ));

// Register application services — Scoped lifetime (one instance per HTTP request)
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<ILoanService, LoanService>();

// Add controllers with JSON serialization options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Library Management API",
        Version = "v1",
        Description = "A Library Management System API built with .NET 8 and Entity Framework Core"
    });
});

// CORS — Allow React frontend to call the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Vite + CRA defaults
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ========================================
// 2. CONFIGURE MIDDLEWARE PIPELINE
// ========================================

// Global exception handling (must be first to catch all errors)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library Management API v1"));
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

// ========================================
// 3. AUTO-MIGRATE DATABASE ON STARTUP (Development only)
// ========================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    
    // This applies any pending migrations automatically
    // In production, you'd use 'dotnet ef database update' instead
    dbContext.Database.Migrate();
}

app.Run();
