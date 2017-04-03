#if INTERACTIVE    
#r "/usr/lib/mono/gtk-sharp-3.0/atk-sharp.dll"   
#r "/usr/lib/mono/gtk-sharp-3.0/gio-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/glib-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gtk-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gdk-sharp.dll"
#endif

/// Always must be called before create any widget 
Gtk.Application.Init () 

//--------- Create Widgets -------------- //
//
//
let window = new Gtk.Window "Tree view example"
window.SetSizeRequest(500, 200)

let tview = new Gtk.TreeView()

let columnProduct  = new Gtk.TreeViewColumn(Title = "Product")
let columnQuantity = new Gtk.TreeViewColumn(Title = "Quantity")
let columnPrice    = new Gtk.TreeViewColumn(Title = "Price")
let columnTotal    = new Gtk.TreeViewColumn(Title = "Total")

let cellProduct  = new Gtk.CellRendererText()
let cellQuantity = new Gtk.CellRendererText()
let cellPrice    = new Gtk.CellRendererText()
let cellTotal    = new Gtk.CellRendererText()


// --------- Pack Widgets ----------------- // 
//
//   
columnProduct.PackStart(cellProduct, true)
columnQuantity.PackStart(cellQuantity, true)
columnPrice.PackStart(cellPrice, true)
columnTotal.PackStart(cellTotal, true)

tview.AppendColumn(columnProduct)
tview.AppendColumn(columnQuantity)
tview.AppendColumn(columnPrice)
tview.AppendColumn(columnTotal)

columnProduct.AddAttribute(cellProduct,   "text", 0)
columnQuantity.AddAttribute(cellQuantity, "text", 1)
columnPrice.AddAttribute(cellPrice,       "text", 2)


let storeModel = new Gtk.ListStore(typeof<string>, typeof<int>, typeof<float>)

storeModel.AppendValues("Smartphone Hiphi", 10, 339.0)
storeModel.AppendValues("Smart TV",         20, 500.0)
storeModel.AppendValues("Laptop Xin",       30, 500.0)



// storeModel.AppendValues("20", "10.20")
// storeModel.AppendValues("10", "4.34")
// storeModel.AppendValues("25", "3.43")
// storeModel.AppendValues("16", "6.23")

tview.Model <- storeModel

window.Add(tview)

// Show main window 
window.ShowAll()


window.DeleteEvent.Subscribe(fun _ -> Gtk.Application.Quit())




#if REPL
         // Run Gtk event loop in background thread when running in F# Repl 
         //
let () = let thread = System.Threading.Thread Gtk.Application.Run
         thread.Start()
#else
         // Run Gtk event loop when compiled or running in main thread.
         //    
let () = Gtk.Application.Run() // Start GTK event loop and listen to events
#endif 

