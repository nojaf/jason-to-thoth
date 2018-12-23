module KestrelInterop

open Freya.Core
open Microsoft.AspNetCore.Builder

module ApplicationBuilder =
    type IApplicationBuilder with
        member this.UseFreya (f:Freya.Core.Pipeline) =
            let owin : OwinMidFunc = OwinMidFunc.ofFreya f
            this.UseOwin(fun p -> p.Invoke owin) |> ignore
            
        member this.UseFreya (f:Freya.Routers.Uri.Template.UriTemplateRouter) =
            let owin : OwinMidFunc = OwinMidFunc.ofFreya f
            this.UseOwin(fun p -> p.Invoke owin) |> ignore