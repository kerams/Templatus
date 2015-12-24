namespace Templatus

open System
open Templatus.Core
open Chessie.ErrorHandling
open Nessos.Argu

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
    let getTemplateNames (parsedArgs: ParseResults<Args>) =
        match parsedArgs.GetResults <@ Templates @> with
        | [] -> parsedArgs.Usage (message = "No templates provided.\nUsage:") |> fail
        | list -> pass list

    let getTemplateParameters (parsedArgs: ParseResults<Args>) =
        match parsedArgs.TryGetResult <@ TemplateParameters @> with
        | Some parameters -> parameters.Split ';'
                             |> List.ofArray
                             |> List.map (fun p -> p.Split '=')
                             |> List.choose (fun ps -> if ps.Length <> 2 then None else Some (ps.[0], ps.[1]))
        | None -> []

    let getDegreeOfParallelism (parsedArgs: ParseResults<Args>) =
        match parsedArgs.TryGetResult <@ Parallelization @> with
        | Some n -> if n < 1 then 1 else n
        | None -> 1

    [<EntryPoint>]
    let main _ =
        let results = ArgumentParser.Create<Args>().Parse(ignoreUnrecognized = true, raiseOnUsage = false)
        let parameters = getTemplateParameters results
        let parallelism = getDegreeOfParallelism results

        let processChunk list = list |> List.map TemplateParser.parse
                                |> List.map (bind (Processor.processTemplate TemplateParser.parse))
                                |> List.map (bind (OutputGenerator.generate parameters))
                                |> Async.singleton
                   
        printfn "Degree of parallelism: %d" parallelism
        printfn "Starting..."

        let createOutput = results |> getTemplateNames
                           >>= Utils.checkTemplatesExist
                           |> lift (List.splitInto parallelism)
                           |> lift (List.map processChunk)
                           |> lift (Async.Parallel >> Async.RunSynchronously)
                           |> lift (List.ofArray >> List.concat)
                           >>= collect

        match createOutput with
        | Ok _ -> printfn "All templates processed successfully."; 0
        | Bad reasons -> reasons |> List.iter (eprintfn "%s"); 1