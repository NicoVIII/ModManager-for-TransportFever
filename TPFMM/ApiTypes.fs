namespace TPFModManager.Api

[<AllowNullLiteral>]
type ModInfo (name :string, url :string, version :string, fileUrl :string, title :string) =
    member this.Name = name
    member this.Url = url
    member this.Version = version
    member this.FileUrl = fileUrl
    member this.Title = title

[<AllowNullLiteral>]
type ModDownloadedInfo (name :string, url :string, version :string, archivePath :string) =
    member this.Name = name
    member this.Url = url
    member this.Version = version
    member this.ArchivePath = archivePath

[<AllowNullLiteral>]
type ModExtractInfo (name :string, url :string, version :string, archivePath :string, extractPath :string, folder :string) =
    member this.Name = name
    member this.Url = url
    member this.Version = version
    member this.ArchivePath = archivePath
    member this.ExtractPath = extractPath
    member this.Folder = folder

[<AllowNullLiteral>]
type Mod (name :string, url :string, websiteVersion :string, folder :string) =
    member this.Name = name
    member this.Url = url
    member this.WebsiteVersion = websiteVersion
    member this.Folder = folder

[<AllowNullLiteral>]
type Settings (tpfModPath :string, deleteArchives :bool) =
    member this.TpfModPath = tpfModPath
    member this.DeleteArchives = deleteArchives