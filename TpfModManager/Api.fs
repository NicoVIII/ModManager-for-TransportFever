namespace TpfModManager

[<AllowNullLiteral>]
type Settings(_internal: SettingsModule.T) =
    let mutable _internal = _internal

    member x.Save () = SettingsModule.saveSettings _internal
    member x.TpfModPath
        with get() =
            _internal.tpfModPath
        and set(value) = 
            _internal <- {_internal with tpfModPath = value}
    new () = Settings({tpfModPath = ""})

module private SettingsAPI =
    let convert settings =
        match settings with
        | None -> null
        | Some settings -> new Settings(settings)

type ModManager() =
    member val Settings =
        SettingsModule.loadSettings()
        |> SettingsAPI.convert
        with get, set

    member x.Check =
        // TODO
        ()