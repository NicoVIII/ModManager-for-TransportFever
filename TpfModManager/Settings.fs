namespace TpfModManager

open FSharp.Data
open IOHelper
open System.IO

module SettingsModule =
    type T = {tpfModPath: string}
    type SettingsJson = JsonProvider<""" { "tpfModPath": "/path/to/tpf", "deleteArchives": true } """>

    module Convert =
        let fromJson (json :SettingsJson.Root) =
            {tpfModPath = json.TpfModPath}

        let toJson settings =
            new SettingsJson.Root(settings.tpfModPath, true)

    let settingsPath = "settings.json"

    let saveSettings settings =
        (Convert.toJson settings).ToString()
        |> saveString settingsPath

    let loadSettings () =
        let createSettings path =
            saveSettings {tpfModPath = ""}
            None

        match File.Exists settingsPath with
        | false ->
            createSettings settingsPath
        | true ->
            let settings = SettingsJson.Load settingsPath
            if settings.TpfModPath = "" then
                None
            else
                Some {tpfModPath = settings.TpfModPath}