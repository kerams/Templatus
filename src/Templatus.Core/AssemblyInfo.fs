namespace System
open System.Reflection
open System.Runtime.InteropServices

[<assembly: AssemblyTitleAttribute("Templatus lib")>]
[<assembly: GuidAttribute("f4a3cb74-b623-4d17-a3d3-7861ca0513f8")>]
[<assembly: AssemblyProductAttribute("Templatus")>]
[<assembly: AssemblyDescriptionAttribute("T4-like templating tool with support for F#")>]
[<assembly: AssemblyVersionAttribute("0.2.1")>]
[<assembly: AssemblyFileVersionAttribute("0.2.1")>]
[<assembly: AssemblyMetadataAttribute("githash","6f3e16736a54aa1f7ca27bf1f51ceef6e7b90acc")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.2.1"
