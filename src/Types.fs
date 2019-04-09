module App.Types

type Msg =
    | InputChanged of string
    | RequestTransformation
    | TransformationResponse of string
    | RequestFailed of exn

type Model =
    { Input: string
      Output: string
      RequestPending: bool
      Error: exn option }