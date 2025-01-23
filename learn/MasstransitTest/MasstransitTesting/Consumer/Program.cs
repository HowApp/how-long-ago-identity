using System.Reflection;
using Consumer.Consumers;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<HelloMessageConsumer, HelloMessageConsumerDefinition>();

    // var entryAssembly = Assembly.GetEntryAssembly();
    // x.AddConsumers(entryAssembly);
    
    x.UsingRabbitMq((context, config) =>
    {
        config.Host("localhost", "/", host =>
        {
            host.Username("guest");
            host.Password("guest");
        });

        // config.ConfigureEndpoints(context);

        config.ReceiveEndpoint("sample-queue", e =>
        {
            e.ConfigureConsumer<HelloMessageConsumer>(context);
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();