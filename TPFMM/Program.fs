﻿module TPFModManager.Program

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
    match System.Diagnostics.Debugger.IsAttached with
    | true ->
        main [| "install" ; "https://www.transportfever.net/filebase/index.php/Entry/2322-v200-in-verschiedenen-Versionen/" |] |> ignore
        Console.ReadKey() |> ignore
        0
    | false ->
        main args