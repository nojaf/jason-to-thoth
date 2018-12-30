module App.Views

open App.Types
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open ReactEditor

let headers =
    Columns.columns [ Columns.IsGapless ; Columns.IsMultiline ; Columns.CustomClass "is-gapless" ]
                    [ Column.column [ Column.Width(Screen.All, Column.IsHalf) ]
                        [ Message.message [ Message.Props [Id "input-message"] ]
                            [ Message.body [ ]
                                [ Text.div [ Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                                    [ str "Type or paste JSON" ] ] ] ]
                      Column.column [ Column.Width(Screen.All, Column.IsHalf) ]
                        [ Message.message [ Message.Props [Id "formatted-message"] ]
                            [ Message.body [ ]
                                [ Text.div [ Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                                    [ str "F# code with Thoth decoders." ] ] ] ] ]


let view (model: Model) dispatch =
    div [] [
        headers
        Columns.columns [Columns.IsGapless; Columns.IsMultiline ; Columns.CustomClass "is-gapless"] [
            Column.column [] [
                Editor.editor [ Editor.Language "json"
                                Editor.IsReadOnly false
                                Editor.Value model.Input
                                Editor.OnChange (InputChanged >> dispatch) ]
            ]
            Column.column [] [
                Editor.editor [ Editor.Language "fsharp"
                                Editor.IsReadOnly false
                                Editor.Value model.Output
                                Editor.OnChange (InputChanged >> dispatch) ]
            ]
        ]
        Columns.columns [Columns.IsGapless] [
            Text.div [Modifiers [Modifier.TextAlignment (Screen.All, TextAlignment.Centered)]; CustomClass "column"] [
                Button.button [Button.Color IsPrimary; Button.OnClick (fun _ -> dispatch Msg.RequestTransformation)] [
                    str "Transform"
                ]
            ]
        ]
    ]
