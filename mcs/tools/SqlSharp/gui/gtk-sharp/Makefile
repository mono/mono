#
# Makefile to build SqlSharpGtk for Mono
#
# Author:
#    Daniel Morgan <danielmorgan@verizon.net>
# (c)copyright 2002-2003 Daniel Morgan 
#

PROJECT = sqlsharpgtk.exe

CSHARPCOMPILER=mcs

GTK_SHARP_LIBS = -r glib-sharp -r gdk-sharp -r pango-sharp -r atk-sharp -r gtk-sharp -r System.Drawing
SQLSHARP_GTK_LIBS = $(GTK_SHARP_LIBS) -r System.Data

SOURCES = sqlsharpgtk.cs SqlEditorSharp.cs LoginDialog.cs DbProvider.cs DbProviderCollection.cs DataGrid.cs FileSelectionDialog.cs SqlSharpDataAdapter.cs

all : $(PROJECT)

$(PROJECT) : $(SOURCES)
	$(CSHARPCOMPILER) -o $(PROJECT) $(SOURCES) $(SQLSHARP_GTK_LIBS)

clean:
	rm *.exe

