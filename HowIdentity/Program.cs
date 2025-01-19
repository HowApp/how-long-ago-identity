﻿namespace HowIdentity;

using Data.Seeds;
using Serilog;

public static class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Log.Information("Starting up");

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((ctx, lc) => lc
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(ctx.Configuration));

            var app = builder
                .ConfigureCors()
                .ConfigureDataAccess()
                .ConfigureCustomService()
                .ConfigureServices()
                .ConfigurePipeline();

            // this seeding is only for the template to bootstrap the DB and users.
            // in, production you will likely want a different approach.
            if (args.Contains("/seed"))
            {
                Log.Information("Seeding database...");
                SeedData.EnsureSeedAdmin(app);
                SeedData.EnsureSeedData(app);
                Log.Information("Done seeding database. Exiting...");
                return;
            }
            
            app.Run();
        }
        catch (Exception ex) when (ex is not HostAbortedException)
        {
            Log.Fatal(ex, "Unhandled exception");
        }
        finally
        {
            Log.Information("Shut down complete");
            Log.CloseAndFlush();
        }
    }
}
