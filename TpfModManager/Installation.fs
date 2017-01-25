namespace TpfModManager

open ModList
open SharpCompress.Archives
open SharpCompress.Readers
open System
open System.IO

module Installation =
    type InstallationError = 
        | ModInvalid
        | NoFolderIncluded
        | ExtractionFailed
        | AlreadyInstalled
        | ModListError

    let getModFolderFromArchive (modArchivePath :string) =
        let getTopLevelFolders list (entry :IArchiveEntry) =
            let determineDirectorySeperator (path :string) =
                if path.Contains (Convert.ToString '\\') then
                    '\\'
                else
                    '/'
            let name =
                let n = entry.Key
                let splitter = determineDirectorySeperator n
                (n.Split splitter).[0]

            if List.forall (fun el -> not (el = name)) list then
                name::list
            else
                list

        use archive = ArchiveFactory.Open modArchivePath
        let topLevelDirectories =
            archive.Entries
            |> Seq.toList
            |> List.fold getTopLevelFolders []
        archive.Dispose ()
        match topLevelDirectories with
        | [directory] -> 
            Ok directory
        | list ->
            if list |> List.exists (fun el -> el = "mod.lua") then
                // TODO determine modname
                Error NoFolderIncluded
            else 
                Error ModInvalid

    let install modList tpfPath modArchivePath =
        let checkIfModIsAlreadyInstalled modList modArchivePath =
            let folder = getModFolderFromArchive modArchivePath
            match folder with
            | Ok folder ->
                Ok (List.exists (fun (``mod`` :Mod) -> ``mod``.folder = folder) modList)
            | Error err ->
                Error err

        let performInstallation tpfPath (modArchivePath :string) =
            try
                use archive = ArchiveFactory.Open modArchivePath
                use reader = archive.ExtractAllEntries()
                let eo = new ExtractionOptions()
                eo.ExtractFullPath <- true
                reader.WriteAllToDirectory (tpfPath, eo)
                reader.Dispose()
                archive.Dispose()
                let getNewMod =
                    getModFolderFromArchive
                    >> map (PathHelper.combine tpfPath)
                    >> bind (ModList.createModFromFolder >> optionToResult ModListError)
                getNewMod modArchivePath
            with
            | :? System.IO.IOException ->
                Error ExtractionFailed

        match checkIfModIsAlreadyInstalled modList modArchivePath with
        | Ok result ->
            match result with
            | true ->
                // TODO upgrade
                Error AlreadyInstalled
            | false ->
                match performInstallation tpfPath modArchivePath with
                | Ok ``mod`` ->
                    let modList' = modList @ [``mod``]
                    saveModList modList'
                    Ok modList'
                | Error error ->
                    Error error
        | Error error ->
            Error error