namespace Templatus

open Templatus.Core
open Chessie.ErrorHandling

module Main =
    let checkArgumentCount args =
        match Array.length args with
        | 0 -> fail "No file provided."
        | _ -> pass args

    let getParsedTemplate (args: string []) =
        match TemplateParser.parse args.[0] with
        | Ok (result, _) -> pass result
        | Bad reasons -> "Template parsing failed: " :: reasons |> Bad

    [<EntryPoint>]
    let main argv = 
        match argv |> checkArgumentCount >>= getParsedTemplate with
        | Ok (result, _) -> printfn "%A" result; 0
        | Bad reasons -> reasons |> List.iter (eprintfn "%s"); 1