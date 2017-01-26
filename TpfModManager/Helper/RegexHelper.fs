﻿namespace TpfModManager

open System.Text.RegularExpressions

module RegexHelper =
    let (|Regex|_|) pattern input =
        let m = Regex.Match(input, pattern)
        if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
        else None