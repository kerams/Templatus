namespace Templatus.Core

type Directive =
    | AssemblyReference of string
    | Include of string 
    | Output of string

type Literal = Literal of string

type Control = Control of string

type TemplatePart =
    | DirectivePart of Directive
    | LiteralPart of Literal
    | ControlPart of Control