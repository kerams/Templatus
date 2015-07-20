namespace Templatus.Core

open Chessie.ErrorHandling
open System.IO

module Processor =
    type DirectiveGrouping = {
        Name: string
        Outputs: string list
        AssemblyReferences: string list
        Includes: string list }

    let extractDirectives templateName parsedTemplateParts =
        let rec extract rest directiveGrouping =
            match rest with
            | [] -> directiveGrouping
            | h :: t -> 
                let newGrouping = match h with
                                  | Output file -> { directiveGrouping with Outputs = file :: directiveGrouping.Outputs }
                                  | AssemblyReference assembly -> { directiveGrouping with AssemblyReferences = assembly :: directiveGrouping.AssemblyReferences }
                                  | Include file -> { directiveGrouping with Includes = file :: directiveGrouping.Includes }
                extract t newGrouping
        
        let directives = parsedTemplateParts |> List.choose (fun x -> match x with ParsedDirective d -> Some d | _ -> None)

        extract directives { Name = templateName; Outputs = []; AssemblyReferences = []; Includes = [] }

    let processTemplate parser templateName templateParts =
        let rec processTemplateInner templateName templateParts =
            let directives = extractDirectives templateName templateParts

            let nonDirectiveParts = 
                templateParts
                |> List.choose (fun p -> match p with
                                         | ParsedDirective (AssemblyReference _) | ParsedDirective (Output _) -> None
                                         | _ -> Some p)

            let processedParts =
                nonDirectiveParts
                |> List.map (fun p -> match p with
                                      | ParsedLiteral l -> ProcessedLiteral l |> pass
                                      | ParsedControl c -> ProcessedControl c |> pass
                                      | ParsedDirective (Include i) -> i |> parser >>= processTemplateInner i >>= (ProcessedInclude >> pass)
                                      | _ -> failwith "You should not see this.")

            let failures =
                processedParts
                |> List.collect (fun e -> match e with
                                          | Ok _ -> []
                                          | Bad reasons -> reasons |> List.map ((+) "| "))

            match failures.Length with
            | 0 ->
                { Name = templateName;
                  AssemblyReferences = directives.AssemblyReferences;
                  Output = directives.Outputs.Head;
                  ProcessedTemplateParts = processedParts |> List.choose (fun p -> match p with Ok (r, _) -> Some r | _ -> None) }
                |> pass
            | _ -> sprintf "Template %s: " templateName :: failures |> Bad

        processTemplateInner (Path.GetFileName templateName) templateParts