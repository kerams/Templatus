#r @"packages/FAKE/tools/FakeLib.dll"

open Fake
open System

let buildDir = "bin"
let mergeDir = "merge"
let solution = "Templatus.sln"

Target "All" DoNothing

Target "Clean" (fun _ ->
    CleanDirs [ buildDir; mergeDir ]
)

Target "Build" (fun _ ->
    !! solution
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

Target "Merge" (fun _ ->
    CreateDir mergeDir

    let toPack =
        [ "Templatus.exe"; "Templatus.Core.dll"; "UnionArgParser.dll"; "Chessie.dll"; "FSharp.Compiler.Service.dll"; "FParsec.dll"; "FParsecCS.dll" ]
        |> List.map (fun l -> buildDir @@ l)
        |> separated " "

    let result =
        ExecProcess
            (fun info -> info.FileName <- currentDirectory @@ "packages" @@ "ILRepack" @@ "tools" @@ "ILRepack.exe"
                         info.Arguments <- sprintf "/verbose /lib:%s /out:%s %s" buildDir (mergeDir @@ "Templatus.exe") toPack)
            (TimeSpan.FromMinutes 5.)

    if result <> 0 then failwith "Error during ILRepack execution."
)

"Clean"
    ==> "Build"
    ==> "Merge"
    ==> "All"

RunTargetOrDefault "All"