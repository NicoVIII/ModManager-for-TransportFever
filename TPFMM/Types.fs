namespace TPFModManager

open FSharp.Data

module private Types =
    type ModInfo =
        {name :string; version :string; url :string; fileUrl :string}
    type ModInfoBytes =
        {name :string; version :string; url :string; bytes: byte []}
    type ModDownloadedInfo =
        {name :string; version :string; url :string; zipPath :string}
    type ModExtractInfo =
        {modDownloadedInfo :ModDownloadedInfo; extractPath :string; folder :string}
    type Mod =
        {name :string; websiteVersion :string; url :string; folder :string}

    type Url = Url of string
    type UrlCode = {url :string; source :HtmlDocument}