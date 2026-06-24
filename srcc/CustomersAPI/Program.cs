using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Read ALL config from environment variables
var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "customersdb";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
var dbPass = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "postgres";
var ns = Environment.GetEnvironmentVariable("NAMESPACE") ?? "customers-ns";
var podName = Environment.GetEnvironmentVariable("POD_NAME") ?? Environment.MachineName;

// Connection string with pooling
var connectionString =
    $"Host={dbHost};" +
    $"Port={dbPort};" +
    $"Database={dbName};" +
    $"Username={dbUser};" +
    $"Password={dbPass};" +
    $"Pooling=true;" +
    $"MinPoolSize=2;" +
    $"MaxPoolSize=20;" +
    $"ConnectionIdleLifetime=300;" +
    $"Include Error Detail=true;";

// Register DbContext with PostgreSQL provider
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Ensuring database schema is created");
        db.Database.EnsureCreated();

        if (!db.Customers.Any())
        {
            logger.LogInformation("Seeding customers...");
            db.Customers.AddRange(SeedData.GetCustomers());
            db.SaveChanges();
            logger.LogInformation("Seed complete.");
        }
        else
        {
            logger.LogInformation("Database already contains data.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed. Check DB_HOST and credentials.");
    }
}

// Root endpoint
app.MapGet("/", () => Results.Ok(new
{
    service = "Customers API",
    version = "v1",
    namespace_ = ns,
    pod = podName,
    endpoints = new[]
    {
        "GET /customers  → all customers", 
        "GET /health     → liveness",
        "GET /info       → pod + namespace info"
    }
}));

// Health endpoint
app.MapGet("/health", async (AppDbContext db) =>
{
    try
    {
        await db.Database.ExecuteSqlRawAsync("SELECT 1");

        return Results.Ok(new
        {
            status = "healthy",
            database = "connected",
            pod = podName,
            namespace_ = ns,
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new
        {
            status = "unhealthy",
            database = "unreachable",
            error = ex.Message,
            pod = podName,
            namespace_ = ns,
            timestamp = DateTime.UtcNow
        }, statusCode: 503);
    }
});

// Info endpoint
// Shows which pod and namespace is serving the request.
// different pod name appears after a pod is killed.
app.MapGet("/info", () => Results.Ok(new
{
    pod = podName,
    namespace_ = ns,
    dbHost = dbHost,
    dbPort = dbPort,
    dbName = dbName,
    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
    timestamp = DateTime.UtcNow
}));

// Get all customers
app.MapGet("/customers", async (AppDbContext db) =>
{
    var customers = await db.Customers
        .OrderBy(c => c.Id)
        .ToListAsync();

    return Results.Ok(new
    {
        count = customers.Count,
        servedBy = podName,
        data = customers
    });
});

app.Run();


// Models
public class Customer
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;
}

//  DbContext — EF Core with PostgreSQL
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Address)
                .HasMaxLength(200);

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);
        });
    }
}

// Seed Data
public static class SeedData
{
    public static List<Customer> GetCustomers() => new()
    {
        new Customer { FirstName = "John", LastName = "Smith", Address = "New York", PhoneNumber = "9876543210" },
        new Customer { FirstName = "Emma", LastName = "Johnson", Address = "Chicago", PhoneNumber = "9876543211" },
        new Customer { FirstName = "Michael", LastName = "Brown", Address = "Dallas", PhoneNumber = "9876543212" },
        new Customer { FirstName = "Sophia", LastName = "Williams", Address = "Seattle", PhoneNumber = "9876543213" },
        new Customer { FirstName = "David", LastName = "Jones", Address = "Boston", PhoneNumber = "9876543214" },
        new Customer { FirstName = "Olivia", LastName = "Miller", Address = "Miami", PhoneNumber = "9876543215" },
        new Customer { FirstName = "James", LastName = "Davis", Address = "Denver", PhoneNumber = "9876543216" },
        new Customer { FirstName = "Isabella", LastName = "Wilson", Address = "Atlanta", PhoneNumber = "9876543217" },
        new Customer { FirstName = "Daniel", LastName = "Taylor", Address = "Phoenix", PhoneNumber = "9876543218" },
        new Customer { FirstName = "Mia", LastName = "Anderson", Address = "Houston", PhoneNumber = "9876543219" }
    };
}