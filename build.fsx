#r @"packages/FAKE/tools/FakeLib.dll"

open Fake

let buildDir = "bin"
let solution = "Templatus.sln"

Target "All" DoNothing

Target "Clean" (fun _ ->
    CleanDirs [ buildDir ]
)

Target "Build" (fun _ ->
    !! solution
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

"Clean"
    ==> "Build"
    ==> "All"

RunTargetOrDefault "All"