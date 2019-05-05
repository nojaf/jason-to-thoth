namespace Nojaf.Functions.JsonToThoth

open System

module Helper =

  let newLine = Environment.NewLine
  let twoNewlines = sprintf "%s%s" newLine newLine

  let optionalOrRequired condition = if condition then "Optional" else "Required"

  let toThothDecoder primitiveType =
    match primitiveType with
    | t when (t = typedefof<FSharp.Data.Runtime.StructuralTypes.Bit0>) -> "int"
    | t when (t = typedefof<FSharp.Data.Runtime.StructuralTypes.Bit1>) -> "int"
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
    | _ -> failwithf "%A is not a known Thoth primitive" primitiveType
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

  /// Source: https://github.com/Humanizr/Humanizer/blob/2e45bca3d4bfc8c9ff651a32490c8e7676558f14/src/Humanizer/InflectorExtensions.cs#L71
  let pascalCase input =
    System.Text.RegularExpressions.Regex.Replace(input, "(?:^|_)(.)", fun (m: System.Text.RegularExpressions.Match) -> m.Groups.[1].Value.ToUpper());