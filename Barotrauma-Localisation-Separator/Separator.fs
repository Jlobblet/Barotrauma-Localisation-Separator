module Barotrauma_Localisation_Separator.Separator

open System.IO
open System.Linq
open System.Xml.Linq
open FSharp.XExtensions
open Barotrauma_Localisation_Separator.ArgumentParser
open FSharpPlus.Data

type XElement with
    static member CreateWithValue name value =
        let xe = XElement.Create name
        xe.Value <- value
        xe

    member this.HandleOverride =
        if this.Name.LocalName.ToLowerInvariant() = "override"
        then this.Elements() |> Seq.head
        else this

    member this.TryAttributeValue(name: string) =
        this.Attribute name
        |> Option.ofObj
        |> Option.map (fun a -> a.Value)

let handleBasic prefix barotraumaPath path =
    query {
        for element in XDocument
            .Load(Path.Combine(barotraumaPath, path))
            .Root.HandleOverride.Elements() do
            let actual = element.HandleOverride
            let identifier = actual.TryAttributeValue "identifier"

            select
                (match identifier with
                 | None -> []
                 | Some i ->
                     let name =
                         actual.TryAttributeValue "name"
                         |> Option.defaultValue ""

                     let desc =
                         actual.TryAttributeValue "description"
                         |> Option.defaultValue ""

                     [ "name", name; "description", desc ]
                     |> List.map (fun (suffix, v) -> XElement.CreateWithValue $"%s{prefix}%s{suffix}.{i}" v))
    }
    |> Seq.collect id
    |> List.ofSeq
    
let handleAffliction = handleBasic "affliction"

let handleEntity = handleBasic "entity"

let handleJob = handleBasic "job"

let contentTypeMap =
    [ "Item", handleEntity
      "Structure", handleEntity
      "Afflictions", handleAffliction
      "Job", handleJob ]
    |> Map.ofList

let parseFilelist =
    let inner ({InputPath = path}: Options) =
        query {
            let doc = XDocument.Load(path)
            for element in doc.Root.Elements() do
                groupValBy (element.Attribute("file").Value) element.Name.LocalName into g
                where (contentTypeMap.ContainsKey g.Key)
                select (g.Key, g)
        }
        |> List.ofSeq
    inner |> Reader

let generateLocalisationElements (key: string, group: IGrouping<string, string>) =
    let inner ({BarotraumaLocation = path}: Options) =
        let func = contentTypeMap.[key] path
        group
        |> Seq.collect func
    inner |> Reader
    
let handleFilelist =
    let traverse func list =
        let (>>=) x f = Reader.bind f x
        let folder head tail =
            func head >>= (fun h ->
                tail >>= fun t ->
                    Reader.Return<_, _> (h :: t))

        List.foldBack folder list (Reader.Return<_, _> [])


    parseFilelist
    |> Reader.bind (traverse generateLocalisationElements)
    |> Reader.map (Seq.collect id >> Array.ofSeq)

let produceXml (elements: XElement[]) =
    let inner options =
        let element = XElement.Create("infotexts", elements)
        element.SetAttributeValue("language", options.Language)
        element.SetAttributeValue("nowhitespace", options.NoWhitespace)
        element.SetAttributeValue("translatedname", options.TranslatedName)
        
        XDocument(XDeclaration("1.0", "utf-8", "yes"), [| element :> obj |])
    inner |> Reader

let makeDocument =
    handleFilelist
    |> Reader.bind produceXml
