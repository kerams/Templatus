namespace Templatus.Core

open System
open System.IO
open System.Text.RegularExpressions
open Chessie.ErrorHandling
open Microsoft.FSharp.Compiler.Interactive.Shell

module OutputGenerator =
    let private prep templateParameters =
        let basis = [
            "open System"
            """let mutable _indentStr = "" """
            "let mutable _indent: string list = []"
            """let _rebuildIndentStr () = _indentStr <- _indent |> List.fold (+) "" """
            "let pushIndent str = _indent <- str :: _indent; _rebuildIndentStr ()"
            "let popIndent () = (_indent <- match _indent with [] -> [] | _ :: t -> t); _rebuildIndentStr ()"
            "let clearIndent () = _indent <- []; _rebuildIndentStr ()"
            "let _output = Text.StringBuilder ()"
            """let tprint o = sprintf "%O" o |> _output.Append |> ignore"""
            """let tprintn o = sprintf "%s%O" _indentStr o |> _output.AppendLine |> ignore"""
            """let tprintf format = Printf.kprintf (sprintf "%s%s" _indentStr >> _output.Append >> ignore) format"""
            """let tprintfn format = Printf.kprintf (sprintf "%s%s" _indentStr >> _output.AppendLine >> ignore) format""" ]

        templateParameters
        |> List.map (fun (name, value) -> sprintf """let %s = "%s" """ name value)
        |> (@) basis

    let private finish (outputFileName: string) =
        [ "IO.File.WriteAllText(\"" + outputFileName.Replace(@"\", @"\\") + "\", _output.ToString ())" ]

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

        processedTemplate.ProcessedTemplateParts
        |> List.map (fun p -> match p with
                              | ProcessedLiteral (Literal l) -> [prepareLiteral l]
                              | ProcessedControl c -> [prepareControl c]
                              | ProcessedInclude i -> prepareTemplateForEval i)
        |> List.concat
        |> (@) assemblyReferences

    let generate templateParameters processedTemplate =
        match processedTemplate.OutputFile with
        | None -> sprintf "Template %s specifies no output file -- missing output directive." processedTemplate.Name |> fail
        | Some f ->
            use out = new StringWriter ()
            use err = new StringWriter ()

            let cfg = FsiEvaluationSession.GetDefaultConfiguration ()
            use fsi = FsiEvaluationSession.Create (cfg, [|"--noninteractive"|], new StringReader (""), out, err)

            prep templateParameters |> List.iter fsi.EvalInteraction

            let preparedTemplate = prepareTemplateForEval processedTemplate |> List.toArray

            let lastExpr = Utils.countSuccessfulOperations fsi.EvalInteraction preparedTemplate

            if lastExpr = preparedTemplate.Length
            then finish f |> List.iter fsi.EvalInteraction; pass ()
            else sprintf "Expression: %s\n\n%s\n" preparedTemplate.[lastExpr] (err |> trimFsiErrors) |> fail