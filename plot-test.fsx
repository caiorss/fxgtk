#r "/usr/lib/mono/gtk-sharp-3.0/atk-sharp.dll"   
#r "/usr/lib/mono/gtk-sharp-3.0/gio-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/glib-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/gtk-sharp.dll"
#r "/usr/lib/mono/gtk-sharp-3.0/cairo-sharp.dll"
   
#load "fxgtk.fsx"

open Fxgtk

App.init ()

let win = Wdg.makeWindow("Plot testing")

let drg = Wdg.makeDrawingArea()

module Reader =
    type Reader<'a,'b> = Reader of ('a -> 'b)

    let run x (Reader fn) = fn x

    let runList (r: Reader<'a, 'b>) (xs: 'a list): 'b list =
        List.map (fun x -> run x r) xs 
    
    let ask = Reader(fun x -> x)

    // let local f (Reader g) = Reader(fun x -> run g (f x))

    let map f (Reader fn) = Reader(fn >> f)   

    let unit x = Reader(fun a -> x)    

    let bind (f: 'b -> Reader<'a, 'c>) (Reader fn) =
        Reader (fun x -> run x (f (fn x)) )

    let (>>=) ma fn = bind fn ma

    let seqIter (fn: 'a -> Reader<'b, unit>) (xs: 'a seq) =
        Reader <| fun b -> Seq.iter (fun x -> run b (fn x)) xs  
        
    type ReaderBuilder () =
        member this.Bind(x, f) = bind x f
        member this.Return(x)  = unit x
        member this.Delay(f)   = f()
        member this.Zero()     = ask 

    let reader = ReaderBuilder()    
        
        

type PlotParam = {
      Center: float * float
    ; Scale:  float * float
    ; Ctx:    Cairo.Context
    }

type Point = float * float 

///
/// (dx, dy) -- Offset from the top left of the screen to new origin
///  
/// (sx, sy) -- Scale X and Y
///
///
let point2screen ((dx, dy): float * float) (sx, sy) (x, y)  =
    (x * sx + dx, - y * sy + dy)


let point2screen2 (x, y): Reader.Reader<PlotParam, Point> =
    Reader.Reader (fun (conf: PlotParam) ->
                   let (dx, dy) = conf.Center
                   let (sx, sy) = conf.Scale
                   (x * sx + dx, - y * sy + dy)
                   )

let lineTo2 (x, y): Reader.Reader<PlotParam, unit> =
    let (>>=) = Reader.(>>=)    
    Reader.ask               >>= fun param    ->
    (x, y) |> point2screen2  >>= fun (xn, yn) ->
    Reader.unit (param.Ctx.LineTo(xn, yn))
    


let moveTo (x, y) =
    Reader.Reader <| fun (conf: PlotParam) -> let (xc, yc) = conf.Center
                                              conf.Ctx.MoveTo(x + xc, yc - y)
                                 

let lineTo (dx, dy) (sx, sy) (x, y) (ctx: Cairo.Context) =
    let (xn, yn) = point2screen (dx, dy) (sx, sy) (x, y)
    ctx.LineTo(xn, yn)
           

let plotRange (xmin: float) xmax step fn =
    let generator =  fun s -> if s < xmax
                              then (let p = (s, fn s)
                                    Some (p, s + step)
                                    )
                              else None
                              
    Seq.unfold generator xmin |> Reader.seqIter lineTo2




let adjustScale (xmin, ymin) (xmax, ymax) (width: float, height) =
    let sx = width  / (xmax - xmin)
    let sy = height / (ymax - ymin)
    printfn "sx = %f sy = %f" sx sy 
    sx, sy 

let vline (ctx: Cairo.Context) (x: float) (width: float, height: float) =
    ctx.MoveTo(x, 0.0)
    ctx.LineTo(x, height)

let hline (ctx: Cairo.Context) (y: float) (width: float, height: float) =
    ctx.MoveTo(0.0, y)
    ctx.LineTo(width, y)
         


drg.Drawn.Subscribe(fun arg ->
                    let ctx = arg.Cr 

                    let (w, h) = Window.getSize win
                    let xc = float w / 2.0
                    let yc = float h / 2.0 

                    let conf = {  Center = xc, yc
                                ; Scale  = adjustScale (0.0, 0.0) (20.0, 200.0) (float w, float h)
                                ; Ctx    = ctx 
                                }


                    printfn "w = %d h =%d" w h 
                    
                    // moveTo (0.0, 0.0) |> Reader.run conf 
                    vline ctx xc (float w, float h)
                    ctx.LineWidth <- 0.5
                    ctx.Stroke()

                    hline ctx yc (float w, float h)
                    ctx.LineWidth <- 0.5
                    ctx.Stroke()
                    
                    
                    plotRange -10.0 10.0 0.1 (fun x -> x * x * x) |> Reader.run conf  
                    
                    // lineTo (xc, yc) (1.0, 1.0) (100.0, 200.0) ctx
                    // lineTo (xc, yc) (1.0, 1.0) (200.0, 300.0) ctx 

                    ctx.LineWidth <- 1.0 
                    // ctx.LineTo(xc + 100.23, yc + 200.34)
                    // ctx.LineTo(xc + 200.0, yc + 300.0)

                    ctx.Stroke()                    

                    )


Wdg.add win drg 
Window.setDefaultSize 600 600 win 

Window.showAll win 
App.run()
