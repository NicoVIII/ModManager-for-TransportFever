namespace TpfModManager

module OptionHelper =
    let unwrap some =
        match some with
        | None -> invalidOp "Only usable on Some values"
        | Some some -> some