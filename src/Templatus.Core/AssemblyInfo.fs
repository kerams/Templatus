namespace System
open System.Reflection
open System.Runtime.InteropServices

[<assembly: AssemblyTitleAttribute("Templatus lib")>]
[<assembly: GuidAttribute("f4a3cb74-b623-4d17-a3d3-7861ca0513f8")>]
[<assembly: AssemblyProductAttribute("Templatus")>]
[<assembly: AssemblyDescriptionAttribute("T4-like templating tool with support for F#")>]
[<assembly: AssemblyVersionAttribute("0.2.0")>]
[<assembly: AssemblyFileVersionAttribute("0.2.0")>]
[<assembly: AssemblyMetadataAttribute("githash","73fa5200c1bc430c3e2ffffb386f7251fa1a8bea")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.2.0"
