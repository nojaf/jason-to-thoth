module ReactEditor

open Fable.Core.JsInterop
open Fable.Core
open Fable.React

module Editor =

    type Props =
        | OnChange of (string -> unit)
        | Value of string
        | Language of string
        | IsReadOnly of bool

    let inline editor (props: Props list) : ReactElement =
        ofImport "default" "./js/Editor.js" (keyValueList CaseRules.LowerFirst props) []
