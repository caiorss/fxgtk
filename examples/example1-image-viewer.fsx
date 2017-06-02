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
open System
module L = Fxgtk.Layout

App.init()


let win = Window.mainWindow "Main window" 

let entryImagePath = Entry.entry()
let buttonOpen     = Button.button "Open Image"
let buttonClear    = Button.button "Clear"
let image          = Image.loadFile ""

let vbox = L.vbox [ L.pack entryImagePath
                  ; L.phbox [ L.pack buttonOpen ; L.pack buttonClear ]
                  ; L.pfill image 
                    ]
                       
Wdg.add win vbox


let model = FModel.make ""

FModel.subscribe model (fun file ->
                       Image.setFromFileScale (Wdg.getHeight image) file image
                       Entry.setText entryImagePath file 
                      )

FModel.addLogger model 

win.SizeAllocated.Subscribe (fun _ -> FModel.trigger model)

let imageFilter = "Image Files", ["*.png" ;"*.jpeg"; "*.png"; "*.tiff"; "*.bmp"; "*.gif"] 

Button.onClick buttonOpen (fun _ ->
                           Dialog.fileChooser("Choose an image"
                                              ,path = Some "~"
                                              ,filter = Some imageFilter
                                              )
                           |> Option.iter (FModel.update model)                           
                           )

Button.onClick buttonClear (fun _ ->
                            FModel.update model ""
                            Dialog.infoDialog win "Image cleaned"
                            )
                           
App.run()
