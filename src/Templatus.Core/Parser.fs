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

    let pDirective: Parser<TemplatePart, string> =
        skipString "<#@" >>. spaces >>. pAnyDirective .>> spaces .>> skipString "#>" |>> DirectivePart

    let pLiteral: Parser<TemplatePart, string> = 
        maxCharsTillString "<#" false |> attempt |>> Literal |>> LiteralPart

    let pLiteralEof: Parser<TemplatePart, string> =
        manyCharsTill anyChar eof |>> Literal |>> LiteralPart

    let pControl: Parser<TemplatePart, string> =
        skipString "<#" >>. maxCharsTillString "#>" true |>> Control |>> ControlPart

    let pAnyTemplatePart: Parser<TemplatePart, string> =
        choice [ pDirective; pControl; pLiteral; ]

    let pTemplate: Parser<TemplatePart list, string> =
        pipe2 (many pAnyTemplatePart) pLiteralEof (fun parts lastPart -> List.append parts [lastPart])

    let parse filePath =
        match runParserOnFile pTemplate "" filePath System.Text.UTF8Encoding.UTF8 with
        | Success (parsed, _, _) -> pass parsed
        | Failure (reason, _, _) -> fail reason