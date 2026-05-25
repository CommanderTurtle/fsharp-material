

namespace Generator

open System
open Giraffe.ViewEngine

module DomRepresentation =
    type DomAttribute =
        | KeyValue of name: string * value: string
        | Boolean of name: string
        | Style of styles: (string * string) list
        | Data of key: string * value: string

    type DomNode =
        | Element of tag: string * attrs: DomAttribute list * children: DomNode list
        | VoidElement of tag: string * attrs: DomAttribute list
        | RawContent of content: string
        | Text of text: string
        | Comment of text: string
        | DocType of doctype: string

    type DomDocument = {
        DocType: string option
        Head: DomNode list
        Body: DomNode list
        CustomStyles: string list
        CustomScripts: string list
    }

    module VoidElements =
        let elements = Set.ofList [
            "area"; "base"; "br"; "col"; "embed"; "hr"; "img"; "input"
            "link"; "meta"; "param"; "source"; "track"; "wbr"
        ]
        let isVoid (tag: string) = Set.contains (tag.ToLower()) elements

    module RawContentElements =
        let elements = Set.ofList ["script"; "style"; "pre"; "textarea"]
        let isRawContent (tag: string) = Set.contains (tag.ToLower()) elements

    module DomNodeDSL =
        let element (tagName: string) attrs children = Element(tagName, attrs, children)
        let text content = Text content
        let rawText content = RawContent content
        let comment text = Comment text
        let voidEl tagName attrs = VoidElement(tagName, attrs)
        let div attrs children = Element("div", attrs, children)
        let span attrs children = Element("span", attrs, children)
        let p attrs children = Element("p", attrs, children)
        let a attrs children = Element("a", attrs, children)
        let img attrs = VoidElement("img", attrs)
        let h1 attrs children = Element("h1", attrs, children)
        let h2 attrs children = Element("h2", attrs, children)
        let nav attrs children = Element("nav", attrs, children)
        let header attrs children = Element("header", attrs, children)
        let footer attrs children = Element("footer", attrs, children)
        let main attrs children = Element("main", attrs, children)
        let section attrs children = Element("section", attrs, children)
        let button attrs children = Element("button", attrs, children)
        let ul attrs children = Element("ul", attrs, children)
        let li attrs children = Element("li", attrs, children)

    module AttrDSL =
        let className value = KeyValue("class", value)
        let id value = KeyValue("id", value)
        let href value = KeyValue("href", value)
        let src value = KeyValue("src", value)
        let alt value = KeyValue("alt", value)
        let title value = KeyValue("title", value)
        let disabled = Boolean("disabled")
        let style styles = Style(styles)
        let data key value = Data(key, value)
        let attr name value = KeyValue(name, value)






