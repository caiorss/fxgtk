#if INTERACTIVE    
#r "/usr/lib/mono/gtk-sharp-3.0/atk-sharp.dll"   
#r "/usr/lib/mono/gtk-sharp-3.0/gio-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/glib-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gtk-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gdk-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/pango-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/cairo-sharp.dll"

#r "../fxgtk.dll"
#endif

open Fxgtk
open Fxgtk.Layout.Attribute
module L = Fxgtk.Layout

App.init()


let tview =
    TreeView.treeViewText [
        "Currency",              typeof<string>
      ; "Exchange rate per USD", typeof<float>
      ; "Total",                 typeof<float>
    ]

let entryAmount =
    L.entry [
          Text    "0.00"
        ; Tooltip "Enter the amount in USD."
        ; BgColor Color.LightBlue  
    ]

let win =
    L.window  [  Text "Exchange Rates"
               ; ExitOnDelete
               ; ShowAll
               ; Size (500, 400)
               ; Resizable false
               ] [
        L.vbox [
            L.phbox [
                      L.pack  <|  L.label [ Text "Amount in USD"
                                            ; BgColor Color.LightGreen
                                          ]                                    
                     ; L.pack  entryAmount
                      ]                     
      ;  L.pfill tview
        ]]


TreeView.addRowList tview [
      [| "USD" ; 1.0  ; 0.0 |]
    ; [| "EUR" ; 0.96 ; 0.0 |]
    ; [| "AUR" ; 0.75 ; 0.0 |]
    ; [| "GBP" ; 0.64 ; 0.0 |]      
    ; [| "BRL" ; 3.25 ; 0.0 |]
    ]

win.Resizable

let updateTotal amount =
    let model = TreeView.getModel tview
    let rates = ListStore.getColumn<float> model 1
    let total = List.map ((*) amount)  rates
    ListStore.setColumn model 2 total


Entry.onTextChange entryAmount
|> Observable.subscribe (fun _ -> match Entry.getFloat entryAmount with
                                  | None     -> ()
                                  | Some qnt -> updateTotal qnt
                         )

#if REPL         
let () =
    App.runThread() // For interactive use. $ fsharpi --use:listbox1.fsx --define:REPL
#else         
let () =
    App.run()       // For compilation and batch mode.
#endif 

