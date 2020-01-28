using System.Text;

using Microsoft.IdentityModel.Tokens;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using AuthService.DAL;
using AuthService.Middleware;

namespace AuthService
{
    class Policies
    {
        public const string AdminOnly = "AdminOnly";
        public const string AdminAndDev = "AdminAndDev";
        public const string All = "All";
    }

    class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            // mongodb service should be registred here

            services.AddSingleton<MongoDAL>();
            services.AddScoped<ErrorHandler>();

            var key = Encoding.ASCII.GetBytes(Secret.secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(Role.Admin.Value));
                options.AddPolicy(Policies.AdminAndDev, policy => policy.RequireRole(Role.Admin.Value, Role.Dev.Value));
                options.AddPolicy(Policies.All, policy => policy.RequireRole(Role.Admin.Value, Role.Dev.Value, Role.User.Value));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<ErrorHandlerMiddleware>();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
