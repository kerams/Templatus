namespace System
open System.Reflection
open System.Runtime.InteropServices

[<assembly: AssemblyTitleAttribute("Templatus command line")>]
[<assembly: GuidAttribute("1a2d4fdb-650e-48c5-abb8-a5348329811b")>]
[<assembly: AssemblyProductAttribute("Templatus")>]
[<assembly: AssemblyDescriptionAttribute("T4-like templating tool with support for F#")>]
[<assembly: AssemblyVersionAttribute("0.2.1")>]
[<assembly: AssemblyFileVersionAttribute("0.2.1")>]
[<assembly: AssemblyMetadataAttribute("githash","6f3e16736a54aa1f7ca27bf1f51ceef6e7b90acc")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.2.1"
