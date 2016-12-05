namespace TPFModManager

open FSharp.Data
open ModInfo
open RobHelper
open System
open System.IO
open System.IO.Compression
open Types

type ModUrl = ModUrl of String

[<AllowNullLiteral>]
type Settings (tpfModPath :string, deleteZips :bool) =
    member this.TpfModPath = tpfModPath
    member this.DeleteZips = deleteZips

// Internal logic to provide API functionality
module private Internal =
    type SettingsJson = JsonProvider<""" { "tpfModPath": "/path/to/tpf", "deleteZips": true } """>

    let tryLoadSettings () =
        let settingsPath = "settings.json"
        match File.Exists settingsPath with
        | true -> Some (SettingsJson.Load settingsPath)
        | false -> None
    
    (*let downloadMod (installEvent :Event<_>) fileUrl (_mod :Mod) =


    let extractMod tpfModPath zipPath =
        try
            ZipFile.ExtractToDirectory(zipPath, tpfModPath) |> ignore
        with
        | :? System.IO.IOException -> ()

    let installMod _mod (installEvent :Event<_>) tpfPath zipPath =
        installEvent.Trigger(InstallProcessEventType.InstallationStarted, _mod)
        extractMod tpfPath zipPath
        saveModInfo (_mod::loadModInfo())
        installEvent.Trigger(InstallProcessEventType.InstallationEnded, _mod)

    let createModFromInfo (name,url,version) =
        new Mod(name, url, version, "")

    let downloadAndInstall (settings :Settings) (installEvent :Event<_>) =
        checkModInstalled
        >> bind TransportFeverNet.getInfo
        >> map createModFromInfo
            let zipPath = 
            downloadMod _mod installEvent filePath
            use file = ZipFile.Open(zipPath, ZipArchiveMode.Read)
            let fold list (entry :ZipArchiveEntry) =
                let name = entry.FullName.TrimEnd '/'
                let name' = (name.Split '/').[0]
                if List.forall (fun el -> not (el = name')) list then
                    name'::list
                else
                    list
            let entries = file.Entries |> Seq.toList |> List.fold fold []
            file.Dispose ()
            match entries with
            | [folder] -> 
                let _mod = new Mod(_mod.Name, _mod.Url, _mod.WebsiteVersion, folder)
                installMod _mod installEvent settings.TpfModPath zipPath
                Ok ()
            | list ->
                if list |> List.exists (fun el -> el = "mod.lua") then
                    let _mod = new Mod(_mod.Name, _mod.Url, _mod.WebsiteVersion, _mod.Name)
                    installMod _mod installEvent (settings.TpfModPath+"/"+_mod.Name) zipPath
                    Ok ()
                else 
                    printfn "%A" list
                    failwith "Mod is not well configured"*)

    (*let downloadAndInstallAll settings event urls =
        let downloadAndInstall url = downloadAndInstall settings event url
        let res = urls |> List.fold (fun res url -> downloadAndInstall url |> combineErrors res) (Ok ())
        if settings.DeleteZips && Directory.Exists("tmp") then
            Directory.Delete("tmp", true)
        res

    let update () =
        let fold list (_mod :Mod) =
            let site = TransportFeverNet.tryGetSite (Url _mod.Url)
            match site with
            | Some site ->
                let newVersion = TransportFeverNet.version site (Url _mod.Url)
                if not (newVersion = Some _mod.WebsiteVersion) then
                    match newVersion with
                    | Some newVersion -> (_mod.Name, _mod.WebsiteVersion, newVersion)::list
                    | None -> list
                else
                    list
            | None -> list

        loadModInfo()
        |> List.fold fold []
        |> List.map (fun (name, oldVersion, newVersion) -> [| name ; oldVersion ; newVersion |])
        |> Array.ofList

    let upgrade (settings :Settings) upgradeEvent installEvent (_mod :Mod) =
        let site = TransportFeverNet.tryGetSite (Url _mod.Url)
        match site with
        | Some site ->
            let newVersion = TransportFeverNet.version site (Url _mod.Url)
            if not (newVersion = Some _mod.WebsiteVersion) then
                match newVersion with
                | Some newVersion ->
                    // Delete old version
                    Directory.Delete(settings.TpfModPath+"/"+_mod.Folder, true)
                    removeModInfo _mod
                    downloadAndInstall settings installEvent (Url _mod.Url)
                    if settings.DeleteZips && Directory.Exists("tmp") then Directory.Delete("tmp", true)
                | None -> ()
        | None -> ()

    let upgradeAll (settings :Settings) upgradeEvent installEvent =
        let upgrade = upgrade settings upgradeEvent installEvent

        loadModInfo()
        |> List.iter (fun _mod -> upgrade _mod)*)
    
    // List
    let list () =
        loadModInfo()
        |> List.sortBy (fun {name=name} -> name)
    
    // Download
    let directoryFromFile (path :string) =
        path.Split [| '/' |]
        |> Array.toList
        |> List.rev
        |> List.tail
        |> List.rev
        |> List.fold (fun path folder -> path+folder+"/") ""

    let saveBytes {name = name; version = version; url = url; bytes = bytes} =
        let path = ("tmp/"+name+"-"+version+".zip")
        directoryFromFile path
        |> Directory.CreateDirectory |> ignore
        File.WriteAllBytes(path, bytes)
        {name = name; version = version; url = url; zipPath = path}

    let download (downloadStartedEvent :Event<_>) (downloadEndedEvent :Event<_>) =
        TransportFeverNet.getInfo
        >> map (tee (ApiHelper.convertModInfo >> downloadStartedEvent.Trigger))
        >> bind TransportFeverNet.getFileBytes
        >> map saveBytes
        >> map (tee (ApiHelper.convertModDownloadedInfo >> downloadEndedEvent.Trigger))

    // Installation
    let checkModInstalled url =
        let (Url urlString) = url
        let mods = loadModInfo()
        let fold state {folder = _; url = url} = state || url = urlString
        let installed = List.fold fold false mods
        match installed with
        | true -> Error [AlreadyInstalled]
        | false -> Ok url
    
    let prepareExtract (settings :Settings) modInfoDownloaded =
        let {name = name; version = version; url = url; zipPath = zipPath} = modInfoDownloaded
        use file = ZipFile.Open(zipPath, ZipArchiveMode.Read)
        let fold list (entry :ZipArchiveEntry) =
            let name = entry.FullName.TrimEnd '/'
            let name' = (name.Split '/').[0]
            if List.forall (fun el -> not (el = name')) list then
                name'::list
            else
                list
        let entries = file.Entries |> Seq.toList |> List.fold fold []
        file.Dispose ()
        match entries with
        | [folder] -> 
            Ok {modDownloadedInfo=modInfoDownloaded; extractPath = settings.TpfModPath; folder = folder}
        | list ->
            if list |> List.exists (fun el -> el = "mod.lua") then
                Ok {modDownloadedInfo=modInfoDownloaded; extractPath = settings.TpfModPath+"/"+name; folder = name}
            else 
                Error [ModInvalid]

    let performExtract {modDownloadedInfo = {name = name; version = version; url = url; zipPath = zipPath}; folder = folder; extractPath = extractPath} =
        try
            ZipFile.ExtractToDirectory(zipPath, extractPath) |> ignore
            Ok {name = name; websiteVersion = version; url = url; folder = folder}
        with
        | :? System.IO.IOException -> Error [ExtractionFailed]

    let extract (settings :Settings) (extractStartedEvent :Event<_>) (extractEndedEvent :Event<_>) =
        prepareExtract settings
        >> map (tee (ApiHelper.convertExtractInfo >> extractStartedEvent.Trigger))
        >> bind performExtract
        >> map (tee (ApiHelper.convertMod >> extractEndedEvent.Trigger))

    let install (settings :Settings) (extractStartedEvent :Event<_>) (extractEndedEvent :Event<_>) (downloadStartedEvent :Event<_>) (downloadEndedEvent :Event<_>) =
        checkModInstalled
        >> bind (download downloadStartedEvent downloadEndedEvent)
        >> bind (extract settings extractStartedEvent extractEndedEvent)

    let installSingle (settings :Settings) (extractStartedEvent :Event<_>) (extractEndedEvent :Event<_>) (downloadStartedEvent :Event<_>) (downloadEndedEvent :Event<_>) =
        install settings extractStartedEvent extractEndedEvent downloadStartedEvent downloadEndedEvent
        >> map addMod