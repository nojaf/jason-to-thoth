module JasonToThoth.Core.Helper

open System
open System.Globalization

let private ti = (CultureInfo("en-US",false)).TextInfo
let pascalCase input = ti.ToTitleCase(input)

let newLine = Environment.NewLine
let twoNewlines = sprintf "%s%s" newLine newLine

let optionalOrRequired condition = if condition then "Optional" else "Required"

let toThothDecoder primitiveType =
  match primitiveType with
  | t when (t = typedefof<int>) -> "int"
  | t when (t = typedefof<string>) -> "string"
  | t when (t = typedefof<Guid>) -> "guid"
  | t when (t = typedefof<int64>) -> "int64"
  | t when (t = typedefof<uint64>) -> "uint64"
  | t when (t = typedefof<bigint>) -> "bigint"
  | t when (t = typedefof<bool>) -> "bool"
  | t when (t = typedefof<float>) -> "float"
  | t when (t = typedefof<decimal>) -> "decimal"
  | t when (t = typedefof<DateTime>) -> "datetime"
  | t when (t = typedefof<DateTimeOffset>) -> "datetimeOffset"
  | _ -> failwith "Not a known Thoth primitive"
  |> sprintf "Decode.%s"

// https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/basic-types

let toSimpleType (t:Type) =
  match t with
  | t when (t = typedefof<bool>) -> "bool"
  | t when (t = typedefof<byte>) -> "byte"
  | t when (t = typedefof<sbyte>) -> "sbyte"
  | t when (t = typedefof<int16>) -> "int16"
  | t when (t = typedefof<uint16>) -> "uint16"
  | t when (t = typedefof<int>) -> "int"
  | t when (t = typedefof<uint32>) -> "uint32"
  | t when (t = typedefof<int64>) -> "int64"
  | t when (t = typedefof<uint64>) -> "uint64"
  | t when (t = typedefof<nativeint>) -> "nativeint"
  | t when (t = typedefof<unativeint>) -> "unativeint"
  | t when (t = typedefof<char>) -> "char"
  | t when (t = typedefof<string>) -> "string"
  | t when (t = typedefof<decimal>) -> "decimal"
  | t when (t = typedefof<single>) -> "single"
  | t when (t = typedefof<double>) -> "double"
  | _ -> t.Name