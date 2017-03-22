

let window = Wdg.makeWindow "Equations"
let btnUpdate = Wdg.makeButton "Update"
let bntClean  = Wdg.makeButton "Clean"


type InputRows = Map<string, Gtk.Entry>

let makeInputRows conf (inpuRows: string list) =
    let rows = inpuRows |> List.map (fun r -> let entry = Wdg.makeEntry()
                                              let label = Wdg.makeLabel r
                                              r, (label, entry)
                                     )
        
    let inputs = rows |> List.map (fun (r, (_, entry)) -> r, entry)

    let hboxList: Gtk.Widget list = rows |> List.map(fun (r, (label, entry)) ->
                                                     upcast Wdg.hbox conf [label; entry])
    
    let vbox = Wdg.vbox conf hboxList
    Map.ofList inputs, vbox 
    

let tryParseFloat (s: string) =
    match s with
    | "" -> Some 0.0
    | _  -> try Some (float s)
             with _ -> None 
    
let getRowValues (inpRows: Map<string, Gtk.Entry>)  =
    Map.map (fun k (v: Gtk.Entry) -> tryParseFloat v.Text) inpRows


let getRowValue (rows: Map<string, Gtk.Entry>) (rowId: string)  =
    rows |> Map.tryFind rowId
         |> Option.map Wdg.getEntryText
         |> Option.bind tryParseFloat

let setOutputValue (rows: Map<string, Gtk.Entry>) (rowId: string) value =
    rows |> Map.tryFind rowId
         |> Option.iter (fun entry -> entry.Text <- value)    


// let inputRows: Gtk.Widget list = inputs |> List.map (fun inp ->
//                                                      let entry = Wdg.makeEntry()
//                                                      let label = Wdg.makeLabel inp 
//                                                      upcast Wdg.hbox conf [label; entry] // :?> Gtk.Widget 

//                                                      )

// let vbox = Wdg.vbox conf inputRows




let outputs = ["a"; "b"]

let conf = Wdg.defaultConf

let (inputDic, inputVbox) = makeInputRows conf ["x"; "y"; "z"]

let (outputDic, outputVbox) = makeInputRows conf ["a"; "b"]

let hsep1 = new Gtk.HSeparator()

let hbox = Wdg.vbox conf [inputVbox ; Wdg.makeLabel "Outputs"; outputVbox] 


let toString obj =
    obj.ToString()




// let optApply3 (xs: 'a option list) fn =

let updateOutput () =
    let (>>=) ma fn = Option.bind fn ma
    let x = getRowValue inputDic "x"
    let y = getRowValue inputDic "y"
    let z = getRowValue inputDic "z"
    
    let a = x >>= fun x ->
            y >>= fun y ->
            z >>= fun z -> 
                Some ( x * 2.0 + 3.0 * y + z)
                
    let b = x  >>= fun x ->
            y  >>= fun y ->
            z  >>= fun z ->
                Some ( x * 3.0 + 4.0 * y + 5.0)

    a |> Option.iter (toString >> setOutputValue outputDic "a")
    b |> Option.iter (toString >> setOutputValue outputDic "b")


inputDic |> Map.iter (fun k (e: Gtk.Entry) ->
                      ignore <| Signal.onKeyRelease e (fun _ -> updateOutput()) )
   
Wdg.add window hbox 

// inputRows |> List.iter window.Add                        
// window.Add (Wdg.hbox conf inputRows)

Window.showAll window

updateOutput()

App.run()


let optSeq (xs: 'a option list) =
    let rec aux xs acc =
        match xs with
        | []       -> Some <| List.rev acc
        | (hd::tl) -> match hd with
                      | None    -> None
                      | Some a  -> aux tl (a::acc)
    aux xs []
    

let applyList3 fn xs =
    if List.length xs < 3
    then None
    else fn (List.item 0 xs) (List.item 1 xs) (List.item 2 xs)


let applyOptList fn xs =
    xs |> optSeq
       |> Option.map fn 
