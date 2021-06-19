#r "paket:
nuget System.IO.Compression.ZipFile
nuget Fake.DotNet.Cli
nuget Fake.IO.FileSystem
nuget Fake.Core.Target //"
#load ".fake/build.fsx/intellisense.fsx"

open System.IO
open System.IO.Compression
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

Target.initEnvironment ()

Target.create "Clean"
<| fun _ -> !! "src/**/bin" ++ "src/**/obj" |> Shell.cleanDirs

Target.create "Build"
<| fun _ -> !! "src/**/*.*proj" |> Seq.iter (DotNet.build id)

Target.create "Publish"
<| fun _ ->
    !! "src/**/*.*proj"
    |> Seq.iter
        (DotNet.publish (fun settings ->
            { settings with
                  SelfContained = Some true
                  Runtime = Some "win10-x64"
                  MSBuildParams =
                      { settings.MSBuildParams with
                            Properties =
                                ("PublishSingleFile", "true")
                                :: settings.MSBuildParams.Properties } }))

Target.create "Zip"
<| fun _ ->
    ZipFile.CreateFromDirectory
        (Path.Combine("src", "Barotrauma-Localisation-Separator", "bin", "Release", "net5.0", "win10-x64", "publish"),
         "Barotrauma-Localisation-Separator.zip")

Target.create "All" ignore

"Clean" ==> "Build" ==> "All"
"Clean" ==> "Publish" ==> "Zip"

Target.runOrDefaultWithArguments "Build"