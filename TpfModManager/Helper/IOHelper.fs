namespace TpfModManager

open System.IO

module private IOHelper =
    let pipePrintfn regex arg =
        printfn regex arg
        arg

    let saveString (file :string) str =
        let directory =
            file.Split [| '/' |]
            |> Array.toList
            |> List.rev
            |> List.tail
            |> List.rev
            |> List.fold (fun path folder -> path+folder+"/") ""
        if directory.Length = 0 then
            ()
        else
            Directory.CreateDirectory directory
            |> ignore
        File.WriteAllText(file, str)