namespace HowIdentity;

using Common.Configurations;
using Common.Constants;
using Common.Extensions;
using Duende.IdentityServer;
using Data;
using Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using HowCommon.Configurations;
using IdentityModel;
using Infrastructure.Processing.Producer;
using Infrastructure.Scheduler;
using Infrastructure.Scheduler.Jobs;
using MassTransit;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Quartz.Impl;
using Services;
using Services.CurrentUser;
using Services.GrpcCommunication;
using Services.SuperAdmin;
using Services.TargetUser;

internal static class HostingExtensions
{
    public static WebApplicationBuilder ConfigureCors(this WebApplicationBuilder builder)
    {
        var baseAppSettings = new BaseApplicationSettings();
        builder.Configuration.Bind(nameof(BaseApplicationSettings), baseAppSettings);
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(IdentityAppConstants.CorsPolicy, appBuilder =>
            {
                appBuilder.WithOrigins(baseAppSettings.AllowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
        
        return builder;
    }
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();
        
        builder.Services.ConfigureConfigurations(builder.Configuration);

        var migrationAssembly = typeof(Program).Assembly.GetName().Name;
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                               throw new ApplicationException("Database Connection string is null!");
        
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        builder.Services.AddIdentity<HowUser, HowRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        
        builder.Services.Configure<IdentityOptions>(o =>
        {
            // Password settings.
            o.Password.RequireDigit = true;
            o.Password.RequireLowercase = true;
            o.Password.RequireNonAlphanumeric = true;
            o.Password.RequireUppercase = true;
            o.Password.RequiredLength = 8;
            o.Password.RequiredUniqueChars = 4;
            
            // Lockout settings.
            o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            o.Lockout.MaxFailedAccessAttempts = 5;
            o.Lockout.AllowedForNewUsers = true;
            
            // User settings.
            o.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            o.User.RequireUniqueEmail = true;
        });

        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;
                
                options.Authentication.CheckSessionCookieName = "HowIdentity.Cookie";
                // options.Authentication.CookieLifetime = TimeSpan.FromMinutes(2);

                options.Authentication.CoordinateClientLifetimesWithUserSession = true;
                
                options.ServerSideSessions.UserDisplayNameClaimType = JwtClaimTypes.Name;
                options.ServerSideSessions.RemoveExpiredSessions = true;
                // options.ServerSideSessions.RemoveExpiredSessionsFrequency = TimeSpan.FromMinutes(30);
                options.ServerSideSessions.ExpiredSessionsTriggerBackchannelLogout = true;
            })
            // .AddInMemoryIdentityResources(Config.IdentityResources)
            // .AddInMemoryApiScopes(Config.ApiScopes)
            // .AddInMemoryClients(Config.Clients)
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = b => b.UseNpgsql(
                    connectionString,
                    sql => sql.MigrationsAssembly(migrationAssembly));
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = b => b.UseNpgsql(
                    connectionString,
                    sql => sql.MigrationsAssembly(migrationAssembly));
                
                // this enables automatic token cleanup. this is optional.
                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 3600; // interval in seconds (default is 3600)
            })
            .AddAspNetIdentity<HowUser>()
            .AddProfileService<CustomProfileService>()
            .AddServerSideSessions();

        builder.Services.AddAuthentication()
            .AddOpenIdConnect("oidc", options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                options.SaveTokens = true;

                options.Authority = "https://demo.duendesoftware.com";
                options.ClientId = "interactive.confidential";
                options.ClientSecret = "secret";
                options.ResponseType = "code";

                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            })
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                // register your IdentityServer with Google at https://console.developers.google.com
                // enable the Google+ API
                // set the redirect URI to https://localhost:5001/signin-google
                options.ClientId = "copy client ID from Google here";
                options.ClientSecret = "copy client secret from Google here";
            });
        
        builder.AddSuperAdminFeature();
        
        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        InitializeDatabase(app); // TODO run only one to init database

        app.UseStaticFiles();
        app.UseRouting();

        app.UseCors(IdentityAppConstants.CorsPolicy);

        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }

    public static WebApplicationBuilder ConfigureDataAccess(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                               throw new ApplicationException("Database Connection string is null!");
        
        builder.Services.AddSingleton<DapperContext>(o => 
            new DapperContext(connectionString));
        
        return builder;
    }
    
    public static WebApplicationBuilder ConfigureCustomService(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<ISuperAdminUserService, SuperAdminUserService>();
        builder.Services.AddTransient<ICurrentUserService, CurrentUserService>();
        builder.Services.AddTransient<ITargetUserService, TargetUserService>();

        // masstransit services
        builder.Services.AddTransient<UserServiceAccountProducer>();

        return builder;
    }

    public static WebApplicationBuilder ConfigureMassTransit(this WebApplicationBuilder builder)
    {
        var rabbitMq = new RabbitMqConfiguration();
        builder.Configuration.Bind(nameof(RabbitMqConfiguration), rabbitMq);
        
        builder.Services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, config) =>
            {
                config.Host(rabbitMq.Host, "/", host =>
                {
                    host.Username(rabbitMq.User);
                    host.Password(rabbitMq.Password);
                });
            });
        });

        builder.Services.AddOptions<MassTransitHostOptions>()
            .Configure(options =>
            {
                options.WaitUntilStarted = true;
                options.StartTimeout = TimeSpan.FromSeconds(15);
                options.StopTimeout = TimeSpan.FromSeconds(30);
            });
        
        builder.Services.AddOptions<HostOptions>()
            .Configure(options =>
            {
                options.StartupTimeout = TimeSpan.FromSeconds(30);
                options.ShutdownTimeout = TimeSpan.FromSeconds(30);
            });
        
        return builder;
    }

    public static WebApplicationBuilder ConfigureGrpcServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddGrpc(o =>
        {
            o.EnableDetailedErrors = true;
            o.MaxReceiveMessageSize = 1024 * 1024; // 1 MB
            o.MaxSendMessageSize = 1024 * 1024; // 1 MB
        });

        builder.Services.AddGrpcClient<UserAccount.UserAccountClient>(o =>
        {
            o.Address = new Uri("https://localhost:7060");
        });

        builder.Services.AddTransient<IUserAccountGrpcService, UserServiceAccountGrpcService>();

        return builder;
    }

    public static WebApplicationBuilder ConfigureScheduler(this WebApplicationBuilder builder)
    {
        builder.Services.AddQuartz(o =>
        {
            o.UseInMemoryStore();
        });
        builder.Services.AddQuartzHostedService(o =>
        {
            o.WaitForJobsToComplete = true;
            o.AwaitApplicationStarted = true;
        });

        builder.Services.AddTransient<AppJobScheduler>();

        builder.Services.RegisterJobs();

        return builder;
    }

    private static void InitializeDatabase(IApplicationBuilder app)
    {
        using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope())
        {
            var persistentContext = serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
            persistentContext.Database.Migrate();
            
            var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            context.Database.Migrate();
            
            var configuration = serviceScope.ServiceProvider.GetRequiredService<IConfiguration>();

            // update Clients
            var existingClients = context.Clients.ToList();
            var configClients = Config.Clients(configuration).Select(c => c.ToEntity());

            var (clientsToRemove, clientsToAdd) = 
                existingClients.CompareByKey(configClients, c => c.ClientId);
            
            if (clientsToRemove.Any())
            {
                context.Clients.RemoveRange(clientsToRemove);
            }

            if (clientsToAdd.Any())
            {
                context.Clients.AddRange(clientsToAdd);
            }

            // update Identity Resources
            var existingIdentityResources = context.IdentityResources.ToList();
            var configIdentityResources = Config.IdentityResources.Select(c => c.ToEntity());
            
            var (identityResourcesToRemove, identityResourcesToAdd) = 
                existingIdentityResources.CompareByKey(configIdentityResources, i => i.Name);
            
            if (identityResourcesToRemove.Any())
            {
                context.IdentityResources.RemoveRange(identityResourcesToRemove);
            }

            if (identityResourcesToAdd.Any())
            {
                context.IdentityResources.AddRange(identityResourcesToAdd);
            }

            // update Api Scopes
            var existingApiScopes = context.ApiScopes.ToList();
            var configApiScopes = Config.ApiScopes.Select(c => c.ToEntity());
            
            var (apiScopesToRemove, apiScopesToAdd) = 
                existingApiScopes.CompareByKey(configApiScopes, i => i.Name);
            
            if (apiScopesToRemove.Any())
            {
                context.ApiScopes.RemoveRange(apiScopesToRemove);
            }

            if (apiScopesToAdd.Any())
            {
                context.ApiScopes.AddRange(apiScopesToAdd);
            }

            // update Api Resources
            var existingApiResources = context.ApiResources.ToList();
            var configApiResources = Config.ApiResources(configuration).Select(c => c.ToEntity());
            
            var (apiResourcesToRemove, apiResourcesToAdd) = 
                existingApiResources.CompareByKey(configApiResources, i => i.Name);
            
            if (apiResourcesToRemove.Any())
            {
                context.ApiResources.RemoveRange(apiResourcesToRemove);
            }

            if (apiResourcesToAdd.Any())
            {
                context.ApiResources.AddRange(apiResourcesToAdd);
            }

            context.SaveChanges();
        }
    }

    private static IServiceCollection ConfigureConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AdminCredentials>(configuration.GetSection("AdminCredentials"));
        
        return services;
    }

    private static IServiceCollection RegisterJobs(this IServiceCollection services)
    {
        services.AddTransient<TestJob>();

        return services;
    }

    private static void AddSuperAdminFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options => 
        {
            options.AddPolicy("super-admin", policy =>
            {
                policy.RequireRole(IdentityRoleConstant.Role.SuperAdmin.Name);
                policy.RequireAuthenticatedUser();
            });
        });

        builder.Services.Configure<RazorPagesOptions>(options =>
        {
            options.Conventions.AuthorizeFolder("/SuperAdmin", "super-admin");
        });
    }
}