using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// configure controllers and JSON options to avoid object cycles when EF navigation properties reference each other
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
// register DbContext
builder.Services.AddDbContext<BTC_ReconciliationAutomation.Server.Models.OracleDbContext>(opts =>
    opts.UseOracle(builder.Configuration.GetConnectionString("OracleDb")));

// register repositories
builder.Services.AddScoped<BTC_ReconciliationAutomation.Server.Repositories.Interfaces.IReconciliationRunRepository, BTC_ReconciliationAutomation.Server.Repositories.Implementation.ReconciliationRunRepository>();
builder.Services.AddScoped<BTC_ReconciliationAutomation.Server.Repositories.Interfaces.ISummaryRepository, BTC_ReconciliationAutomation.Server.Repositories.Implementation.SummaryRepository>();
builder.Services.AddScoped<BTC_ReconciliationAutomation.Server.Repositories.Interfaces.IFileRepository, BTC_ReconciliationAutomation.Server.Repositories.Implementation.FileRepository>();
builder.Services.AddScoped<BTC_ReconciliationAutomation.Server.Repositories.Interfaces.ISystemLogRepository, BTC_ReconciliationAutomation.Server.Repositories.Implementation.SystemLogRepository>();
builder.Services.AddScoped<BTC_ReconciliationAutomation.Server.Repositories.Interfaces.IConfigurationRepository, BTC_ReconciliationAutomation.Server.Repositories.Implementation.ConfigurationRepository>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // The project is configured to use the SpaProxy support via the
    // Microsoft.AspNetCore.SpaProxy hosting startup assembly. The SPA proxy is
    // enabled by the `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES` environment
    // variable in `launchSettings.json` and the `SpaProxyLaunchCommand` in the
    // .csproj. When debugging, this will automatically run the client `npm run dev`
    // command and proxy unknown requests to the frontend dev server.

    // Try to open both the SPA root and the Swagger UI once the server has
    // started. This is best-effort and failures are ignored.
    var addressesFeature = app.Services.GetService<IServerAddressesFeature>();
    var chosenAddress = addressesFeature?.Addresses.FirstOrDefault(a => a.StartsWith("https"))
                        ?? addressesFeature?.Addresses.FirstOrDefault();
    if (!string.IsNullOrEmpty(chosenAddress))
    {
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = chosenAddress, UseShellExecute = true });
                var swaggerUrl = chosenAddress.TrimEnd('/') + "/swagger";
                Process.Start(new ProcessStartInfo { FileName = swaggerUrl, UseShellExecute = true });
            }
            catch
            {
                // ignore failures to open the browser
            }
        });
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
