namespace TPFModManager

open FSharp.Data
open RobHelper
open System.Net
open System.Text.RegularExpressions
open Types

module private TransportFeverNet =
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
                Http.RequestString (urlString, cookieContainer=cookieContainer)
                |> HtmlDocument.Parse
            | _ -> failwith ("[Error] Confirmation of Terms failed! - "+urlString)
        else
            site
    
    let getSite url =
        let (Url urlString) = url
        try
            let site = Http.RequestString (urlString, cookieContainer=cookieContainer) |> HtmlDocument.Parse
            Ok ({url = urlString; source = acceptTerms site url})
        with
        | :? System.Net.WebException as ex -> Error [NoConnection]
    
    let private getName (source :HtmlDocument) =
        let node = source.CssSelect("#content header > h1 > a")
        match node with
        | [header] ->
            let name = HtmlNode.innerText header
            Ok (name.Replace ("/", "_"))
        | _ ->
            Error [UnsupportedLayout]

    let private getVersion (source :HtmlDocument) =
        let node = source.CssSelect(".messageBody")
        match node with
        | [node] ->
            let text = node.ToString()
            let m = Regex.Match(text, @"<dt>[\sA-z0-9]*?[Vv]ersion[\sA-z0-9]*?</dt>[\s\r\n]*<dd>[\s\r\n]*(.*?)[\s\r\n]*</dd>")
            match m.Success with
            | true ->
                Ok m.Groups.[1].Value
            | false ->
                Error [NoVersionOnWebsite]
        | _ ->
            Error [NoVersionOnWebsite]

    let private getFilePath (source :HtmlDocument) =
        let node = source.CssSelect(".filebaseFileList h3 > a")
        match node with
        | [node] ->
            let title =(node.Attribute "title").Value ()
            let href = (node.Attribute "href").Value ()
            if title.EndsWith ".zip" then
                Ok href
            else
                let (extension::_) = title.Split '.' |> List.ofArray |> List.rev
                Error [NotSupportedFormat extension]
        | _ ->
            Error [MoreThanOneFile]

    let extractInfo {url = url; source = source} =
        match getName source, getVersion source, getFilePath source with
        | Ok n, Ok v, Ok f -> Ok {name=n; version=v; url=url; fileUrl=f}
        | Error e, Ok _, Ok _ | Ok _, Error e, Ok _ | Ok _, Ok _, Error e -> Error e
        | Error e1, Error e2, Ok _ | Error e1, Ok _, Error e2 | Ok _, Error e1, Error e2 -> Error (e1 @ e2)
        | Error e1, Error e2, Error e3 -> Error (e1 @ e2 @ e3)

    let getInfo =
        getSite
        >> bind extractInfo

    let getFileBytes modInfo =
        let {name = name; version = version; url = url; fileUrl = fileUrl} = modInfo
        match Http.Request(fileUrl, cookieContainer=cookieContainer).Body with
        | Text text ->
            Error [NotBinary]
        | Binary bytes ->
            Ok {name = name; version = version; url = url; bytes = bytes}