# F# Compiler path 
FSC        := fsharpc

# Gtk Sharp Home directory 
GTK_HOME   := /usr/lib/mono/gtk-sharp-3.0/


lib    := fxgtk.dll 
libxml := fxgtk.xml 
libsrc := fxgtk.fsx


#
## ------------------------------------------------- ##

LIBS       := atk-sharp.dll gio-sharp.dll glib-sharp.dll gtk-sharp.dll gdk-sharp.dll

REFS       := $(addprefix -r:$(GTK_HOME), $(LIBS))



## --------- RULES ----------------------------------- ##

all: lib

lib: $(lib)

$(lib): $(libsrc)
	$(FSC) $(libsrc) --out:$(lib) --doc:$(libxml) --target:library  $(REFS)

clean:
	rm -rf $(lib)
