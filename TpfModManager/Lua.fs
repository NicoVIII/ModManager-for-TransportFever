namespace TpfModManager

open MoonSharp.Interpreter
open MoonSharp.Interpreter.Loaders
open System.IO
open System.Text.RegularExpressions

module Lua =
    type LuaInfo = {name: string; author: string}

    let getNameWithoutStringsLua modLua =
        let script = new Script()
        // TODO add support for require
        (*let tpfPath =
            modLua
            |> Path.GetDirectoryName
            |> Path.GetDirectoryName
            |> Path.GetDirectoryName
        let scriptPath = Path.Combine(tpfPath, "res", "scripts").Replace(Path.DirectorySeparatorChar, '/')
        printfn "%s" scriptPath
        (script.Options.ScriptLoader :?> ScriptLoaderBase).ModulePaths <- [|"../"|]*)

        script.DoString(@"function _ (s) return s; end") |> ignore
        let file = File.ReadAllText modLua
        Regex.Replace(file, "require .*", "")
        |> script.DoString |> ignore//*)
        //script.DoFile modLua |> ignore
        let info =
            script.Globals.["data"]
            |> script.Call
        DynValue.FromObject(script, info.Table.["info"]).Table.["name"]
        |> string

    let getNameFromFolder path =
        let stringsLua = Path.Combine(path, "strings.lua")
        let modLua = Path.Combine(path, "mod.lua")
        if File.Exists stringsLua then
            (*let script = new Script()
            script.DoFile(stringsLua) |> ignore
            let value = script.Globals.["data"] |> script.Call 
            let translations = DynValue.FromObject(script, value.Table.["en"]).Table*)
            ""
        else
            getNameWithoutStringsLua modLua