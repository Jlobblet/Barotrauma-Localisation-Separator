module Barotrauma_Localisation_Separator.ArgumentParser

open System
open System.IO
open Argu

type Arguments =
    | [<MainCommand; ExactlyOnce; AltCommandLine("-i")>] InputPath of path: string
    | [<Unique; AltCommandLine("-o")>] OutputPath of path: string
    | [<Unique; CustomAppSettings("language")>] Language of lang: string
    | [<Unique; CustomAppSettings("translatedname")>] TranslatedName of name: string
    | [<Unique; CustomAppSettings("nowhitespace")>] NoWhitespace of bool
    | [<Unique; CustomAppSettings("BarotraumaLocation")>] BarotraumaLocation of path: string
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | InputPath _ -> "Path to filelist.xml"
            | OutputPath _ -> "Folder to put output in"
            | Language _ -> "The identifier of the language used"
            | TranslatedName _ -> "The translated name of the language"
            | NoWhitespace _ -> "Whether the language has whitespace in it or not"
            | BarotraumaLocation _ -> "The location where Barotrauma is installed"

let errorHandler =
    ProcessExiter
        (colorizer = function
        | ErrorCode.HelpText -> None
        | _ -> Some ConsoleColor.Red)

let parser =
    ArgumentParser.Create<Arguments>(errorHandler = errorHandler)

type Options =
    { InputPath: string
      OutputPath: string
      Language: string
      TranslatedName: string
      NoWhitespace: bool
      BarotraumaLocation: string }
    static member FromArgv argv =
        let results = parser.Parse argv
        
        let inputPath = results.GetResult <@ InputPath @>
        let barotraumaLocation = results.GetResult <@ BarotraumaLocation @>
        let language = results.GetResult <@ Language @>
        let translatedName = results.GetResult <@ Language @>
        let noWhitespace = results.GetResult <@ NoWhitespace @>
        let outputPath =
            let default' () = Path.Combine(Path.GetDirectoryName inputPath, language, $"%s{language}Vanilla.xml")
            results.TryGetResult <@ OutputPath @> |> Option.defaultWith default'

        { InputPath = inputPath
          OutputPath = outputPath
          Language = language
          TranslatedName = translatedName
          NoWhitespace = noWhitespace
          BarotraumaLocation = barotraumaLocation }