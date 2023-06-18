using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebStore.Data;
using WebStore.Models;
using System.Security.Claims;
using WebStore.Controllers;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebStore.Authentication
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
            );

            services.ConfigureApplicationCookie(options =>
            {
                options.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = async context =>
                    {
                        UserManager<IdentityUser> userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
                        IdentityUser user = await userManager.GetUserAsync(context.Principal);

                        if (user != null)
                        {
                            IList<string> roles = await userManager.GetRolesAsync(user);
                            if (roles.Count == 0)
                            {
                                RoleManager<IdentityRole> roleManager = context.HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();
                                IdentityRole publicRole = await roleManager.FindByNameAsync("customer");

                                if (publicRole != null)
                                {
                                    await userManager.AddToRoleAsync(user, publicRole.Name);
                                }
                            }
                        }
                    }
                };
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.SlidingExpiration = true;
            });

            services.AddTransient<IEmailSender, EmailSender>();
            services.Configure<AuthMessageSenderOptions>(Configuration);

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(1);
            });

            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true; // Требуется ли цифра в пароле
                options.Password.RequireLowercase = true; // Требуется ли символ в нижнем регистре в пароле
                options.Password.RequireUppercase = true; // Требуется ли символ в верхнем регистре в пароле
                options.Password.RequireNonAlphanumeric = true; // Требуется ли специальный символ в пароле
                options.Password.RequiredLength = 6; // Минимальная длина пароля
                options.Lockout.MaxFailedAccessAttempts = 5; // Максимальное количество неудачных попыток входа
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10); // Время блокировки после превышения количества неудачных попыток
                options.SignIn.RequireConfirmedEmail = true; // Требуется ли подтверждение электронной почты при регистрации
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddUserManager<UserManager<IdentityUser>>()
            .AddDefaultTokenProviders();

            services.AddIdentityServer()
                .AddInMemoryClients(Config.GetClients())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddDeveloperSigningCredential();

            services.AddScoped<Config>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://localhost:7204";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:SecretKey"]))
                    };
                });

            services.AddControllers();

            services.AddRazorPages();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}"
                );

                endpoints.MapControllerRoute(
                    name: "orders",
                    pattern: "{controller=Home}/{action=MyOrders}"
                );

                endpoints.MapControllerRoute(
                    name: "removeOrder",
                    pattern: "{controller=Home}/{action=RemoveOrder}/{orderId?}"
                );

                endpoints.MapControllerRoute(
                    name: "allOrders",
                    pattern: "{controller=Home}/{action=AllOrders}"
                );
            });

            if (!roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                IdentityRole role = new IdentityRole("Admin");
                roleManager.CreateAsync(role).GetAwaiter().GetResult();
            }
        }
    }
}
