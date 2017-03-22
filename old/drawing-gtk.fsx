#if INTERACTIVE
#r "/usr/lib/mono/gtk-sharp-3.0/atk-sharp.dll"   
#r "/usr/lib/mono/gtk-sharp-3.0/gio-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/glib-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gtk-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/cairo-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gdk-sharp.dll"
#endif 

open System
open System.Reflection
open System.Runtime.InteropServices

Gtk.Application.Init()

let window = new Gtk.Window("Gtk First Drawing")
window.SetDefaultSize(800, 600)

// let r = ref <| Gdk.Color()
// let col  = Gdk.Color.Parse("#010", r)

let col = new Gdk.Color(255uy, 0uy, 0uy)

printfn "x = %A " window.Window
window.ModifyBg(Gtk.StateType.Normal, col)

let drawing = new Gtk.DrawingArea()
drawing.SetSizeRequest(400, 600)
window.Add(drawing)


drawing.Drawn.Subscribe(fun arg ->
                        let cr = arg.Cr
                        // cr.Color <- new Cairo.Color(0.0, 0.0, 0.0)
                        cr.MoveTo(new Cairo.PointD(10.0, 10.0))
                        cr.LineTo(200.0, 300.0)
                        cr.Stroke()
                        )




// let cr = Gdk.CairoHelper.Create(drawing.GdkWindow)



// let onDraw = new System.EventHandler (fun sender args ->
                                      
//                                        )
             

window.DeleteEvent.Subscribe(fun _ -> Gtk.Application.Quit())


window.ShowAll()
Gtk.Application.Run() 
