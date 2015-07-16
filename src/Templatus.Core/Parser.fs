namespace Templatus.Core

open FParsec

type ParsingResult =
    | Success of TemplatePart list
    | Failure of string

module TemplateParser =
    let maxCharsTillString str skipString =
        charsTillString str skipString System.Int32.MaxValue

    let pAssemblyReference: Parser<Directive, string> =
        skipString "assembly" >>. spaces1 >>. skipString "assembly=\"" >>. maxCharsTillString "\"" true |>> AssemblyReference

    let pInclude: Parser<Directive, string> =
        skipString "include" >>. spaces1 >>. skipString "file=\"" >>. maxCharsTillString "\"" true |>> Include

    let pOutput: Parser<Directive, string> =
        skipString "output" >>. spaces1 >>. skipString "filename=\"" >>. maxCharsTillString "\"" true |>> Output

    let pDirective: Parser<TemplatePart, string> =
        let pDirectiveType = choice [ pAssemblyReference; pInclude; pOutput ]
        skipString "<#@" >>. spaces >>. pDirectiveType .>> spaces .>> skipString "#>" |>> DirectivePart

    let pLiteral: Parser<TemplatePart, string> = 
        maxCharsTillString "<#" false |> attempt |>> Literal |>> LiteralPart

    let pLiteralEof: Parser<TemplatePart, string> =
        manyCharsTill anyChar eof |>> Literal |>> LiteralPart

    let pControl: Parser<TemplatePart, string> =
        skipString "<#" >>. maxCharsTillString "#>" true |>> Control |>> ControlPart

    let pTemplate: Parser<TemplatePart list, string> =
        pipe2 (many (choice [ pDirective; pControl; pLiteral; ])) pLiteralEof (fun parts lastPart -> List.append parts [lastPart])

    let parse filePath =
        match runParserOnFile pTemplate "" filePath System.Text.UTF8Encoding.UTF8 with
        | ParserResult.Success (parsed, _, _) -> Success parsed
        | ParserResult.Failure (reason, _, _) -> Failure reason