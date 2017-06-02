// #!/usr/bin/env fsharpi

open System

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

module SysUtils = 

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


let buildLib () =
    FsharpCompiler.CompileLibrary(
        ["src/fxgtk.fsx"]
        ,gtkDependencies
        ,"bin/fxgtk.dll"
        ,"bin/fxgtk.xml"
        )

let buildExample example =
    let outputFile = example |> SysUtils.joinPath "bin/"
                             |> SysUtils.replaceExt "exe"

    printfn "Building Example: %s" example

    FsharpCompiler.CompileExecutableWEXE(
        [SysUtils.joinPath "examples/" example]
        ,["bin/fxgtk.dll"] @ gtkDependencies
        ,outputFile
        )

    printfn "Example %s build. Ok"  outputFile


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
