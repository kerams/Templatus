namespace Templatus

open Templatus.Core
open Chessie.ErrorHandling
open Argu

type Args =
    | [<CustomCommandLine("-t")>] Templates of string
    | [<CustomCommandLine("-p")>][<Rest>] TemplateParameters of string
    | [<CustomCommandLine("-parallelization")>] Parallelization of int
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Templates _ -> "path to the templates to process"
            | TemplateParameters _ -> "parameters to pass into the templates, i.e. -p age=3 name=Timmy"
            | Parallelization _ -> "the maximum number of templates processed in parallel"

module Main =
    let getTemplateNames (parsedArgs: ParseResults<Args>) =
        match parsedArgs.TryGetResult <@ Templates @> with
        | Some t -> t.Split ';' |> List.ofArray |> pass
        | None -> parsedArgs.Usage (message = "No templates provided.\nUsage:") |> fail

    let getTemplateParameters (parsedArgs: ParseResults<Args>) =
        parsedArgs.GetResults <@ TemplateParameters @>
        |> List.map (fun p -> p.Split([|'='|], 2))
        |> List.choose (fun ps -> if ps.Length <> 2 then None else Some (ps.[0], ps.[1]))

    let getDegreeOfParallelism (parsedArgs: ParseResults<Args>) =
        match parsedArgs.TryGetResult <@ Parallelization @> with
        | Some n -> if n < 1 then 1 else n
        | None -> 1

    [<EntryPoint>]
    let main _ =
        let parsedArgs = ArgumentParser.Create<Args>().Parse(ignoreUnrecognized = true, raiseOnUsage = false)
        let parameters = getTemplateParameters parsedArgs
        let parallelism = getDegreeOfParallelism parsedArgs

        let processList = List.map TemplateParser.parse
                          >> List.map (bind (Processor.processTemplate TemplateParser.parse))
                          >> List.map (bind (OutputGenerator.generate parameters))
                          >> Async.singleton

        printfn "Degree of parallelism: %d" parallelism
        printfn "Starting..."

        let createOutput = parsedArgs |> getTemplateNames
                           >>= Utils.checkTemplatesExist
                           |> lift (List.splitInto parallelism)
                           |> lift (List.map processList)
                           |> lift (Async.Parallel >> Async.RunSynchronously)
                           |> lift (List.ofArray >> List.concat)
                           >>= collect

        match createOutput with
        | Ok _ -> printfn "All templates processed successfully."; 0
        | Bad reasons -> reasons |> List.iter (eprintfn "%s"); 1