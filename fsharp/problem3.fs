module ChadNedzlek.AdventOfCode.Y2022.FSharp.problem3
    open System.Linq

    let bag (line:string) =
        (line[..(line.Length/2)-1], line[(line.Length/2)..])
        
    let priority (c:char) =
        match c with
        | lower when c >= 'A' && c <= 'Z' -> (int lower) - (int 'A') + 27
        | upper when c >= 'a' && c <= 'z' -> (int upper) - (int 'a') + 1
        | _ -> failwith "invalid character"
        
    let execute(data) =
        let bagged =
            data
            |> Seq.map bag
            |> Seq.map (fun (a, b) -> a.Intersect(b))
            |> Seq.map Seq.head
            |> Seq.map priority
            |> Seq.sum

        printfn $"Total priority %d{bagged}"

        let rec chunk3 list =
            match list with
            | a::t ->
                match t with
                | b :: t ->
                    match t with
                    | c :: t ->
                        seq {
                            yield [a,b,c]
                            yield! chunk3 t
                        }
            | _ -> []

        let elfed =
            data
            |> List.ofSeq
            |> chunk3
            |> Seq.map (fun ([a,b,c]) -> a.Intersect(b).Intersect(c))
            |> Seq.map Seq.head
            |> Seq.map priority
            |> Seq.sum

        printfn $"Badge priority %d{elfed}"
