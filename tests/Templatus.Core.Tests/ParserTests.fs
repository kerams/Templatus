namespace Tempaltus.Core.Tests

open Xunit
open Swensen.Unquote
open Templatus.Core
open Templatus.Core.TemplateParser
open FParsec

[<AutoOpen>]
module ParserTestHelpers =
    let parse parser text =
        match runParserOnString parser "" "" text with
        | Success (parsed, _, _) -> Some parsed
        | Failure _ -> None

module DirectiveParsers =
    [<Fact>]
    let ``should correctly parse assembly reference`` () =
        let path = "somePath"
        let text = sprintf """<#@ assembly name="%s" #>""" path
        let expected = AssemblyReference path |> ParsedDirective |> Some

        parse pDirective text =! expected

    [<Fact>]
    let ``should correctly parse include`` () =
        let file = "somePath"
        let text = sprintf """<#@ include file="%s" #>""" file
        let expected = Include file |> ParsedDirective |> Some

        parse pDirective text =! expected

    [<Fact>]
    let ``should correctly parse output`` () =
        let filename = "somePath"
        let text = sprintf """<#@ output filename="%s" #>""" filename
        let expected = Output filename |> ParsedDirective |> Some

        parse pDirective text =! expected

module ControlParsers =
    [<Fact>]
    let ``should correctly parse control expression`` () =
        let content = @"expr\ndsadasd"
        let text = sprintf "<#=%s#>" content
        let expected = ControlExpression content |> ParsedControl |> Some

        parse pControlExpression text =! expected

    [<Fact>]
    let ``should correctly parse control block`` () =
        let content = @"expr\ndsadasd"
        let text = sprintf "<#%s#>" content
        let expected = ControlBlock content |> ParsedControl |> Some

        parse pControlBlock text =! expected

module LiteralParsers =
    [<Fact>]
    let ``should correctly parse eof literal`` () =
        let literal = @"sda\nsadas"
        let expected = Literal literal |> ParsedLiteral |> Some

        parse pLiteralEof literal =! expected

    [<Fact>]
    let ``should correctly parse literal`` () =
        let literal = @"sda\nsadas "
        let text = sprintf "%s<#" literal
        let expected = Literal literal |> ParsedLiteral |> Some

        parse pLiteral text =! expected

module TemplateParsers =
    [<Fact>]
    let ``should correctly parse template`` () =
        let expected = [
            "file" |> Output |> ParsedDirective
            "abcd\n" |> Literal |> ParsedLiteral
            "DateTime.Now" |> ControlExpression |> ParsedControl
            "\nend " |> Literal |> ParsedLiteral ] |> Some

        let text = @"<#@ Output filename=""file"" #>
abcd
<#=DateTime.Now#>
end "

        parse pTemplate text =! expected