using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuitQ1_Hx;
using QuitQ1_Hx.Data;
using QuitQ1_Hx.Models;
using System.Text.Json.Serialization;
using QuitQ1_Hx.Services;
using System.Text;
using NuGet.Protocol.Plugins;
using QuitQ1_Hx.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "QuitQ1_Hx", Version = "v1" });
});

// Add cart service
builder.Services.AddScoped<ICartService, CartService>();

builder.Services.AddScoped<ITempItemsService, TempItemsService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IRefundService, RefundService>();
// Add Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }
    )
);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .WithOrigins("http://localhost:3000") // Replace with your React app's URL
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()); // Add this if you need to send cookies
});

// Configure JWT Authentication
var key = builder.Configuration["Jwt:Key"];
var issuer = builder.Configuration["Jwt:Issuer"];
if (string.IsNullOrEmpty(key))
{
    throw new ArgumentNullException("JWT Key is missing from appsettings.json");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = issuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization();

// Register the AuthService
builder.Services.AddTransient<IAuthService, AuthService>();

// Configure Swagger with JWT
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer YOUR_TOKEN'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

// Database Initialization and Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Ensure the database is created (or recreated if needed)
        // For development, you may want to use:
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        Console.WriteLine("Database created successfully");
        // Check and create Cart tables if needed
        if (context.Model.FindEntityType(typeof(Cart)) != null &&
            context.Model.FindEntityType(typeof(CartItem)) != null)
        {
            // These tables should be created by EF Core automatically
            Console.WriteLine("Cart tables are available");
        }
        // Seed Categories if they don't exist
        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { Name = "Electronics", Description = "Electronic devices and accessories" },
                new Category { Name = "Clothing", Description = "Apparel and accessories" },
                new Category { Name = "Home & Kitchen", Description = "Household items" }
            );
            context.SaveChanges();
            Console.WriteLine("Categories seeded successfully");
        }

        // Seed Sellers if they don't exist
        if (!context.Sellers.Any())
        {
            context.Sellers.Add(new Seller
            {
                Name = "Default Seller",
                Email = "seller@example.com",
                PhoneNumber = "1234567890",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Address = "123 Main St, City, State, 12345" // Added address
            });
            context.SaveChanges();
            Console.WriteLine("Default seller created successfully");
        }

        // Check and create Cart tables if needed
        if (context.Model.FindEntityType(typeof(Cart)) != null &&
            context.Model.FindEntityType(typeof(CartItem)) != null)
        {
            // These tables should be created by EF Core automatically
            Console.WriteLine("Cart tables are available");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
        Console.WriteLine($"Error seeding database: {ex.Message}");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "QuitQ1_Hx v1"));
}
app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseRouting();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();