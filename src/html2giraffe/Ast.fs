

namespace Html2Giraffe

/// <summary>
/// Abstract Syntax Tree (AST) types for HTML representation.
/// </summary>
module Ast =

    /// <summary>
    /// Represents an HTML attribute.
    /// </summary>
    type Attribute = {
        Name: string
        Value: string option
        IsBoolean: bool
    }

    /// <summary>
    /// Represents a text node.
    /// </summary>
    type TextNode = {
        Content: string
        IsRaw: bool
    }

    /// <summary>
    /// Represents any HTML node.
    /// </summary>
    type HtmlNode =
        | Element of ElementNode
        | Text of TextNode
        | Comment of string
        | DocType of string
        | Raw of string

    /// <summary>
    /// Represents an HTML element.
    /// </summary>
    and ElementNode = {
        TagName: string
        Attributes: Attribute list
        Children: HtmlNode list
        IsVoid: bool
    }

    /// <summary>
    /// Represents an HTML document.
    /// </summary>
    type HtmlDocument = {
        DocType: string option
        Children: HtmlNode list
    }

    /// <summary>
    /// Creates a text node.
    /// </summary>
    let text content = Text { Content = content; IsRaw = false }

    /// <summary>
    /// Creates a raw text node.
    /// </summary>
    let rawText content = Text { Content = content; IsRaw = true }

    /// <summary>
    /// Creates an element.
    /// </summary>
    let element tagName =
        Element { TagName = tagName; Attributes = []; Children = []; IsVoid = false }

    /// <summary>
    /// Creates an element with attributes.
    /// </summary>
    let elementWithAttrs tagName attrs =
        Element { TagName = tagName; Attributes = attrs; Children = []; IsVoid = false }

    /// <summary>
    /// Creates an element with children.
    /// </summary>
    let elementWithChildren tagName attrs children =
        Element { TagName = tagName; Attributes = attrs; Children = children; IsVoid = false }

    /// <summary>
    /// Creates a void element.
    /// </summary>
    let voidElement tagName attrs =
        Element { TagName = tagName; Attributes = attrs; Children = []; IsVoid = true }

    /// <summary>
    /// Creates an attribute.
    /// </summary>
    let attr name value =
        { Name = name; Value = Some value; IsBoolean = false }

    /// <summary>
    /// Creates a boolean attribute.
    /// </summary>
    let boolAttr name =
        { Name = name; Value = None; IsBoolean = true }

    /// <summary>
    /// Creates an empty document.
    /// </summary>
    let emptyDocument = { DocType = None; Children = [] }

    /// <summary>
    /// Creates a document with children.
    /// </summary>
    let document children = { DocType = None; Children = children }






