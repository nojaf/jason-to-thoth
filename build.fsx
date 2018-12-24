#r "paket:
nuget Fake.Core.Target
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.IO.FileSystemOperators
open Fake.DotNet
open System.IO

let sln = Path.Combine(__SOURCE_DIRECTORY__, "jason-to-thoth.sln")
let web = Path.Combine(__SOURCE_DIRECTORY__, "src", "JasonToThoth.Web" , "JasonToThoth.Web.fsproj")
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
)

Target.create "Build" (fun _ ->
    DotNet.build (fun opt -> { opt with
                                       Configuration = DotNet.BuildConfiguration.Release
                                       Common = { opt.Common with CustomParams = Some "--no-restore" } }) sln
)

Target.create "Pack" (fun _ ->
    DotNet.publish (fun opt -> { opt with NoBuild = true; OutputPath = Some artifacts }) web
)

// Build order

"Clean"
    ==> "Restore"
    ==> "Build"
    ==> "Pack"

// start build
Target.runOrDefault "Default"