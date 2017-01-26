namespace TpfModManager

open FSharp.Data

module TransportFeverNet =
    type TPFnetCSV = CsvProvider<"ID|UID|UNAME|MODTITLE|IMAGEID|TIME|LASTEDITTIME|VERSION|STATUS|FILEID|FILENAME|CATEGORYID|DOWNLOADTOTAL|DOWNLOAD\n1|1|example|example|example|1|1|1.0.0|example|1|example|1|1|1", "|">

    let getCSV() =
        match TPFnet.csvPath with
        | "" -> None
        | csv -> TPFnetCSV.Load csv |> Some

    //let updateRemoteVersion modList csv =
