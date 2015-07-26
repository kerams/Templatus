namespace Templatus

open Templatus.Core
open Chessie.ErrorHandling
open Nessos.UnionArgParser

type Args =
    | [<AltCommandLine("-t")>] Template of string
    | [<CustomCommandLine("--params")>][<AltCommandLine("-p")>] TemplateParameters of string
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Template _ -> "path to the template to process"
            | TemplateParameters _ -> "parameters to pass in the template, i.e. --params age=3;name=Timmy"

module Main =
    let getTemplateName (parsedArgs: ArgParseResults<Args>) =
        match parsedArgs.TryGetResult <@ Template @> with
        | Some s -> pass s
        | None -> parsedArgs.Usage (message = "No template provided.\nUsage:") |> fail

    let getTemplateParameters (parsedArgs: ArgParseResults<Args>) =
        match parsedArgs.TryGetResult <@ TemplateParameters @> with
        | Some parameters -> parameters.Split ';'
                             |> List.ofArray
                             |> List.map (fun p -> p.Split '=')
                             |> List.choose (fun ps -> if ps.Length <> 2 then None else Some (ps.[0], ps.[1]))
        | None -> []

    [<EntryPoint>]
    let main _ =
        let results = UnionArgParser.Create<Args>().Parse(ignoreUnrecognized = true, raiseOnUsage = false)

        let doStuff = results |> getTemplateName
                      >>= TemplateParser.parse
                      >>= Processor.processTemplate TemplateParser.parse
                      >>= OutputGenerator.generate (getTemplateParameters results)

        match doStuff with
        | Ok _ -> printfn "Aw yisss!"; 0
        | Bad reasons -> reasons |> List.iter (eprintfn "%s"); 1