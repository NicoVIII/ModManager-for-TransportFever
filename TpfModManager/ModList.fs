namespace TpfModManager

open FSharp.Data
open IOHelper
open System.IO
open System.Text.RegularExpressions
open TpfNet
open Types

module ModList =
    type Version = {major: int; minor: int}
    type Mod = {name: string; authors: Author list; folder: string; image: string option; version: Version; tpfNetId: int; remoteVersion: Version option}
    type ModListJson = JsonProvider<""" { "mods": [{ "name": "modname", "authors": [{"name": "author1", "tpfNetId": 12345}], "folder": "author_mod_version", "image": "image_00.tga", "major": 1, "minor": 2, "tpfNetId": 12345 }] } """>

    module private Convert =
        let fromJson (json :ModListJson.Root) =
            let convertImage image =
                match image with
                | "" -> None
                | img -> Some img

            let convertAuthor (jsonAuthor :ModListJson.Author) =
                {Author.name = jsonAuthor.Name; tpfNetId = jsonAuthor.TpfNetId}

            let convertMod (modJson :ModListJson.Mod) =
                let authors =
                    modJson.Authors
                    |> Array.toList
                    |> List.map convertAuthor
                let image =
                    convertImage modJson.Image

                {name = modJson.Name; authors = authors; folder = modJson.Folder; image = image; version = {major = modJson.Major; minor = modJson.Minor}; tpfNetId = modJson.TpfNetId; remoteVersion = None}

            json.Mods
            |> List.ofArray
            |> List.map convertMod

        let toJson modList =
            let convertImage image =
                match image with
                | None -> ""
                | Some img -> img

            let convertAuthor author =
                let {Author.name = name; tpfNetId = tpfNetId} = author
                new ModListJson.Author(name, tpfNetId)

            let convertMod ``mod`` =
                let authors =
                    ``mod``.authors
                    |> List.map convertAuthor
                    |> List.toArray
                new ModListJson.Mod(``mod``.name, authors, ``mod``.folder, convertImage ``mod``.image, ``mod``.version.major, ``mod``.version.minor, ``mod``.tpfNetId)
            
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
            Some {name = luaInfo.name; authors = luaInfo.authors; folder = folder; image = image; version = version; tpfNetId = 0; remoteVersion = None}

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

    (*let lookUpRemoteVersion (csv :TpfNetCsv) ``mod`` =
        let parseVersion version =


        let {tpfNetId = id} = ``mod``
        let modRow =
            csv.Rows
            |> Seq.toList
            |> List.tryFind (function row -> row.ID = id)
        match modRow with
        | None -> None
        | Some modRow ->
            modRow.VERSION*)