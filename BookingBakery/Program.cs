using BookingBakery.Application.IService;
using BookingBakery.Application.Service;
using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using BookingBakery.Infrastructure.Persistence;
using BookingBakery.Infrastructure.Helper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace BookingBakery
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ─── MongoDB ────────────────────────────────────────────────
            builder.Services.AddSingleton<MongoDbContext>();

            // Đăng ký Repository cụ thể cho Authentication, Category và Product
            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
            builder.Services.AddScoped<IProductIngredientRepository, ProductIngredientRepository>();

            // Đăng ký Repository cụ thể cho UserProfile
            builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();

            // ─── Application Services ────────────────────────────────────
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IIngredientService, IngredientService>();
            builder.Services.AddScoped<IProductIngredientService, ProductIngredientService>();


            //
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
            builder.Services.AddScoped<ICartService, CartService>();

            // ─── Cloudinary ──────────────────────────────────────────────
            var cloudinarySettings = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>()
                ?? throw new InvalidOperationException("CloudinarySettings is not configured.");
            builder.Services.AddSingleton(cloudinarySettings);
            builder.Services.AddSingleton<HelperCloudinary>();

            // ─── JWT Authentication ──────────────────────────────────────
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();

            // ─── Swagger với JWT Bearer ──────────────────────────────────
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BookingBakery API",
                    Version = "v1",
                    Description = "API quản lý đặt bánh - BookingBakery",
                    Contact = new OpenApiContact
                    {
                        Name = "BookingBakery Team",
                        Email = "support@bookingbakery.com"
                    }
                });

                // Định nghĩa JWT Bearer Security
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập JWT token theo định dạng: Bearer {token}\n\nVí dụ: Bearer eyJhbGci..."
                });

                // Yêu cầu JWT cho tất cả endpoint có [Authorize]
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // Gieo dữ liệu vai trò mặc định (Seeding)
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    await BookingBakery.Infrastructure.Persistence.MongoDbSeeder.SeedRolesAsync(services);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi gieo dữ liệu vai trò: {ex.Message}");
                }
            }

            // ─── HTTP Pipeline ───────────────────────────────────────────
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "BookingBakery API v1");
                    options.RoutePrefix = "swagger";
                    options.DocumentTitle = "BookingBakery API";
                });

                // Tự động chuyển hướng trang chủ "/" sang "/swagger" trong môi trường Development
                app.MapGet("/", context =>
                {
                    context.Response.Redirect("/swagger");
                    return Task.CompletedTask;
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // In đường dẫn Swagger ra console khi ứng dụng khởi động thành công
            app.Lifetime.ApplicationStarted.Register(() =>
            {
                Console.WriteLine("\n==========================================================");
                Console.WriteLine("API is running!");
                foreach (var address in app.Urls)
                {
                    Console.WriteLine($"Access Swagger at: {address}/swagger");
                }
                Console.WriteLine("==========================================================\n");
            });

            await app.RunAsync();
        }
    }
}
