namespace TpfModManager

module ListHelper =
    let updateAll list item item' =
        List.map (fun i -> if i = item then item' else i) list

    (*let updateFirst list item item' =
        let fold (index, i) it =
            match index with
            | Some _ -> (index, i+1)
            | None ->
                if it = item then
                    (Some i, i+1)
                else
                    (None, i+1)
            
        let (index, _) = List.fold fold (None, 0) list
        match index with
        | Some i when i > 0 -> list.GetSlice(None, Some (i-1)) @ [item'] @ list.GetSlice(Some(i+1), None)
        | Some i -> [item'] @ list.GetSlice(Some(i+1), None)
        | None -> list*)