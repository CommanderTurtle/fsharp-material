module Generated.Views

open Giraffe.ViewEngine

let page =
    html [] [
        head [] [
            title [] [
                str "Script Test"
            ]
        ]
        body [] [
            div [] [
                script [] [
                        rawText ("""// Line 1
function test() {
  // Line 3
  return "hello";
  // Line 5
}
// Line 7""")
                ]
                p [] [
                    str "After script"
                ]
            ]
        ]
    ]

let render() =
    page |> Giraffe.ViewEngine.RenderView.AsString.htmlDocument
