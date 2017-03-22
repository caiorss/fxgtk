
#r "/usr/lib/mono/gtk-sharp-3.0/gio-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/glib-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gtk-sharp.dll"

Gtk.Application.Init()

let win = new Gtk.Window("Sample GTK# App")

win.Resize(300, 300)

let label = new Gtk.Label(Text = "Hello world")

win.Add(label)

win.ShowAll()

Gtk.Application.Run()


