namespace Templatus.Core

open Chessie.ErrorHandling
open System.IO

type DirectiveGrouping = {
        Output: string list
        AssemblyReferences: string list
        Includes: string list }

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

        partitionInner templateParts { Output = []; AssemblyReferences = []; Includes = [] }

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
        match partitionDirectives templateParts |> validateDirectiveGrouping with
        | Ok _ -> ()
        | Bad _ -> ()