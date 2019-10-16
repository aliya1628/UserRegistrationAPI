using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserRegistrationAPI.Models;

namespace UserRegistrationAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Inject appsettings
            services.Configure<ApplicationSettings>(Configuration.GetSection("ApplicationSettings"));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddDbContext<AuthenticationContext>(options => options.UseSqlServer(Configuration.GetConnectionString("IdentityConnection")));

            /*by default these two classes invocation will do the dependency injection for the constructor in the controller class i.e.ApplicationUserController*/
            services.AddDefaultIdentity<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AuthenticationContext>();

            services.Configure<IdentityOptions>(options => 
            {
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 4;
            });

            services.AddCors();

            //JWT Authentication
            //var key = Encoding.UTF8.GetBytes("1234567890123456");  //key should have atleast 16 characters

            /*Now instead of hardcoding this JWT secret code (key) inside this data file here we have to move that into the configuration file(appsetting.Json)
             just below the connection string add another key 
             "ApplicationSettings":{
             "JWT_Secret" : "1234567890123456"} */

            //To access key from configuration file(appsetting.Json)
            var key = Encoding.UTF8.GetBytes(Configuration["ApplicationSettings:JWT_Secret"].ToString());

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                /*RequireHttpsMetadata:  we can restrict the authenticaion resources to only request which is only of type Https*/
                x.RequireHttpsMetadata = false;

                /*SaveToken: means after a successful authentication whether we want to save that token inside the server or not */
                x.SaveToken = false;

                /*TokenValidationParameters: how do we want to validate a token once it is recieved from the client side after successful authentication*/
                x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    /*ValidateIssuerSigningKey: system will validate the security key during the token validation*/
                    ValidateIssuerSigningKey = true,

                    /*to access key here we have to create instance of symmetric security key here class */
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,  //ValidateIssuer: who generated this token obviously name of server
                    ValidateAudience = false, //ValidateAudience: targeted audience for the token
                    
                    /*Note: while receiving the token after successfull authentication we can compare whether we have the same Issuer or audience 
                     which we have provided during generation of the token, we dont have to validate those properties ValidateIssuer & ValidateAudience so we have 
                     reset the properties as false */

                    ClockSkew = TimeSpan.Zero /*checking expiration time of token there is no timezone difference between server and client side 
                                                so we have set this timespan as zero*/

                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Use(async (ctx, next) =>
            {
                await next();
                if (ctx.Response.StatusCode == 204)
                {
                    ctx.Response.ContentLength = 0;
                }
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder =>
            builder.WithOrigins(Configuration["ApplicationSettings:Client_URL"].ToString())
            .AllowAnyHeader()
            .AllowAnyMethod());
            app.UseAuthentication();
            app.UseMvc();

        }
    }
}
