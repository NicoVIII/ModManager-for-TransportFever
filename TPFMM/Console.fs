namespace TPFModManager

// Module for console use
module private ConsoleApp =
    let println = printfn "%s"

    // Help Command
    let help =
        println "Transport Fever Mod Manager (TPFMM)"
        println ""
        println "Usage:"
        println "-h | help: Displays this help page"
        println "-i <url>.. | install <url>..: Installs a mod from given url"
        println "-l | list: Shows a list of installed mods"

    let execCommand command args =
        match command with
        | "-l" | "list" -> TPFMM.List()
        | "-i" | "install" -> TPFMM.InstallAll args
        | "-h" | "help" | _ -> help