namespace TodoFs.Api

open System.Text.Json.Serialization
open TodoFs.Api.Data
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

type Startup private () =
    new(configuration: IConfiguration) as this =
        Startup() then
        this.Configuration <- configuration

    // This method gets called by the runtime. Use this method to add services to the container.
    member this.ConfigureServices(services: IServiceCollection) =
        // Add framework services.
        services.AddControllers()
            .AddNewtonsoftJson()
            .AddJsonOptions(fun o -> o.JsonSerializerOptions.Converters.Add(JsonStringEnumConverter())) |> ignore
        services.AddMemoryCache() |> ignore
        services.AddLogging() |> ignore
        services.AddSingleton<TodosRepository>() |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IHostEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore
        else
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts() |> ignore

        app.UseRouting() |> ignore
        app.UseEndpoints(fun endpoints ->
            endpoints.MapControllers() |> ignore
        ) |> ignore
        app.UseHttpsRedirection() |> ignore

    member val Configuration: IConfiguration = null with get, set
