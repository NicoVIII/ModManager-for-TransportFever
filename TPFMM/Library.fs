namespace TPFModManager

open FSharp.Data
open System
open System.Net

type private Url = Url of String
type private WebCode = WebCode of String

// Internal logic to provide API functionality
module private Internal =
    type Settings = JsonProvider<""" { "tpfPath": "/path/to/tpf" } """>

    // Mod Management
    type Mods =
        JsonProvider<""" { "installed_mods": [{ "name": "s", "url": "s", "minorVersion": 0, "websiteVersion": "s" }, { "name": "s", "url": "s", "minorVersion": 0, "websiteVersion": "s" } ] } """>
    
    type InstallStatus = | Installed | NotInstalled

    let loadModsFrom (path :string) =
        let mods = Mods.Load(path)
        mods.InstalledMods |> Array.toList

    let loadMods () = loadModsFrom "mods.json"

    let modStatus (Url url) =
        let mods = loadMods()
        let fold state (m :Mods.InstalledMod) = state || m.Url = url
        let installed = List.fold fold false mods
        match installed with
        | true -> Installed 
        | false -> NotInstalled

    // Functionality
    let cookieContainer = new CookieContainer()

    let acceptTerms (Url urlString) =
        let fold inputs input =
            let fold (n, v) (atr :HtmlAttribute) =
                match atr.Name() with
                | "name" -> (atr.Value(), v)
                | "value" -> (n, atr.Value())
                | _ -> (n, v)
            let atrPair = HtmlNode.attributes input |> List.fold fold ("", "")
            let (name, value) = atrPair
            printfn "%s-%s" name value
            atrPair::inputs

        let site = HtmlDocument.Load(urlString)
        let query = site.CssSelect("#content form input") |> List.fold fold []
        let action = site.CssSelect("#content form")
        match action with
        | [action] ->
            let action' = action |> HtmlNode.attribute "action"
         // Send HttpRequest
            Http.RequestString (action'.Value(), body = FormValues query, httpMethod="POST", cookieContainer=cookieContainer) |> ignore
        | _ -> failwith ("[Error] Confirmation of Terms failed! - "+urlString)


    let nameFromSite (Url urlString) =
        let source = Http.RequestString (urlString, cookieContainer=cookieContainer)
        let source' = HtmlDocument.Parse(source)
        let node = source'.CssSelect("#content header > h1 > a")
        match node with
        | [header] -> HtmlNode.innerText header
        | _ -> failwith "[Error] Unsupported layout of website! - "+urlString

    let downloadMod url =
        let (Url urlString) = url
        acceptTerms url
        printfn "%s" (nameFromSite url)

    let list () =
        printfn "%s" "Installed mods:"
        //printfn "%-60s %30s" "Name:" "Version:"
        let printMod (m :Mods.InstalledMod) =
            printfn "%-60s %30s" m.Name m.WebsiteVersion
        loadModsFrom "mods.json" |> List.sortBy (fun m -> m.Name) 
        |> List.iter printMod

    let install url =
        let (Url urlString) = url
        match modStatus url with
        | Installed ->
            printfn "[Error] A mod with url '%s' is already installed." urlString
        | NotInstalled ->
            downloadMod url

    let installAll urls =
        urls |> List.iter (fun url -> install url)

// API
type TPFMM =
    static member List = Internal.list
    static member Install url = Internal.install (Url url)
    static member InstallAll urls =
        Array.toList urls
        |> List.map (fun url -> Url url)
        |> Internal.installAll