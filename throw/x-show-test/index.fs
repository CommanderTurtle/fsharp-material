namespace Generated

open Giraffe.ViewEngine

module Views =

    let page =
        html [] [
            head [] [
                title [] [
                    str "Alpine.js Test"
                ]
            ]
            body [] [
                div [ attr "x-show" "length > 0"; _class "container" ] [
                    h1 [] [
                        str "Hello World"
                    ]
                    p [] [
                        str "This tests quote-aware parsing with > in attributes."
                    ]
                ]
                div [ attr "x-data" "{ open: false }" ] [
                    button [ attr "@click" "open = !open" ] [
                        str "Toggle"
                    ]
                    div [ attr "x-show" "open"; attr "style" "display: none;" ] [
                        p [] [
                            str "Hidden content with"
                            strong [] [
                                str "bold"
                            ]
                            str "text."
                        ]
                    ]
                ]
            ]
        ]

    let render() =
        page |> Giraffe.ViewEngine.RenderView.AsString.htmlDocument
