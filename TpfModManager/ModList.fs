namespace TpfModManager

open FSharp.Data
open IOHelper
open System.IO

module ModList =
    type Version = {major: int; minor: int}
    type Mod = {image: string option; version: Version}
    type ModListJson = JsonProvider<""" { "mods": [{ "image": "image_00.tga", "major": 1, "minor": 2 }] } """>

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
                {image = None; version = {major = 0; minor = 0}}
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
                new ModListJson.Mod(convertImage ``mod``.image, ``mod``.version.major, ``mod``.version.minor)
            
            let mods =
                modList
                |> List.map convertMod
                |> Array.ofList
            new ModListJson.Root(mods)

    let modListPath = "mods.json"

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
            (ModListJson.Load modListPath).Mods
            |> Array.toList