using EcommerceApi.Configuration;
using EcommerceApi.Data;
using EcommerceApi.Services;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true);
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EcommerceApi", Version = "v1" });

    // Đoạn code này để hiện nút "Authorize" hình ổ khóa
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Dán Token vào đây theo cấu trúc: Bearer [cách] [token của bạn]",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
//Dangky
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Nhom1")));


builder.Services.Configure<GoogleAuthSettings>(settings =>
{
    var resolved = GoogleAuthSettings.FromConfiguration(builder.Configuration);
    settings.ClientId = resolved.ClientId;
    settings.ClientSecret = resolved.ClientSecret;
});

builder.Services.Configure<AdminSettings>(settings =>
{
    var resolved = AdminSettings.FromConfiguration(builder.Configuration);
    settings.Emails = resolved.Emails;
});

var googleAuth = GoogleAuthSettings.FromConfiguration(builder.Configuration);

var authBuilder = builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Chuỗi_Bí_Mật_Cực_Dài_Của_Bạn_123")),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    })
    .AddCookie("External", options =>
    {
        options.Cookie.Name = "ext_auth";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    });

if (googleAuth.IsConfigured)
{
    authBuilder.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.SignInScheme = "External";
        options.ClientId = googleAuth.ClientId;
        options.ClientSecret = googleAuth.ClientSecret;
        options.CallbackPath = "/signin-google";
        options.SaveTokens = true;
    });
}
else
{
    Console.WriteLine("WARN: Chưa cấu hình Authentication:Google:ClientId/ClientSecret trong appsettings.");
}

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IDemoUserStore, DemoUserStore>();
builder.Services.AddScoped<CustomerRankingService>();
builder.Services.AddScoped<UserProfileResolver>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ProductReviewService>();
var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
    DbSchemaEnsurer.EnsureOrderCustomerColumns(context);
    DbSchemaEnsurer.EnsureUsersForGoogleLogin(context);
    DbSchemaEnsurer.EnsurePromotionalProductsTable(context);
    DbSchemaEnsurer.EnsureOrderAdminReviewColumns(context);
    DbSchemaEnsurer.EnsureNotificationsTable(context);
    DbSchemaEnsurer.EnsureProductReviewsTable(context);
    DbSchemaEnsurer.EnsureCostAndProfitColumns(context);

    if (!context.Categories.Any())
    {
        context.Categories.AddRange(CategorySeeder.GetCategories());
        context.SaveChanges();
    }

    if (!context.Products.Any())
    {
        var products = ProductSeeder.GetProducts();
        context.Products.AddRange(products);
        context.SaveChanges();
    }

    foreach (var p in ProductSeeder.GetTwentyExtraProducts())
    {
        if (!context.Products.Any(x => x.Name == p.Name))
            context.Products.Add(p);
    }
    context.SaveChanges();

    if (!context.PromotionalProducts.Any())
    {
        var seedProducts = context.Products.OrderBy(p => p.Id).Take(3).ToList();
        var order = 0;
        foreach (var p in seedProducts)
        {
            context.PromotionalProducts.Add(new EcommerceApi.Models.PromotionalProduct
            {
                ProductId = p.Id,
                Headline = p.Name,
                Subtitle = "Ưu đãi đặc biệt — mua ngay hôm nay",
                BadgeText = "HOT",
                PromoPrice = Math.Round(p.Price * 0.9m, 0),
                SortOrder = order++,
                IsActive = true
            });
        }
        context.SaveChanges();
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseDefaultFiles(); // Để nó tự tìm index.html hoặc login.html
app.UseStaticFiles();  // Cho phép trình duyệt truy cập các file trong wwwroot

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
