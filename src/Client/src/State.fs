module App.State

open App.Types
open Elmish
open Fetch
open Fable.Core.JsInterop

let init _ =
    { Input = "{ \"Foo\":\"Bar\" }"
      Output = ""
      RequestPending = false
      Error = None }, Cmd.none
    
let transform (input:string) =
    let url = "https://nojaf-functions.azurewebsites.net/api/JsonToThoth"
    Fetch.fetch url [RequestProperties.Body (!^ input); RequestProperties.Method HttpMethod.POST]
    |> Promise.bind (fun res -> res.text())

let update msg model =
    match msg with
    | InputChanged v ->
        { model with Input = v }, Cmd.none
    | RequestTransformation ->
        { model with RequestPending = true }, Cmd.OfPromise.either transform (model.Input) Msg.TransformationResponse Msg.RequestFailed
    | RequestFailed exn ->
        { model with Error = Some exn }, Cmd.none
    | TransformationResponse response ->
        { model with Output = response }, Cmd.none