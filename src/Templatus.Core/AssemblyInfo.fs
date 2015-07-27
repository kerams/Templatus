namespace System
open System.Reflection
open System.Runtime.InteropServices

[<assembly: AssemblyTitleAttribute("Templatus lib")>]
[<assembly: GuidAttribute("f4a3cb74-b623-4d17-a3d3-7861ca0513f8")>]
[<assembly: AssemblyProductAttribute("Templatus")>]
[<assembly: AssemblyDescriptionAttribute("T4-like templating tool")>]
[<assembly: AssemblyVersionAttribute("0.1")>]
[<assembly: AssemblyFileVersionAttribute("0.1")>]
[<assembly: AssemblyMetadataAttribute("githash","06a8636b7a8f76f841f826b55c5742856f1fc421")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.1"
