module App.Views

open App.Types
open Fable.Helpers.React
open Fulma

let view (model: Model) dispatch =
    let output = Option.map (fun output -> pre [] [str output]) model.Output
    
    div [] [
        Columns.columns [Columns.IsGapless] [
            Column.column [] [
                Textarea.textarea [
                    Textarea.CustomClass "mh440"
                    Textarea.Placeholder "Paste some JSON and hope for the best."
                    Textarea.Value model.Input
                    Textarea.OnChange (fun ev -> dispatch (InputChanged ev.Value))
                ] []
            ]
            Column.column [] [
                Notification.notification [Notification.CustomClass "mh440"] [
                    ofOption output
                ]
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
