namespace Templatus

open System
open Templatus.Core
open Chessie.ErrorHandling
open Nessos.UnionArgParser

type Args =
    | [<CustomCommandLine("-t")>] Templates of string
    | [<CustomCommandLine("-p")>] TemplateParameters of string
    | [<CustomCommandLine("-parallelization")>] Parallelization of int
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Templates _ -> "path to the template to process"
            | TemplateParameters _ -> "parameters to pass in the templates, i.e. --params age=3;name=Timmy"
            | Parallelization _ -> "degree of parallelism of template processing"

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

    let getDegreeOfParallelism (parsedArgs: ArgParseResults<Args>) =
        match parsedArgs.TryGetResult <@ Parallelization @> with
        | Some n -> if n < 1 then 1 else n
        | None -> 1

    open Chessie.ErrorHandling.AsyncExtensions

    [<EntryPoint>]
    let main _ =
        let results = UnionArgParser.Create<Args>().Parse(ignoreUnrecognized = true, raiseOnUsage = false)
        let parameters = getTemplateParameters results
        let parallelism = getDegreeOfParallelism results

        let processChunk list = async {
            return list |> List.map TemplateParser.parse
                   |> List.map (bind (Processor.processTemplate TemplateParser.parse))
                   |> List.map (bind (OutputGenerator.generate parameters)) }

        let createOutput = results |> getTemplateNames
                           >>= Utils.checkTemplatesExist
                           |> lift (List.splitInto parallelism)
                           |> lift (List.map processChunk)
                           |> lift (Async.Parallel >> Async.RunSynchronously)
                           |> lift (List.ofArray >> List.concat)
                           >>= collect

        match createOutput with
        | Ok _ -> 0
        | Bad reasons -> reasons |> List.iter (eprintfn "%s"); 1