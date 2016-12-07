namespace TPFModManager

open TPFModManager.Types

module private ApiHelper =
    let convertModInfo modInfo =
        let {name = name; url = url; version = version; fileUrl = fileUrl} = modInfo
        new Api.ModInfo(name, url, version, fileUrl)

    let convertModDownloadedInfo modDownloadedInfo =
        let {name = name; url = url; version = version; zipPath = zipPath} = modDownloadedInfo
        new Api.ModDownloadedInfo(name, url, version, zipPath)

    let convertExtractInfo modExtractInfo =
        let {modDownloadedInfo={name = name; url = url; version = version; zipPath = zipPath}; extractPath = extractPath; folder = folder} = modExtractInfo
        new Api.ModExtractInfo(name, url, version, zipPath, extractPath, folder)

    let convertMod _mod =
        let {name = name; url = url; websiteVersion = version; folder = folder} = _mod
        new Api.Mod(name, url, version, folder)

    let deconvertMod (_mod :Api.Mod) =
        {name = _mod.Name; url = _mod.Url; websiteVersion = _mod.WebsiteVersion; folder = _mod.Folder}

    let convertSettings (settings :Settings) =
        let {tpfModPath = tpfModPath; deleteZips = deleteZips} = settings
        new Api.Settings(tpfModPath, deleteZips)

    let deconvertSettings (settings :Api.Settings) =
        {tpfModPath = settings.TpfModPath; deleteZips = settings.DeleteZips}