open Generated.Views
open System.IO
let html = page |> Giraffe.ViewEngine.RenderView.AsString.htmlDocument
File.WriteAllText("output.html", html)
