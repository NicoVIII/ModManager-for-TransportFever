namespace TPFModManager.Api

[<AllowNullLiteral>]
type ModInfo (name :string, url :string, version :string, fileUrl :string) =
    member this.Name = name
    member this.Url = url
    member this.Version = version
    member this.FileUrl = fileUrl

[<AllowNullLiteral>]
type ModDownloadedInfo (name :string, url :string, version :string, zipPath :string) =
    member this.Name = name
    member this.Url = url
    member this.Version = version
    member this.ZipPath = zipPath

[<AllowNullLiteral>]
type ModExtractInfo (name :string, url :string, version :string, zipPath :string, extractPath :string, folder :string) =
    member this.Name = name
    member this.Url = url
    member this.Version = version
    member this.ZipPath = zipPath
    member this.ExtractPath = extractPath
    member this.Folder = folder

[<AllowNullLiteral>]
type Mod (name :string, url :string, websiteVersion :string, folder :string) =
    member this.Name = name
    member this.Url = url
    member this.WebsiteVersion = websiteVersion
    member this.Folder = folder

[<AllowNullLiteral>]
type Settings (tpfModPath :string, deleteZips :bool) =
    member this.TpfModPath = tpfModPath
    member this.DeleteZips = deleteZips