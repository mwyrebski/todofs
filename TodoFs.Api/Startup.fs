namespace TodoFs.Api

open TodoFs.Api.Data
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Newtonsoft.Json.Converters

type Startup private () =
    new(configuration: IConfiguration) as this =
        Startup() then
        this.Configuration <- configuration

    // This method gets called by the runtime. Use this method to add services to the container.
    member this.ConfigureServices(services: IServiceCollection) =
        // Add framework services.
        services.AddMvc()
            .AddJsonOptions(fun x -> x.SerializerSettings.Converters.Add(new StringEnumConverter()))
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2) |> ignore
        services.AddMemoryCache() |> ignore
        services.AddLogging() |> ignore
        services.AddSingleton<TodosRepository>() |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IHostingEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore
        else
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts() |> ignore

        app.UseHttpsRedirection() |> ignore
        app.UseMvc() |> ignore

    member val Configuration: IConfiguration = null with get, set
