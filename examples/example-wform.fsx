
#if INTERACTIVE    
#r "/usr/lib/mono/gtk-sharp-3.0/atk-sharp.dll"   
#r "/usr/lib/mono/gtk-sharp-3.0/gio-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/glib-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gtk-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gdk-sharp.dll"
#r @"../fxgtk.dll"
#endif

open System 
open Fxgtk
open Fxgtk.Forms


App.init()

let btnClickMe = Button.button "Click me"
let entry1 = Entry.entry()
let entry2 = Entry.entry()
let entry3 = Entry.entry()

/// Wigets are positioned by coordinates like Windows forms
///
let form = WForm.makeForm "My form"
           |> WForm.onDeleteExit
           |> WForm.setBgColor Color.LightGreen
           // Add widgets 
           |> WForm.put btnClickMe (50, 20)
           |> WForm.put entry1 (50, 50)
           |> WForm.put entry2 (50, 100)
           |> WForm.put entry3 (50, 200)
           // Show Form 
           |> WForm.show

// entry1.Activated.Subscribe(fun _ -> printfn "text = %s" <| Entry.getText entry1)


let updateSum () =
    let n1 = Entry.getFloat entry1
    let n2 = Entry.getFloat entry2
    match (n1, n2) with
    | Some x1, Some x2
       -> Entry.setValue entry3 (x1 + x2)
    | _
       -> Entry.setText entry3 "Error: Invalid input"
        

Observable.merge (Entry.onTextChange entry1) (Entry.onTextChange entry2)
|> Observable.subscribe (fun _ -> updateSum ())


WForm.onMouseMove form |> Observable.subscribe (printfn "%A")
           
App.run()
  
