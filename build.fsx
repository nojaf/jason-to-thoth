#r "nuget: Fun.Build, 0.3.8"

open System.IO
open Fun.Build

let sln = "jason-to-thoth.sln"
let (</>) a b = Path.Combine(a, b)
let clientPath = Path.GetFullPath "./src/Client"
let fableProject = clientPath </> "src" </> "Client.fsproj"
let serverPath = Path.GetFullPath "./src/Server"

let functionProject =
    serverPath </> "Nojaf.JasonToThoth" </> "Nojaf.JasonToThoth.fsproj"

let testPath = Path.GetFullPath "./tests"

let testProject =
    testPath </> "Nojaf.JasonToThoth.Tests" </> "Nojaf.JasonToThoth.Tests.fsproj"

let artifactsPath = Path.GetFullPath "./artifacts"

let cleanFolders (input: string seq) =
    async {
        input
        |> Seq.iter (fun dir ->
            if Directory.Exists(dir) then
                Directory.Delete(dir, true))
    }

pipeline "Build" {
    workingDir __SOURCE_DIRECTORY__
    stage "Clean" { run (fun _ -> cleanFolders [ artifactsPath ]) }

    stage "Install Client" {
        workingDir clientPath
        run "yarn"
    }

    stage "Restore" { run "dotnet restore" }

    // stage "Build Client" {
    //     workingDir clientPath
    //     run "yarn build"
    // }

    stage "Build Server" { run "dotnet build -c Release" }

    stage "Tests" { run "dotnet test -c Release --no-build --no-restore" }

    runIfOnlySpecified false
}
