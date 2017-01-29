﻿namespace TpfModManager

open ModList
open SharpCompress.Archives
open SharpCompress.Readers
open System
open System.IO
open Types

module Installation =
    type InstallationError = 
        | InstallationModInvalid
        | InstallationNoFolderInArchive
        | InstallationExtractionFailed
        | InstallationModAlreadyInstalled of Folder
        | InstallationModListError
        | FileNotAnArchive

    type UninstallError =
        | UninstallModNotInstalled
        | UninstallFolderDoesNotExist

    type UpgradeError =
        | InstallError of InstallationError
        | UninstallError of UninstallError
    
    let private getArchiveHandler (modArchivePath :string) =
        try
            Ok (ArchiveFactory.Open modArchivePath)
        with
        | :? InvalidOperationException -> Error FileNotAnArchive

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
                Error InstallationNoFolderInArchive
            else 
                Error InstallationModInvalid

    let isModInstalled modList folder =
        List.exists (fun (``mod`` :Mod) -> ``mod``.folder = folder) modList

    let install modList tpfPath modArchivePath =
        let findInstalledMod modList handler =
            let folder = getModFolderFromArchive handler
            match folder with
            | Ok folder ->
                match isModInstalled modList folder with
                | true -> Some folder
                | false -> None
            | Error err ->
                None
        
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
                Error InstallationExtractionFailed

        let performInstallation tpfPath =
            perform extractArchive
            >=> getModFolderFromArchive
            >=> switch (function Folder folder -> PathHelper.combine tpfPath folder)
            >=> (ModList.createModFromFolder >> optionToResult InstallationModListError)
       
        getArchiveHandler modArchivePath
        >>= (function handler -> match findInstalledMod modList handler with
                                 | Some ``mod`` -> Error (InstallationModAlreadyInstalled ``mod``)
                                 | None -> Ok handler)
        >>= performInstallation tpfPath
        >>= switch (function ``mod`` -> modList @ [``mod``])
        >>= switch (tee saveModList)
    
    let private removeModFromModList modList (``mod`` :Mod) =
        List.filter (function m -> not (``mod`` = m)) modList

    let private getModByFolder modList folder =
        List.tryFind (function ``mod`` -> ``mod``.folder = folder) modList
        |> optionToResult UninstallModNotInstalled

    let private uninstallPerform modList tpfPath folder =
        getModByFolder modList folder
        >>= switchTee (function {folder = Folder folder} -> Directory.Delete(Path.Combine(tpfPath, folder), true))

    let uninstall modList tpfPath folder =
        uninstallPerform modList tpfPath folder
        >>= switch (removeModFromModList modList)
        >>= switch (tee saveModList)

    let upgrade modList tpfPath folder modArchivePath =
        uninstall modList tpfPath folder
        |> doubleMap id (function err -> UninstallError err)
        >>= ((function modList -> install modList tpfPath modArchivePath) >> doubleMap id (function err -> InstallError err))