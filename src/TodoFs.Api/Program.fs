namespace TodoFs.Api

open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting

module Program =
    let exitCode = 0

    let CreateWebHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(
                fun b -> b.UseStartup<Startup>() |> ignore
            )

    [<EntryPoint>]
    let main args =
        CreateWebHostBuilder(args).Build().Run()

        exitCode
