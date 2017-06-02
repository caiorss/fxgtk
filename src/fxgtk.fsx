

(* Compile with: 
  -------------------------------------------------------
                                                          
$ fsc fxgtk.fsx --target:library --out:fxgtk.dll \
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
#r "/usr/lib/mono/gtk-sharp-3.0/pango-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/cairo-sharp.dll"
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


    let getTextView (builder: T) widgetID =
        builder.GetObject(widgetID) :?> Gtk.TextView


    let getImage (builder: T) widgetID =
        builder.GetObject(widgetID) :?> Gtk.Image

    let getTreeView (builder: T) widgetID =
        builder.GetObject(widgetID) :?> Gtk.TreeView


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


type Dialog =

    /// Get user profile directory
    static member private getUserDir () =
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile)

    static member run(diag: Gtk.Dialog) =
        ignore <| diag.Run()
        diag.Destroy()

    /// Run dialog safely when the Gtk event loop is running
    /// in a background thread.
    ///
    static member runInter (dialog: Gtk.Dialog) =
        App.invoke(fun () -> ignore <| dialog.Run())

    static member fileFilter(name, extensions) =
        let filter = new Gtk.FileFilter()
        filter.Name <- name
        extensions |> List.iter filter.AddPattern
        filter

    static member fileChooser(label: string, ?path, ?filter) =
        let win  = new Gtk.Window("")
        let diag = new Gtk.FileChooserDialog(label
                                            ,win
                                            ,Gtk.FileChooserAction.Open
                                            ,"Cancel"
                                            ,Gtk.ResponseType.Cancel
                                            ,"Open"
                                            ,Gtk.ResponseType.Accept
                                            )
        diag.CanDefault    <- true

        let path = defaultArg path None
        match path with
        | Some "~" -> ignore <| diag.SetCurrentFolder (Dialog.getUserDir())
        | Some dir -> ignore <| diag.SetCurrentFolder dir
        | _        -> ()


        let filter = defaultArg filter None
        match filter with
        | Some (name, extList)
         ->  diag.AddFilter <| Dialog.fileFilter(name, extList)
        | None
         -> ()

        let out = if diag.Run () = int Gtk.ResponseType.Accept
                  then Some diag.Filename
                  else None
        diag.Destroy()
        win.Destroy()
        out 

    static member dirChooser (win: Gtk.Window, label: string, ?path) =
        let diag = new Gtk.FileChooserDialog(label
                                            ,win
                                            ,Gtk.FileChooserAction.Open
                                            ,"Cancel"
                                            ,Gtk.ResponseType.Cancel
                                            ,"Open"
                                            ,Gtk.ResponseType.Accept
                                            )

        let path = defaultArg path None
        match path with
        | Some "~" -> ignore <| diag.SetCurrentFolder (Dialog.getUserDir())
        | Some dir -> ignore <| diag.SetCurrentFolder dir
        | _        -> ()

        diag.Action <- Gtk.FileChooserAction.SelectFolder
        Option.iter (fun p -> ignore <| diag.SetCurrentFolder p) path
        diag.CanDefault <- true
        let out = if diag.Run () = int Gtk.ResponseType.Accept
                  then Some diag.Filename
                  else None
        diag.Destroy()
        out

        
    static member infoDialog (parent: Gtk.Window) (message: string)   =
        let dialog = new Gtk.MessageDialog(parent
                                           ,Gtk.DialogFlags.DestroyWithParent
                                           ,Gtk.MessageType.Info
                                           ,Gtk.ButtonsType.Close
                                           ,message
                                           )
        ignore <| dialog.Run ()
        dialog.Destroy ()


    static member warningDialog (parent: Gtk.Window) (message: string)   =
        let dialog = new Gtk.MessageDialog(parent
                                           ,Gtk.DialogFlags.DestroyWithParent
                                           ,Gtk.MessageType.Warning
                                           ,Gtk.ButtonsType.Close
                                           ,message
                                           )
        ignore <| dialog.Run ()
        dialog.Destroy ()



    static member errorDialog (parent: Gtk.Window) (message: string)   =
        let dialog = new Gtk.MessageDialog(parent
                                           ,Gtk.DialogFlags.DestroyWithParent
                                           ,Gtk.MessageType.Error
                                           ,Gtk.ButtonsType.Close
                                           ,message
                                           )
        ignore <| dialog.Run ()
        dialog.Destroy ()



    /// Question Dialog with Yes and No button
    static member questionDialog (parent: Gtk.Window) (message: string)  handler  =
        let dialog = new Gtk.MessageDialog(parent
                                           ,Gtk.DialogFlags.DestroyWithParent
                                           ,Gtk.MessageType.Question
                                           ,Gtk.ButtonsType.YesNo
                                           ,message
                                           )
        let resp =  dialog.Run () = int Gtk.ResponseType.Yes 
        dialog.Destroy ()
        resp                           



module TextView =

    type T = Gtk.TextView

    /// Create new text view widget
    let textview () =
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

    let setFromPixbuf (pbuf: Gdk.Pixbuf) (wdg: T)=
        wdg.Pixbuf <- pbuf

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


/// General widget functions.
///
module Wdg =

    
    type T = Gtk.Widget

    /// Upcast widget to Gtk.Widget class
    let toWdg obj = obj :> T

    /// Create drawing area 
    let makeDrawingArea () = new Gtk.DrawingArea()


    let makeIconFromFile (file: string) = new Gtk.StatusIcon(file)
    

    let setSize (wdg: T) (width: int) (height: int) =
        wdg.SetSizeRequest(width, height)

    /// Set background color     
    let modifyBg col (wdg: #T) =
        wdg.ModifyBg(Gtk.StateType.Normal, col)

    let getSize (wdg: T) =
        wdg.AllocatedWidth, wdg.AllocatedHeight

    let getHeight (wdg: T) =
        wdg.AllocatedHeight

    let getWidth (wdg: T) =
        wdg.AllocatedWidth

    let getSizeRequest (wdg: T) =
        let w = ref 0
        let h = ref 0
        wdg.GetSizeRequest (w, h)
        (!w, !h)

    let getWidthRequest: T -> int = getSizeRequest >> fst

    let getHeightRequest: T -> int = getSizeRequest >> snd

    /// Show widget
    let show (wdg: T) = wdg.Show()

    let showAll (wdg: T) = wdg.ShowAll()
        
    let add (parent: Gtk.Container) (child: T) =
        parent.Add(child)
        parent

/// Button Combinators
module Button =

    type T = Gtk.Button

    let button label = new Gtk.Button(Label = label)

    /// Add click event to button
    let onClick(btn: T) handler =
        btn.Clicked.Subscribe(fun arg -> handler arg)

    /// Programatically click the button
    let click(btn: T) =
        btn.Click()

/// Label Combinators
module Label =

    type T = Gtk.Label

    /// Create new label
    let label (title: string) = new Gtk.Label(title)

    let getText (lbl: T) =
        lbl.Text

    let setText (lbl: T) text =
        lbl.Text <- text

/// Entry Combinators
module Entry =

    type T = Gtk.Entry

    /// Create new entry
    let entry () = new Gtk.Entry()

    let getText (wdg: T): string =
        wdg.Text

    let setValue (wdg: T) value : unit =
        wdg.Text <- sprintf "%A" value

    let setText (wdg: T) (text: string) =
        wdg.Text <- text

    let getFloat (wdg: T): float option =
        try  Some (float wdg.Text)
        with _ -> None

    let getInt (wdg: T): int option =
        try Some (int wdg.Text)
        with _ -> None

    /// Event that happens when user releases some key in the entry box.
    /// The text in the entry is passed to the callback / envent handler.
    let onTextChange (wdg: T) : System.IObservable<string> =
        wdg.KeyReleaseEvent
        |> Observable.map (fun _ -> wdg.Text)

    /// Event that happens when user press returns in the entry box.
    /// The text in the entry is passed to the callback / envent handler.
    let onReturnKey (wdg: T) : System.IObservable<string> =
        wdg.KeyReleaseEvent
        |> Observable.map (fun _ -> wdg.Text)


/// Gtk Layout combinators
module Layout =
    type children = Gtk.Widget list

    type callback<'a>= 'a -> unit

    module Attribute =
        type WidgetAttribute =
            | Name    of string         // Widget Unique ID 
            | Text    of string         
            | Tooltip of string
            | BgColor of Gdk.Color      // Widget Background Color 
            | Icon    of Gdk.Pixbuf     // Window Icon 
            | Image   of Gdk.Pixbuf 
            | Size    of int * int      // Widget Size  
            | ShowAll                   // Show widget

            | Resizable of bool         // If true the window is resizable.
            
            | Expand  of bool
            | Hexpand of bool
            | Vexpand of bool

            //---- Events -------
            | OnClick  of callback<unit>
            | OnDelete of callback<unit>
            | OnMouseMove of callback<int * int>
            | ExitOnDelete

    module private Setters =
        open Attribute
        let setText (wdg: Gtk.Widget) text =
            match wdg with
            | :? Gtk.Window as w   -> w.Title <- text
            | :? Gtk.Entry as w    -> w.Text  <- text
            | :? Gtk.Label as w    -> w.Text  <- text
            | :? Gtk.Button as w   -> w.Label <- text
            | :? Gtk.TextView as w -> w.Buffer.Text <- text
            | _                    -> failwith "This property is not valid for this widget"

        let setName (wdg: Gtk.Widget) name =
            wdg.Name <- name

        let setPixbuf (wdg: Gtk.Widget) image =
            match wdg with
            | :? Gtk.Image  as w    -> w.Pixbuf <- image
            | _                     -> failwith "This property is not valid for this widget"

        let setOnClick (wdg: Gtk.Widget) callback =
            match wdg with
            | :? Gtk.Button as w   -> w.Clicked.Add(fun _ -> callback())
            | _                    -> failwith "This property is not valid for this widget"

        let setIcon (wdg: Gtk.Widget) pbuf =
            match wdg with
            | :? Gtk.Window as w -> w.Icon <- pbuf
            | _                  -> failwith "This property is not valid for this widget"

        let setResizable (wdg: Gtk.Widget) flag =
            match wdg with
            | :? Gtk.Window as w -> w.Resizable <- flag
            | _                  -> failwith "This property is not valid for this widget"

        let setAttrs (wdg: Gtk.Widget) (attrlist: WidgetAttribute list) =
            let aux attr =
                match attr with
                | Text s         -> setText wdg s
                | Name s         -> wdg.Name <- s
                | Tooltip s      -> wdg.TooltipText <- s
                | Size (w, h)    -> wdg.SetSizeRequest(w, h)
                | BgColor col    -> Wdg.modifyBg col wdg
                | ShowAll        -> wdg.ShowAll()
                | Icon pbuf      -> setIcon wdg pbuf
                | Resizable flag -> setResizable wdg flag 
                | Expand  flag   -> wdg.Expand  <- flag
                | Hexpand flag   -> wdg.Hexpand <- flag
                | Vexpand flag   -> wdg.Vexpand <- flag

                | ExitOnDelete   -> wdg.DeleteEvent.Add(fun _ -> Gtk.Application.Quit())
                | OnClick cb     -> setOnClick wdg cb
                | _              -> failwith "Error: Not implemented"
            List.iter aux attrlist

    open Attribute
    open Setters

    let addChildren (parent: Gtk.Container) (children: children) =
        List.iter parent.Add children


    let label attrs =
        let lbl = new Gtk.Label()
        setAttrs lbl attrs
        lbl

    let button attrs =
        let btn = new Gtk.Button(Label = "Button")
        setAttrs btn attrs
        btn

    let window attrs (children: Gtk.Widget list) =
        let win = new Gtk.Window("")
        addChildren win children
        setAttrs win attrs
        win

    let scrolled (child: Gtk.Widget) =
        let sc = new Gtk.ScrolledWindow()
        sc.Add(child)
        sc

    let entry attrs =
        let wdg = new Gtk.Entry()
        setAttrs wdg attrs
        wdg

    let textview attrs =
        let wdg = new Gtk.TextView()
        setAttrs wdg attrs
        wdg

    let image attrs =
        let wdg = new Gtk.Image()
        setAttrs wdg attrs
        wdg

    /// Pack widget with fill = false and expand = false
    let pack (wdg: Gtk.Widget) =
        false, false, wdg

    let pset (expand: bool) (fill: bool) (wdg: Gtk.Widget) =
        expand, fill, wdg

    /// pack widget with fill = true and expand = true
    let pfill (wdg: Gtk.Widget) =
        true, true, wdg

    let hbox chlist =
        let h = new Gtk.HBox()
        chlist  |> List.iter (fun (expand, fill, wdg) ->
                              h.PackStart(wdg, expand, fill, 0u))
        h

    let vbox chlist =
        let h = new Gtk.VBox()
        chlist  |> List.iter (fun (expand, fill, wdg) ->
                              h.PackStart(wdg, expand, fill, 0u))
        h

    let phbox chlist =
        pack <| hbox chlist

    let pvbox chlist =
        pack <| vbox chlist



module Window =
    type T = Gtk.Window

    let defaultWidth  = 683
    let defaultHeight = 397

    // Window position constants
    //
    let PosCenter         = Gtk.WindowPosition.Center
    let PosCenterAlways   = Gtk.WindowPosition.CenterAlways
    let PosCenterOnParent = Gtk.WindowPosition.CenterOnParent

    // ========= Events =================== //

    let onDelete (win: T) handler =
        win.DeleteEvent.Subscribe(fun _ -> handler ())

    let onDeleteExit (win: T) =
        win.DeleteEvent.Subscribe(fun _ -> App.exit())

    let onAdded (win: T) handler =
        win.Added.Subscribe(fun arg -> handler arg)

    // ======= Constructors Functions ====== //

    /// Create new window
    let window (title: string) =
        let win = new Gtk.Window(title)
        win.SetSizeRequest(defaultWidth, defaultHeight)
        ignore <| win.Added.Subscribe(fun _ -> win.ShowAll())
        win

    /// Create new window configured as main window.
    let mainWindow (title: string) =
        let win = new Gtk.Window(title)
        win.ShowAll()
        // Auto update window when a widget is added.
        ignore <| win.Added.Subscribe(fun _ -> win.ShowAll())
        win.SetSizeRequest(defaultWidth, defaultHeight)
        // Exit application when user close window
        ignore <| win.DeleteEvent.Subscribe(fun _ -> App.exit())
        win

    // ============== Getters ================== //

    let getSize (wdg: T) = wdg.GetSize()


    let getPosition (wdg: T) = wdg.GetPosition()

    /// Get mouse position from the upper left corner of window
    let getPointer (wdg: T) = wdg.GetPointer()


    // ============== Setters ================== //

    let setSize w h (wdg: T) =
        wdg.SetSizeRequest(w, h)
        wdg

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



/// Combinators for Cairo.Context
///
module Draw =
    open System

    type T = Cairo.Context

    /// Helper functions for coordinate transformations
    module DrawTransforms =

        let translate (dx: float, dy: float) (x, y) =
            dx + x, dy + y

        let scale (kx:float) (ky: float) (x, y) =
            kx * x, ky * y

        let rotate (angle: float) (x, y) =
            let c = cos angle
            let s = sin angle
            x * c - y * s, -(x * s + y * c)

        let rotateDeg (angle: float) =
            rotate <| angle * Math.PI / 180.0

        let flipX (x:float,  y:float) = (-x, y)
        let flipY (x: float, y:float) = (x, -y)

        let worldToViewport (xmin, xmax, ymin, ymax) (x, y) (width, height) =
            let xv = width / (xmax - xmin) * (x - xmin)
            let yv = height - height / (ymax - ymin) * (y - ymin)
            (xv, yv)

    /// Primitve Cairo drawing functions
    module DrawPrimitives =
        module DT = DrawTransforms

        //  Primitive stateful coordinate transformations
        //----------------------------------------

        let rotate angle (ctx: T) =
            ctx.Rotate(angle)

        let scale (kx: float) (ky: float) (ctx: T) =
            ctx.Scale(kx, ky)

        let translate (tx, ty) (ctx: T) =
            ctx.Translate(tx, ty)


        ///  Translate the coodinate system to a given point
        ///  and flip the Y-axis.
        ///
        ///   Gtk default coodinate system is at upper left corner of screen.
        ///   It sets the coordinate system origin to point P (px, py) and flips
        ///   they Y-axis.
        ///
        ///   (0, 0)
        ///         +-------------------------------+
        ///         |                      y        |
        ///         |  +----> x          |          |
        ///         |  |                 |          |
        ///         |  |                 +----- x   |
        ///         | \ / y            P (px, py)   |
        ///         |                               |
        ///         +-------------------------------+
        ///
        let setCoordinate (px, py) (ctx: T) =
            ctx.Translate(px, py)  // Translate to new origin
            ctx.Scale(1.0, -1.0)   // Flip Y axis
            ctx.MoveTo(0.0, 0.0)   // Move to new origin


        let setCoordinateBottom (canvas: Gtk.DrawingArea) (ctx: T) =
            let height = float canvas.AllocatedHeight
            ctx.Translate(0.0, height)
            ctx.Scale(1.0, -1.0)
            ctx.MoveTo(0.0, 0.0)

        let setCoordinateCenter (canvas: Gtk.DrawingArea) (ctx: T) =
            let height = float canvas.AllocatedHeight
            let width  = float canvas.AllocatedWidth
            ctx.Translate(width / 2.0, height / 2.0)
            ctx.Scale(1.0, -1.0)
            ctx.MoveTo(0.0, 0.0)

        let moveTo (x, y) (ctx: T) =
            ctx.MoveTo(x, y)


        let stroke (ctx: T) =
            ctx.Stroke()

        // Primitive operations
        //------------------------------------------

        /// No operation - do nothing
        let nop (ctx: T) = ()

        let lineTo (x, y) (ctx: T) =
            ctx.LineTo(x, y)

        let circle (x, y) radius (ctx: T) =
            ctx.Save()
            ctx.MoveTo(x + radius, y)
            ctx.Arc(x, y, radius, 0.0, 2.0 * Math.PI)
            ctx.Restore()

        let arc (x, y) radius angle1 angle2 (ctx: T) =
            ctx.Arc(x, y, radius, angle1, angle2)

        let showText (text: string) (ctx: T) =
            ctx.ShowText text

        let textAt (x, y) (text: string) (ctx: T) =
            ctx.Save()
            ctx.MoveTo(x, y)
            ctx.ShowText text
            ctx.Restore()

        let line (xa, ya) (xb, yb) (ctx: T) =
            moveTo (xa, ya) ctx
            lineTo (xb, yb) ctx

        let hline (xmin, xmax) y (ctx: T) =
            ctx.Save()
            moveTo (xmin, y) ctx 
            lineTo (xmax, y) ctx
            ctx.Restore()

        let hlineFull y (canvas: Gtk.DrawingArea) (ctx: T) =
            hline (0.0, float canvas.AllocatedWidth) y ctx

            
        //  Drawing setting functions
        //----------------------------------------


        let setLineWidth (w: float) (ctx: T) =
            ctx.LineWidth <- w

        let setFontSize (h: float) (ctx: T) =
            ctx.SetFontSize h

        let getLineWidth (ctx: T) =
            ctx.LineWidth

        let setSourceRgb (r, g, b) (ctx: T) =
            ctx.SetSourceRGB(r, g, b)


    module DrawCmdTypes =
        type Point = float * float
        type Radius = float
        type Angle = float

        type DrawState = {
             Scale:  (float * float) ref
             Origin: (float * float) ref
            }

        type DrawCmd =
            | DrawSetFontSzie of float
            | DrawSetLineWidth of float
            | DrawSetRgb of float * float * float

            | DrawSetScale of float * float
            | DrawSetOrigin of float * float
            | DrawSetOriginBottom
            | DrawSetUserCoord of float * float * float * float

            | DrawLineTo of Point
            | DrawMoveTo of Point
            | DrawStroke
            | DrawCircle of Point * Radius
            | DrawArc    of Point * Radius * Angle * Angle
            | DrawText   of Point * string

            | DrawLine   of Point * Point

            | DrawList of DrawCmd list
            | DrawForeach of (float -> DrawCmd) * float list
            | DrawForRange of (float -> DrawCmd) * (float * float * float)


    module DrawCmd =
        module DP = DrawPrimitives
        open DrawCmdTypes

        let private applyTransf (state: DrawState) (x, y) =
            let (sx, sy) = !state.Scale
            let (tx, ty) = !state.Origin
            sx * x + tx, sy * y + ty


        let rec private runCmdSingle (canvas: Gtk.DrawingArea, ctx: Cairo.Context)
                                     (state: DrawState)
                                     (cmd: DrawCmd)  =

            // printfn "command = %A" cmd
            // printfn "state = %A" state
            // printfn "---------------------------\n\n"

            match cmd with
            | DrawSetScale (sx, sy)  -> state.Scale  := (sx, sy)
            | DrawSetOrigin (tx, ty) -> state.Origin := (tx, ty)

            | DrawSetOriginBottom
              -> let h = float canvas.AllocatedHeight
                 state.Scale := (1.0, -1.0)
                 state.Origin := (0.0, h)

            | DrawSetUserCoord (xmin, xmax, ymin, ymax)
              -> let w = float canvas.AllocatedWidth
                 let h = float canvas.AllocatedHeight
                 let sx = w / (xmax - xmin)
                 let sy = h / (ymax - ymin)
                 let tx = - xmin * sx
                 let ty = h + ymin * sy
                 state.Scale   := sx, -sy
                 state.Origin  := tx, ty


            | DrawLineTo p
               -> let pn = applyTransf state p
                  DP.lineTo pn ctx

            | DrawMoveTo p
                -> let pn = applyTransf state p
                   DP.moveTo pn ctx

            | DrawStroke
                -> DP.stroke ctx

            | DrawCircle (p, r)
                -> let pn = applyTransf state p
                   printfn "pn = %A" pn
                   DP.circle pn r ctx

            | DrawArc (p, r, a1, a2)
                -> let pn = applyTransf state p
                   DP.arc pn r a1 a2 ctx

            | DrawSetRgb (r, g, b)   -> DP.setSourceRgb (r, g, b) ctx

            | DrawText (p, text)
                -> let pn = applyTransf state p
                   DP.textAt pn text ctx

            | DrawLine (p1, p2)
                -> let pn1 = applyTransf state p1
                   let pn2 = applyTransf state p2
                   DP.line pn1 pn2 ctx

            | DrawSetFontSzie s      -> DP.setFontSize s ctx

            | DrawForeach (fn, paramlist)
               -> paramlist |> List.iter (fun p -> runCmdSingle (canvas, ctx) state (fn p))

            | DrawForRange (fn, (xmin, xmax, step))
               -> for x in [xmin .. step .. xmax] do
                      runCmdSingle (canvas, ctx) state (fn x)


            | DrawList cmdList
              -> cmdList |> List.iter (runCmdSingle (canvas, ctx) state)


            | _                      -> failwith "Error: Not implemented"

        /// Draw Command interpreter
        let runCmd (cmd: DrawCmd) (canvas: Gtk.DrawingArea, ctx: Cairo.Context) =
            let state = { Scale  = ref (1.0, 1.0)
                        ; Origin = ref (0.0, 0.0)
                        }
            DP.moveTo (applyTransf state (0.0, 0.0)) ctx
            runCmdSingle (canvas, ctx) state cmd



        let lineTo p = DrawLineTo p

        let moveTo p = DrawMoveTo p

        let stroke = DrawStroke

        let circle p r = DrawCircle (p, r)

        let line p1 p2 = DrawLine (p1, p2)

        let text p msg = DrawText (p, msg)

        let setColorRgb r g b = DrawSetRgb (r, g, b)

        let setFontSize s = DrawSetFontSzie s

        let setOrigin p = DrawSetOrigin p
        let setScale s = DrawSetScale s
        let setOriginBottom = DrawSetOriginBottom

        let setUserCoord xmin xmax ymin ymax =
            DrawSetUserCoord (xmin, xmax, ymin, ymax)

        let forEach fn plist = DrawForeach (fn, plist)

        let forRange fn xmin xmax step = DrawForRange (fn, (xmin, xmax, step))

        let cmdList xs = DrawList xs

    // /// Draw Command types
    // module DrawCmdTypes =
    //     type DrawCmd



/// Wrapper around Gtk.DrawingArea widget
///
module Canvas =
    module DP = Draw.DrawPrimitives
    type T = Gtk.DrawingArea
    type Ctx = Cairo.Context


    let getHeight (wdg: T) =
        wdg.Allocation.Height

    let getWidth (wdg: T) =
        wdg.Allocation.Width

    /// Create a drawing area object / canvas
    let canvas(): T =
        let draw = new Gtk.DrawingArea()
        draw.AddEvents <| int ( Gdk.EventMask.ButtonPressMask
                        ||| Gdk.EventMask.ButtonReleaseMask
                        ||| Gdk.EventMask.KeyPressMask
                        ||| Gdk.EventMask.PointerMotionMask
                        )
        draw

    /// Crate a canvas (aka drawing area) with an update handler
    //  function
    //
    let canvasWithHandler () =
        let draw = canvas()
        let updateFn = ref (fun (cr: Ctx) -> ())
        ignore <| draw.Drawn.Subscribe(fun arg ->
                                       let cr = arg.Cr
                                       // Set the coordinate system at bottom left with Y-axis upward
                                       // from bottom to top and X axis from left to right.
                                       // DP.setCoordinateBottom draw cr
                                       cr.MoveTo(0.0, 0.0)
                                       !updateFn cr
                                       cr.Stroke()
                                       )

        let updateDraw handler =
            updateFn := handler
            draw.QueueDraw()

        draw, updateDraw


    /// Update Drawing Area after the drawing was changed.
    let update (wdg: T) =
        App.invoke wdg.QueueDraw

    /// Event that happens when the user moves the mouse (pointer).
    let onMouseMove (wdg: T) =
        wdg.MotionNotifyEvent
        |> Observable.map (fun arg -> arg.Event.X, arg.Event.Y)

    /// Event that happens when the drawing area is update (repainted).
    let onDraw (wdg: T) =
        wdg.Drawn
        |> Observable.map (fun arg -> arg.Cr)

    let onButtonRelease (wdg: T) =
        wdg.ButtonReleaseEvent

    let onButtonPress (wdg: T) =
        wdg.ButtonPressEvent

        
module ListStore =
    type T = Gtk.ListStore

    /// Create new ListStore object
    ///
    ///  Example:
    ///
    ///  > let l1 = listStore [| typeof<string>; typeof<float> |]
    ///    val l1 : Gtk.ListStore
    ///
    let listStore (types: System.Type list) =
        new Gtk.ListStore(Array.ofList types)

    /// Add a row of values to ListStore
    let addRow (lstore: Gtk.ListStore) row =
        lstore.AppendValues(row)

    let getIterIndex (lstore: T) path =
        let p = new Gtk.TreePath([| path |])
        let titer = ref Unchecked.defaultof<Gtk.TreeIter>
        let resp = lstore.GetIter(titer, p)
        match resp with
        | false -> None
        | true  -> Some !titer 
        
        
    let getIterFromString (lstore: T) path =
        let titer = ref Unchecked.defaultof<Gtk.TreeIter>
        let resp = lstore.GetIterFromString(titer, path)
        match resp with
        | false -> None
        | true  -> Some !titer 

    let getValue (lstore: T) row col =
        let n = lstore.NColumns
        let iter = getIterIndex lstore row 
        match col, iter with
        | _  when col >= n || col < 0  -> None  
        | _, None                      -> None
        | k, Some i                    -> Some <| lstore.GetValue(i, col)

    let setValue (lstore: T) row col (value: obj) =
        let iter = getIterIndex lstore row
        let n = lstore.NColumns
        match col, iter with
        | _  when col >= n || col < 0  -> false
        | _, None                      -> false
        | k, Some i                    -> lstore.SetValue(i, col, value)
                                          true
                                          
        
    /// Get number of columns
    let getNcolumns (lstore: T) =
        lstore.NColumns

    /// Get type of nth column
    let getColumnType (lstore: T) col =
        lstore.GetColumnType col

    /// Get all rows
    let getRows (lstore: T) =
        let enum = lstore.GetEnumerator()
        let rec aux acc =
            match enum.MoveNext () with
            | false -> List.rev acc
            | true  -> let row = enum.Current :?> obj []
                       aux <| (row::acc)
        aux []


    let getRow (lstore: T) row =
        let n = lstore.NColumns
        [for col = 0 to n - 1 do yield getValue lstore row col ]

    let setRow (lstore: T) row rowValues =
        let titer = getIterIndex lstore row
        match titer with
        | None    -> false
        | Some i  -> lstore.SetValues(i, rowValues); true 

    /// Get the nth column
    let getColumn<'a> (lstore: T) col  =
        let enum = lstore.GetEnumerator()
        let rec aux acc =
            match enum.MoveNext () with
            | false -> List.rev acc
            | true  -> let elem = enum.Current
                                 :?> obj []
                                 |>  Array.item col
                                 :?> 'a
                       aux <| (elem::acc)
        aux []

    let setColumn (lstore: T) col values =
        values |> List.iteri (fun row item -> ignore <| setValue lstore row col item)
    
    /// Clear ListStore, remove all columns.
    let clear (lstore: T) =
        lstore.Clear()




module TreeView =

    /// (ColumnLabel, ColumnType)
    ///
    // type TColumn = string *  System.Type

    module Types =
        type ColRender =
            | ColText
            | ColImage
            | ColCombo
            | ColToggle

        type ColDesc = {
             ColLabel:  string
           ; ColType:   System.Type
           ; ColRender: ColRender
            }

    //========== Constructor Functions ================= //

    /// Create new TreeView object
    let treeView () = new Gtk.TreeView ()

    /// Create new TreeView Column object
    let treeViewColumn title = new Gtk.TreeViewColumn(Title=title)

    /// Create cell renderer text object
    let cellRenderText () =
        new Gtk.CellRendererText()

    let cellRenderPixbuf () =
        new Gtk.CellRendererPixbuf()

    let cellRenderToggle () =
        new Gtk.CellRendererToggle()

    let cellRendererCombo () =
        new Gtk.CellRendererCombo()

    /// Get TreeView model as ListStore.
    let getModel (tview: Gtk.TreeView) = tview.Model :?> Gtk.ListStore


    let addRow (tview: Gtk.TreeView) row =
        let m = tview.Model :?> Gtk.ListStore
        ignore <| m.AppendValues(row)

    let addRowList (tview: Gtk.TreeView) rowList =
        rowList |> List.iter (addRow tview)



    /// TreeView with all column as text
    ///
    let treeViewText colList =
        let tview = new Gtk.TreeView()
        let model = ListStore.listStore <| List.map snd colList
        tview.Model <- model
        colList |> List.iteri (fun idx (label, _) ->
                               let col  = new Gtk.TreeViewColumn(Title = label)
                               let cell = new Gtk.CellRendererText()
                               col.PackStart(cell, true)
                               col.AddAttribute(cell, "text", idx)
                               ignore <| tview.AppendColumn(col)

                              )
        tview


    let treeViewDesc (colList: Types.ColDesc list) =
        let tview = new Gtk.TreeView()
        let model = ListStore.listStore <| List.map (fun (c: Types.ColDesc) -> c.ColType) colList
        tview.Model <- model
        colList |> List.iteri (fun idx cdesc ->
                               let col  = new Gtk.TreeViewColumn(Title = cdesc.ColLabel)
                               match cdesc.ColRender with
                               | Types.ColText  ->  let cell = new Gtk.CellRendererText()
                                                    col.PackStart(cell, true)
                                                    col.AddAttribute(cell, "text", idx)

                               | Types.ColImage ->  let cell = new Gtk.CellRendererPixbuf()
                                                    col.PackStart(cell, true)
                                                    col.AddAttribute(cell, "text", idx)

                               | _              ->  failwith "Error: Not implemented"
                               ignore <| tview.AppendColumn(col)
                              )
        tview





    //========== Events ================= //


    let onChanged (tview: Gtk.TreeView) =
        tview.Selection.Changed


    //========== Getters  ================= //

    /// Get index of selected row
    let getSelectedRow (tview: Gtk.TreeView) =
        let (tpath, _) = tview.GetCursor()
        tpath.Indices.[0]

    /// Get value of selected column
    /// Note: The returned value must be cast to the column type.
    ///
    let getSelectedCol (tview: Gtk.TreeView) column =
        let titer = ref Unchecked.defaultof<Gtk.TreeIter>
        ignore <| tview.Selection.GetSelected titer
        tview.Model.GetValue(!titer, column)


    let getSelected (tview: Gtk.TreeView) =
        let model = tview.Model
        [| for i = 1 to model.NColumns do yield getSelectedCol tview i |]



module IconView =

    type T = Gtk.IconView

    /// Create new IconView object
    ///
    /// Parameters
    ///
    /// - model       - Holds the PixbufData and icons (ListStore)
    /// - pixbufCol   - Pixbuf column number in the model
    /// - markupCol   - Label column number in the model
    ///
    let iconView (model: Gtk.ListStore) pixbufCol markupCol =
        let icv = new Gtk.IconView()
        icv.Model        <- model
        icv.PixbufColumn <- pixbufCol
        icv.MarkupColumn <- markupCol
        icv

    let setSelectionMode (icv: T) mode =
        icv.SelectionMode <- mode

    /// Get IconView model or ListStore
    let getModel (icv: T) = icv.Model


    /// Get nth column item form IconView model (ListStore)
    /// Note: The return value must be type cast to the appropriate type.
    ///
    let getSelecetdItem (icv: Gtk.IconView) (col: int)  =
        let tpath = icv.SelectedItems.[0]
        let iter  = ref Unchecked.defaultof<Gtk.TreeIter>
        ignore <| icv.Model.GetIter(iter, tpath)
        icv.Model.GetValue(!iter, col)


    /// Event triggered when the user clicks at some icon or image
    let onActivated (icv: T) handler =
        icv.ItemActivated.Subscribe(fun arg -> handler arg)

    /// Event triggered when the cursor moves selecting a new item
    let onMoveCursor (icv: T) handler =
        icv.MoveCursor.Subscribe(fun arg -> handler arg)



/// Model for MVC (Model - View - Controller)
/// with observer pattern
///
module FModel = 

    type Model<'a> = {
        State:     'a ref 
       ;OnChanged:  Event<'a>
        }
    
    /// Create a new model 
    let make (init: 'a) =
        { State     = ref init
        ; OnChanged = new Event<'a>()
        }

    /// Update Model state and notify all observers.
    let update (m: Model<'a>) (value: 'a) =
        m.State := value
        m.OnChanged.Trigger value

    /// Get model state     
    let get (m: Model<'a>) =
        !m.State

    /// Notify all observers with current model state    
    let trigger (m: Model<'a>) =
        m.OnChanged.Trigger !m.State

    /// Apply a function to model state and notify all observers
    let apply (m: Model<'a>) (fn: 'a -> 'a) =
        m.State := fn(!m.State)
        m.OnChanged.Trigger !m.State 
        
    /// Subscribe to model updates 
    let subscribe (m: Model<'a>) fn =
        m.OnChanged.Publish.Add(fn)

    /// Add a console view to model 
    let addLogger (m: Model<'a>) =
        m.OnChanged.Publish.Add(fun s -> printfn "Model changed to = %A" s)
