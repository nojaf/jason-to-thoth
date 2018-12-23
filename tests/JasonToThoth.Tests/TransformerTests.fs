module Tests

open JasonToThoth.Core.Helper
open System
open Xunit
open JasonToThoth.Core.Transformer

let appendNewline input =
    sprintf "%s%s" newLine input
    
let tap name x =
    printfn "%s %A" name x
    x

let expectEqualString a b =
    Assert.Equal(a,b)

[<Fact>]
let ``Simple structure with primitives`` () =
    let source =
        """
        {"Id":7,"Name":"Het Waterhuis aan de Bierkant","Lat":51.056415557861328,"Lng":3.7224469184875488}
        """

    parse source
    |> appendNewline
    |> expectEqualString """
open System
open Thoth.Json

type Root =
    { Id: Int32
      Name: String
      Lat: Decimal
      Lng: Decimal }

    static member Decoder : Decoder<Root> =
          Decode.object
                (fun get ->
                  { Id = get.Required.Field "Id" Decode.int
                    Name = get.Required.Field "Name" Decode.string
                    Lat = get.Required.Field "Lat" Decode.decimal
                    Lng = get.Required.Field "Lng" Decode.decimal }
                )"""
