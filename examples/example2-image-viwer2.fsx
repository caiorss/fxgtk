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
open Fxgtk.TreeView.Types
open Fxgtk.WUtils



App.init()

//--------------- viwer GUI Layout ------------------------  // 

let window = Window.mainWindow "TreeViewTest"
             // |> Window.setSize 143 200
             |> Window.setPosition Window.PosCenter

let imview = ImageView.makeImageView "Image Display"
ImageView.show imview


let model =  ListStore.listStore (
    [
     typeof<Gdk.Pixbuf> // (Col 0) Image Thumbnail  - IconView image
    ;typeof<string>     // (Col 1) File name        - IconView label
    ;typeof<Gdk.Pixbuf> // (Col 2) Original image read from file
    ;typeof<string>     // (Col 3) Full file name with path
     ])


let iconview =  IconView.iconView model 0 1 
iconview.SelectionMode <- Gtk.SelectionMode.Browse

// Add widget 
Wdg.add window (Layout.scrolled iconview)

// Show Window 
Wdg.showAll window   

// --------------    Event Handlers and Tasks ---------- // 


let filterExtension (extList: string list) (fileName: string) =
    List.exists fileName.EndsWith extList

let findFiles extList directory =
    System.IO.Directory.EnumerateFiles(directory, "*.*", System.IO.SearchOption.AllDirectories)
    |> Seq.filter (filterExtension extList)

let findImages = findFiles [".png"; ".jpg"; ".jpeg"; ".gif"; ".tiff"; ".mp4"; ".avi"]


let updateImage iconview display =
    let image = IconView.getSelecetdItem iconview 2 :?> Gdk.Pixbuf
    ImageView.setImageFromPbuf display image 


let addImageFile (lstore: Gtk.ListStore) file =
    let label = System.IO.Path.GetFileName file
    let image = file |> Pixbuf.loadFile
    let thumb = Pixbuf.scaleToHeight 50 image 
    ignore <| lstore.AppendValues(thumb, label, image, file)



let startModel path = 
    path |> findImages
         |> Seq.iter (addImageFile model)

let args = System.Environment.GetCommandLineArgs()
          |> List.ofArray
          |> List.tail 

let chooseDirectory () =
    let path = Dialog.dirChooser(window
                                 ,"Choose a directory"
                                 ,Some "~"
                                 )         
    match path with
    | None   -> App.exit ()
    | Some p -> startModel p
    


let setClipboard text =
    let clip = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));
    clip.Text <- text


let update () =  updateImage iconview imview    


IconView.onActivated iconview (fun _ -> update ())
IconView.onMoveCursor iconview (fun _ -> update ())


chooseDirectory()

#if REPL         
let () = App.runThread()
#else         
let () = App.run() 
#endif 

