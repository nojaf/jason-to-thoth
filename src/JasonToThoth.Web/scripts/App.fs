module App.Entry

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Elmish
open App


#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram State.init State.update Views.view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
|> Elmish.HMR.Program.run
#else
|> Program.run
#endif
