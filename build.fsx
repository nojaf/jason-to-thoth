#r "paket:
nuget Fake.Core.Target
nuget Fake.IO.FileSystem
nuget Fake.JavaScript.Yarn
nuget Fake.DotNet.Cli //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.DotNet
open System.IO
open Fake.JavaScript

let sln = Path.Combine(__SOURCE_DIRECTORY__, "jason-to-thoth.sln")
let webFolder = Path.Combine(__SOURCE_DIRECTORY__, "src", "JasonToThoth.Web")
let web =  Path.Combine(webFolder, "JasonToThoth.Web.fsproj")
let tests = Path.Combine(__SOURCE_DIRECTORY__, "tests", "JasonToThoth.Tests", "JasonToThoth.Tests.fsproj")
let artifacts = Path.Combine(__SOURCE_DIRECTORY__, "artifacts")

// Default target
Target.create "Default" (fun _ ->
  Trace.trace "Hello World from FAKE"
)

Target.create "Clean" (fun _ ->
    !! "src/**/bin"
    ++ "src/**/obj"
    ++ "tests/**/bin"
    ++ "tests/**/obj"
    ++ artifacts
    |> Seq.iter Shell.cleanDir
)

Target.create "Restore" (fun _ ->
    DotNet.restore id sln
    Yarn.install (fun par -> { par with WorkingDirectory = webFolder })
)

Target.create "Build" (fun _ ->
    Yarn.exec "webpack -p" (fun par -> { par with WorkingDirectory = webFolder })
    
    DotNet.build (fun opt -> { opt with
                                       Configuration = DotNet.BuildConfiguration.Release
                                       Common = { opt.Common with CustomParams = Some "--no-restore" } }) sln
    
)

Target.create "Watch" (fun _ ->
    let server = async {
        let envs =
             [("ASPNETCORE_ENVIRONMENT", "Development")
              ("ASPNETCORE_URLS", "http://localhost:9700")]
             |> Map.ofList
        
        let cmd =
            Command.RawCommand("dotnet", Arguments.ofList ["watch"; "--project"; web; "run"])
            |> CreateProcess.fromCommand
            |> CreateProcess.setEnvironmentVariable "ASPNETCORE_ENVIRONMENT" "Development"
            |> CreateProcess.setEnvironmentVariable "ASPNETCORE_URLS" "http://localhost:9700"
            |> Proc.startAndAwait
        
        let! _ = cmd
        return ()
    }
    let client = async {
        Yarn.exec "webpack-dev-server" (fun opt -> { opt with WorkingDirectory = webFolder })
    }

    [ server; client ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

Target.create "Tests" (fun _ ->
    DotNet.test (fun opt -> { opt with
                                  NoBuild = true
                                  NoRestore = true
                                  Configuration = DotNet.BuildConfiguration.Release
                                  }) tests
)

Target.create "Pack" (fun _ ->
    DotNet.publish (fun opt -> { opt with NoBuild = true; OutputPath = Some artifacts }) web
)

// Build order

"Clean"
    ==> "Restore"
    ==> "Build"
    ==> "Tests"
    ==> "Pack"

"Clean"
    ==> "Restore"
    ==> "Watch"

// start build
Target.runOrDefault "Default"