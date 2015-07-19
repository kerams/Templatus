namespace Templatus.Core

open FParsec
open Chessie.ErrorHandling

module TemplateParser =
    let maxCharsTillString str skipString =
        charsTillString str skipString System.Int32.MaxValue

    let pAssemblyReference: Parser<Directive, string> =
        skipString "assembly" >>. spaces1 >>. skipString "name=\"" >>. maxCharsTillString "\"" true |>> AssemblyReference

    let pInclude: Parser<Directive, string> =
        skipString "include" >>. spaces1 >>. skipString "file=\"" >>. maxCharsTillString "\"" true |>> Include

    let pOutput: Parser<Directive, string> =
        skipString "output" >>. spaces1 >>. skipString "filename=\"" >>. maxCharsTillString "\"" true |>> Output

    let pAnyDirective: Parser<Directive, string> =
        choice [ pAssemblyReference; pInclude; pOutput ]

    let pDirective: Parser<ParsedTemplatePart, string> =
        skipString "<#@" >>. spaces >>. pAnyDirective .>> spaces .>> skipString "#>" |>> ParsedDirective

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

    let parse filePath =
        match runParserOnFile pTemplate "" filePath System.Text.UTF8Encoding.UTF8 with
        | Success (parsed, _, _) -> pass parsed
        | Failure (reason, _, _) -> fail reason