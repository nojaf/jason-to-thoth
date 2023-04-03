module Tests

open NUnit.Framework
open Nojaf.Functions.JsonToThoth.Helper
open Nojaf.Functions.JsonToThoth.Transformer
open System
open System.Text.RegularExpressions

let normalizeEndings originalString =
    Regex.Replace(originalString, @"\r\n|\n\r|\n|\r", Environment.NewLine)

let appendNewline input = sprintf "%s%s" newLine input

let tap name x =
    printfn "%s %A" name x
    x

let expectEqualString a b =
    let a' = normalizeEndings a
    let b' = normalizeEndings b
    Assert.AreEqual(a', b')

[<Test>]
let ``Simple structure with primitives`` () =
    let source =
        """
        {"Id":7,"Name":"Foobar","Lat":51.056415557861328,"Lng":3.7224469184875488}
        """

    parse source
    |> appendNewline
    |> expectEqualString
        """
namespace rec Jason.Thoth

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

[<Test>]
let ``Nested structure`` () =
    let source =
        """
{
  "data" : {
   "location": {"Id":7,"Name":"Foobar","Lat":51.056415557861328,"Lng":3.7224469184875488}
  }
}
"""

    parse source
    |> appendNewline
    |> expectEqualString
        """
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

[<Test>]
let ``DateTime and DateTimeOffset`` () =
    let source = "{\"a\":\"2018-12-26T15:04:22.730Z\", \"b\": \"2018-12-26T14:58\"}"

    parse source
    |> appendNewline
    |> expectEqualString
        """
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

[<Test>]
let ``preserve propertyCasing`` () =
    let source = "{\"npmPackages\": [\"foo\",\"bar\"]}"

    parse source
    |> appendNewline
    |> expectEqualString
        """
open System
open Thoth.Json

type Root =
    { NpmPackages: string array }

    static member Decoder : Decoder<Root> =
          Decode.object
                (fun get ->
                  { NpmPackages = get.Required.Field "npmPackages" Decode.array Decode.string }
                )"""

[<Test>]
let ``empty string should parse`` () =
    let source = "{ \"test\" : \"\" }"

    parse source
    |> appendNewline
    |> expectEqualString
        """
open System
open Thoth.Json

type Root =
    { Test: string }

    static member Decoder : Decoder<Root> =
          Decode.object
                (fun get ->
                  { Test = get.Required.Field "test" Decode.string }
                )"""

[<Test>]
let ``array of objects`` () =
    let source =
        """
 {
    "ronnies": [
      {
        "id": 1,
        "name": "Afsnis"
      },
      {
        "id": 2,
        "name": "Het Spijker"
      }
    ]
  }
"""

    parse source
    |> appendNewline
    |> expectEqualString
        """
open System
open Thoth.Json

type Ronny =
    { Id: int
      Name: string }

    static member Decoder : Decoder<Ronny> =
          Decode.object
                (fun get ->
                  { Id = get.Required.Field "id" Decode.int
                    Name = get.Required.Field "name" Decode.string }
                )

type Root =
    { Ronnies: Ronny array }

    static member Decoder : Decoder<Root> =
          Decode.object
                (fun get ->
                  { Ronnies = get.Required.Field "ronnies" (Decode.array Ronny.Decoder) }
                )"""
