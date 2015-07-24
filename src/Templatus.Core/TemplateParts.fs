namespace Templatus.Core

type Directive =
    | AssemblyReference of string
    | Include of string
    | Output of string

type Literal = Literal of string

type Control =
    | ControlBlock of string
    | ControlExpression of string

type ParsedTemplatePart =
    | ParsedDirective of Directive
    | ParsedLiteral of Literal
    | ParsedControl of Control

type ParsedTemplate = {
    Name: string
    ParsedTemplateParts: ParsedTemplatePart list }

type ProcessedTemplatePart =
    | ProcessedLiteral of Literal
    | ProcessedControl of Control
    | ProcessedInclude of ProcessedTemplate

and ProcessedTemplate = {
    Name: string
    AssemblyReferences: string list
    OutputFile: string option
    ProcessedTemplateParts: ProcessedTemplatePart list }