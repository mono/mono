#
# Makefile for Linux to build SqlSharpGtk for Mono
#
# Author:
#    Daniel Morgan <danmorg@sc.rr.com>
#
# (c)copyright 2002 Daniel Morgan 
#

PROJECT = sqlsharpgtk.exe

CSC = mcs

GTK_SHARP_LIBS = -r glib-sharp.dll -r pango-sharp.dll -r atk-sharp.dll -r gtk-sharp.dll -r gdk-sharp -r System.Drawing.dll
SQLSHARP_GTK_LIBS = $(GTK_SHARP_LIBS) -r System.Data.dll

SOURCES = sqlsharpgtk.cs SqlEditorSharp.cs LoginDialog.cs DbProvider.cs DbProviderCollection.cs DataGrid.cs FileSelectionDialog.cs SqlSharpDataAdapter.cs

all : $(PROJECT)

$(PROJECT) : $(SOURCES)
	$(CSC) -o $(PROJECT) $(SOURCES) -lib:$(MONO_PATH) $(SQLSHARP_GTK_LIBS)

clean:
	rm *.exe

