# Barotrauma-Localisation-Separator

Separate Barotrauma mods into the content and localised text.

## Installation

Prerequisites:
- [.NET Core SDK 5.0](https://dotnet.microsoft.com/download/dotnet/5.0)

To compile, run `dotnet build`.

## Usage

### Configuration

Settings to be used on all runs can be configured in [`app.config`](Barotrauma-Localisation-Separator/app.config).
Running the program with no arguments will print the help message.

### Running

The main argument is the input path, which should be the location of `filelist.xml`.
You can optionally include an `--outputpath` or `-o` switch that specifies where the output should be placed.
