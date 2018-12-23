module Program

open System
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open KestrelInterop.ApplicationBuilder
open System.IO

let clientPath = "wwwroot"
let port = 5000

let configureApp (app : IApplicationBuilder) =
    app.UseFileServer() |> ignore
    app.UseFreya (Api.root) |> ignore

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
            .UseUrls("http://0.0.0.0:" + port.ToString() + "/")
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
    
