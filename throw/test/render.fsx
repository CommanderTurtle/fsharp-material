#r "nuget: Giraffe.ViewEngine"
#load "index.fs"
open Generated.Views
System.IO.File.WriteAllText("throw/test/output.html", page |> Giraffe.ViewEngine.RenderView.AsString.htmlDocument)
