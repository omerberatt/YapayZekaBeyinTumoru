using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using muguremreCVBackend.Business;
using SIFAIBackend.Business;
using SIFAIBackend.DataAccess;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;

namespace SIFAIBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            ConfigureMiddleware(app);

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Add Controllers
            services.AddControllers();

            // Add Swagger with File Upload Support
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "SIFAIBackend", Version = "v1" });

                // File Upload Support
                options.OperationFilter<SwaggerFileUploadOperationFilter>();
            });

            // Configure Database Context
            services.AddDbContext<SifaiContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("MyDatabase")));

            // Dependency Injection for Custom Services
            services.AddScoped<UserDal>();
            services.AddScoped<AuthService>(serviceProvider =>
            {
                var jwtSettings = configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"];
                var expireHours = double.Parse(jwtSettings["ExpireHours"]);
                var userDal = serviceProvider.GetRequiredService<UserDal>();

                return new AuthService(userDal, secretKey, expireHours);
            });

            services.AddScoped<IRegisterRepository, RegisterRepository>();
            services.AddScoped<IRegisterManager, RegisterManager>();
            services.AddScoped<ITumorService, TumorService>();

            // Add Authentication with JWT
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = configuration["JwtSettings:ValidIssuer"],
                    ValidAudience = configuration["JwtSettings:ValidAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"])),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });

            // Authorization
            services.AddAuthorization();

            // CORS Configuration
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });

            // API Behavior Options
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // JSON Serializer Options
            services.AddMvc().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
            });
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            // CORS Middleware
            app.UseCors();

            // Swagger for Development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // HTTPS Redirection
            app.UseHttpsRedirection();

            // Static files for serving uploaded images
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(uploadsFolder),
                RequestPath = "/uploads" // URL'de "/uploads" ile eriþim saðlanacak
            });

            // Authentication and Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map Controllers
            app.MapControllers();
        }
    }
}
