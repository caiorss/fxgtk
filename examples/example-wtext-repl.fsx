#if INTERACTIVE    
#r "/usr/lib/mono/gtk-sharp-3.0/atk-sharp.dll"   
#r "/usr/lib/mono/gtk-sharp-3.0/gio-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/glib-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gtk-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gdk-sharp.dll"
#r "../bin/fxgtk.dll"
#endif


open System
open Fxgtk
open Fxgtk.Forms

let runThread (fn: unit -> unit) =
    let th = System.Threading.Thread fn
    th.Start()


App.init()

let wtext = WText.makeWText "TextView"

WText.show wtext


let words (s: string) =
    s.Split([|' '|], System.StringSplitOptions.RemoveEmptyEntries)
    |> List.ofArray

let readFile file = System.IO.File.ReadAllText(file)

let rec forever (fn: unit -> unit): unit =
    fn()
    forever fn 

let queryUser () =
    printf "Enter next line: "
    let line = System.Console.ReadLine()
    match words line with
    | ["exit" ]      -> App.exit()

    | ["clear" ]     -> App.invoke(fun _ -> WText.setText wtext "")

    | ["read"; file] -> App.invoke(fun _ -> WText.setText wtext (readFile file))
        
    | _              -> App.invoke(fun _ -> WText.addText wtext (line + "\n"))
                        

runThread (fun () -> forever queryUser)


App.run()



