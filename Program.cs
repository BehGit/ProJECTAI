using Microsoft.EntityFrameworkCore;
using ProjectAI.Data;
using ProjectAI.Services;
using Scalar.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Polly;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
//builder.Services.AddSwaggerGen();

var configuration = builder.Configuration;

builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddHttpClient<ImageDownloader>();

var handler = new HttpClientHandler();
handler.ServerCertificateCustomValidationCallback =
    (message, cert, chain, sslPolicyErrors) => true;

var client = new HttpClient(handler);

builder.Services.AddHttpClient("MyClient")
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                (sender, cert, chain, sslPolicyErrors) => true
        });

//builder.Services.AddHttpClient<IAiService, GigaChatAiService>();

builder.Services.AddSingleton<IAiService, MockAiService>();

//builder.Services.AddHttpClient<GigaChatAiService>()
//                .AddTransientHttpErrorPolicy(policy =>
//                    policy.WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(Math.Pow(2, retry))));
//builder.Services.AddSingleton<IAiService, GigaChatAiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.Run();
