using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SmartishTable.Samples.Client;

public class Program
{
  public static async Task Main(string[] args)
  {
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>("#app");

    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

    builder.Services.AddBlazoredLocalStorage(config =>
    {
      config.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

    if (builder.HostEnvironment.IsDevelopment())
    {
      builder.Logging.SetMinimumLevel(LogLevel.Debug);
      // filters out Microsoft logs that aren't warning or higher
      builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
    }
    else
      builder.Logging.SetMinimumLevel(LogLevel.Warning);

    await builder.Build().RunAsync();
  }
}
