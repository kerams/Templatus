namespace Templatus

open Templatus.Core

module Main =
    [<EntryPoint>]
    let main argv = 
        match argv.Length with
        | 0 -> eprintfn "No file provided"; 1
        | _ -> match TemplateParser.parse argv.[0] with
               | Success result -> printfn "%A" result; 0
               | Failure reason -> eprintfn "Template parsing failed: %s" reason; 2