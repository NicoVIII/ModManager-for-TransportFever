namespace TpfModManager

open MoonSharp.Interpreter
open MoonSharp.Interpreter.Loaders
open System
open System.IO
open System.Text.RegularExpressions
open Types
open Tuple2Helper

module Lua =
    type LuaInfo = {name: string; authors: Author list; minorVersion: int; tpfNetId: int option}

    let private getScriptWithoutStringsLua =
        let script = new Script()
        script.DoString(@"function _ (s) return s; end") |> ignore
        script

    let private getScriptWithStringsLua (langKey :string) stringsLua modLua =
        let script = new Script()
        script.DoFile(stringsLua) |> ignore
        let value = script.Globals.["data"] |> script.Call 
        let translations = value.Table.Get(langKey).Table
        if not (translations = null) then
            // Build custom _-function
            let luaFunction =
                let buildIfClause (beginning, first) (keyValuePair :TablePair) =
                    let escape (s :string) =
                        s.Replace("\n", "\\n").Replace("\"", "\\\"")

                    let key (pair :TablePair) =
                        pair.Key.String
                        |> escape

                    let value (pair :TablePair) =
                        pair.Value.String
                        |> escape

                    let ifClause = "if s == \""+(key keyValuePair)+"\" then return \""+(value keyValuePair)+"\"\n"
                    if first then
                        (beginning + ifClause, false)
                    else
                        (beginning + "else"+ifClause, false)

                translations.Pairs
                |> Seq.toList
                |> List.fold buildIfClause ("function _(s)\n", true)
                |> first
                |> (+) <| "else return s end\nend"
            script.DoString(luaFunction) |> ignore
            script
        else
            getScriptWithoutStringsLua

    let private callModLua modLua (script :Script) =
        (* let tpfPath =
            modLua
            |> Path.GetDirectoryName
            |> Path.GetDirectoryName
            |> Path.GetDirectoryName
        let scriptPath = Path.Combine(tpfPath, "res", "scripts").Replace(Path.DirectorySeparatorChar, '/')
        printfn "%s" scriptPath
        (script.Options.ScriptLoader :?> ScriptLoaderBase).ModulePaths <- [|"../"|]*)

        //script.DoFile modLua |> ignore
        // HACK removes all requires for now
        try
            let file = File.ReadAllText modLua
            Regex.Replace(file, "require .*", "")
            |> script.DoString |> ignore

            let info =
                script.Globals.["data"]
                |> script.Call
            info.Table.Get("info").Table
            |> Some
        with
            | :? SyntaxErrorException -> None

    let private getInfo langKey path =
        let stringsLua = Path.Combine(path, "strings.lua")
        let modLua = Path.Combine(path, "mod.lua")
        if File.Exists stringsLua then
            getScriptWithStringsLua langKey stringsLua modLua
            |> callModLua modLua
        else
            getScriptWithoutStringsLua
            |> callModLua modLua

    let private getTpfNetId table =
        let getTpfNetIdWithIdentifier (table :Table) (id :string) =
            let field = table.Get(id)
            if field.IsNil() then
                None
            else
                match int (field.Number) with
                | id when id > 0 -> Some id
                | id -> None
        
        ["tfnetId"]
        |> List.map (getTpfNetIdWithIdentifier table)
        |> List.fold (fun result id ->
                match result with
                | Some _ -> result
                | None -> id
            ) None

    let getInfoFromLuaFiles langKey path =
        let info = getInfo langKey path
        match info with
        | None -> None
        | Some info ->
            let name = info.Get("name").String
            let authors =
                try
                    info.Get("authors").Table.Values
                    |> Seq.toList
                    |> List.fold (fun authors author ->
                            let tpfNetId = getTpfNetId (author.Table)
                            let author = {Author.name = author.Table.Get("name").String; tpfNetId = tpfNetId}
                            author::authors
                        ) []
                    |> List.rev
                with
                | :? NullReferenceException -> []
            let minor = info.Get("minorVersion").Number |> int
            let tpfNetId = getTpfNetId info

            Some {name = name; authors = authors; minorVersion = minor; tpfNetId = tpfNetId}