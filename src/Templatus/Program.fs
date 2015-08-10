namespace Templatus

open System
open Templatus.Core
open Chessie.ErrorHandling
open Nessos.UnionArgParser

type Args =
    | [<AltCommandLine("-t")>] Templates of string
    | [<CustomCommandLine("--params")>][<AltCommandLine("-p")>] TemplateParameters of string
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Templates _ -> "path to the template to process"
            | TemplateParameters _ -> "parameters to pass in the templates, i.e. --params age=3;name=Timmy"

module Main =
    let getTemplateNames (parsedArgs: ArgParseResults<Args>) =
        match parsedArgs.GetResults <@ Templates @> with
        | [] -> parsedArgs.Usage (message = "No templates provided.\nUsage:") |> fail
        | list -> pass list

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

        let createOutput = results |> getTemplateNames
                           >>= Utils.checkTemplatesExist
                           >>= (List.map TemplateParser.parse >> collect)
                           >>= (List.map (Processor.processTemplate TemplateParser.parse) >> collect) 
                           >>= (List.map (OutputGenerator.generate (getTemplateParameters results)) >> collect)

        match createOutput with
        | Ok _ -> 0
        | Bad reasons -> reasons |> List.iter (eprintfn "%s"); 1