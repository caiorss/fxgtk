# F# Compiler path 
FSC        := fsharpc

# Gtk Sharp Home directory 
GTK_HOME   := /usr/lib/mono/gtk-sharp-3.0/

# --> Library building settins 
lib    := fxgtk.dll 
libxml := fxgtk.xml 
libsrc := fxgtk.fsx


# --> App building settings 
app    := app.exe
appsrc := loader.fsx 

#
## ------------------------------------------------- ##

# Gtk-sharp Dependencies 
LIBS       := atk-sharp.dll gio-sharp.dll glib-sharp.dll gtk-sharp.dll gdk-sharp.dll cairo-sharp.dll pango-sharp.dll

REFS       := $(addprefix -r:$(GTK_HOME), $(LIBS))



## --------- RULES ----------------------------------- ##

all: lib

lib: $(lib)

app: $(app)

$(lib): $(libsrc)
	$(FSC) $(libsrc) --out:$(lib) --doc:$(libxml) --target:library  $(REFS)

$(app): lib
	$(FSC) $(appsrc) --out:$(app) --target:winexe $(REFS) -r:$(lib)

clean:
	rm -rf $(lib)
