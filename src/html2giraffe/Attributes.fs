

namespace Html2Giraffe

open System

module Attributes =
    let voidElements =
        [ "area"; "base"; "br"; "col"; "embed"; "hr"; "img"; "input"
          "link"; "meta"; "param"; "source"; "track"; "wbr" ]
        |> Set.ofList

    let booleanAttributes =
        [ "allowfullscreen"; "async"; "autofocus"; "autoplay"; "checked"
          "controls"; "default"; "defer"; "disabled"; "formnovalidate"
          "hidden"; "ismap"; "itemscope"; "loop"; "multiple"; "muted"
          "nomodule"; "novalidate"; "open"; "playsinline"; "readonly"
          "required"; "reversed"; "selected"; "truespeed" ]
        |> Set.ofList

    let isVoidElement (tagName: string) =
        voidElements.Contains(tagName.ToLowerInvariant())

    let isBooleanAttribute (attrName: string) =
        booleanAttributes.Contains(attrName.ToLowerInvariant())

    let giraffeAttributeMap =
        [ "class", "_class"
          "for", "_for"
          "type", "_type"
          "id", "id"
          "href", "href"
          "src", "src"
          "alt", "alt"
          "title", "title"
          "name", "name"
          "value", "value"
          "placeholder", "placeholder"
          "disabled", "disabled"
          "readonly", "readonly"
          "required", "required"
          "checked", "checked"
          "selected", "selected"
          "multiple", "multiple"
          "style", "style"
          "target", "target"
          "rel", "rel" ]
        |> Map.ofList

    let getGiraffeAttrName (attrName: string) =
        let key = attrName.ToLowerInvariant()
        match giraffeAttributeMap.TryFind key with
        | Some name -> name
        | None ->
            if key.StartsWith("data-") then
                sprintf "data \"%s\"" (key.Substring(5))
            elif key.StartsWith("aria-") then
                sprintf "aria \"%s\"" (key.Substring(5))
            else
                match key with
                | "abstract" | "and" | "as" | "assert" | "base" | "begin"
                | "class" | "default" | "delegate" | "do" | "done" | "downcast"
                | "downto" | "elif" | "else" | "end" | "exception" | "extern"
                | "false" | "finally" | "fixed" | "for" | "fun" | "function"
                | "global" | "if" | "in" | "inherit" | "inline" | "interface"
                | "internal" | "lazy" | "let" | "match" | "member" | "module"
                | "mutable" | "namespace" | "new" | "not" | "null" | "of"
                | "open" | "or" | "override" | "private" | "public" | "rec"
                | "return" | "sig" | "static" | "struct" | "then" | "to"
                | "true" | "try" | "type" | "upcast" | "use" | "val"
                | "void" | "when" | "while" | "with" | "yield" ->
                    sprintf "_%s" key
                | _ -> key

    let escapeForFsharp (str: string) : string =
        str.Replace("\\", "\\\\")
           .Replace("\"", "\\\"")
           .Replace("\n", "\\n")
           .Replace("\r", "\\r")
           .Replace("\t", "\\t")






