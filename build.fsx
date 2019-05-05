#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"

#if !FAKE
#r "netstandard"
// #r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif

open Fake.Core
open Fake.IO
open Fake.JavaScript
open Fake.DotNet
open Fake.Core.TargetOperators
open Fake.IO.FileSystemOperators

let clientPath = Path.getFullName "./src/Client"
let fableProject =  clientPath @@ "src" @@ "Client.fsproj"
let serverPath = Path.getFullName "./src/Server"
let functionProject = serverPath @@ "Nojaf.JasonToThoth" @@ "Nojaf.JasonToThoth.fsproj"
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

Target.create "InstallServer" (fun _ ->
    DotNet.restore id functionProject
)

Target.create "BuildServer" (fun _ ->
    DotNet.build (fun opts -> { opts with Configuration = DotNet.BuildConfiguration.Release }) functionProject
)

Target.create "Build" ignore

Target.create "Watch" (fun _ ->
    let client =
        async {
            do Yarn.exec "webpack-dev-server" (fun args -> { args with WorkingDirectory = clientPath })
        }

    let runFunc () =
        let funcTool = ProcessUtils.tryFindFileOnPath "func" |> Option.get
        let funcPath = (serverPath @@ "Nojaf.JasonToThoth" @@ "bin" @@ "Release" @@ "netcoreapp2.1" @@ "publish")
        Command.RawCommand (funcTool, ["start";"--script-root";funcPath] |> Arguments.OfArgs)
        |> CreateProcess.fromCommand
        |> CreateProcess.ensureExitCode
        |> Proc.run
        |> ignore

    let server =
        async {
            DotNet.publish (fun opts -> { opts with NoRestore = true }) functionProject
            runFunc()
        }

    Async.Parallel [client;server]
    |> Async.Ignore
    |> Async.RunSynchronously
)

"InstallClient" ==> "BuildClient"
"InstallServer" ==> "BuildServer"

"Clean"
    ==> "BuildClient"
    ==> "BuildServer"
    ==> "Build"
    
"Clean"
    ==> "InstallClient"
    ==> "InstallServer"
    ==> "Watch"

Target.runOrDefault "Build"