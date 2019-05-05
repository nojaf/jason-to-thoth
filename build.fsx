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

let sln = "jason-to-thoth.sln"
let clientPath = Path.getFullName "./src/Client"
let fableProject =  clientPath @@ "src" @@ "Client.fsproj"
let serverPath = Path.getFullName "./src/Server"
let functionProject = serverPath @@ "Nojaf.JasonToThoth" @@ "Nojaf.JasonToThoth.fsproj"
let testPath = Path.getFullName "./tests"
let testProject = testPath @@ "Nojaf.JasonToThoth.Tests" @@ "Nojaf.JasonToThoth.Tests.fsproj"
let artifactsPath = Path.getFullName "./artifacts"

Target.create "Id" (fun _ -> printfn "Installing Fake stuff")

Target.create "Clean" (fun _ ->
    Shell.cleanDirs [artifactsPath]
)

Target.create "Install" (fun _ ->
    Yarn.installPureLock (fun args -> { args with WorkingDirectory = clientPath })
    DotNet.restore id sln
)

Target.create "BuildClient" (fun _ ->
    Yarn.exec "build" (fun args -> { args with WorkingDirectory = clientPath })
)

Target.create "DeployClient" (fun _ ->
    Yarn.exec "deploy" (fun args -> { args with WorkingDirectory = clientPath })
)

Target.create "BuildServer" (fun _ ->
    DotNet.build (fun opts -> { opts with Configuration = DotNet.BuildConfiguration.Release }) functionProject
    DotNet.build (fun opts -> { opts with Configuration = DotNet.BuildConfiguration.Release }) testProject
)

Target.create "Tests" (fun _ ->
    DotNet.test (fun opts -> { opts with NoRestore = true; NoBuild = true; Configuration = DotNet.BuildConfiguration.Release }) testProject
)

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

"Install" ==> "BuildClient"
"Install" ==> "BuildServer"

"Clean"
    ==> "BuildClient"
    ==> "BuildServer"
    ==> "Tests"
    
"Clean"
    ==> "Install"
    ==> "Watch"
    
"BuildClient"
    ==> "DeployClient"

Target.runOrDefault "Tests"