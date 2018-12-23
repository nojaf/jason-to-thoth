module Api

open Freya.Core
open Freya.Machines.Http
open Freya.Types.Http
open Freya.Routers.Uri.Template
open Newtonsoft.Json
open System
open Freya.Machines
open Freya.Optics.Http
open System.IO

let name_ = Route.atom_ "name"

let name =
    freya {
        let! nameO = Freya.Optic.get name_

        match nameO with
        | Some name -> return name
        | None -> return "World" }

let sayHello =
    freya {
        let! name = name

        return Represent.text (sprintf "Hello, %s!" name) }

let helloMachine =
    freyaMachine {
        methods [GET; HEAD; OPTIONS]
        handleOk sayHello }
    
// JSON helpers
let toJson value =
    JsonConvert.SerializeObject(value)

//let ofJson<'a> (json:string) : 'a =
//    JsonConvert.DeserializeObject<'a>(json)

let json<'a> value =
    let data =
        JsonConvert.SerializeObject(value)
        |> Text.UTF8Encoding.UTF8.GetBytes
    let desc =
        { Encodings = None
          Charset = Some Charset.Utf8
          Languages = None
          MediaType = Some MediaType.Json }
    { Data = data
      Description = desc }
    
let readBody =
    Freya.Optic.get Request.body_
    |> Freya.map (fun body ->
        using(new StreamReader (body))(fun reader -> 
            reader.ReadToEnd ()
        )
    )

//let readJson<'t> =
//    readBody
//    |> Freya.map (ofJson<'t>)
    
let transformJson = freya {
    let! json = readBody
    let code = JasonToThoth.Core.Transformer.parse json
    return Represent.text code
}

let transformMachine =
    freyaMachine {
        methods [POST]
        handleOk transformJson
    }

let root =
    freyaRouter {
        resource "/api/transform" transformMachine
        resource "/hello{/name}" helloMachine }
