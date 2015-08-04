namespace Templatus.Core

open FParsec
open Chessie.ErrorHandling
open System

module TemplateParser =
    let maxCharsTillString str skipString =
        charsTillString str skipString System.Int32.MaxValue

    let pAssemblyReference: Parser<Directive, string> =
        skipStringCI "assembly" >>. spaces1 >>. skipStringCI "name=\"" >>. maxCharsTillString "\"" true |>> AssemblyReference

    let pInclude: Parser<Directive, string> =
        skipStringCI "include" >>. spaces1 >>. skipStringCI "file=\"" >>. maxCharsTillString "\"" true |>> Include

    let pOutput: Parser<Directive, string> =
        skipStringCI "output" >>. spaces1 >>. skipStringCI "filename=\"" >>. maxCharsTillString "\"" true |>> Output

    let pAnyDirective: Parser<Directive, string> =
        choice [ pAssemblyReference; pInclude; pOutput ]

    let pDirective: Parser<ParsedTemplatePart, string> =
        skipString "<#@" >>. spaces >>. pAnyDirective .>> spaces .>> skipString "#>" .>> optional newline |>> ParsedDirective

    let pLiteral: Parser<ParsedTemplatePart, string> = 
        maxCharsTillString "<#" false |> attempt |>> Literal |>> ParsedLiteral

    let pLiteralEof: Parser<ParsedTemplatePart, string> =
        manyCharsTill anyChar eof |>> Literal |>> ParsedLiteral

    let pControlBlock: Parser<ParsedTemplatePart, string> =
        skipString "<#" >>. maxCharsTillString "#>" true |>> ControlBlock |>> ParsedControl

    let pControlExpression: Parser<ParsedTemplatePart, string> =
        skipString "<#=" >>. maxCharsTillString "#>" true |>> ControlExpression |>> ParsedControl

    let pAnyTemplatePart: Parser<ParsedTemplatePart, string> =
        choice [ pDirective; pControlExpression; pControlBlock; pLiteral; ]

    let pTemplate: Parser<ParsedTemplatePart list, string> =
        pipe2 (many pAnyTemplatePart) pLiteralEof (fun parts lastPart -> List.append parts [lastPart])

    let parse file =
        match runParserOnFile pTemplate "" file Text.UTF8Encoding.UTF8 with
        | Success (parsed, _, _) -> pass { ParsedTemplateParts = parsed; Name = file }
        | Failure (reason, _, _) -> reason.Split ([| "\r\n"; "\n" |], StringSplitOptions.RemoveEmptyEntries)
                                    |> List.ofArray
                                    |> List.map ((+) "| ")
                                    |> fun l -> sprintf "Template %s: " file :: l
                                    |> Bad