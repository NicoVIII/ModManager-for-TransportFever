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
        main [| "install" ; "https://www.transportfever.net/filebase/index.php/Entry/2322-v200-in-verschiedenen-Versionen/" ; "https://www.transportfever.net/filebase/index.php/Entry/2448-DMA-Modern-Buses/" ; "https://www.transportfever.net/filebase/index.php/Entry/2362-Bremswagon-Zugbooster/" |] |> ignore
        main [| "list" |] |> ignore
        Console.ReadKey() |> ignore
        0
    | false ->
        main args
    #else
    main args
    #endif