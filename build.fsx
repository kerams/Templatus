#r @"packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open System
open ReleaseNotesHelper
open Fake.FileSystemHelper

let commitHash = Information.getCurrentSHA1 (".")
let release = LoadReleaseNotes "RELEASE_NOTES.md"
let description = "T4-like templating tool with support for F#"

let buildDir = "bin"
let mergeDir = "merge"
let nugetDir = "nuget"
let solution = "Templatus.sln"

Target "Clean" (fun _ ->
    CleanDirs [ buildDir; mergeDir ]
)

Target "SetAssemblyInfo" (fun _ ->
    let commonAttributes = [
        Attribute.Product "Templatus"
        Attribute.Description description
        Attribute.Version release.AssemblyVersion
        Attribute.FileVersion release.AssemblyVersion
        Attribute.Metadata("githash", commitHash) ]

    CreateFSharpAssemblyInfo (currentDirectory @@ "src" @@ "Templatus" @@ "AssemblyInfo.fs")
        ([ Attribute.Title "Templatus command line"
           Attribute.Guid "1a2d4fdb-650e-48c5-abb8-a5348329811b" ] @ commonAttributes)
    
    CreateFSharpAssemblyInfo (currentDirectory @@ "src" @@ "Templatus.Core" @@ "AssemblyInfo.fs")
        ([ Attribute.Title "Templatus lib"
           Attribute.Guid "f4a3cb74-b623-4d17-a3d3-7861ca0513f8" ] @ commonAttributes)
)

Target "Build" (fun _ ->
    !! solution
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

Target "Merge" (fun _ ->
    CreateDir mergeDir

    let toPack =
        [ "Templatus.exe"; "Templatus.Core.dll"; "UnionArgParser.dll"; "Chessie.dll"; "FSharp.Compiler.Service.dll"; "FParsec.dll"; "FParsecCS.dll"; ]
        |> List.map (fun l -> buildDir @@ l)
        |> separated " "

    let result =
        ExecProcess
            (fun info -> info.FileName <- currentDirectory @@ "packages" @@ "ILRepack" @@ "tools" @@ "ILRepack.exe"
                         info.Arguments <- sprintf "/attr:%s /lib:%s /out:%s %s" (currentDirectory @@ "bin" @@ "Templatus.exe") buildDir (mergeDir @@ "Templatus.exe") toPack)
            (TimeSpan.FromMinutes 5.)

    [ "FSharp.Core.dll"; "FSharp.Core.optdata"; "FSharp.Core.sigdata" ]
    |> List.map (fun l -> buildDir @@ l)
    |> CopyFiles mergeDir

    if result <> 0 then failwith "Error during ILRepack execution."
)

Target "Default" DoNothing

Target "CreateNuget" (fun _ ->
    Paket.Pack (fun p ->
        { p with
            Version = release.NugetVersion
            ReleaseNotes = toLines release.Notes
            OutputPath = nugetDir })
)

Target "PublishNuget" (fun _ ->
    Paket.Push (fun p ->
        { p with
            WorkingDir = nugetDir })
)

Target "Release" (fun _ ->
    StageAll ""
    Commit "" (sprintf "Release version %s" release.NugetVersion)
    Branches.push ""

    Branches.tag "" release.NugetVersion
    Branches.pushTag "" "origin" release.NugetVersion
)

"Clean"
    ==> "SetAssemblyInfo"
    ==> "Build"
    ==> "Merge"
    ==> "Default"
    =?> ("CreateNuget", not isLinux)
    =?> ("PublishNuget", not isLinux)
    ==> "Release"

RunTargetOrDefault "Default"