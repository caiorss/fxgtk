#if INTERACTIVE
#r "/usr/lib/mono/gtk-sharp-3.0/gio-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/glib-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gtk-sharp.dll"
#endif 

let gui = new Gtk.Builder()

Gtk.Application.Init() 

gui.AddFromFile("/home/arch/gtk/entry1.glade")


let win: Gtk.Window = gui.GetObject("mainWindow") :?> Gtk.Window

let btn: Gtk.Button = gui.GetObject("buttonClick") :?> Gtk.Button 

win.ShowAll()

win.DeleteEvent.Subscribe(fun _ -> Gtk.Application.Quit ())


btn.Clicked.Subscribe(fun _ -> printfn "I was clicked !")

Gtk.Application.Run()
