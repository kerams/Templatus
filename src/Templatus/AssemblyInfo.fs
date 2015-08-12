namespace System
open System.Reflection
open System.Runtime.InteropServices

[<assembly: AssemblyTitleAttribute("Templatus command line")>]
[<assembly: GuidAttribute("1a2d4fdb-650e-48c5-abb8-a5348329811b")>]
[<assembly: AssemblyProductAttribute("Templatus")>]
[<assembly: AssemblyDescriptionAttribute("T4-like templating tool with support for F#")>]
[<assembly: AssemblyVersionAttribute("0.2.0")>]
[<assembly: AssemblyFileVersionAttribute("0.2.0")>]
[<assembly: AssemblyMetadataAttribute("githash","73fa5200c1bc430c3e2ffffb386f7251fa1a8bea")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.2.0"
