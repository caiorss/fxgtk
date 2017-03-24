

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


/// Gtk Color combinators
module Color =

    /// Parse color name returning None (option) if it fails.
    let parse colName =
        let col = ref <| new Gdk.Color()
        match Gdk.Color.Parse(colName, col) with
        | true  -> Some !col
        | false -> None

    /// Parse color and throw exception if it fail.
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
    let Green  = parseOrFail "green"
    let Grey   = parseOrFail "gray"

    let LightBlue  = parseOrFail "lightblue"
    let LightGreen = parseOrFail "lightgreen"
    let LigthGray  = parseOrFail "lightgray"


   
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

    let exit () =
        Gtk.Application.Quit()
        ignore <| System.Environment.Exit(0)

    let runWith initializer =
        Gtk.Application.Init()
        initializer()
        Gtk.Application.Run()

    let withThread thunk =
        Gdk.Threads.Init ()
        Gdk.Threads.Enter()
        let resp = thunk ()
        Gdk.Threads.Leave ()
        System.Threading.Thread.Sleep 10
        resp


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

    /// Question Dialog with Yes and No button
    let questionDialog (message: string) handler (parent: Gtk.Window) =
        let dialog = new Gtk.MessageDialog(parent
                                           ,Gtk.DialogFlags.DestroyWithParent
                                           ,Gtk.MessageType.Question
                                           ,Gtk.ButtonsType.YesNo
                                           ,message
                                           )
        App.invoke (fun () ->
                    handler (dialog.Run () = int Gtk.ResponseType.Yes );                    
                    dialog.Destroy ()

                    )
       


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



module TextView =

    type T = Gtk.TextView

    /// Create new text view widget
    let make () =
        new Gtk.TextView()

    let getBuffer (tvw: T) =
        tvw.Buffer

    let getText (tvw: T) =
        tvw.Buffer.Text

    let setText (tvw: T) text =
        tvw.Buffer.Text <- text

    let clearText (tvw: T) =
        tvw.Buffer.Text <- ""

    let addText (tvw: T) text =
        tvw.Buffer.Text <- tvw.Buffer.Text + text

    let addLine (tvw: T) line =
        tvw.Buffer.Text <- tvw.Buffer.Text + "\n" + line

    let setEditable (tvw: T) flag =
        tvw.Editable <- flag



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
    let getPixbuf (wdg: T) =
        Option.ofObj wdg.Pixbuf

    /// Get image width and height
    let getSize (wdg: T): (int * int) option =
        wdg |> getPixbuf
            |> Option.map (fun w -> wdg.Pixbuf.Width, wdg.Pixbuf.Height )

    let getHeight (wdg: T) =
        wdg |> getPixbuf
            |> Option.map (fun pb -> pb.Height)

    let getWidth (wdg: T) =
        wdg |> getPixbuf
            |> Option.map (fun pb -> pb.Width)

    /// Resize image by factor
    let scaleByFactor (scale: float) (stdHeight: int) (wdg: T) =
        wdg.Pixbuf <- Pixbuf.scaleByFactor scale stdHeight wdg.Pixbuf

    let scaleToHeight (maxHeight: int) (wdg: T) =
        wdg |> getPixbuf
            |> Option.iter (fun pb -> wdg.Pixbuf <- Pixbuf.scaleToHeight maxHeight pb)


    let setFromFileScale (height: int) (file: string) (wdg: T) =
        wdg.File <- file
        scaleToHeight height wdg




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

    let getSize (wdg: T) =
        let w = ref 0
        let h = ref 0
        wdg.GetSizeRequest (w, h)
        (!w, !h)


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

    let fixedItems (itemsList: (#Gtk.Widget * int * int) list) =
        let fix = new Gtk.Fixed ()
        itemsList |> List.iter fix.Put ;
        fix

    /// Position widget with coordinates in a fix container
    ///
    /// put (fix: Fixed) (widget, x, y)
    ///
    /// Example:
    ///         > put fixed (entry, 200, 300)
    ///
    let put (fix: Gtk.Fixed) ((wdg, x, y): Gtk.Widget * int * int) =
        fix.Put(wdg, x, y)

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



module TreeView =

    type ListBox = {
         ListBoxView:      Gtk.TreeView
       ; ListBoxContainer: Gtk.ScrolledWindow
       ; ListBoxModel:     Gtk.ListStore
        }

    /// Create new TreeView object
    let treeView () = new Gtk.TreeView ()

    /// Create new TreeView Column object
    let treeViewColumn title = new Gtk.TreeViewColumn(Title=title)

    /// Create cell renderer text object
    let cellRenderText () = new Gtk.CellRendererText()


    /// Get value of selected column
    /// Note: The returned value must be cast to the column type.
    ///
    let treeViewGetSelectedCol (tview: Gtk.TreeView) (model: Gtk.ListStore) column =
        let titer = ref Unchecked.defaultof<Gtk.TreeIter>
        tview.Selection.GetSelected titer
        model.GetValue(!titer, column)


    let onTreeViewChanged (tview: Gtk.TreeView) (handler: unit -> unit) : System.IDisposable =
        tview.Selection.Changed.Subscribe(fun _ -> handler ())

    /// Create new ListStore object
    ///
    ///  Example:
    ///
    ///  > let l1 = listStore [| typeof<string>; typeof<float> |]
    ///    val l1 : Gtk.ListStore
    ///
    let listStore (types: System.Type []) = new Gtk.ListStore(types)

    /// Add a row of values to ListStore
    let listStoreAddRow (lstore: Gtk.ListStore) row =
        lstore.AppendValues(row)

    let listStoreAddValue (lstore: Gtk.ListStore) value : unit =
        ignore <| lstore.AppendValues([|value|])

    let listBox colType label =
        let tview = treeView()
        let col   = treeViewColumn label
        let model = listStore [| colType |]
        let cell  = cellRenderText ()
        col.PackStart(cell, true)
        col.AddAttribute(cell, "text", 0)
        ignore <| tview.AppendColumn(col)
        tview.Model <- model
        {ListBoxView = tview ; ListBoxModel = model}

    let listBoxAddValue (lbox: ListBox) value =
        listStoreAddValue lbox.ListBoxModel value




module WUtils =

    let private defaultWidth  = 683
    let private defaultHeight = 397

    module WForm =

        type  T = {  Window:   Gtk.Window
                   ; Fixed:    Gtk.Fixed
                   ; EventBox: Gtk.EventBox
                  }

        let getWindow   (form: T) = form.Window
        let getFixed    (form: T) = form.Fixed
        let getEventBox (form: T) = form.EventBox

        /// Make a .NET Windows Forms like window which the components can
        /// be positioned by coordinates
        ///
        let makeForm (title: string): T =
            let win = new Gtk.Window(title)
            let box = new Gtk.EventBox()
            let fix = new Gtk.Fixed()
            win.SetSizeRequest(defaultWidth, defaultHeight)
            box.Add(fix)
            win.Add(box)
            {Window = win ; Fixed = fix; EventBox = box}

        let show (form: T) =
            let win = form.Window
            win.ShowAll()
            form

        let put (form: T) (wdg, x, y): unit =
            form.Fixed.Put(wdg, x, y)

        let addList (form: T) (wdgList: (Gtk.Widget * int * int) list) =
            let fix = form.Fixed
            wdgList |> List.iter (fun (w, x, y) -> fix.Put(w, x, y))


