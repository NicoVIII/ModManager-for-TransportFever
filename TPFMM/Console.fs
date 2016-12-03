namespace TPFModManager

// Module for console use
module private ConsoleApp =
    let printCmd long desc= printfn "%-10s %s" long desc

    let help () =
        printfn "Transport Fever Mod Manager (TPFMM)"
        printfn ""
        printfn "Usage:"
        printCmd "help" "Displays this help page"
        printCmd "install <url>.." "Installs a mod from given url"
        printCmd "list" "Shows a list of installed mods"
        printCmd "update" "Shows a list of available mod updates"
        printCmd "upgrade" "Upgrades all installed mods"
    
    let list () =
        let printMod (m :Mods.InstalledMod) =
            printfn "%-50s %s" m.Name m.WebsiteVersion

        let mods = TPFMM.List |> List.ofArray
        // Output
        match mods with
        | [] ->
            printfn "No mods installed."
        | mods ->
            printfn "%s" "Installed mods:"
            mods
            |> List.iter printMod

    let update () =
        let printUpdate (name, oldVersion, newVersion) =
            printfn "%-50s %-18s %s" name oldVersion newVersion

        let updates = TPFMM.Update |> List.ofArray |> List.map (fun arr -> (arr.[0], arr.[1], arr.[2]))
        match updates with
        | [] ->
            printfn "No updates available."
        | updates ->
            printfn "%-50s %-18s %s" "Available Updates:" "Installed" "Available"
            updates
            |> List.iter printUpdate

    let upgrade () =
        printfn "Upgrade started.\n"
        TPFMM.UpgradeAll
        printfn "Upgrade finished."

    let execCommand command args =
        match command with
        | "update" -> update ()
        | "upgrade" -> upgrade ()
        | "list" -> list ()
        | "install" ->
            printfn "Uploading of downloaded mods to other sites is prohibited!\n"
            TPFMM.Install args
            printfn "Uploading of downloaded mods to other sites is prohibited!"
        | "help" | _ -> help ()