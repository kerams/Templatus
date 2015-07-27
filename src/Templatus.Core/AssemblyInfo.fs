namespace System
open System.Reflection
open System.Runtime.InteropServices

[<assembly: AssemblyTitleAttribute("Templatus lib")>]
[<assembly: GuidAttribute("f4a3cb74-b623-4d17-a3d3-7861ca0513f8")>]
[<assembly: AssemblyProductAttribute("Templatus")>]
[<assembly: AssemblyDescriptionAttribute("T4-like templating tool")>]
[<assembly: AssemblyVersionAttribute("0.1")>]
[<assembly: AssemblyFileVersionAttribute("0.1")>]
[<assembly: AssemblyMetadataAttribute("githash","def613d855f17c3f08048baf4ce62139a6e4bba9")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.1"
