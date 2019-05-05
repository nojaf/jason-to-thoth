module Tests

open Nojaf.Functions.JsonToThoth.Helper
open Nojaf.Functions.JsonToThoth.Transformer
open System
open System.Text.RegularExpressions
open Xunit

let normalizeEndings originalString = Regex.Replace(originalString, @"\r\n|\n\r|\n|\r", Environment.NewLine)
let appendNewline input = sprintf "%s%s" newLine input

let tap name x =
    printfn "%s %A" name x
    x

let expectEqualString a b =
    let a' = normalizeEndings a
    let b' = normalizeEndings b
    Assert.Equal(a', b')

[<Fact>]
let ``Simple structure with primitives``() =
    let source = """
        {"Id":7,"Name":"Foobar","Lat":51.056415557861328,"Lng":3.7224469184875488}
        """
    parse source
    |> appendNewline
    |> expectEqualString """
open System
open Thoth.Json

type Root =
    { Id: int
      Name: string
      Lat: decimal
      Lng: decimal }

    static member Decoder : Decoder<Root> =
          Decode.object
                (fun get ->
                  { Id = get.Required.Field "Id" Decode.int
                    Name = get.Required.Field "Name" Decode.string
                    Lat = get.Required.Field "Lat" Decode.decimal
                    Lng = get.Required.Field "Lng" Decode.decimal }
                )"""

[<Fact>]
let ``Nested structure``() =
    let source = """
{
  "data" : {
   "location": {"Id":7,"Name":"Foobar","Lat":51.056415557861328,"Lng":3.7224469184875488}
  }
}
"""
    parse source
    |> appendNewline
    |> expectEqualString """
open System
open Thoth.Json

type Location =
    { Id: int
      Name: string
      Lat: decimal
      Lng: decimal }

    static member Decoder : Decoder<Location> =
          Decode.object
                (fun get ->
                  { Id = get.Required.Field "Id" Decode.int
                    Name = get.Required.Field "Name" Decode.string
                    Lat = get.Required.Field "Lat" Decode.decimal
                    Lng = get.Required.Field "Lng" Decode.decimal }
                )

type Data =
    { Location: Location }

    static member Decoder : Decoder<Data> =
          Decode.object
                (fun get ->
                  { Location = get.Required.Field "location" Location.Decoder }
                )

type Root =
    { Data: Data }

    static member Decoder : Decoder<Root> =
          Decode.object
                (fun get ->
                  { Data = get.Required.Field "data" Data.Decoder }
                )"""

[<Fact>]
let ``DateTime and DateTimeOffset``() =
    let source = "{\"a\":\"2018-12-26T15:04:22.730Z\", \"b\": \"2018-12-26T14:58\"}"
    parse source
    |> appendNewline
    |> expectEqualString """
open System
open Thoth.Json

type Root =
    { A: DateTimeOffset
      B: DateTime }

    static member Decoder : Decoder<Root> =
          Decode.object
                (fun get ->
                  { A = get.Required.Field "a" Decode.datetimeOffset
                    B = get.Required.Field "b" Decode.datetime }
                )"""

[<Fact>]
let ``preserve propertyCasing``() =
    let source = "{\"npmPackages\": [\"foo\",\"bar\"]}"
    parse source
    |> appendNewline
    |> expectEqualString """
open System
open Thoth.Json

type Root =
    { NpmPackages: string array }

    static member Decoder : Decoder<Root> =
          Decode.object
                (fun get ->
                  { NpmPackages = get.Required.Field "npmPackages" Decode.array Decode.string }
                )"""

[<Fact>]
let ``empty string should parse``() =
    let source = "{ \"test\" : \"\" }"
    parse source
    |> appendNewline
    |> expectEqualString """
open System
open Thoth.Json

type Root =
    { Test: string }

    static member Decoder : Decoder<Root> =
          Decode.object
                (fun get ->
                  { Test = get.Required.Field "test" Decode.string }
                )"""
