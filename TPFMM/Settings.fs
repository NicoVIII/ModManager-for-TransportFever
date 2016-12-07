module private TPFModManager.Settings

open FSharp.Data
open IO
open System.IO
open Types

type SettingsJson = JsonProvider<""" { "tpfModPath": "/path/to/tpf", "deleteZips": true } """>

let settingsPath = "settings.json"

let saveSettings (settings :SettingsJson.Root) =
    saveString settingsPath (settings.ToString())

let createSettings path =
    saveSettings (new SettingsJson.Root("", true))
    Error NoValidTpfPath

let loadSettings () =
    match File.Exists settingsPath with
    | false ->
        createSettings settingsPath
    | true ->
        let settings = SettingsJson.Load settingsPath
        if settings.TpfModPath = "" then
            Error NoValidTpfPath
        else
            Ok {tpfModPath = settings.TpfModPath; deleteZips = settings.DeleteZips}