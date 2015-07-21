namespace Templatus.Core

open System
open System.IO
open System.Text
open System.Text.RegularExpressions
open Chessie.ErrorHandling
open Microsoft.FSharp.Compiler.Interactive.Shell

module OutputGenerator =
    let private prep outputFileName = [
        "open System"
        "let templateOutputFile = new System.IO.StreamWriter \"" + outputFileName + "\""
        "let tprintf o = sprintf \"%O\" o |> templateOutputFile.Write"
        "let tprintfn o = sprintf \"%O\" o |> templateOutputFile.WriteLine" ]

    let private finish = [ "templateOutputFile.Close ()" ]

    let private failed = [ "sprintf \"%s---FAILED---\" Environment.NewLine |> tprintf" ]

    let private prepareControl = function
        | ControlBlock block -> block.Replace("\t", "    ") // FSI complains about tabs
        | ControlExpression expr -> expr.Replace("\t", "    ") |> sprintf "%s |> tprintf"

    let private normalizeRegex = new Regex (@"\r\n|\n\r|\n|\r", RegexOptions.Compiled)

    let private prepareLiteral text =
        normalizeRegex.Replace(text, Environment.NewLine).Replace(@"\", @"\\").Replace("\"", "\\\"")
        |> sprintf "tprintf \"%s\""

    let rec private prepareTemplateForEval processedTemplate =
        let assemblyReferences = processedTemplate.AssemblyReferences |> List.map (sprintf "#r @\"%s\"")

        let toEval =
            processedTemplate.ProcessedTemplateParts
            |> List.map (fun p -> match p with
                                  | ProcessedLiteral (Literal l) -> [prepareLiteral l]
                                  | ProcessedControl c -> [prepareControl c]
                                  | ProcessedInclude i -> prepareTemplateForEval i)
            |> List.concat

        List.append assemblyReferences toEval

    let generate processedTemplate =
        let sbOut = StringBuilder ()
        let sbErr = StringBuilder ()

        let cfg = FsiEvaluationSession.GetDefaultConfiguration()
        let fsi = FsiEvaluationSession.Create(cfg, [|"--noninteractive"|], new StringReader (""), new StringWriter (sbOut), new StringWriter (sbErr))

        prep processedTemplate.Output |> List.iter fsi.EvalInteraction

        let preparedTemplate = prepareTemplateForEval processedTemplate |> List.toArray

        let currInstr = ref 0; 
        try
             preparedTemplate
             |> Array.iter (fun x -> x |> fsi.EvalInteraction
                                     incr currInstr)
        with _ -> failed |> List.iter fsi.EvalInteraction

        finish |> List.iter fsi.EvalInteraction

        let errors = sbErr.ToString ()

        match errors.Length with
        | 0 -> pass ()
        | _ -> sprintf "Expression: %s\n%s" preparedTemplate.[!currInstr] (errors.Trim()) |> fail