module ReactEditor

open Fable.Core.JsInterop
open Fable.Import
open Fable.Core
open Fable.Helpers.React

module Editor =

    type Props =
        | OnChange of (string -> unit)
        | Value of string
        | Language of string
        | IsReadOnly of bool

    let inline editor (props: Props list) : React.ReactElement =
        ofImport "default" "./js/Editor.js" (keyValueList CaseRules.LowerFirst props) []
