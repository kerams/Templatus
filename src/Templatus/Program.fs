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
        | Bad reasons -> Bad reasons

    let processParsedTemplate parser name parts =
        match Processor.processTemplate parser name parts with
        | Ok (result, _) -> pass result
        | Bad reasons -> Bad reasons

    let generateOutput processed =
        match OutputGenerator.generate processed with
        | Ok _ -> pass ()
        | Bad reasons -> Bad reasons

    [<EntryPoint>]
    let main argv = 
        let doStuff = argv |> checkArgumentCount >>= getParsedTemplate >>= (processParsedTemplate TemplateParser.parse argv.[0]) >>= generateOutput

        match doStuff with
        | Ok _ -> printfn "Aw yisss!"; 0
        | Bad reasons -> reasons |> List.iter (eprintfn "%s"); 1