namespace System
open System.Reflection
open System.Runtime.InteropServices

[<assembly: AssemblyTitleAttribute("Templatus command line")>]
[<assembly: GuidAttribute("1a2d4fdb-650e-48c5-abb8-a5348329811b")>]
[<assembly: AssemblyProductAttribute("Templatus")>]
[<assembly: AssemblyDescriptionAttribute("T4-like templating tool with support for F#")>]
[<assembly: AssemblyVersionAttribute("0.1.2")>]
[<assembly: AssemblyFileVersionAttribute("0.1.2")>]
[<assembly: AssemblyMetadataAttribute("githash","bbf11a8063c7a054aec5d77824e9bf2a49f4d0a7")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.1.2"
