namespace Templatus.Core

open System.IO
open Chessie.ErrorHandling

module Utils =
    let checkTemplatesExist templateNames =
        let inexistent = templateNames
                         |> List.filter (not << File.Exists)

        match inexistent with
        | [] -> pass templateNames
        | _ -> inexistent
               |> String.concat ", "
               |> sprintf "Cannot find templates %s."
               |> fail

    let countSuccessfulOperations f input =
        let last = Array.length input

        let rec countInner current =
            if last = current
            then current
            else
                let success =
                    try
                        f input.[current]
                        true
                    with _ -> false
                        
                if success then countInner (current + 1) else current

        countInner 0