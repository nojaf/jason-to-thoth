namespace JasonToThoth.Core

open System.Globalization
open FSharp.Data
open FSharp.Data.Runtime.StructuralTypes
open ProviderImplementation.JsonInference
open JasonToThoth.Core.Helper

module Transformer =
    let private (|IRecord|_|) inferedType =
      match inferedType with
      | InferedType.Record(n, p, o) as r -> Some (n, p, o)
      | _ -> None
    
    let rec private collectAllRecords rootInferedType =
      match rootInferedType with
      | InferedType.Record (n,p,o) as r ->
        let children =
          p
          |> List.map (fun prop -> collectAllRecords prop.Type)
          |> List.collect id
          
        r::children
        
      | InferedType.Collection(o, types) ->
        types
        |> Map.toList
        |> List.collect (snd >> snd >> collectAllRecords)
    
      | _ -> []
      
    let private parseProperty (prop: InferedProperty) =
        let name = pascalCase prop.Name
        match prop.Type with
        | InferedType.Record(n, _, isOptional) -> (name, name)
        | InferedType.Primitive(t,_,isOptional) -> (name, toSimpleType t)
        | InferedType.Collection(order, types) ->
            order
            |> List.tryHead
            |> Option.map (fun pt ->
              match pt with
              | InferedTypeTag.Record(Some(n)) -> (name, pascalCase n |> sprintf "%s array")
              | InferedTypeTag.Guid -> (name, "Guid array")
              | InferedTypeTag.String -> (name, "string array")
              | _ -> (name, "not supported array")
            )
            |> Option.defaultValue (name, "obj array")
              
                
        | _ -> ("not","supported")
        |> fun (k,v) -> sprintf "%s: %s" k v
        
    let private parseDecoderOfType name (ifType: InferedType) =
      let propertyName = pascalCase name
      
      match ifType with
      | InferedType.Record(n, _, isOptional) ->
        let staticDecoder = sprintf "%s.Decoder" propertyName
        sprintf "%s = get.%s.Field \"%s\" %s" propertyName (optionalOrRequired isOptional) name staticDecoder
        
      | InferedType.Primitive(t,_, opt) ->
        sprintf "%s = get.%s.Field \"%s\" %s" propertyName (optionalOrRequired opt) name (toThothDecoder t)
        
      | InferedType.Collection(o,types) ->
        let decoder =
          types
          |> Map.toList
          |> List.tryHead
          |> Option.map (fun (_, (mul, ifType)) ->
            match ifType with
            | InferedType.Record(Some(n),_,_) -> sprintf "%s.Decoder" (pascalCase n)
            | InferedType.Primitive(t,_,_) -> toThothDecoder t
            | _ -> failwith "Not implemented yet"
          )
          |> Option.defaultValue "failWith"
          |> sprintf "Decode.array %s"
          
          
        sprintf "%s = get.Required.Field \"%s\" %s" propertyName name decoder
        
      | _ -> "not supported"
      
    let private parseType name fields =
      let name =
        name
        |> Option.map pascalCase
        |> Option.defaultValue "Root"
      
      let properties =
        fields
        |> List.map parseProperty
        |> String.concat (sprintf "%s      " newLine)
        
      let decoder =
        let properties =
          fields
          |> List.map (fun prop -> parseDecoderOfType prop.Name prop.Type)
          |> String.concat (sprintf "%s                    " newLine)
          
        sprintf """static member Decoder : Decoder<%s> =
          Decode.object
                (fun get ->
                  { %s }
                )""" name properties
    
      sprintf "type %s =%s    { %s }%s    %s" name newLine properties twoNewlines decoder
     
    let parse json : string =
        JsonValue.Parse(json)
        |> inferType true (CultureInfo.InvariantCulture) "Root"
        |> collectAllRecords
        |> List.map (function | (IRecord r) -> Some r | _ -> None)
        |> List.choose id
        |> List.rev
        |> List.map (fun (n,f,_) -> parseType n f)
        |> String.concat (twoNewlines)
        |> sprintf "open System%sopen Thoth.Json%s%s" newLine twoNewlines
