namespace TpfModManager

open FSharp.Data
open ModList
open RegexHelper

module TpfNet =
    type TpfNetCsv = CsvProvider<"ID|UID|UNAME|MODTITLE|IMAGEID|TIME|LASTEDITTIME|VERSION|STATUS|FILEID|FILENAME|CATEGORYID|DOWNLOADTOTAL|DOWNLOAD\n1|1|example|example|example|1|1|1.0.0|example|1|example|1|1|1", "|">

    let getCSV() =
        match TpfNetOptions.csvPath with
        | "" -> None
        | csv ->
            try
                TpfNetCsv.Load csv |> Some
            with
            | :? System.Net.WebException -> None
            | :? System.IO.IOException -> None

    // TODO add error types
    let lookUpRemoteVersion (csv :TpfNetCsv) ``mod`` =
        let parseVersion version =
            match version with
            | Regex "^([0-9]+)\.([0-9]+)$" wellFormed ->
                Some {major = wellFormed.Item 0 |> System.Convert.ToInt32; minor = wellFormed.Item 1 |> System.Convert.ToInt32}
            | _ ->
                None

        let {Mod.tpfNetId = id} = ``mod``
        match id with
        | None -> None
        | Some id -> 
            let modRow =
                csv.Rows
                |> Seq.toList
                |> List.tryFind (function row -> row.ID = id)
            match modRow with
            | None ->
                None
            | Some modRow ->
                match parseVersion modRow.VERSION with
                | None -> None
                | Some version -> Some version