open Fake.DotNet

#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"

#if !FAKE
#r "netstandard"
// #r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif

open Fake.Core
open Fake.IO
open Fake.JavaScript
open System.IO

let clientPath = Path.getFullName "./src/Client"
let fableProject =  Path.Combine(clientPath, "src", "Client.fsproj")
let artifactsPath = Path.getFullName "./artifacts"

Target.create "Id" (fun _ -> printfn "Installing Fake stuff")

Target.create "Clean" (fun _ ->
    Shell.cleanDirs [artifactsPath]
)

Target.create "InstallClient" (fun _ ->
    Yarn.installPureLock (fun args -> { args with WorkingDirectory = clientPath })
    DotNet.restore id fableProject
)

Target.create "BuildClient" (fun _ ->
    Yarn.exec "build" (fun args -> { args with WorkingDirectory = clientPath })
)

Target.create "DeployClient" (fun _ ->
    Yarn.exec "deploy" (fun args -> { args with WorkingDirectory = clientPath })
)

Target.create "Watch" (fun _ ->
    let client =
        async {
            do Yarn.exec "webpack-dev-server" (fun args -> { args with WorkingDirectory = clientPath })
        }
        
    Async.Parallel [client]
    |> Async.Ignore
    |> Async.RunSynchronously
)

Target.runOrDefault "Build"