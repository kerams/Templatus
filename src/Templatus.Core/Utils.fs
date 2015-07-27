namespace Templatus.Core

open System.IO
open Chessie.ErrorHandling

module Utils =
    let checkTemplateExists templateName =
        if File.Exists templateName then pass templateName else sprintf "Cannot find template %s." templateName |> fail