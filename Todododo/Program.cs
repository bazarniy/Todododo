using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Todododo.ViewModels;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Material;
using Blazorise.Icons.Material;
using DynamicData;
using IdGen;
using Todododo.Data;
using Todododo.Helpers;

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
            builder.Services.AddSingleton<IIdGenerator<long>>(_ => new IdGenerator(0));
            builder.Services.AddScoped<Func<Node<ToDo, long>, ToDoViewModel>>(c => node => new ToDoViewModel(node, c.GetRequiredService<TodoService>(), c.GetRequiredService<IMapper>()));
            builder.Services.AddTransient<ToDosViewModel>();

            builder.Services.AddScoped<TodoService>();
            builder.Services.AddScoped<DragAndDropContainer>();
            
            builder.Services.AddBlazoredLocalStorage();

            builder.Services.AddSingleton(new MapperConfiguration(c =>
            {
                c.AllowNullCollections = true;
                c.AllowNullDestinationValues = true;
                c.AddProfile<Mapping>();
            }).CreateMapper());

            var host = builder.Build();
            host.Services
              .UseMaterialProviders()
              .UseMaterialIcons();
            await host.RunAsync();
        }
    }
}
