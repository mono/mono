srcdir=.
SOURCES=main.cs ../lib/*.cs

LIBS = \
	-r glib-sharp.dll	\
	-r pango-sharp.dll	\
	-r atk-sharp.dll	\
	-r gdk-sharp.dll	\
	-r gtk-sharp.dll	\
	-r gnome-sharp.dll	\
	-r glade-sharp.dll	\
	-r System.Drawing

RESOURCES = \
	 -resource:$(srcdir)/gtk-monodoc.glade,gtk-monodoc.glade \
	-resource:$(srcdir)/mono.png,mono.png

monodoc: $(SOURCES)
	mcs $(SOURCES) $(LIBS) $(RESOURCES) -o monodoc.exe