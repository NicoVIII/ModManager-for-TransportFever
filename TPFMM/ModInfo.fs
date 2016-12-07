namespace TPFModManager

open FSharp.Data
open IO
open Types

module private ModInfo =
    let modInfoPath = "mods.json"

    type ModsJson = JsonProvider<""" { "installed_mods": [{ "name": "s", "url": "s", "websiteVersion": "s", "folder": "s" }] } """>

    let private loadModInfoFrom (path :string) =
        let mods = ModsJson.Load(path)
        mods.InstalledMods
        |> Array.toList
        |> List.map (fun m -> {name = m.Name; url = m.Url; websiteVersion = m.WebsiteVersion; folder = m.Folder})

    let loadModInfo() = loadModInfoFrom modInfoPath

    let private saveModInfoTo (path :string) (mods :Mod list) =
        let mods' = mods |> List.map (fun {name=name; websiteVersion=version; url=url; folder=folder} -> new ModsJson.InstalledMod(name, url, version, folder))
        let modsObj = new ModsJson.Root(Array.ofList mods')
        saveString path (modsObj.ToString())

    let private saveModInfo = saveModInfoTo modInfoPath 

    let addMods mods =
        let modInfo = loadModInfo ()
        mods @ modInfo
        |> saveModInfo

    let addMod modi =
        addMods [modi]

    let removeModInfo (_mod :Mod) =
        loadModInfo ()
        |> List.filter (fun {url=url} -> not (url = _mod.url))
        |> saveModInfo