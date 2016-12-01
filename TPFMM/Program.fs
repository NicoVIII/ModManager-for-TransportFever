module TPFModManager.Program
    [<EntryPoint>]
    let main argv = 
        // Testing
        Console.execCommand "install" ["https://www.transportfever.net/filebase/index.php/Entry/2322-v200-in-verschiedenen-Versionen/"]

        (*let args = Array.toList argv
        match args with
        | [] -> TPFMM.List
        | command::args -> Console.execCommand command args*)
        0 // return an integer exit code