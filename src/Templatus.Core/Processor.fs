namespace Templatus.Core

open Chessie.ErrorHandling
open System.IO

type DirectiveGrouping = {
        Output: string list
        AssemblyReferences: string list
        Includes: string list }

type ProcessedTemplate = {
        Directives: DirectiveGrouping
        FilteredTemplateParts: TemplatePart list }

module Processor =
    let partitionDirectives templateParts =
        let rec partitionInner rest directiveGrouping =
            match rest with
            | [] -> directiveGrouping
            | h :: t -> 
                let newGrouping = match h with
                                  | Output file -> { directiveGrouping with Output = file :: directiveGrouping.Output }
                                  | AssemblyReference assembly -> { directiveGrouping with AssemblyReferences = assembly :: directiveGrouping.AssemblyReferences }
                                  | Include file -> { directiveGrouping with Includes = file :: directiveGrouping.Includes }
                partitionInner t newGrouping
        
        let directives = templateParts |> List.choose (fun x -> match x with DirectivePart d -> Some d | _ -> None)

        partitionInner directives { Output = []; AssemblyReferences = []; Includes = [] }

    let validateDirectiveGrouping grouping =
        let validateOutput grouping =
            match grouping.Output |> List.length with
            | 1 -> pass grouping
            | _ -> fail "Exactly one output directive needs to be specified."

        let validateIncludes grouping =
            match grouping.Includes |> List.forall File.Exists with
            | true -> pass grouping
            | false -> fail "One or more includes cannot be found."

        grouping |> validateOutput >>= validateIncludes

    let processTemplate templateParts =
        let directiveGrouping = partitionDirectives templateParts
        let nonDirectiveTemplateParts = templateParts |> List.choose (fun x -> match x with DirectivePart _ -> None | _ -> Some x)

        match directiveGrouping |> validateDirectiveGrouping with
        | Ok _ -> pass { Directives = directiveGrouping; FilteredTemplateParts = nonDirectiveTemplateParts }
        | Bad reasons-> Bad reasons