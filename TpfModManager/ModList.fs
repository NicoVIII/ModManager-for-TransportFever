namespace TpfModManager

open FSharp.Data
open IOHelper
open RegexHelper
open System.IO
open System.Text.RegularExpressions
open Types

module ModList =
    let modListPath = "mods.json"
    let folderRegex = ".*_([0-9][0-9]*)$"

    type Version = {major: VersionNumber; minor: VersionNumber}
    type Mod = {name: string; authors: Author list; folder: Folder; image: string option; version: Version; tpfNetId: TpfNetId option; remoteVersion: Version option}
    type ModListJson = JsonProvider<""" { "mods": [{ "name": "modname", "authors": [{"name": "author1", "tpfNetId": 12345}], "folder": "author_mod_version", "image": "image_00.tga", "major": 1, "minor": 2, "tpfNetId": 12345 }] } """>

    module private Convert =
        module private FromJson =
            let convertImage image =
                match image with
                | "" -> None
                | img -> Some img

            let convertAuthor (jsonAuthor :ModListJson.Author) =
                let tpfNetId =
                    match jsonAuthor.TpfNetId with
                    | -1 -> None
                    | id -> Some (tpfNetId id)
                {Author.name = jsonAuthor.Name; tpfNetId = tpfNetId}

            let convertAuthors =
                Array.toList
                >> List.map convertAuthor

            let convertTpfNetId tpfnetId =
                match tpfnetId with
                | id when id < 0 -> None
                | id -> Some (tpfNetId id)

            let convertMod (modJson :ModListJson.Mod) =
                {name = modJson.Name; authors = convertAuthors modJson.Authors;
                 folder = Folder modJson.Folder; image = convertImage modJson.Image;
                 version = {major = versionNumber modJson.Major; minor = versionNumber modJson.Minor};
                 tpfNetId = convertTpfNetId modJson.TpfNetId; remoteVersion = None }

        let fromJson (json :ModListJson.Root) =
            json.Mods
            |> List.ofArray
            |> List.map FromJson.convertMod

        module private ToJson =
            let convertImage image =
                match image with
                | None -> ""
                | Some img -> img

            let convertAuthor author =
                let {Author.name = name; tpfNetId = tpfNetId} = author
                let tpfNetId' =
                    match tpfNetId with
                    | None -> -1
                    | Some id -> int id
                new ModListJson.Author(name, tpfNetId')

            let convertAuthors =
                List.map convertAuthor
                >> List.toArray

            let convertTpfNetId tpfnetId =
                match tpfnetId with
                | None -> -1
                | Some id -> int id

            let convertMod ``mod`` =
                let {name = name; authors = authors; folder = Folder folder; image = image; version = {major = major; minor = minor}; tpfNetId = tpfNetId} = ``mod``
                new ModListJson.Mod(name, convertAuthors authors, folder, convertImage image, int major, int minor, convertTpfNetId tpfNetId)

        let toJson modList =
            let mods =
                modList
                |> List.map ToJson.convertMod
                |> Array.ofList
            new ModListJson.Root(mods)

    let saveModList =
        Convert.toJson
        >> (function root -> root.ToString())
        >> saveString modListPath

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

    let createModFromFolder langKey path =
        let getFolderFromPath (path :string) =
            Path.GetFileName path
            |> Folder

        let getImageFromPath path =
                let image =
                    Directory.GetFiles(path, "image_00.tga")
                    |> Array.toList
                match image with
                | [image] -> Some "image_00.tga"
                | _ -> None

        let getVersion path (luaInfo :Lua.LuaInfo) =
            let getMajor path =
                let m = Regex.Match(path, folderRegex)
                versionNumberFromString (m.Groups.Item(1).Value)

            {major = getMajor path; minor = luaInfo.minorVersion}
        
        // TODO use language specific names/descriptions
        let luaInfo = Lua.getInfoFromLuaFiles langKey path
        match luaInfo with
        | None -> None
        | Some luaInfo ->
            let folder = getFolderFromPath path
            let image = getImageFromPath path
            let version = getVersion path luaInfo
            Some {name = luaInfo.name; authors = luaInfo.authors; folder = folder; image = image; version = version; tpfNetId = luaInfo.tpfNetId; remoteVersion = None}

    let changeTpfNetId tpfNetId folder modList =
        let ``mod`` = List.find (fun ``mod`` -> ``mod``.folder = folder) modList
        ListHelper.updateAll modList ``mod`` {``mod`` with tpfNetId = Some tpfNetId}

    let createModListFromPath langKey path =
        Directory.GetDirectories(path)
        |> Array.toList
        |> List.filter (fun dir -> Regex.IsMatch(dir, folderRegex))
        |> List.filter (fun dir -> not (Regex.IsMatch(dir, Regex.Escape((System.Convert.ToString Path.DirectorySeparatorChar)+"urbangames_"))))
        |> List.map (createModFromFolder langKey)
        |> List.filter Option.isSome
        |> List.map OptionHelper.unwrap