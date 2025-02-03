namespace HowIdentity;

using Data.Seeds;
using Infrastructure.Scheduler;
using Infrastructure.Scheduler.Jobs;
using Quartz;
using Serilog;

public static class Program
{
    public static async Task Main(string[] args)
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
                .ConfigureMassTransit()
                .ConfigureGrpcServices()
                .ConfigureScheduler()
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

            var scheduler = app.Services.GetRequiredService<AppJobScheduler>();
            // var schedulerFactory = app.Services.GetRequiredService<ISchedulerFactory>();
            // var scheduler = await schedulerFactory.GetScheduler();
            
            // define the job and tie it to our HelloJob class
            var job = JobBuilder.Create<TestJob>()
                .WithIdentity("myJob", "group1")
                .Build();

            // Trigger the job to run now, and then every 40 seconds
            var trigger = TriggerBuilder.Create()
                .WithIdentity("myTrigger", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(40)
                    .RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
            
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
