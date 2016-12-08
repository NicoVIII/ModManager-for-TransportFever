module TPFModManager_ConsoleGui.Program

open System

let main argv = 
    let args = Array.toList argv
    match args with
    | [] -> ConsoleApp.execCommand "help" []
    | command::args -> ConsoleApp.execCommand command args
    0

[<EntryPoint>]
// Testing
let test args =
    #if DEBUG
    match System.Diagnostics.Debugger.IsAttached with
    | true ->
        main [| "schwachsinn" |] |> ignore
        printfn "" |> ignore
        main [| "list" |] |> ignore
        printfn "" |> ignore
        main [| "update" |] |> ignore
        printfn "" |> ignore
        main [| "install" ; "https://www.transportfever.net/filebase/index.php/Entry/2342-SBB-Zwergsignal/" ; "https://www.transportfever.net/filebase/index.php/Entry/2505-Wartturm-Eichenzell-by-Schwarzfahrer/" ; "https://www.transportfever.net/filebase/index.php/Entry/2396-kkStB-280-380/" ; "https://www.transportfever.net/filebase/index.php/Entry/2418-DB-BR-110-BR-140-BR-141-BR-150-Einheitsloks-der-Deutschen-Bundesbahn/" ; "https://www.transportfever.net/filebase/index.php/Entry/2396-kkStB-280-380/" |] |> ignore
        printfn "" |> ignore
        main [| "list" |] |> ignore
        printfn "" |> ignore
        main [| "update" |] |> ignore
        printfn "" |> ignore
        main [| "upgrade" |] |> ignore
        printfn "" |> ignore
        main [| "list" |] |> ignore
        printfn "" |> ignore
        main [| "update" |] |> ignore
        Console.ReadKey() |> ignore
        0
    | false ->
        main args
	#else
    main args
	#endif