namespace Nojaf.Functions.JsonToThoth

open Microsoft.AspNetCore.Http
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.Extensions.Logging
open System.IO
open System.Net
open System.Net.Http
open Nojaf.Functions.JsonToThoth

module Function =

    [<FunctionName("JsonToThoth")>]
    let Run([<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)>] req: HttpRequest, log: ILogger) =
        log.LogInformation("F# HTTP trigger function processed a request.")
        let content = using (new StreamReader(req.Body)) (fun stream -> stream.ReadToEnd())
        let code = Transformer.parse content
        new HttpResponseMessage(HttpStatusCode.OK, Content = new StringContent(code, System.Text.Encoding.UTF8, "text/plain"))
 