# Templatus
[![Build Status](https://travis-ci.org/kerams/Templatus.svg)](https://travis-ci.org/kerams/Templatus)
[![Build status](https://ci.appveyor.com/api/projects/status/trjc0x6f9b8m77tr?svg=true)](https://ci.appveyor.com/project/kerams/templatus)
[![NuGet package](https://img.shields.io/nuget/v/Templatus.svg)](https://www.nuget.org/packages/Templatus/)

## What?
Templatus is a templating tool that works (and currently also looks) a lot like T4. The major difference is the fact that blocks that emit text are written in F# instead of C#.

## Why?
T4 does not support F# :^). Using T4 templates at compile-time is also rather problematic.

## How?
Template parsing is implemented using the excellent library [FParsec](http://www.quanttec.com/fparsec/) and the execution of text-emitting code is done through the hosting of F# Interactive and [F# Compiler Services](https://github.com/fsharp/FSharp.Compiler.Service).

## Features?
- Design-time processing on save (the default for T4) is not yet supported. You are required to execute the processor in your build pipeline (MSBuild, [FAKE](https://github.com/fsharp/FAKE), etc.).
- Text-emitting code can access parameters passed into the processor by their name. If these parameters are not later passed into the processor, an error is raised.
- `popIndent`, `clearIndent`, `pushIndent` providing the same functionality as in T4.
- Referencing assemblies
- Template nesting

## Installation?
You can get Templatus as a Nuget package https://www.nuget.org/packages/Templatus/. It resides in the `tools` folder under `packages\Templatus`.

## Getting started
### Template parts
- Directives - delimited by `<#@` and `#>`
  - Output directive `<#@ output filename="..\gen\output.txt" #>` - Specifies the file to generate
  - Include directive `<#@ include file="sub\include.ttus" #>` - Specifies a nested template whose output will be embedded in place of the directive
  - Assembly reference directive `<#@ assembly name="TestLib.dll" #>` - Specifies an assembly (either in GAC or local to the template) to reference
- Controls
  - Expression `<#= DateTime.Now #>` - Expressions that are evaluated, converted to string (using the `%O` format specifier) and printed out
  - Block `<# [1..2] |> List.iter tprintn #>` - Contains arbitrary logic (including function/module declarations that will be available in any control from that point onwards) and can use any of the `tprint` functions to print into the output file
- Literals
  - Any text that is neither a directive nor a control
  - Printed into the output "as is"

*Note*: Bodies of any control are whitespace-sensitive just like regular F# code, meaning you have to abide by F#'s whitespace rules. Also, tabs are automatically converted to 4 spaces.

### Convenience functions
- `tprint` - Takes any object and after calling ToString on it prints the result into the output
- `tprintn` - The same as above but with a trailing newline
- `tprintf` - The `sprintf` equivalent for printing into the output
- `tprintfn` - The same as above but with a trailing newline
- `pushIndent` - Takes a string and puts it on the stack of indent strings. The indent strings are used as a prefix for everything that `tprintn` and `tprintfn` output.
- `popIndent` - Removes a string from the top of the stack of indent strings. Does not throw an exception when the stack is empty.
- `clearIndent` - Clears the entire stack of indent strings

### Template processor
Templatus.exe is a command-line template processor that takes a template and an optional list of arguments to make accessible in the template. The following flags are available:
- `-t ..\..\myTemplate.ttus` - Specifies the template to be processed. You can also specify more templates by using `-t [path]` repeatedly. All of them share the variables that you pass in.
- `-p name=Timmy;age=3` - Defines `name` and `age` variables that you can directly refer to in the template. Note that the variables are always defined as strings.

### Example
Suppose I have a folder structure like this:

    MySolutionDir
    │ MySolutionFile.sln
    │
    ├─lib
    │   TestLib.dll
    │
    ├─MyProjectDir
    │   include.ttus
    │   testTemplate.ttus
    │
    └─packages
        └─Templatus
            └─tools
                Templatus.exe

The contents of `testTemplate.ttus`:

    <#@ output filename="output.txt" #>
    <#@ assembly name="..\lib\TestLib.dll" #>
    Number from TestLib: <#= TestLib.Test.Number () #>

    Params: <#= sprintf "Name: %s, Age: %s" name age #>

    <#@ include file="include.ttus" #>
    Indented numbers:
    <#
        [1 .. 10]
        |> Seq.iter (fun num -> pushIndent " "; tprintfn "%d" num)

	    [9 .. -1 .. 1]
        |> Seq.iter (fun num -> popIndent (); sprintf "%d" num |> tprintn)

        clearIndent ()
        tprintn "----"
    #>

And the template being included:

    A line in include
    Time in include: <#= DateTime.Now #>

To generate the output file, I just need to execute `Templatus.exe` and pass in `testTemplate.ttus`:

    D:\MySolutionDir> packages\Templatus\tools\Templatus.exe -t MyProjectDir\testTemplate.ttus -p name=Timmy;age=3

`output.txt` is created and looks like this:

    Number from TestLib: 5

    Params: Name: Timmy, Age: 3

    A line in include
    Time in include: 07-Aug-15 10:14:56
    Indented numbers:
     1
      2
       3
        4
         5
          6
           7
            8
             9
              10
             9
            8
           7
          6
         5
        4
       3
      2
     1
    ----

### Sample MsBuild 4 target (added to your MyProjectDir.fsproj or MyProjectDir.csproj)
```
  <PropertyGroup>
    <PackagesFolder>$([System.IO.Path]::GetFullPath('$(MSBuildProjectDirectory)\..\packages'))</PackagesFolder>
  </PropertyGroup>
  <Target Name="Templates" BeforeTargets="Build" Inputs="Template.ttus" Outputs="output.txt">
    
    <Exec Command="$(PackagesFolder)\Templatus.0.2.0\tools\Templatus.exe -t &quot;$(MSBuildProjectDirectory)\Template.ttus&quot; -p name=Timmy;age=3" Outputs="output.txt" />
  </Target>
```
