using System;
using System.Net.Http;
using System.Threading.Tasks;
using Todododo.ViewModels;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Material;
using Blazorise.Icons.Material;

namespace Todododo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services
                .AddBlazorise(options =>
                {
                    options.ChangeTextOnKeyPress = true;
                })
                .AddMaterialProviders()
                .AddMaterialIcons();
            builder.RootComponents.Add<App>("app");

            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSingleton<FetchDataViewModel>();

            builder.Services.AddBlazoredLocalStorage();

            var host = builder.Build();
            host.Services
              .UseMaterialProviders()
              .UseMaterialIcons();
            await host.RunAsync();
        }
    }
}
