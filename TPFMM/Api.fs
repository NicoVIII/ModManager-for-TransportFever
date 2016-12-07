namespace TPFModManager.Api

open RobHelper
open TPFModManager
open TPFModManager.ApiHelper
open TPFModManager.ModInfo
open TPFModManager.Types

// API
type TPFMM(settings :Api.Settings) =
    // Events
    let downloadStartedEvent = new Event<_>()
    let downloadEndedEvent = new Event<_>()
    let extractStartedEvent = new Event<_>()
    let extractEndedEvent = new Event<_>()

    // Property
    member this.Settings = settings

    // Methods
    member this.List =
        Internal.list ()
        |> List.map convertMod
        |> Array.ofList
    member this.Install url =
        Internal.installSingle (deconvertSettings this.Settings) downloadStartedEvent downloadEndedEvent extractStartedEvent extractEndedEvent (Url url)
    member this.Update =
        Internal.update ()
        |> List.map (fun (name, oldVersion, newVersion) -> [| name ; oldVersion ; newVersion |])
        |> Array.ofList
    member this.Upgrade =
        deconvertMod
        >> Internal.upgrade(deconvertSettings this.Settings) downloadStartedEvent downloadEndedEvent extractStartedEvent extractEndedEvent

    // Callbacks
    member this.RegisterDownloadStartedListener handler = Event.add handler downloadStartedEvent.Publish
    member this.RegisterDownloadEndedListener handler = Event.add handler downloadEndedEvent.Publish
    member this.RegisterExtractStartedListener handler = Event.add handler extractStartedEvent.Publish
    member this.RegisterExtractEndedListener handler = Event.add handler extractEndedEvent.Publish

    // Static methods
    static member loadSettings =
        match Settings.loadSettings () with
        | Ok settings -> (convertSettings settings)
        | Error error -> null