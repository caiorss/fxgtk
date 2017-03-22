# F# Compiler path 
FSC        := fsharpc

# Gtk Sharp Home directory 
GTK_HOME   := /usr/lib/mono/gtk-sharp-3.0/


lib    := fxgtk.dll 
libsrc := fxgtk.fsx 

#
## ------------------------------------------------- ##

LIBS       := atk-sharp.dll gio-sharp.dll glib-sharp.dll gtk-sharp.dll gdk-sharp.dll

REFS       := $(addprefix -r:$(GTK_HOME), $(LIBS))



## --------- RULES ----------------------------------- ##

all: lib

lib: $(lib)

$(lib): $(libsrc)
	$(FSC) $(libsrc) --target:library --out:$(lib) $(REFS)

clean:
	rm -rf $(lib)
