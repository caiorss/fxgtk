#if INTERACTIVE
#r "/usr/lib/mono/gtk-sharp-3.0/atk-sharp.dll"   
#r "/usr/lib/mono/gtk-sharp-3.0/gio-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/glib-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gtk-sharp.dll"
#r "/usr/lib/mono/webkit-sharp/webkit-sharp.dll"
#endif 

Gtk.Application.Init()

let window = new Gtk.Window("Webkit Gtk test")
window.SetDefaultSize(800, 600)
window.DeleteEvent.Subscribe(fun _ -> Gtk.Application.Quit())

let webview = new WebKit.WebView()


let scrolledWindow = new Gtk.ScrolledWindow()
scrolledWindow.Add(webview)

// window.Add(scrolledWindow)


window.ShowAll()
// webview.Open("http://www.httbin.org/get")

Gtk.Application.Run() 
