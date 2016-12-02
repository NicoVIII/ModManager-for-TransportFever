namespace TPFModManager

// Module for console use
module private ConsoleApp =
    let printCmd short long desc= printfn "%-14s %-20s %s" short long desc

    // Help Command
    let help =
        printfn "Transport Fever Mod Manager (TPFMM)"
        printfn ""
        printfn "Usage:"
        printCmd "-h" "help" "Displays this help page"
        printCmd "-i <url>.." "install <url>.." "Installs a mod from given url"
        printCmd "-l" "list" "Shows a list of installed mods"

    let execCommand command args =
        match command with
        | "-l" | "list" -> TPFMM.List()
        | "-i" | "install" ->
            printfn "Uploading of downloaded mods to other sites is prohibited!\n"
            TPFMM.InstallAll args
            printfn "Uploading of downloaded mods to other sites is prohibited!"
        | "-h" | "help" | _ -> help