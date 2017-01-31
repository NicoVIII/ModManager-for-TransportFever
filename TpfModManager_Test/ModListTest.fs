namespace TpfModManager

open ModList
open System
open Types
open NUnit.Framework
open FsUnit
open FsCheck

[<TestFixture>]
type Test() =
    let (.=.) left right = left = right |@ sprintf "%A = %A" left right

    [<Test>]
    member x.``test changing of TpfNetId``() =
        let property tpfNetId modList folder ``mod`` =
            // No double folders
            let modList' =
                modList
                |> List.toSeq
                |> Seq.distinctBy (fun {ModList.Mod.folder = folder} -> folder)
                |> Seq.toList
            let folder' =
                match folder with
                | null -> ""
                | folder -> folder
            let mod' = {``mod`` with ModList.Mod.folder = Folder folder'}

            let {ModList.Mod.folder = folder} = mod'
            ModList.changeTpfNetId tpfNetId folder (mod'::modList') .=. {mod' with tpfNetId = Some tpfNetId}::modList'

        Check.QuickThrowOnFailure property