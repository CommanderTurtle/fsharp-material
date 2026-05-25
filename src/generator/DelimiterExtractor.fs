
namespace Generator

open System
open System.Text

/// <summary>
/// Delimiter-based content extraction similar to the XML project approach.
/// Extracts sections from HTML/JS/CSS using custom delimiters.
/// </summary>
module DelimiterExtractor =

    /// <summary>
    /// Represents an extracted section.
    /// </summary>
    type ExtractedSection = {
        Name: string
        StartDelimiter: string
        EndDelimiter: string
        Content: string
        LineStart: int
        LineEnd: int
    }

    /// <summary>
    /// Configuration for extraction.
    /// </summary>
    type ExtractionConfig = {
        StartDelimiter: string
        EndDelimiter: string
        IncludeDelimiters: bool
        TrimWhitespace: bool
    }

    /// <summary>
    /// Default extraction config.
    /// </summary>
    let defaultConfig = {
        StartDelimiter = "<!-- BEGIN:"
        EndDelimiter = "<!-- END:"
        IncludeDelimiters = false
        TrimWhitespace = true
    }

    /// <summary>
    /// Extracts all sections matching the delimiter pattern.
    /// </summary>
    let extractSections (content: string) (config: ExtractionConfig) : ExtractedSection list =
        let lines = content.Split([|"\r\n"; "\n"|], StringSplitOptions.None)
        let sections = ResizeArray<ExtractedSection>()
        let mutable currentSection = None
        let mutable sectionContent = ResizeArray<string>()
        let mutable sectionStartLine = 0
        
        for i = 0 to lines.Length - 1 do
            let line = lines.[i]
            
            match currentSection with
            | None ->
                // Looking for start delimiter
                if line.Contains(config.StartDelimiter) then
                    // Extract section name from delimiter
                    let nameStart = line.IndexOf(config.StartDelimiter) + config.StartDelimiter.Length
                    let nameEnd = line.IndexOf(" -->", nameStart)
                    let name = 
                        if nameEnd > nameStart then
                            line.Substring(nameStart, nameEnd - nameStart).Trim()
                        else
                            $"section_{sections.Count}"
                    
                    currentSection <- Some name
                    sectionStartLine <- i
                    sectionContent.Clear()
                    
                    if config.IncludeDelimiters then
                        sectionContent.Add(line)
            | Some name ->
                // Looking for end delimiter
                if line.Contains(config.EndDelimiter) then
                    if config.IncludeDelimiters then
                        sectionContent.Add(line)
                    
                    // Complete the section
                    let content = String.concat "\n" sectionContent
                    let finalContent = 
                        if config.TrimWhitespace then content.Trim()
                        else content
                    
                    sections.Add({
                        Name = name
                        StartDelimiter = config.StartDelimiter
                        EndDelimiter = config.EndDelimiter
                        Content = finalContent
                        LineStart = sectionStartLine
                        LineEnd = i
                    })
                    
                    currentSection <- None
                    sectionContent.Clear()
                else
                    sectionContent.Add(line)
        
        sections |> Seq.toList

    /// <summary>
    /// Extracts JavaScript sections from HTML.
    /// </summary>
    let extractJavaScript (html: string) : ExtractedSection list =
        let config = {
            StartDelimiter = "<script"
            EndDelimiter = "</script>"
            IncludeDelimiters = true
            TrimWhitespace = false
        }
        extractSections html config

    /// <summary>
    /// Extracts CSS sections from HTML.
    /// </summary>
    let extractCSS (html: string) : ExtractedSection list =
        let config = {
            StartDelimiter = "<style"
            EndDelimiter = "</style>"
            IncludeDelimiters = true
            TrimWhitespace = false
        }
        extractSections html config

    /// <summary>
    /// Extracts HTML body content.
    /// </summary>
    let extractBody (html: string) : string option =
        let bodyStart = html.IndexOf("<body")
        let bodyEnd = html.LastIndexOf("</body>")
        
        if bodyStart >= 0 && bodyEnd > bodyStart then
            let content = html.Substring(bodyStart, bodyEnd - bodyStart + 7)
            Some content
        else
            None

    /// <summary>
    /// Extracts HTML head content.
    /// </summary>
    let extractHead (html: string) : string option =
        let headStart = html.IndexOf("<head")
        let headEnd = html.IndexOf("</head>")
        
        if headStart >= 0 && headEnd > headStart then
            let content = html.Substring(headStart, headEnd - headStart + 7)
            Some content
        else
            None

    /// <summary>
    /// Tokenizes content by a delimiter.
    /// Similar to CMD's tokenization but in F#.
    /// </summary>
    let tokenize (content: string) (delimiter: string) : string list =
        content.Split([|delimiter|], StringSplitOptions.None)
        |> Array.toList

    /// <summary>
    /// Gets a substring using CMD-style slicing.
    /// %var:~start,length% equivalent.
    /// </summary>
    let slice (content: string) (start: int) (length: int) : string =
        if start < 0 then
            // Negative start = from end
            let actualStart = Math.Max(0, content.Length + start)
            let actualLength = Math.Min(length, content.Length - actualStart)
            if actualLength <= 0 then ""
            else content.Substring(actualStart, actualLength)
        elif length < 0 then
            // Negative length = from end
            let actualLength = Math.Max(0, content.Length + length - start)
            if actualLength <= 0 then ""
            else content.Substring(start, actualLength)
        else
            // Normal slice
            let actualStart = Math.Min(start, content.Length)
            let actualLength = Math.Min(length, content.Length - actualStart)
            content.Substring(actualStart, actualLength)

    /// <summary>
    /// CMD-style substring: %var:~start% (to end)
    /// </summary>
    let sliceToEnd (content: string) (start: int) : string =
        if start < 0 then
            let actualStart = Math.Max(0, content.Length + start)
            content.Substring(actualStart)
        else
            let actualStart = Math.Min(start, content.Length)
            content.Substring(actualStart)

    /// <summary>
    /// Finds all occurrences of a pattern and returns positions.
    /// </summary>
    let findAll (content: string) (pattern: string) : int list =
        let positions = ResizeArray<int>()
        let mutable pos = 0
        
        while pos < content.Length do
            let found = content.IndexOf(pattern, pos)
            if found >= 0 then
                positions.Add(found)
                pos <- found + 1
            else
                pos <- content.Length
        
        positions |> Seq.toList

    /// <summary>
    /// Replaces content between delimiters.
    /// </summary>
    let replaceBetween (content: string) (startDelim: string) (endDelim: string) (replacement: string) : string =
        let start = content.IndexOf(startDelim)
        if start < 0 then content
        else
            let endPos = content.IndexOf(endDelim, start + startDelim.Length)
            if endPos < 0 then content
            else
                let before = content.Substring(0, start + startDelim.Length)
                let after = content.Substring(endPos)
                before + replacement + after

    /// <summary>
    /// Wraps each line with custom prefix/suffix.
    /// </summary>
    let wrapLines (content: string) (prefix: string) (suffix: string) : string =
        let lines = content.Split([|"\r\n"; "\n"|], StringSplitOptions.None)
        lines
        |> Array.map (fun line -> prefix + line + suffix)
        |> String.concat "\n"

    /// <summary>
    /// Creates a line-indexed view of content.
    /// Similar to the XML project's array-based approach.
    /// </summary>
    let toIndexedArray (content: string) : (int * string) [] =
        content.Split([|"\r\n"; "\n"|], StringSplitOptions.None)
        |> Array.mapi (fun i line -> (i, line))

    /// <summary>
    /// Filters lines by a predicate.
    /// </summary>
    let filterLines (content: string) (predicate: string -> bool) : string =
        content.Split([|"\r\n"; "\n"|], StringSplitOptions.None)
        |> Array.filter predicate
        |> String.concat "\n"

    /// <summary>
    /// Removes empty lines from content.
    /// </summary>
    let removeEmptyLines (content: string) : string =
        filterLines content (fun line -> not (String.IsNullOrWhiteSpace(line)))


