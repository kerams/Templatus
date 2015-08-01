namespace Templatus.Core

open System.IO
open Chessie.ErrorHandling

module Utils =
    let checkTemplateExists templateName =
        if File.Exists templateName then pass templateName else sprintf "Cannot find template %s." templateName |> fail

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