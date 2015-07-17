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

    let private prepareControlPart (text: string) =
        text.Replace("\t", "    ") // FSI complains about tabs

    let private normalizeRegex = new Regex (@"\r\n|\n\r|\n|\r", RegexOptions.Compiled)

    let private prepareLiteralPart text =
        normalizeRegex.Replace(text, Environment.NewLine) |> sprintf "tprintf \"%s\""

    let private prepareTemplateForEval processedTemplate =
        processedTemplate.FilteredTemplateParts
        |> List.map (fun p -> match p with
                              | LiteralPart (Literal l) -> prepareLiteralPart l
                              | ControlPart (Control c) -> prepareControlPart c
                              | DirectivePart _ -> failwith "DirectivePart not filtered out")

    let generate processedTemplate =
        let sbOut = StringBuilder ()
        let sbErr = StringBuilder ()

        let cfg = FsiEvaluationSession.GetDefaultConfiguration()
        let fsi = FsiEvaluationSession.Create(cfg, [|"--noninteractive"|], new StringReader (""), new StringWriter (sbOut), new StringWriter (sbErr))

        prep processedTemplate.Directives.Output.Head |> List.iter fsi.EvalInteraction

        try
            processedTemplate |> prepareTemplateForEval |> List.iter fsi.EvalInteraction
        with _ -> ()

        finish |> List.iter fsi.EvalInteraction

        let errors = sbErr.ToString ()

        match errors.Length with
        | 0 -> pass ()
        | _ -> fail errors