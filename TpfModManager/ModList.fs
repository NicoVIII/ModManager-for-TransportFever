namespace TpfModManager

open FSharp.Data
open IOHelper
open MoonSharp.Interpreter
open MoonSharp.Interpreter.Loaders
open System.IO
open System.Text.RegularExpressions

module ModList =
    type Version = {major: int; minor: int}
    type Mod = {name: string; folder: string; image: string option; version: Version}
    type ModListJson = JsonProvider<""" { "mods": [{ "name": "modname", "folder": "author_mod_version", "image": "image_00.tga", "major": 1, "minor": 2 }] } """>

    module Convert =
        let fromJson (json :ModListJson.Root) =
            let changeImage image ``mod`` =
                {``mod`` with image = image}

            let convertImage image ``mod`` =
                match image with
                | "" -> changeImage None ``mod``
                | img -> changeImage (Some img) ``mod``

            let convertVersion major minor ``mod`` =
                {``mod`` with version = {major = major; minor = minor}}

            let map (modJson :ModListJson.Mod) =
                {name = modJson.Name; folder = modJson.Folder; image = None; version = {major = 0; minor = 0}}
                |> convertImage modJson.Image
                |> convertVersion modJson.Major modJson.Minor

            json.Mods
            |> List.ofArray
            |> List.map map

        let toJson modList =
            let convertImage image =
                match image with
                | None -> ""
                | Some img -> img

            let convertMod ``mod`` =
                new ModListJson.Mod(``mod``.name, ``mod``.folder, convertImage ``mod``.image, ``mod``.version.major, ``mod``.version.minor)
            
            let mods =
                modList
                |> List.map convertMod
                |> Array.ofList
            new ModListJson.Root(mods)

    let modListPath = "mods.json"
    let folderRegex = ".*_([0-9][0-9]*)$"

    let getNameWithoutStringsLua modLua =
        let script = new Script()
        // TODO add support for require
        (*let tpfPath =
            modLua
            |> Path.GetDirectoryName
            |> Path.GetDirectoryName
            |> Path.GetDirectoryName
        let scriptPath = Path.Combine(tpfPath, "res", "scripts").Replace(Path.DirectorySeparatorChar, '/')
        printfn "%s" scriptPath
        (script.Options.ScriptLoader :?> ScriptLoaderBase).ModulePaths <- [|"../"|]*)

        script.DoString(@"function _ (s) return s; end") |> ignore
        let file = File.ReadAllText modLua
        Regex.Replace(file, "require .*", "")
        |> script.DoString |> ignore//*)
        //script.DoFile modLua |> ignore
        let info =
            script.Globals.["data"]
            |> script.Call
        DynValue.FromObject(script, info.Table.["info"]).Table.["name"]
        |> string

    let getNameFromFolder path =
        let stringsLua = Path.Combine(path, "strings.lua")
        let modLua = Path.Combine(path, "mod.lua")
        if File.Exists stringsLua then
            (*let script = new Script()
            script.DoFile(stringsLua) |> ignore
            let value = script.Globals.["data"] |> script.Call 
            let translations = DynValue.FromObject(script, value.Table.["en"]).Table*)
            ""
        else
            getNameWithoutStringsLua modLua

    let getFolderFromPath (path :string) =
        Path.GetFileName path

    let getImageFromFolder path =
            let image =
                Directory.GetFiles(path, "image_00.tga")
                |> Array.toList
            match image with
            | [image] -> Some "image_00.tga"
            | [] -> None
            | _::_ -> None

    let getVersionFromFolder path =
        let getMajor path =
            let m = Regex.Match(path, folderRegex)
            int (m.Groups.Item(1).Value)
        let getMinor path =
            // TODO
            1

        {major = getMajor path; minor = getMinor path}

    let createModFromFolder path =
        let name = getNameFromFolder path
        let folder = getFolderFromPath path
        let image = getImageFromFolder path
        let version = getVersionFromFolder path
        {name = name; folder = folder; image = image; version = version}

    let saveModList modList =
        (Convert.toJson modList).ToString()
        |> saveString modListPath

    let loadModList () =
        let createModList path =
            saveModList []
            []

        match File.Exists modListPath with
        | false ->
            createModList modListPath
        | true ->
            (ModListJson.Load modListPath)
            |> Convert.fromJson

    let createModListFromPath path =
        Directory.GetDirectories(path)
        |> Array.toList
        |> List.filter (fun dir -> Regex.IsMatch(dir, folderRegex))
        |> List.map createModFromFolder
        |> saveModList
        loadModList()