namespace TpfModManager

[<AllowNullLiteral>]
type Settings(``internal``: SettingsModule.T) =
    let mutable ``internal`` = ``internal``

    member x.Save () = SettingsModule.saveSettings ``internal``
    member x.TpfModPath
        with get() =
            ``internal``.tpfModPath
        and set(value) = 
            ``internal`` <- {``internal`` with tpfModPath = value}
    new () = Settings({tpfModPath = ""})

module private SettingsAPI =
    let convert settings =
        match settings with
        | None -> null
        | Some settings -> new Settings(settings)

type Version(``internal``: ModList.Version) =
    let mutable ``internal`` = ``internal``

    member x.Major = ``internal``.major
    member x.Minor = ``internal``.minor

type Mod(``internal``: ModList.Mod) =
    let mutable ``internal`` = ``internal``

    member x.Version = ``internal``.version

type ModManager() =
    member val Settings =
        SettingsModule.loadSettings()
        |> SettingsAPI.convert
        with get, set

    member x.Check =
        // TODO
        ()