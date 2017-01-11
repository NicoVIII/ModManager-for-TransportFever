namespace TpfModManager

open MoonSharp.Interpreter
open MoonSharp.Interpreter.Loaders
open System.IO
open System.Text.RegularExpressions
open Tuple2Helper

module Lua =
    type LuaInfo = {name: string}

    let getInfoFromLuaFiles langKey path =
        let getNameFromFolder path =
            let callModLua modLua (script :Script) =
                (*let tpfPath =
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
                    DynValue.FromObject(script, info.Table.["info"]).Table.["name"]
                    |> string
                with
                    | :? SyntaxErrorException -> "not readable mod.info :("

            let getNameWithStringsLua (langKey :string) stringsLua modLua =
                let script = new Script()
                script.DoFile(stringsLua) |> ignore
                let value = script.Globals.["data"] |> script.Call 
                let translations = DynValue.FromObject(script, value.Table.[langKey]).Table
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
                        |> (+) <| "else return \"*\"..s end\nend"
                    script.DoString(luaFunction) |> ignore
                    callModLua modLua script
                else
                    printfn "%s" (Path.GetDirectoryName modLua)
                    "wtf happened?"

            let getNameWithoutStringsLua modLua =
                let script = new Script()
                script.DoString(@"function _ (s) return s; end") |> ignore
                callModLua modLua script

            let stringsLua = Path.Combine(path, "strings.lua")
            let modLua = Path.Combine(path, "mod.lua")
            if File.Exists stringsLua then
                getNameWithStringsLua langKey stringsLua modLua
            else
                getNameWithoutStringsLua modLua

        {name = getNameFromFolder path}

