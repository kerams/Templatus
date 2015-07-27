namespace System
open System.Reflection
open System.Runtime.InteropServices

[<assembly: AssemblyTitleAttribute("Templatus command line")>]
[<assembly: GuidAttribute("1a2d4fdb-650e-48c5-abb8-a5348329811b")>]
[<assembly: AssemblyProductAttribute("Templatus")>]
[<assembly: AssemblyDescriptionAttribute("T4-like templating tool")>]
[<assembly: AssemblyVersionAttribute("0.1")>]
[<assembly: AssemblyFileVersionAttribute("0.1")>]
[<assembly: AssemblyMetadataAttribute("githash","def613d855f17c3f08048baf4ce62139a6e4bba9")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.1"
