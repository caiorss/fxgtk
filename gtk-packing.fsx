#if INTERACTIVE
#r "/usr/lib/mono/gtk-sharp-3.0/atk-sharp.dll"   
#r "/usr/lib/mono/gtk-sharp-3.0/gio-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/glib-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gtk-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/cairo-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gdk-sharp.dll"
#endif 


Gtk.Application.Init()

let window = new Gtk.Window("GTK WIndow Test")



let makeButton label =
    let btn = new Gtk.Button()
    btn.Label <- label 
    btn 

let bt1 = makeButton "Run"
let bt2 = makeButton "Start"
let bt3 = makeButton "Stop"


type LayoutConf = {
       Homon:   bool
     ; Space:   int 
     ; Fill:    bool
     ; Expand:  bool 
     ; Padding: int 
    }



let hbox (conf: LayoutConf) (wdglist: Gtk.Widget list)  =
    let hbox = new Gtk.HBox(conf.Homon, conf.Space)
    wdglist |> List.iter (fun w -> hbox.PackStart(w,
                                                  conf.Expand,
                                                  conf.Fill,
                                                  System.Convert.ToUInt32 conf.Padding))
    hbox 


let vbox (conf: LayoutConf) (wdglist: Gtk.Widget list)  =
    let vbox = new Gtk.VBox(conf.Homon, conf.Space)
    wdglist |> List.iter (fun w -> vbox.PackStart(w,
                                                  conf.Expand,
                                                  conf.Fill,
                                                  System.Convert.ToUInt32 conf.Padding))
    vbox 


// let hbox = new Gtk.HBox(true, 0)
// hbox.PackStart(bt1, true, true, 0u)
// hbox.PackStart(bt2, true, true, 0u)
// hbox.PackStart(bt3, true, true, 0u)

let entry = new Gtk.Entry()

let conf = {Homon = true; Space = 1; Fill = true; Expand = true; Padding = 0}

window.Add <| vbox conf [  hbox conf [bt1; bt2; bt3]
                         ; entry
                        ]  

window.ShowAll()

Gtk.Application.Run()
