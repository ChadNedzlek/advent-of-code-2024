open System.IO
open System.Reflection
open System.Text.RegularExpressions
    
[<EntryPoint>]
let main args =
    
    let rootDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
    let dataRoot = Path.Combine(rootDir, "data")
    
    let allModules =
        Assembly.GetExecutingAssembly().GetTypes()
        |> Seq.filter (fun t -> t.IsAbstract && t.IsSealed && t.IsPublic)
        |> Seq.filter (fun t -> Regex.IsMatch(t.Name, "problem(\d+)"))
        |> Seq.sortBy (fun t -> t.FullName[7..])
        |> Seq.rev
    
    let highestModule = allModules |> Seq.head
    
    let index = int highestModule.Name[7..]
    
    let problemType = if Array.contains "--example" args then "example" else "real"     
    
    let targetFile = Path.Combine(dataRoot, $@"data-%02d{index}-{problemType}.txt")

    let lines = File.ReadLines(targetFile) |> Seq.cache
    
    let targetMethod = highestModule.GetMethod("execute")
    
    let toExecute =
        match targetMethod with
        | method when method.IsGenericMethod -> method.MakeGenericMethod([|lines.GetType()|])
        | x -> x
    
    toExecute.Invoke(null, [|lines|]) |> ignore
    0
    
    