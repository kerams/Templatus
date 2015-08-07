namespace System
open System.Reflection
open System.Runtime.InteropServices

[<assembly: AssemblyTitleAttribute("Templatus lib")>]
[<assembly: GuidAttribute("f4a3cb74-b623-4d17-a3d3-7861ca0513f8")>]
[<assembly: AssemblyProductAttribute("Templatus")>]
[<assembly: AssemblyDescriptionAttribute("T4-like templating tool with support for F#")>]
[<assembly: AssemblyVersionAttribute("0.1.2")>]
[<assembly: AssemblyFileVersionAttribute("0.1.2")>]
[<assembly: AssemblyMetadataAttribute("githash","bbf11a8063c7a054aec5d77824e9bf2a49f4d0a7")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.1.2"
