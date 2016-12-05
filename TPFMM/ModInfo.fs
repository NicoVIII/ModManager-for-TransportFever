namespace TPFModManager

open FSharp.Data
open System.IO
open Types

module private ModInfo =
    let modInfoPath = "mods.json"

    type ModsJson = JsonProvider<""" { "installed_mods": [{ "name": "s", "url": "s", "websiteVersion": "s", "folder": "s" }] } """>

    let private saveString (file :string) str =
        let directory = file.Split [| '/' |] |> Array.toList |> List.rev |> List.tail |> List.rev |> List.fold (fun path folder -> path+folder+"/") ""
        if directory.Length = 0 then () else Directory.CreateDirectory directory |> ignore
        File.WriteAllText(file, str)

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

    let removeModInfo {url = modUrl} =
        loadModInfo ()
        |> List.filter (fun {url=url} -> not (url = modUrl))
        |> saveModInfo