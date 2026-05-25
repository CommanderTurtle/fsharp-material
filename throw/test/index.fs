namespace Generated

open Giraffe.ViewEngine

module Views =

    let page =
        html [ _lang "en" ] [
            head [] [
                title [] [
                    str "Test"
                ]
            ]
            body [] [
                div [ _class "container" ] [
                    h1 [] [
                        str "Hello World"
                    ]
                    p [] [
                        str "This is a"
                        strong [] [
                            str "test"
                        ]
                        str "with"
                        a [ _href "/link" ] [
                            str "a link"
                        ]
                        str "."
                    ]
                    script [] [ rawText ("""console.log("Hello");""") ]
                ]
            ]
        ]

    let render() =
        page |> Giraffe.ViewEngine.RenderView.AsString.htmlDocument
