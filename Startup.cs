using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NSwag;
using NSwag.Generation.Processors.Security;
using PasswordPoliciesDemo.API.Auth.Token;
using PasswordPoliciesDemo.API.Controllers;
using PasswordPoliciesDemo.API.Infrastructure;
using PasswordPoliciesDemo.API.Infrastructure.Application.Configurations;
using PasswordPoliciesDemo.API.Infrastructure.Auth;
using PasswordPoliciesDemo.API.Infrastructure.Auth.Token;
using PasswordPoliciesDemo.API.Infrastructure.Common.Exceptions;
using PasswordPoliciesDemo.API.Infrastructure.Common.Extensions;
using PasswordPoliciesDemo.API.Infrastructure.Domain;
using PasswordPoliciesDemo.API.Infrastructure.Filters;
using PasswordPoliciesDemo.API.Infrastructure.Persistence;
using PasswordPoliciesDemo.API.Infrastructure.Services.Identity;
using PasswordPoliciesDemo.API.ViewModels.Validations;

namespace PasswordPoliciesDemo.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services
                .AddCustomMvc()
                .AddCustomAuthentication()
                .AddApplication(Configuration)
                .AddPersistence(Configuration)
                .AddInfrastructure(Configuration)
                .AddHealthChecks(Configuration)
                .AddCustomSwagger(Configuration)
                .AddCustomConfiguration(Configuration);





            //Services
                


            var container = new ContainerBuilder();
            container.Populate(services);
            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }



            app.UseCors("CorsPolicy");




            app.UseHttpsRedirection();

            app.UseRouting();

            ConfigureAuth(app);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllers();

                endpoints.MapHealthChecks( "/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
                endpoints.MapHealthChecks( "/health", new HealthCheckOptions
                {

                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    }
                });
            });








            app.UseOpenApi(c => {
                c.Path = "/docs/{documentName}/swagger.json";
            });
            app.UseSwaggerUi3(settings =>
            {
                settings.Path =  "/docs";
                settings.DocumentPath =   "/docs/{documentName}/swagger.json";

            });
        }

        private void ConfigureAuth(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }











    static class DependencyInjection
    {


        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {

            services.AddSingleton<IJwtHandler, JwtHandler>();
            services.AddSingleton<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
            services.AddTransient<IUserManager, UserManager>();
            return services;
        }

        public static IServiceCollection AddCustomSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOpenApiDocument(config =>
            {

                config.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "Password Policy Demo API ";
                    document.Info.Description = "A Demo API by Peter Makafan";
                    document.Info.TermsOfService = "None";

                };

                config.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Type into the textbox: Bearer {your JWT token}."
                });

                config.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));


            });

            return services;
        }




        public static IServiceCollection AddCustomMvc(this IServiceCollection services)
        {
            // Add framework services.
            services.AddControllers(options =>
                {
                    options.Filters.Add(typeof(HttpGlobalExceptionFilter));

                })
                .AddApplicationPart(typeof(IdentityController).Assembly)
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<ChangePasswordValidation>());

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .SetIsOriginAllowed((host) => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            return services;
        }



        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration["ConnectionString"];
            services
                .AddDbContext<DemoContext>(options =>
                    {
                        options.UseMySql(connectionString,
                            sqlOptions =>
                            {
                                sqlOptions.MigrationsAssembly(typeof(DemoContext).GetTypeInfo().Assembly.GetName().Name);
                                // ReSharper disable once ArgumentsStyleLiteral
                                // ReSharper disable once AssignNullToNotNullAttribute
                                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), errorNumbersToAdd: null);

                            });
                        // options.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
                    } //Showing explicitly that the DbContext is shared across the HTTP request scope (graph of objects started in the HTTP request)

                );
            return services;


        }




        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());


            //Services 



            services.AddHttpContextAccessor();

            //Customise default API behaviour
            services.Configure<ApiBehaviorOptions>(options =>
            {
                //options.SuppressModelStateInvalidFilter = true;
                //Temporary Fix For Model State Validation
                options.InvalidModelStateResponseFactory = (context) =>
                {
                    var modelStates = context.ModelState;
                    var validationException = new ValidationException()
                    {
                        Failures = modelStates
                            .Where(x => x.Value.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.Errors.Select(e =>
                                        !string.IsNullOrEmpty(e.ErrorMessage) ? e.ErrorMessage : e.Exception.Message)
                                    .ToArray()
                            )
                    };
                    var result = validationException.ToErrorResponse();
                    return new BadRequestObjectResult(result);
                };
            });

            services.AddScoped<ICurrentUserService, CurrentUserService>();



            return services;
        }




        public static IServiceCollection AddCustomConfiguration(this IServiceCollection services, IConfiguration configuration)
        {

            //Settings
            services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
            services.Configure<ApplicationUserSettings>(configuration.GetSection(nameof(ApplicationUserSettings)));
            services.AddOptions();
            return services;
        }



        public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var c = configuration.GetValue("UseHealthCheck", true);
            if (c)
            {
                var hcBuilder = services.AddHealthChecks();
                hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy())
                    .AddCheck<DatabaseHealthCheck>("Database Health Check");
            }


            return services;
        }




        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    CustomAuthenticationSchemes.BearerAuthenticationScheme;
                options.DefaultChallengeScheme =
                    CustomAuthenticationSchemes.BearerAuthenticationScheme;

            }).AddTokenAuthentication(CustomAuthenticationSchemes.BearerAuthenticationScheme,
                "Custom Bearer Authentication Scheme",
                o =>
                {

                }
            );

            //static cache manager

            return services;
        }
    }







}
