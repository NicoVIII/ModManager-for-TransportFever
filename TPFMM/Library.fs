namespace TPFModManager

open FSharp.Data
open System
open System.IO
open System.Net
open System.Text.RegularExpressions

type private Url = Url of String
type private WebCode = WebCode of String

// Internal logic to provide API functionality
module private Internal =
    type Settings = JsonProvider<""" { "tpfPath": "/path/to/tpf" } """>

    // Mod Management
    type Mods =
        JsonProvider<""" { "installed_mods": [{ "name": "s", "url": "s", "websiteVersion": "s" }, { "name": "s", "url": "s", "websiteVersion": "s" } ] } """>
    
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

    let acceptTerms (site :HtmlDocument) (Url urlString) =
        let fold inputs input =
            let fold (n, v) (atr :HtmlAttribute) =
                match atr.Name() with
                | "name" -> (atr.Value(), v)
                | "value" -> (n, atr.Value())
                | _ -> (n, v)
            let atrPair = HtmlNode.attributes input |> List.fold fold ("", "")
            atrPair::inputs
        
        let header = site.CssSelect("header h1").ToString()
        if header.Contains "Terms" || header.Contains "Nutzungsbedingungen" then
            let query = site.CssSelect("#content form input") |> List.fold fold []
            let action = site.CssSelect("#content form")
            match action with
            | [action] ->
                let action' = action |> HtmlNode.attribute "action"
                // Send HttpRequest
                Http.RequestString (action'.Value(), body = FormValues query, cookieContainer=cookieContainer) |> ignore
                Http.RequestString (urlString, cookieContainer=cookieContainer) |> HtmlDocument.Parse
            | _ -> failwith ("[Error] Confirmation of Terms failed! - "+urlString)
        else
            site

    let getSite url =
        let (Url urlString) = url
        let site = Http.RequestString (urlString, cookieContainer=cookieContainer) |> HtmlDocument.Parse
        acceptTerms site url

    let nameFromSite (source :HtmlDocument) (Url urlString) =
        let node = source.CssSelect("#content header > h1 > a")
        match node with
        | [header] -> HtmlNode.innerText header
        | _ -> failwith "[Error] Unsupported layout of website! (name)"+urlString

    let versionFromSite (source :HtmlDocument) (Url urlString) =
        let node = source.CssSelect(".messageBody")
        match node with
        | [node] ->
            let text = node.ToString()
            let m = Regex.Match(text, @"<dt>.*?[Vv]ersion.*?</dt>[\s\r\n]*<dd>[\s\r\n]*(.*?)[\s\r\n]*</dd>")
            match m.Success with
            | true ->
                m.Groups.[1].Value
            | false -> failwith "[Error] Unsupported layout of website! (version)"+urlString
        | _ -> failwith "[Error] Unsupported layout of website! (version)"+urlString

    let filePathFromSite (source :HtmlDocument) (Url urlString) =
        let node = source.CssSelect(".filebaseFileList h3 > a")
        match node with
        | [node] ->
            let atr = node.Attribute "href"
            Some (atr.Value ())
        | _ ->
            printfn "[Error] Mods with more than one downloadable file are not supported yet. %s" urlString
            None

    let safeBytes (file :string) bytes =
        let directory = file.Split [| '/' |] |> Array.toList |> List.rev |> List.tail |> List.rev |> List.fold (fun path folder -> path+folder+"/") ""
        Directory.CreateDirectory directory |> ignore
        File.WriteAllBytes(file, bytes)
    
    let downloadMod url =
        let (Url urlString) = url
        let source = getSite url
        let name = nameFromSite source url
        let version = versionFromSite source url
        let filePath = filePathFromSite source url
        match filePath with
        | Some filePath ->
            printfn "%s - %s:" name version
            printf "* Downloading..."
            match Http.Request(filePath, cookieContainer=cookieContainer).Body with
            | Text text ->
                failwith "Invalid filepath!"
            | Binary bytes -> 
                safeBytes ("tmp/"+name+"-"+version+".zip") bytes
            printfn "\r%-16s" "* Downloaded."
        | None -> ()

    let installMod filePath =
        printf "* Installing... (not implemented yet)" |> ignore
        printfn "\r%-15s" "* Installed." |> ignore

    let downloadAndInstall url =
        let (Url urlString) = url
        match modStatus url with
        | Installed ->
            printfn "[Info] A mod with url '%s' is already installed." urlString
        | NotInstalled ->
            downloadMod url
        printfn ""

    let downloadAndInstallAll urls =
        urls |> List.iter (fun url -> downloadAndInstall url)

    let list () =
        printfn "%s" "Installed mods:"
        //printfn "%-60s %30s" "Name:" "Version:"
        let printMod (m :Mods.InstalledMod) =
            printfn "%-60s %30s" m.Name m.WebsiteVersion
        loadModsFrom "mods.json" |> List.sortBy (fun m -> m.Name) 
        |> List.iter printMod

// API
type TPFMM =
    static member List = Internal.list
    static member Install url = Internal.downloadAndInstall (Url url)
    static member InstallAll urls =
        Array.toList urls
        |> List.map (fun url -> Url url)
        |> Internal.downloadAndInstallAll