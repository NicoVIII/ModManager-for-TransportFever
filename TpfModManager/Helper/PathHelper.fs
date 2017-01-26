namespace TpfModManager

open System.IO

module PathHelper =
    let combine path1 path2 =
        Path.Combine(path1, path2)

    let combineReverse path2 path1 =
        Path.Combine(path1, path2)