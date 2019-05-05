module App.Entry

open Elmish
open App


#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram State.init State.update Views.view
#if DEBUG
|> Program.withConsoleTrace
|> Program.withReactBatched "elmish-app"
#else
|> Elmish.React.Program.withReact "elmish-app"
#endif
#if DEBUG
|> Program.withDebugger
|> Elmish.HMR.Program.run
#else
|> Program.run
#endif
