namespace Templatus

open Templatus.Core
open Chessie.ErrorHandling

module Main =
    [<EntryPoint>]
    let main argv = 
        match argv.Length with
        | 0 -> eprintfn "No file provided"; 1
        | _ -> match TemplateParser.parse argv.[0] with
               | Ok (result, _) -> printfn "%A" result; 0
               | Bad reasons -> eprintf "Template parsing failed: %s" reasons.Head; 2