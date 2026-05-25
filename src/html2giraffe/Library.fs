

namespace Html2Giraffe

/// <summary>
/// Public API for Html2Giraffe library.
/// </summary>
module Library =

    open System
    open System.IO
    open Ast
    open Parser
    open Converter
    open Roundtrip

    /// <summary>
    /// Converts HTML string to F# code.
    /// </summary>
    let htmlToFsharp (html: string) : string =
        let doc = parse html
        convertToString doc

    /// <summary>
    /// Converts HTML file to F# code.
    /// </summary>
    let htmlFileToFsharp (filePath: string) : string =
        let doc = parseFile filePath
        convertToString doc

    /// <summary>
    /// Converts HTML to F# module.
    /// </summary>
    let htmlToFsharpModule (namespaceName: string) (moduleName: string) (viewName: string) (html: string) : string =
        let doc = parse html
        convertToModule namespaceName moduleName viewName doc

    /// <summary>
    /// Performs roundtrip validation on HTML.
    /// </summary>
    let validateRoundtrip (html: string) : ValidationResult =
        testRoundtrip html

    /// <summary>
    /// Gets the AST representation of HTML.
    /// </summary>
    let getAst (html: string) : HtmlDocument =
        parse html

    /// <summary>
    /// Renders AST back to HTML.
    /// </summary>
    let renderHtml (doc: HtmlDocument) : string =
        renderDocumentToHtml doc

    /// <summary>
    /// Converts HTML file to F# file.
    /// </summary>
    let convertFile (inputPath: string) (outputPath: string) : unit =
        let fsharpCode = htmlFileToFsharp inputPath
        File.WriteAllText(outputPath, fsharpCode)

    /// <summary>
    /// Converts HTML file to F# file with custom module name.
    /// </summary>
    let convertFileWithModule (namespaceName: string) (moduleName: string) (viewName: string) (inputPath: string) (outputPath: string) : unit =
        let html = File.ReadAllText(inputPath)
        let doc = parse html
        let fsharpCode = convertToModule namespaceName moduleName viewName doc
        File.WriteAllText(outputPath, fsharpCode)






