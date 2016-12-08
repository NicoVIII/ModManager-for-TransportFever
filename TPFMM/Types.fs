namespace TPFModManager

open FSharp.Data

module private Types =
    type ModInfo =
        {name :string; version :string; url :string; fileUrl :string; title :string}
    type ModInfoBytes =
        {name :string; version :string; url :string; bytes: byte []; title :string}
    type ModDownloadedInfo =
        {name :string; version :string; url :string; archivePath :string}
    type ModExtractInfo =
        {modDownloadedInfo :ModDownloadedInfo; extractPath :string; folder :string}
    type Mod =
        {name :string; websiteVersion :string; url :string; folder :string}

    type Settings =
        {tpfModPath :string; deleteArchives :bool}

    type Url = Url of string
    type UrlCode = {url :string; source :HtmlDocument}