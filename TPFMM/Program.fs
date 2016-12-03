module TPFModManager.Program

open System

let main argv = 
    let args = Array.toList argv
    match args with
    | [] -> ConsoleApp.execCommand "help" [||]
    | command::args -> ConsoleApp.execCommand command (Array.ofList args)
    0

[<EntryPoint>]
// Testing
let test args =
    #if DEBUG
    match System.Diagnostics.Debugger.IsAttached with
    | true ->
        main [| "list" |] |> ignore
        main [| "update" |] |> ignore
        main [| "upgrade" |] |> ignore
        main [| "install" ; "https://www.transportfever.net/filebase/index.php/Entry/2342-SBB-Zwergsignal/" ; "https://www.transportfever.net/filebase/index.php/Entry/2505-Wartturm-Eichenzell-by-Schwarzfahrer/" |] |> ignore
        Console.ReadKey() |> ignore
        0
    | false ->
        main args
	#else
    main args
	#endif