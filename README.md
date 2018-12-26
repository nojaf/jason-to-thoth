# JSON to Thoth

> Convert a JSON snippet to a F# record with a Thoth decode function.

![Red Hood](logo.png)

Check it out [online](https://nojaf.com/redhood/)

## Development

Install Paket .NETCore

> dotnet tool install --tool-path ".paket" Paket --add-source https://api.nuget.org/v3/index.json

Install FAKE .NETCore

> dotnet tool install fake-cli --tool-path .fake

Build with `.fake\fake.exe run build.fsx -t Build`.
Or `.fake/fake run build.fsx -t Build` on Unix.

## How to contribute

Create a unit test in `JasonToThoth.Tests` where you illustrate what you want to fix.
Open an issue first if you are not sure whether your change will be accepted.