// #!/usr/bin/env fsharpi

open System


module Term =
    open System
    
    let blue     = ConsoleColor.Blue
    let yellow   = ConsoleColor.Yellow
    let red      = ConsoleColor.Red
    let darkBlue = ConsoleColor.DarkBlue

    let withColor color action =
        System.Console.ForegroundColor <- color
        action()
        System.Console.ResetColor()

    let printWithColor color (msg: string) =
        System.Console.ForegroundColor <- color
        System.Console.WriteLine(msg)
        System.Console.ResetColor()


module SysUtils =
    open System
               
    let runShellCmd (program: String) (args: string list) =
        let argString = String.Join(" ", args)
        let p = System.Diagnostics.Process.Start(program, argString)
        printfn "Running %s %s" program argString
        p.WaitForExit()
        p.ExitCode

    let joinPath path1 path2 =
        System.IO.Path.Combine(path1, path2)

    let baseName file =
        System.IO.Path.GetFileName file 

    let replaceExt ext file =
        System.IO.Path.ChangeExtension(file, ext)

    let getFilesExt path pattern =
        System.IO.Directory.GetFiles(path, pattern)
        |> Seq.ofArray

    let getCommandLineArgs () =
        let args = Environment.GetCommandLineArgs()
        let idx = Array.tryFindIndex (fun a -> a = "--") args
        match idx with
        | None   -> []
        | Some i -> args.[(i+1)..] |> List.ofArray


type FsharpCompiler =
    static member CompileLibrary(sources:      string list 
                                ,dependencies: string list 
                                ,output
                                ,doc
                                 )  =

        let args = [ String.Join(" ", sources)
                   ; "--target:library"
                   ; "--out:" + output
                   ; "--doc:" + doc
                   ; "--debug+"
                   ; "--nologo"
                   ; String.Join(" ", dependencies |> List.map (fun s -> "-r:" + s))
                   ]

        printfn "%A" args 
        SysUtils.runShellCmd "fsharpc" args 


    static member CompileExecutableWEXE( sources:      string list 
                                        ,dependencies: string list 
                                        ,output                             
                                        )  =

        let args = [ String.Join(" ", sources)
                   ; "--target:winexe"
                   ; "--out:" + output
                   ; "--debug+"
                   ; "--nologo"
                   ; String.Join(" ", dependencies |> List.map (fun s -> "-r:" + s))
                   ]

        // printfn "%A" args 
        SysUtils.runShellCmd "fsharpc" args 



// ------------------- U S E R   O P T I O N S ---------------- //

let gtkHome = "/usr/lib/mono/gtk-sharp-3.0/"

let gtkDependencies =
    let gtkDlls = [
        "atk-sharp.dll"
        ;"gio-sharp.dll"
        ;"glib-sharp.dll"
        ;"gtk-sharp.dll"
        ;"gdk-sharp.dll"
        ;"cairo-sharp.dll"
        ;"pango-sharp.dll"
        ]
    List.map (fun p -> System.IO.Path.Combine(gtkHome, p)) gtkDlls




let buildLib () =
    let status = FsharpCompiler.CompileLibrary(["src/fxgtk.fsx"]
                                               ,gtkDependencies
                                               ,"bin/fxgtk.dll"
                                               ,"bin/fxgtk.xml"
                                               )

    match status with
    | 0 -> printfn "Build successful. Ok"
    | _ -> printfn "Build failed"

let buildExample example =
    let outputFile = example |> SysUtils.joinPath "bin/"
                             |> SysUtils.replaceExt "exe"

    Term.withColor Term.blue (fun () -> printfn "Building Example: %s\n" example)

    let status = FsharpCompiler.CompileExecutableWEXE([SysUtils.joinPath "examples/" example]
                                                      ,["bin/fxgtk.dll"] @ gtkDependencies
                                                      ,outputFile
                                                      )

    match status with
    | 0 -> Term.printWithColor Term.blue (sprintf "\nBuild %s successful. Ok"  outputFile)
    | _ -> Term.printWithColor Term.red  (sprintf "\nBuild %s Failed." outputFile)
    printfn "-------------------------------------\n\n"


let getExamples () =
    SysUtils.getFilesExt "examples" "*.fsx"
    |> Seq.map SysUtils.baseName

let runArgs args =
    match args with
        
   // Build library fxgtk.dll 
    | ["--lib"] -> buildLib()
        
    // Show all examples  
    | ["--example"]
      -> getExamples () |> Seq.iter (printfn "%s")
         

    // Build all examples   
    | ["--example" ; "--all"]
      -> getExamples() |> Seq.iter buildExample

    
    // Build a given example 
    | ["--example"; fileName] -> buildExample fileName
    
    | cmd -> printfn "Error: Invalid command: %A" cmd



let () =
    runArgs <| SysUtils.getCommandLineArgs() 
