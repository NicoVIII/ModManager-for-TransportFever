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

    let safeBytes (file :string) bytes =
        let directory = file.Split [| '/' |] |> Array.toList |> List.rev |> List.tail |> List.rev |> List.fold (fun path folder -> path+folder+"/") ""
        Directory.CreateDirectory directory |> ignore
        File.WriteAllBytes(file, bytes)

    let safeString (file :string) str =
        let directory = file.Split [| '/' |] |> Array.toList |> List.rev |> List.tail |> List.rev |> List.fold (fun path folder -> path+folder+"/") ""
        if directory.Length = 0 then () else Directory.CreateDirectory directory |> ignore
        File.WriteAllText(file, str)

    let loadModsFrom (path :string) =
        let mods = Mods.Load(path)
        mods.InstalledMods |> Array.toList

    let loadMods() = loadModsFrom "mods.json"

    let safeModsTo (mods :Mods.InstalledMod list) (path :string) =
        let modsObj = new Mods.Root(Array.ofList mods)
        safeString path (modsObj.ToString())

    let safeMods mods = safeModsTo mods "mods.json"

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
            let m = Regex.Match(text, @"<dt>[\sA-z0-9]*?[Vv]ersion
            [\sA-z0-9]*?</dt>[\s\r\n]*<dd>[\s\r\n]*(.*?)[\s\r\n]*</dd>")
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
    
    let downloadMod (_mod :Mods.InstalledMod) filePath =
            printfn "%s - %s:" _mod.Name _mod.WebsiteVersion
            printf "* Downloading..."
            match Http.Request(filePath, cookieContainer=cookieContainer).Body with
            | Text text ->
                failwith "Invalid filepath!"
            | Binary bytes -> 
                safeBytes ("tmp/"+_mod.Name+"-"+_mod.WebsiteVersion+".zip") bytes
            printfn "\r%-16s" "* Downloaded."

    let installMod _mod =
        printf "* Installing... (not implemented yet)" |> ignore
        safeMods (_mod::loadMods())
        printfn "\r%-15s" "* Installed." |> ignore

    let downloadAndInstall url =
        let (Url urlString) = url
        match modStatus url with
        | Installed ->
            printfn "[Info] A mod with url '%s' is already installed." urlString
        | NotInstalled ->
            let (Url urlString) = url
            let source = getSite url
            let name = nameFromSite source url
            let version = versionFromSite source url
            let filePath = filePathFromSite source url
            match filePath with
            | Some filePath ->
                let _mod = new Mods.InstalledMod(name, urlString, version)
                downloadMod _mod filePath
                installMod _mod
            | None -> ()
        printfn ""

    let downloadAndInstallAll urls =
        urls |> List.iter (fun url -> downloadAndInstall url)

    let list () =
        printfn "%s" "Installed mods:"
        //printfn "%-50s %30s" "Name:" "Version:"
        let printMod (m :Mods.InstalledMod) =
            printfn "%-50s %20s" m.Name m.WebsiteVersion
        loadMods() |> List.sortBy (fun m -> m.Name) 
        |> List.iter printMod

// API
type TPFMM =
    static member List = Internal.list
    static member Install url = Internal.downloadAndInstall (Url url)
    static member InstallAll urls =
        Array.toList urls
        |> List.map (fun url -> Url url)
        |> Internal.downloadAndInstallAll