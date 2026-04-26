using Microsoft.EntityFrameworkCore;
using Minio;
using ms_filtering_ia.Data;
using ms_filtering_ia.Messaging.Publisher;
using ms_filtering_ia.Middleware;
using ms_filtering_ia.Options;
using ms_filtering_ia.Repositories;
using ms_filtering_ia.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ── Base de données ───────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── MinIO ─────────────────────────────────────────────────────────────────
builder.Services.AddMinio(config => config
    .WithEndpoint(builder.Configuration["Minio:Endpoint"])
    .WithCredentials(
        builder.Configuration["Minio:AccessKey"],
        builder.Configuration["Minio:SecretKey"])
    .WithSSL(false)
    .Build());

// ── Options ───────────────────────────────────────────────────────────────
builder.Services.Configure<RabbitMQSettings>(
    builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<TesseractOptions>(
    builder.Configuration.GetSection("Tesseract"));

// ── Infrastructure ────────────────────────────────────────────────────────
builder.Services.AddScoped<IMinioService,          MinioService>();
builder.Services.AddScoped<IPdfExtractorService,   PdfExtractorService>();
builder.Services.AddScoped<IOcrService,            OcrService>();         // Scoped — TesseractEngine non thread-safe

// ── Domain services ───────────────────────────────────────────────────────
builder.Services.AddScoped<ICvTextExtractorService,    CvTextExtractorService>();
builder.Services.AddScoped<IFilteringService,          FilteringService>();
builder.Services.AddScoped<IFilteringProfileService,   FilteringProfileService>();

// ── Repositories ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IFilteringResultRepository,  FilteringResultRepository>();
builder.Services.AddScoped<IFilteringProfileRepository, FilteringProfileRepository>();

// ── RabbitMQ ──────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();
builder.Services.AddScoped<IRabbitMQPublisher,     RabbitMQPublisher>();

// ── HttpClient Python API ─────────────────────────────────────────────────
// Timeout long : pipeline OCR + LLM peut dépasser 60s sur gros volumes
builder.Services.AddHttpClient<IPythonApiService, PythonApiService>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["PythonApi:BaseUrl"] ?? "http://localhost:8000");
    client.Timeout = TimeSpan.FromSeconds(
        builder.Configuration.GetValue<int>("PythonApi:TimeoutSeconds", 1200));
});

// ── CORS ──────────────────────────────────────────────────────────────────
builder.Services.AddCors(opt => opt.AddPolicy("AllowAngular", p =>
    p.WithOrigins("http://localhost:4200")
     .AllowAnyHeader()
     .AllowAnyMethod()));

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseCors("AllowAngular");

// ── Middlewares (ordre important) ─────────────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();   // 1 — capture toutes les exceptions
app.UseMiddleware<GatewayAuthMiddleware>();       // 2 — vérifie secret + rôle HR

app.UseAuthorization();
app.MapControllers();
app.Run();