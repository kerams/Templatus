# Templatus
[![Build Status](https://magnum.travis-ci.com/kerams/Templatus.svg?token=MzTGNBqs9peqx7A5xToB&branch=master)](https://magnum.travis-ci.com/kerams/Templatus)

## What?
Templatus is a templating tool that works (and currently also looks) a lot like T4. The major difference is that you use F# instead of C# in blocks that emit text.

## Why?
While T4 is a great tool, I ran into a scenario where it falls short – accessing Visual Studio's variables such as `$(ProjectDir)` when templates are processed during compile-time. Templatus addresses this by allowing you to pass parameters directly to the command-line processor.

As a completely open-source project, Templatus can moreover easily get more features and stop being a mere clone of T4.

## How?
Template parsing is implemented using the excellent library [FParsec](http://www.quanttec.com/fparsec/) and the execution of text-emitting code is done through the hosting of F# Interactive and [F# Compiler Services](https://github.com/fsharp/FSharp.Compiler.Service).

## What features are and aren't available?
- Templates can be processed at compile-time. Design-time processing on save (the default for T4) is not yet supported.
- Text-emitting code can access parameters passed into the processor by their name. If these parameters are not later passed into the processor, an error is raised.
- `popIndent`, `clearIndent`, `pushIndent` providing the same functionality as in T4.
- Referencing assemblies
- Template nesting
