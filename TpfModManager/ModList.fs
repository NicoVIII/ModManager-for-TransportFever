﻿namespace TpfModManager

open FSharp.Data
open IOHelper
open System.IO
open System.Text.RegularExpressions

module ModList =
    type Version = {major: int; minor: int}
    type Mod = {name: string; authors: string list; folder: string; image: string option; version: Version}
    type ModListJson = JsonProvider<""" { "mods": [{ "name": "modname", "authors": ["author1"], "folder": "author_mod_version", "image": "image_00.tga", "major": 1, "minor": 2 }] } """>

    module private Convert =
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
                {name = modJson.Name; authors = modJson.Authors |> Array.toList; folder = modJson.Folder; image = None; version = {major = 0; minor = 0}}
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
                new ModListJson.Mod(``mod``.name, ``mod``.authors |> List.toArray, ``mod``.folder, convertImage ``mod``.image, ``mod``.version.major, ``mod``.version.minor)
            
            let mods =
                modList
                |> List.map convertMod
                |> Array.ofList
            new ModListJson.Root(mods)

    let modListPath = "mods.json"
    let folderRegex = ".*_([0-9][0-9]*)$"

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

    let createModFromFolder path =
        let getFolderFromPath (path :string) =
            Path.GetFileName path

        let getImageFromFolder path =
                let image =
                    Directory.GetFiles(path, "image_00.tga")
                    |> Array.toList
                match image with
                | [image] -> Some "image_00.tga"
                | _ -> None

        let getVersion path (luaInfo :Lua.LuaInfo) =
            let getMajor path =
                let m = Regex.Match(path, folderRegex)
                int (m.Groups.Item(1).Value)

            {major = getMajor path; minor = luaInfo.minorVersion}
        
        // TODO use language specific names/descriptions
        let luaInfo = Lua.getInfoFromLuaFiles "en" path
        match luaInfo with
        | None -> None
        | Some luaInfo ->
            let folder = getFolderFromPath path
            let image = getImageFromFolder path
            let version = getVersion path luaInfo
            Some {name = luaInfo.name; authors = luaInfo.authors; folder = folder; image = image; version = version}

    let createModListFromPath path =
        Directory.GetDirectories(path)
        |> Array.toList
        |> List.filter (fun dir -> Regex.IsMatch(dir, folderRegex))
        |> List.filter (fun dir -> not (Regex.IsMatch(dir, Regex.Escape((System.Convert.ToString Path.DirectorySeparatorChar)+"urbangames_"))))
        |> List.map createModFromFolder
        |> List.filter Option.isSome
        |> List.map OptionHelper.unwrap
        |> saveModList
        loadModList()