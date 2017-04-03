#!/usr/bin/env sh
# 
# Build script to compile Fsxgtk library sample code in examples/ directory.
# 


#---------------------------------------------------
## Enable Debug
#  - Uncomment the line below to enable debug.
# set -x


#
# Compile sample F# script to executable
#
# > build app.fsx
#
# Creates app.exe
#
function build (){
    file=$1
    app="$(basename $file .fsx).exe"

    echo "Building "$app
    echo ""
    
    fsharpc "$1"  --out:examples/$app --target:winexe -O    \
            -r:/usr/lib/mono/gtk-sharp-3.0/atk-sharp.dll    \
            -r:/usr/lib/mono/gtk-sharp-3.0/gio-sharp.dll    \
            -r:/usr/lib/mono/gtk-sharp-3.0/glib-sharp.dll   \
            -r:/usr/lib/mono/gtk-sharp-3.0/gtk-sharp.dll    \
            -r:/usr/lib/mono/gtk-sharp-3.0/gdk-sharp.dll    \
            -r:/usr/lib/mono/gtk-sharp-3.0/cairo-sharp.dll  \
            -r:/usr/lib/mono/gtk-sharp-3.0/pango-sharp.dll  \
            -r:fxgtk.dll \
            --staticlink:fxgtk
    chmod +x examples/$app

    echo ""
    echo "Build successful $app Ok."
    echo "------------------------------------------------"
    echo ""
}

function buildRun(){
    build $1
    app="$(basename $file .fsx).exe"
    mono examples/$app
}

case $1 in

    # Show all examples
    -show)
        ls -l examples/*.fsx | awk -F" " '{print $9}'  
        ;;
    
    # Build exe
    -exe)
        build $2
        ;;

    # Build and run exe
    -exe-run)
        buildRun $2
        ;;

    # Build all examples
    -all)
        ls examples -l                  \
            | awk -F" " '{print $9}'    \
            | grep ".fsx"               \
            | xargs -i echo examples/{} \
            | xargs -i ./build.sh -exe {}

       
        ;;

    # Clean
    -clean)
        rm -rf examples/*.exe
        ;;
    *)
        cat <<EOF
Build script to compile Fsxgtk examples.

Commands: 

 -show                  - Show all examples (*.fsx files)

 -exe [script.fsx]      - Build script.exe 

 -exe-run [script.fsx]  - Build and run script.fsx 
                
 -all                   - Compile all examples in examples/ directory.

 -clean                 - Remove all *.fsx files in examples/ directory.

EOF
        
        ;;
esac
