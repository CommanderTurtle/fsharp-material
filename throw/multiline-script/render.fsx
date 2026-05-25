#r "nuget: Giraffe.ViewEngine"
#load "index.fs"
open Generated.Views
System.IO.File.WriteAllText("output.html", page |> Giraffe.ViewEngine.RenderView.AsString.htmlDocument)
.ViewEngine.RenderView.AsString.htmlDocument)
