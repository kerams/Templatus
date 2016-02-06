namespace Templatus.Core

open System
open System.IO
open System.Text.RegularExpressions
open Chessie.ErrorHandling
open Microsoft.FSharp.Compiler.Interactive.Shell

module OutputGenerator =
    let private prep (outputFileName: string) templateParameters =
        let basis = [
            "open System"
            """let mutable _indentStr = "" """
            "let mutable _indent: string list = []"
            """let _rebuildIndentStr () = _indentStr <- _indent |> List.fold (+) "" """
            "let pushIndent str = _indent <- str :: _indent; _rebuildIndentStr ()"
            "let popIndent () = (_indent <- match _indent with [] -> [] | _ :: t -> t); _rebuildIndentStr ()"
            "let clearIndent () = _indent <- []; _rebuildIndentStr ()"
            "let _output = new IO.StreamWriter \"" + outputFileName.Replace(@"\", @"\\") + "\""
            """let tprint o = sprintf "%O" o |> _output.Write"""
            """let tprintn o = sprintf "%s%O" _indentStr o |> _output.WriteLine"""
            "let tprintf format = fprintf _output format"
            """let tprintfn format = Printf.kprintf (fprintfn _output "%s%s" _indentStr) format""" ]

        let parameters = templateParameters |> List.map (fun (name, value) -> sprintf """let %s = "%s" """ name value)

        List.append basis parameters

    let private finish = [ "_output.Close ()" ]

    let private prepareControl = function
        | ControlBlock block -> block.Replace("\t", "    ") // FSI complains about tabs
        | ControlExpression expr -> expr.Replace("\t", "    ") |> sprintf "%s |> tprint"

    let private newlineRegex = new Regex (@"\r\n|\n\r|\n|\r", RegexOptions.Compiled)

    let private fsiStripperRegex = new Regex(@"input\.fsx\(\d+,\d+\):\s*")

    let private trimFsiErrors input = fsiStripperRegex.Replace(input.ToString().Trim(), "")

    let private prepareLiteral text =
        newlineRegex.Replace(text, Environment.NewLine).Replace("\"", "\"\"")
        |> sprintf """tprint @"%s" """

    let rec private prepareTemplateForEval processedTemplate =
        let assemblyReferences = processedTemplate.AssemblyReferences |> List.map (sprintf """#r @"%s" """)

        let toEval =
            processedTemplate.ProcessedTemplateParts
            |> List.map (fun p -> match p with
                                  | ProcessedLiteral (Literal l) -> [prepareLiteral l]
                                  | ProcessedControl c -> [prepareControl c]
                                  | ProcessedInclude i -> prepareTemplateForEval i)
            |> List.concat

        List.append assemblyReferences toEval

    let generate templateParameters processedTemplate =
        match processedTemplate.OutputFile with
        | None -> sprintf "Template %s specifies no output file -- missing output directive." processedTemplate.Name |> fail
        | Some f ->
            use out = new StringWriter ()
            use err = new StringWriter ()

            let cfg = FsiEvaluationSession.GetDefaultConfiguration ()
            use fsi = FsiEvaluationSession.Create (cfg, [|"--noninteractive"|], new StringReader (""), out, err)

            prep f templateParameters |> List.iter fsi.EvalInteraction

            let preparedTemplate = prepareTemplateForEval processedTemplate |> List.toArray

            let lastExpr = Utils.countSuccessfulOperations fsi.EvalInteraction preparedTemplate

            finish |> List.iter fsi.EvalInteraction

            if lastExpr = preparedTemplate.Length
            then pass ()
            else sprintf "Expression: %s\n\n%s\n" preparedTemplate.[lastExpr] (err |> trimFsiErrors) |> fail