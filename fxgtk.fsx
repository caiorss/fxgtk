

(* Compile with: 
  -------------------------------------------------------
                                                          
$ fsc fxgtk.fsx --target:winexe --out:fxgtk-app.exe \
    -r:/usr/lib/mono/gtk-sharp-3.0/atk-sharp.dll    \
    -r:/usr/lib/mono/gtk-sharp-3.0/gio-sharp.dll    \
    -r:/usr/lib/mono/gtk-sharp-3.0/glib-sharp.dll   \
    -r:/usr/lib/mono/gtk-sharp-3.0/gtk-sharp.dll    \
    -r:/usr/lib/mono/gtk-sharp-3.0/gdk-sharp.dll


*)   

#if INTERACTIVE    
#r "/usr/lib/mono/gtk-sharp-3.0/atk-sharp.dll"   
#r "/usr/lib/mono/gtk-sharp-3.0/gio-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/glib-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gtk-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gdk-sharp.dll"
#endif 
   
/// Build interface from Glade Generated layout.
/// Gtk Builder wrapper.
///
module Builder =
    type T = Gtk.Builder

    let errorIfNull inp =
        match inp with 
        | null -> failwith <| "Error: Object not found " + inp.ToString()
        | _    -> inp
        
    /// Load glade generated layout from file 
    let loadFile gladeFile =
        let gui = new Gtk.Builder()
        ignore <| gui.AddFromFile(gladeFile)
        gui

    /// Load glade generated layout from string 
    let load gladeString =
        let gui = new Gtk.Builder()
        ignore <| gui.AddFromFile(gladeString)
        gui

    /// Get window object from glade builder     
    let getWindow (builder: T) widgetID =
        builder.GetObject(widgetID) :?> Gtk.Window

    /// Get button object from glade builder     
    let getButton (builder: T) widgetID =
        builder.GetObject(widgetID) :?> Gtk.Button

    /// Get entry object from glade builder     
    let getEntry (builder: T) widgetID =
        builder.GetObject(widgetID) :?> Gtk.Entry
      


module EventTypes =
    type Event =
        | EventDelete
        | EventClicked


module Signal =
    open EventTypes
    
    type T = Gtk.Widget

    let onDelete (wdg: T) handler =
        wdg.DeleteEvent.Subscribe(fun arg -> handler arg)

    let onKeyRelease (wdg: T) handler =
        wdg.KeyReleaseEvent.Subscribe(fun arg -> handler arg)
    
    let onClick (wdg: T) handler =
        match wdg with
        | :? Gtk.Button as w -> w.Clicked.Subscribe(fun obs -> handler obs)
        // | :? Gtk.Window as w -> w.Clicked.Subscribe(handler)
        | _  -> failwith "Error: Not implemented for this widget"

    /// Add event handler to exit application when widget is destroyed.
    /// It is useful to add this event to the main window.
    ///
    let onDeleteExit (wdg: T) =
        wdg.DeleteEvent.Subscribe(fun _ -> Gtk.Application.Quit ())
    
    // let register (widget: T) evt handler =
    //     match evt with
    //     | EventDelete  -> widget.DeleteEvent.Subscribe(handler)
    //     | EventClicked -> let btn = widget :


/// Gtk.Application Wrapper module. 
module App =
    /// Start GTK application. Must be invoked before the GUI
    /// and widgets be created.
    let init () = Gtk.Application.Init()    

    /// Run GTK event loop 
    let run  () = Gtk.Application.Run()

    /// Run Gtk event loop in background thread 
    let runThread () =
        let thread = System.Threading.Thread Gtk.Application.Run
        // thread.IsBackground <- true
        thread.Start()

    let quit = Gtk.Application.Quit

    let runWith initializer =
        Gtk.Application.Init()
        initializer()
        Gtk.Application.Run()

    /// Safe timer to update Gtk GUI like Windows Forms timer.
    /// It is not safe to update the GUI from other threads.
    ///    
    /// Parameters:
    /// - delay   : int               timeout in milliseconds 
    /// - handler : unit -> unit      callback to be called each dycle 
    ///
    let runTimer (delay: int) handler =
        let timeoutHnd = new GLib.TimeoutHandler(fun _ ->  handler (); true)
        ignore <| GLib.Timeout.Add(System.Convert.ToUInt32 delay, timeoutHnd)

    let invoke (handler: unit -> unit) =
        let hnd = new System.EventHandler(fun _ _ -> handler ())
        Gtk.Application.Invoke(hnd)






module Dialog =

    /// Run dialog.
    let run (dialog: Gtk.Dialog) =
        ignore <| dialog.Run () ;
        dialog.Destroy ()


    /// Run dialog safely when the Gtk event loop is running
    /// in a background thread.
    ///
    let runInter (dialog: Gtk.Dialog) =
        App.invoke(fun () -> ignore <| dialog.Run())

    module AboutTypes =
        type AboutTypes =
            | AboutProgramName    of string
            | AboutProgramVersion of string
            | AboutCopyright      of string
            | AboutComments       of string
            | AboutWebsite        of string

    let private setAboutProp (about: Gtk.AboutDialog) prop =
        match prop with
        | AboutTypes.AboutProgramName v    -> about.ProgramName  <- v
        | AboutTypes.AboutProgramVersion v -> about.Version      <- v
        | AboutTypes.AboutCopyright v      -> about.Copyright    <- v
        | AboutTypes.AboutComments v       -> about.Comments     <- v
        | AboutTypes.AboutWebsite v        -> about.Website      <- v
     //   | _                     -> failwith "Error: Not implemented."


    let aboutDialog values =
        let about = new Gtk.AboutDialog()
        List.iter (setAboutProp about) values
        about

    let infoDialog (message: string) (parent: Gtk.Window)  =
        let dialog = new Gtk.MessageDialog(parent
                                           ,Gtk.DialogFlags.DestroyWithParent
                                           ,Gtk.MessageType.Info
                                           ,Gtk.ButtonsType.Close
                                           ,message
                                           )
        App.invoke (fun () -> ignore <| dialog.Run () ; dialog.Destroy ())


    let warningDialog (message: string) (parent: Gtk.Window)  =
        let dialog = new Gtk.MessageDialog(parent
                                           ,Gtk.DialogFlags.DestroyWithParent
                                           ,Gtk.MessageType.Warning
                                           ,Gtk.ButtonsType.Close
                                           ,message
                                           )
        App.invoke (fun () -> ignore <| dialog.Run () ; dialog.Destroy ())


    let errorDialog (message: string) (parent: Gtk.Window)  =
        let dialog = new Gtk.MessageDialog(parent
                                           ,Gtk.DialogFlags.DestroyWithParent
                                           ,Gtk.MessageType.Error
                                           ,Gtk.ButtonsType.Close
                                           ,message
                                           )
        App.invoke (fun () -> ignore <| dialog.Run () ; dialog.Destroy ())




    let runFileChoose (label: string) (path: string option) handler (win: Gtk.Window)  =

        let diag = new Gtk.FileChooserDialog(label
                                            ,win
                                            ,Gtk.FileChooserAction.Open
                                            ,"Cancel"
                                            ,Gtk.ResponseType.Cancel
                                            ,"Open"
                                            ,Gtk.ResponseType.Accept
                                            )

        Option.iter (fun p -> ignore <| diag.SetCurrentFolder p) path
        diag.CanDefault <- true

        App.invoke (fun () -> if diag.Run () = int Gtk.ResponseType.Accept
                              then handler <| Some diag.Filename
                              else handler None
                              diag.Destroy()
                    )



/// Image manipulation
module Pixbuf =
    type T = Gdk.Pixbuf

    /// Load image from file
    let loadFile (file: string) = new Gdk.Pixbuf(file)

    let getWidth (pb: T) = pb.Width

    let getHeight (pb: T) = pb.Height

    let getSize (pb: T) : (int * int) =
        (pb.Width, pb.Height)

    let scaleByFactor (scale: float) (stdHeight: int) (pb: T) =
        let w = float pb.Width
        let h = float pb.Height
        let hnew = scale * float stdHeight
        pb.ScaleSimple(int <| hnew * w / h ,
                       int hnew,
                       Gdk.InterpType.Bilinear)

    /// Scale image to a given height keeping the image proportions.
    ///
    /// Example: Image has size 300 x 400 it will be adjusted to height 500
    ///          - new height = 500
    ///          - new width  = 300 / 400 * 500 = 375
    ///          - new size   = 375 x 500
    ///
    let scaleToHeight (height: int) (pb: T) =
        let w = float pb.Width
        let h = float pb.Height
        pb.ScaleSimple(int <| w / h * float height,
                       height,
                       Gdk.InterpType.Bilinear)


/// Interface to Gtk Image
module Image =
    type T = Gtk.Image

    /// Create new image
    // let make = new Gtk.Image()

    /// Load image from file
    let loadFile (file: string) = new Gtk.Image(file)

    /// Set image from file
    let setFromFile (file: string) (wdg: T) =
        wdg.File <- file

    /// Get image buffer from image
    let getPixbuf (wdg: T) = wdg.Pixbuf

    /// Get image width and height
    let getSize (wdg: T): (int * int) =
        (wdg.Pixbuf.Width, wdg.Pixbuf.Height)

    let getHeight (wdg: T) = wdg.Pixbuf.Height

    let getWidth (wdg: T) = wdg.Pixbuf.Width

    /// Resize image by factor
    let scaleByFactor (scale: float) (stdHeight: int) (wdg: T) =
        wdg.Pixbuf <- Pixbuf.scaleByFactor scale stdHeight wdg.Pixbuf

    let scaleToHeight (maxHeight: int) (wdg: T) =
        wdg.Pixbuf <- Pixbuf.scaleToHeight maxHeight wdg.Pixbuf

module Menu =

    let make () = new Gtk.Menu ()

    let makeItem (label: string) handler =
        let item = new Gtk.MenuItem(label)
        item.Activated.Add(fun _ -> handler())
        item

    let popup (menu: Gtk.Menu) = menu.Popup()


/// Widget Module 
///
module Wdg =

    
    type T = Gtk.Widget

    /// Upcast widget to Gtk.Widget class
    let toWdg obj = obj :> T

    /// Create new Window  
    let makeWindow (title: string) = new Gtk.Window(title)

    /// Create new button 
    let makeButton label = new Gtk.Button(Label = label)

    /// Create new entry 
    let makeEntry () = new Gtk.Entry()

    /// Create textview 
    let makeTextView () = new Gtk.TextView()

    /// Create drawing area 
    let makeDrawingArea () = new Gtk.DrawingArea()

    /// Create new label  
    let makeLabel (label: string) = new Gtk.Label(label)

    let makeIconFromFile (file: string) = new Gtk.StatusIcon(file)
    
    let getEntryText (wdg: Gtk.Entry): string =
        wdg.Text
        
    let setEntryText (wdg: Gtk.Entry) (text: string) =
        wdg.Text <- text

    let setSize (wdg: T) (width: int) (height: int) =
        wdg.SetSizeRequest(width, height)

    /// Set background color     
    let modifyBg col (wdg: #T) =
        wdg.ModifyBg(Gtk.StateType.Normal, col)


    /// Show widget
    let show (wdg: T) = wdg.Show()

    let showAll (wdg: T) = wdg.ShowAll()
        
    let add (parent: Gtk.Container) (child: T) =
        parent.Add(child)
        parent

  
 
module LayoutConf =
    type LayoutConf = {
           Homon:   bool
         ; Space:   int 
         ; Fill:    bool
         ; Expand:  bool 
         ; Padding: int 
        }    



/// Gtk Containers 
module Container =
    open LayoutConf
    
        
    let defaultConf  =
        {Homon   = true;
         Space   = 1;
         Fill    = true;
         Expand  = false;
         Padding = 0
         }
        
    /// Create scrolledwindow    
    let scrolledWindow (child: Gtk.Widget) =
        let sc = new Gtk.ScrolledWindow()
        sc.Add(child)
        sc 

    /// Create fixed container which can position widgets by coordinate.
    let makeFixed () = new Gtk.Fixed ()


    /// Horizontal box container
    let hbox (conf: LayoutConf) (wdglist: Gtk.Widget list)  =
        let hbox = new Gtk.HBox(conf.Homon, conf.Space)
        wdglist |> List.iter (fun w -> hbox.PackStart(w,
                                                      conf.Expand,
                                                      conf.Fill,
                                                      System.Convert.ToUInt32 conf.Padding))
        hbox 


    /// Vertical box container
    let vbox (conf: LayoutConf) (wdglist: Gtk.Widget list)  =
        let vbox = new Gtk.VBox(conf.Homon, conf.Space)
        wdglist |> List.iter (fun w -> vbox.PackStart(w,
                                                      conf.Expand,
                                                      conf.Fill,
                                                      System.Convert.ToUInt32 conf.Padding))
        vbox 



module Window =
    type T = Gtk.Window

    /// Window position constants
    module WindowPos =
        let center       = Gtk.WindowPosition.Center
        let centerAlways = Gtk.WindowPosition.CenterAlways
        let centOnParent = Gtk.WindowPosition.CenterOnParent

    let setDefaultSize w h (wdg: T) =
        wdg.SetDefaultSize(w, h)
        wdg

    let setPosition pos (wdg: T) =
        wdg.SetPosition(pos)
        wdg

    let setIconFromFile (file: string) (wdg: T) =
        wdg.SetIconFromFile(file)

    let setBorderWidth (width: int) (wdg: T) =
        wdg.BorderWidth <- System.Convert.ToUInt32 width
        

    let getSize (wdg: T) = wdg.GetSize()

    let getPosition (wdg: T) = wdg.GetPosition()    


module Color =
    let parse colName =
        let col = ref <| new Gdk.Color()
        match Gdk.Color.Parse(colName, col) with 
        | true  -> Some !col
        | false -> None

    let parseOrFail colName =
        match parse colName with
        | Some col -> col
        | _        -> failwith "Error: invalid color name"

    /// Create color from rgb (red blue green)
    ///
    let rgb (r: int) (g: int) (b: int) =
        let rr = System.Convert.ToByte(r)
        let gg = System.Convert.ToByte(g)
        let bb = System.Convert.ToByte(b)
        new Gdk.Color(rr, gg, bb)

    // ======== Color constants ========= \\
    //    
    let Red    = parseOrFail "red"
    let Blue   = parseOrFail "blue"
    let Yellow = parseOrFail "yellow"
    let Brown  = parseOrFail "brown"
    let White  = parseOrFail "white"
    let Black  = parseOrFail "black"
    let Cyan   = parseOrFail "cyan"




