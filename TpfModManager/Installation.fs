namespace TpfModManager

open ModList
open SharpCompress.Archives
open SharpCompress.Readers
open System
open System.IO
open Types

module Installation =
    type InstallationError = 
        | ModInvalid
        | NoFolderIncluded
        | ExtractionFailed
        | AlreadyInstalled
        | ModListError
        | NotAnArchive

    type UninstallError =
        | ModNotInstalled
        | FolderDoesNotExist
    
    let getModFolderFromArchive (handler :IArchive) =
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

        let topLevelDirectories =
            handler.Entries
            |> Seq.toList
            |> List.fold getTopLevelFolders []
        match topLevelDirectories with
        | [directory] -> 
            Ok (Folder directory)
        | list ->
            if list |> List.exists (fun el -> el = "mod.lua") then
                // TODO determine modname
                Error NoFolderIncluded
            else 
                Error ModInvalid

    let isModInstalled modList folder =
        List.exists (fun (``mod`` :Mod) -> ``mod``.folder = folder) modList

    let install modList tpfPath (modArchivePath :string) =
        let getArchiveHandler (modArchivePath :string) =
            try
                Some (ArchiveFactory.Open modArchivePath)
            with
            | :? InvalidOperationException -> None

        let checkIfModIsAlreadyInstalled modList modArchivePath =
            let folder = getModFolderFromArchive modArchivePath
            match folder with
            | Ok folder ->
                Ok (isModInstalled modList folder)
            | Error err ->
                Error err
        
        let extractArchive (handler :IArchive) =
            try
                use reader = handler.ExtractAllEntries()
                let eo = new ExtractionOptions()
                eo.ExtractFullPath <- true
                reader.WriteAllToDirectory (tpfPath, eo)
                reader.Dispose()
                Ok ()
            with
            | :? System.IO.IOException ->
                Error ExtractionFailed

        let performInstallation tpfPath =
            perform extractArchive
            >=> getModFolderFromArchive
            >=> switch (function Folder folder -> PathHelper.combine tpfPath folder)
            >=> (ModList.createModFromFolder >> optionToResult ModListError)
       
        match getArchiveHandler modArchivePath with
        // Not an archive
        | None -> Error NotAnArchive
        | Some handler ->
            match checkIfModIsAlreadyInstalled modList handler with
            | Ok result ->
                match result with
                | true ->
                    // TODO upgrade
                    Error AlreadyInstalled
                | false ->
                    match performInstallation tpfPath handler with
                    | Ok ``mod`` ->
                        handler.Dispose()
                        let modList' = modList @ [``mod``]
                        saveModList modList'
                        Ok modList'
                    | Error error ->
                        handler.Dispose()
                        Error error
            | Error error ->
                handler.Dispose()
                Error error
    
    let private removeModFromModList modList (``mod`` :Mod) =
        List.filter (function m -> not (``mod`` = m)) modList

    let private getModByFolder modList folder =
        List.tryFind (function ``mod`` -> ``mod``.folder = folder) modList
        |> optionToResult ModNotInstalled

    let private uninstallPerform modList tpfPath =
        getModByFolder modList
        >=> switchTee (function {folder = Folder folder} -> Directory.Delete(Path.Combine(tpfPath, folder), true))

    let uninstall modList tpfPath =
        boolVault FolderDoesNotExist (function (Folder folder) -> Path.Combine (tpfPath,folder) |> Directory.Exists)
        >=> uninstallPerform modList tpfPath
        >=> switch (removeModFromModList modList)
        >=> switch (tee saveModList)