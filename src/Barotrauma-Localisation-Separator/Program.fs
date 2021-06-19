open System.IO
open Barotrauma_Localisation_Separator.Separator
open Barotrauma_Localisation_Separator.ArgumentParser
open FSharpPlus.Data

[<EntryPoint>]
let main argv =
    if argv |> Array.isEmpty then
        parser.PrintUsage() |> printfn "%s"
        exit 0

    let options = Options.FromArgv argv
    
    let document =
        makeDocument
        |> Reader.run
        <| options
    
    Directory.CreateDirectory (Path.GetDirectoryName options.OutputPath) |> ignore
    document.Save options.OutputPath
    
    0 // return an integer exit code
