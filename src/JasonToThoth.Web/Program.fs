module Program

open System
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open KestrelInterop.ApplicationBuilder
open System.IO

let clientPath = "wwwroot"

let configureApp (app : IApplicationBuilder) =
    app.UseFileServer() |> ignore
    app.UseFreya (Api.root) |> ignore
    
// Set urls by changing ASPNETCORE_URLS environment variable.

[<EntryPoint>]
let main argv =
    try
        let args = Array.toList argv
        let clientPath =
            match args with
            | clientPath:: _  when Directory.Exists clientPath -> clientPath
            | _ ->
                Path.Combine(".", clientPath)
            |> Path.GetFullPath

        WebHost
            .CreateDefaultBuilder()
            .UseWebRoot(clientPath)
            .UseContentRoot(clientPath)
            .Configure(Action<IApplicationBuilder> (configureApp))
            .Build()
            .Run()
        0
    with
    | exn ->
        let color = Console.ForegroundColor
        Console.ForegroundColor <- System.ConsoleColor.Red
        Console.WriteLine(exn.Message)
        Console.ForegroundColor <- color
        1
    
