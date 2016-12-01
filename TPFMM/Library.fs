namespace TPFModManager

open FSharp.Data

// Internal logic to provide API functionality
module private Internal =
    type Settings = JsonProvider<""" { "tpfPath": "/path/to/tpf" } """>

    // Mod Management
    type Mods =
        JsonProvider<""" { "installed_mods": [{ "name": "s", "url": "s", "minorVersion": 0, "websiteVersion": "s" }, { "name": "s", "url": "s", "minorVersion": 0, "websiteVersion": "s" } ] } """>
    
    type InstallStatus = | Installed | NotInstalled

    let loadModsFrom (path :string) =
        let mods = Mods.Load(path)
        mods.InstalledMods |> Array.toList

    let loadMods () = loadModsFrom "mods.json"

    let modStatus url =
        let mods = loadMods()
        let fold state (m :Mods.InstalledMod) = state || m.Url = url
        let installed = List.fold fold false mods
        match installed with
        | true -> Installed 
        | false -> NotInstalled

    // Functionality
    let downloadMod url =
        printf "%s" "I download!" |> ignore

    let list () =
        printfn "%s" "Installed mods:"
        // Print all installed mods
        loadModsFrom "mods.json" |> List.sortBy (fun m -> m.Name) 
        |> List.iter (fun m -> printfn "%s" m.Name)

    let install url =
        match modStatus url with
        | Installed ->
            printfn "A mod with url '%s' is already installed." url
        | NotInstalled ->
            downloadMod url

    let installAll urls =
        urls |> List.iter (fun url -> install url)

// API
type TPFMM =
    static member List = Internal.list
    static member InstallAll urls = Internal.installAll urls
    static member Install url = Internal.install url