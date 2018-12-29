module App.Types

type Msg =
    | InputChanged of string
    | RequestTransformation
    | TransformationResponse of string
    | RequestFailed of exn

type Model =
    { Input: string
      Output: string option
      RequestPending: bool
      Error: exn option }