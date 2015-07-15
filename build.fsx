#r @"packages/FAKE/tools/FakeLib.dll"

open Fake

let buildDir = "bin"
let solution = "Templatus.sln"

Target "All" DoNothing

Target "PackageRestore" RestorePackages

Target "Clean" (fun _ ->
    CleanDirs [ buildDir ]
)

Target "Build" (fun _ ->
    !! solution
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

"Clean"
    ==> "PackageRestore"
    ==> "Build"
    ==> "All"

RunTargetOrDefault "All"
