(*
This example shows how to build an GUI with two windows using the
layout combinators. One window is an image viewer and the other one is
text viewer.


*)   

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

open System 
open Fxgtk
module L = Fxgtk.Layout
open Fxgtk.Layout.Attribute

App.init()

//---------- Window 1 - Text Editor ---------- //

let buttonOpen =
    L.button [
        Text "Open"
      ; Tooltip "Open a file and display its content."
        ]


let buttonClear =
    L.button [
        Text "Clear"
      ; Tooltip "Clear text view"
        ]


let buttonClose =
    L.button [
        Text "Close"
      ; Name "buttonClose"
      ; Tooltip "Exit application"
      ; OnClick App.exit                                 
        ]

let tview = L.textview [ Vexpand true ; Expand true]



let win =
    L.window [   Text "Gtk Text Editor"
               ; Size (500, 400)
               ; ShowAll
               // ; Icon <| Pixbuf.loadFile "images/icontest.jpg"
               ; ExitOnDelete
               ] [
        L.vbox [
           L.pack <| L.hbox [
                              L.pack buttonOpen
                            ; L.pack buttonClear
                            ; L.pack buttonClose
                              ]            
         ; L.pfill <| L.scrolled  tview
           ] // End of vbox 
        ]


let updateEntry msg =
    match msg with
    | None       -> () 
    | Some file  -> let text = System.IO.File.ReadAllText file
                    TextView.setText tview text 

    
Button.onClick buttonClear (fun _ -> TextView.clearText tview)

Button.onClick buttonOpen (fun _ ->
                           Dialog.fileChooser win
                                              "Choose a file"
                                              None
                                              updateEntry
                           )

// --------- Window 2 - Image Viewer -------- //


let img = L.image [] 


let buttonShowImage =
    L.button [
          Text "Open Image"
        ; Tooltip "Select an image file and display it."       
        ]
    
let win2 =
    L.window [
          Size (500, 400)
        ; Text "Sample Image Viwer"
        ; ShowAll
        ; ExitOnDelete

        ] [
        L.vbox [
           L.pack  <| L.hbox [ L.pack buttonShowImage ] 
         ; L.pfill <| img 
        ]]


let updateImage msg =
    match msg with
    | None      -> () 
    | Some file -> let h = Wdg.getHeight img 
                   Image.setFromFileScale h file img
                   
Button.onClick buttonShowImage (fun _ ->
                                Dialog.fileChooser win2
                                                   "Choose an image"
                                                   None
                                                   updateImage
                                )


App.run()
