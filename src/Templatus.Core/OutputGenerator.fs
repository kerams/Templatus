namespace Templatus.Core

open System.IO
open System.Text
open Chessie.ErrorHandling
open Microsoft.FSharp.Compiler.Interactive.Shell

module OutputGenerator =
    let prep outputFileName = [
        "open System;;"
        "let templateOutputFile = new System.IO.StreamWriter \"" + outputFileName + "\";;"
        "let tprintf object = templateOutputFile.Write <| sprintf \"%O\" object;;"
        "let tprintfn object = templateOutputFile.WriteLine <| sprintf \"%O\" object;;" ]

    let finish = [ "templateOutputFile.Close ();;" ]

    let generate processedTemplate =
        let sbOut = StringBuilder ()
        let sbErr = StringBuilder ()

        let cfg = FsiEvaluationSession.GetDefaultConfiguration()
        let fsi = FsiEvaluationSession.Create(cfg, [|"--noninteractive"|], new StringReader (""), new StringWriter (sbOut), new StringWriter (sbErr))

        prep processedTemplate.Directives.Output.Head |> List.iter fsi.EvalInteraction
        fsi.EvalInteraction """tprintfn "Hello from OutputGenerator!";;"""
        finish |> List.iter fsi.EvalInteraction

        let errors = sbErr.ToString ()

        match errors.Length with
        | 0 -> pass ()
        | _ -> fail errors