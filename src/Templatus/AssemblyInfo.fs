namespace System
open System.Reflection
open System.Runtime.InteropServices

[<assembly: AssemblyTitleAttribute("Templatus command line")>]
[<assembly: GuidAttribute("1a2d4fdb-650e-48c5-abb8-a5348329811b")>]
[<assembly: AssemblyProductAttribute("Templatus")>]
[<assembly: AssemblyDescriptionAttribute("T4-like templating tool")>]
[<assembly: AssemblyVersionAttribute("0.1")>]
[<assembly: AssemblyFileVersionAttribute("0.1")>]
[<assembly: AssemblyMetadataAttribute("githash","06a8636b7a8f76f841f826b55c5742856f1fc421")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.1"
