namespace TPFModManager_ConsoleGui

open TPFModManager
open TPFModManager.Api

// Module for console use
module private ConsoleApp =
    let printCmd long desc= printfn "%-10s %s" long desc

    let downloadStarted (modInfo :ModInfo) :unit =
        printfn "%s - %s:" modInfo.Name modInfo.Version
        printf "* Downloading..."

    let downloadEnded (modInfo :ModDownloadedInfo) :unit =
        printfn "\r%-16s" "* Downloaded."

    let extractStarted (modInfo :ModExtractInfo) :unit =
        printf "* Extracting..."

    let extractEnded (modInfo :Mod) :unit =
        printfn "\r%-15s" "* Extracted."

    let registerListeners (tpfmm :TPFMM) =
        tpfmm.RegisterDownloadStartedListener(downloadStarted)
        tpfmm.RegisterDownloadEndedListener(downloadEnded)
        tpfmm.RegisterExtractStartedListener(extractStarted)
        tpfmm.RegisterExtractEndedListener(extractEnded)

    let help () =
        printfn "Transport Fever Mod Manager (TPFMM)"
        printfn ""
        printfn "Usage:"
        printCmd "help" "Displays this help page"
        printCmd "install <url>.." "Installs a mod from given url"
        printCmd "list" "Shows a list of installed mods"
        //printCmd "update" "Shows a list of available mod updates"
        //printCmd "upgrade" "Upgrades all installed mods"
    
    let list () =
        let printMod (m :Mod) =
            printfn "%-50s %s" m.Name m.WebsiteVersion
        
        let tpfmm = new TPFMM(null);
        let mods = tpfmm.List |> List.ofArray
        // Output
        match mods with
        | [] ->
            printfn "No mods installed."
        | mods ->
            printfn "%s" "Installed mods:"
            mods
            |> List.iter printMod

    (*let update () =
        let printUpdate (name, oldVersion, newVersion) =
            printfn "%-50s %-18s %s" name oldVersion newVersion
        
        let tpfmm = new TPFMM(null);
        let updates = tpfmm.Update |> List.ofArray |> List.map (fun arr -> (arr.[0], arr.[1], arr.[2]))
        match updates with
        | [] ->
            printfn "No updates available."
        | updates ->
            printfn "%-50s %-18s %s" "Available Updates:" "Installed" "Available"
            updates
            |> List.iter printUpdate*)

    (*let upgrade () =
        printfn "Upgrade started.\n"
        let tpfmm = new TPFMM(TPFMM.loadSettings);
        tpfmm.UpgradeAll
        printfn "Upgrade finished."*)

    let outputError (error :TPFMMError) =
        match error with
        | NoConnection ->
            printfn "[Error] Couldn't connect to 'transportfever.net'."
        | AlreadyInstalled ->
            printfn "[Error] Mod is already installed."
        | ExtractionFailed ->
            printfn "\n[Error] Extraction of mod could not be completed. Did you install this mod manually? Delete the mod folder and restart installation."
        | ModInvalid ->
            printfn "[Error] Requested mod has no supported format."
        | MoreThanOneFile ->
            printfn "[Error] Mods with more than one uploaded file are not supported yet."
        | NoVersionOnWebsite ->
            printfn "[Error] Mods with no version on 'transportfever.net' are not supported yet."
        | NotBinary ->
            printfn "[Error] The requested file is not binary and has therefore no valid format."
        | NotSupportedFormat ext ->
            printfn "[Error] The mod archive has '.%s' format. Only '.zip' is supported yet." ext
        | UnsupportedLayout ->
            printfn "[Error] The site of given url can not be parsed correctly."

    let outputErrors = List.iter outputError

    let installSingle (tpfmm :TPFMM) url = 
        let result = tpfmm.Install url
        match result with
        | Ok _ -> ()
        | Error errors -> outputErrors errors
        printfn ""

    let installAll args =
        printfn "Uploading of downloaded mods to other sites is prohibited!\n"
        let tpfmm = new TPFMM(TPFMM.loadSettings)
        registerListeners tpfmm
        args |> List.iter (installSingle tpfmm)
        printfn "Uploading of downloaded mods to other sites is prohibited!"

    let execCommand command args =
        match command with
        //| "update"      -> update ()
        //| "upgrade"     -> upgrade ()
        | "list"        -> list ()
        | "install"     -> installAll args
        | "help" | _    -> help ()