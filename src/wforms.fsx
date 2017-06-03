#if INTERACTIVE
#r "/usr/lib/mono/gtk-sharp-3.0/atk-sharp.dll"   
#r "/usr/lib/mono/gtk-sharp-3.0/gio-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/glib-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gtk-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gdk-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/pango-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/cairo-sharp.dll"
#load "fxgtk.fsx"
#endif 

namespace Fxgtk.Forms

/// Window Utilities with useful windows containing widgets
///
/// It contains modules that simplifies simple tasks like display image,
//  display text and so on.
///

open Fxgtk


/// Window with image widget ready to use. It is useful for
/// displaying images and test Gtk Image widget.
module ImageView =

    let private defaultWidth  = 683
    let private defaultHeight = 397

    type ImageView =
        private { window: Gtk.Window 
                ; image:  Gtk.Image 
                }
                with
                    static member Create(win, image) = { window = win ; image = image }
                    member this.GetWindow () = this.window 
                    member this.GetImage  () = this.image


    /// Make a window containing only an image widget
    /// It is useful to display images.
    ///    
    let makeImageView (title: string) =
        let win = new Gtk.Window(title)
        let img = new Gtk.Image()

        win.SetSizeRequest(defaultWidth, defaultHeight)
        win.Add(img)
        ImageView.Create (win, img)

    let show (w: ImageView) =
        w.GetWindow().ShowAll()

    let destroy (w: ImageView) =
        w.GetWindow().Destroy()

    let setImageFromPbuf (w: ImageView) pbuf =
        let (_, h) = w.GetWindow().GetSize()
        let img = w.GetImage()            
        img.Pixbuf <- pbuf
        Image.scaleToHeight h img

    let setImageFromFile (w: ImageView) file =
        let (_, h) = w.GetWindow().GetSize()
        let img = w.GetImage()
        img.File <- file
        Image.scaleToHeight h img

/// Window with TextView widget. Useful to display multi line text
/// and test GTK TextView widget.
///
module WText =
    let private defaultWidth  = 683
    let private defaultHeight = 397


    type WText =
        private { window:    Gtk.Window 
                ; textview:  Gtk.TextView
                }
                with
                    static member Create(win, tview) = { window = win ; textview = tview }
                    member this.GetWindow () = this.window 
                    member this.GetTextView () = this.textview


    /// Make a window containing only a text view
    /// It is useful to display multi line texts.
    let makeWText (title: string) =
        let win = new Gtk.Window(title)
        let txt = new Gtk.TextView()
        let scr = new Gtk.ScrolledWindow()
        // Set initial window size 
        win.SetSizeRequest(defaultWidth, defaultHeight)
        scr.Add(txt)
        win.Add(scr)
        WText.Create(win, txt)

    let show (w: WText) =
        w.GetWindow().ShowAll()

    let hide (w: WText) =
        w.GetWindow().Hide()

    let destroy (w: WText) =
        w.GetWindow().Destroy()

    let getText (w: WText) =
        w.GetTextView().Buffer.Text

    let setText (w: WText) text =
        w.GetTextView().Buffer.Text <- text

    let addText (w: WText) text =
        setText w (getText w + text)                        


module WForm =
    let private defaultWidth  = 683
    let private defaultHeight = 397

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

    let put wdg (x, y) (form: T)  =
        form.Fixed.Put(wdg, x, y)
        form

    let putSize (wdg: Gtk.Widget) (x, y) (w, h) (form: T) =
        form.Fixed.Put(wdg, x, y)
        Wdg.setSize wdg w h
        form

    let add wdg (form: T) =
        put wdg (0, 0) form

    let addList (form: T) (wdgList: (Gtk.Widget * int * int) list) =
        let fix = form.Fixed
        wdgList |> List.iter (fun (w, x, y) -> fix.Put(w, x, y))

    /// Set backgrund color
    let setBgColor color (form: T) =
        Wdg.modifyBg color form.Window
        form

    let onMouseMove (form: T) =
        form.EventBox.MotionNotifyEvent |> Observable.map (fun arg -> arg.Event.X, arg.Event.Y)

    /// Add event to exit application if form is destroyed (user click on
    /// right corner exit button )
    ///
    let onDeleteExit (form: T): T =
        ignore <| form.Window.DeleteEvent.Subscribe(fun _ -> Gtk.Application.Quit())
        form

    // let onMouseMove (form: T) =
    //     form.EventBox.Poin

