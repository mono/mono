#
# Makefile for Win32 to build SqlSharpGtk for Mono
#
# Author:
#    Daniel Morgan <danmorg@sc.rr.com>
#
# (c)copyright 2002 Daniel Morgan 
#

PROJECT = sqlsharpgtk.exe

# Environment Variable CSHARPCOMPILER needs to be defined to use for your compiler
# For example on Cygwin:
# export CSHARPCOMPILER="mono f:/cygwin/home/DanielMorgan/mono/install/bin/mcs.exe"
# For example on Linux:
# export CSHARPCOMPILER=mcs

# Environment Variable CLR_LIBS_PATH needs to be defined to find the CLR class libraries
# For example on Cygwin:
#      $ export CLR_LIBS_PATH="f:/cygwin/home/DanielMorgan/mono/install/lib"
# For example on Linux:
#      $ export CLR_LIBS_PATH="$HOME/mono/install/lib"

GTK_SHARP_LIBS = -r glib-sharp.dll -r pango-sharp.dll -r atk-sharp.dll -r gtk-sharp.dll -r System.Drawing.dll
SQLSHARP_GTK_LIBS = $(GTK_SHARP_LIBS) -r System.Data.dll

SOURCES = sqlsharpgtk.cs SqlEditorSharp.cs LoginDialog.cs DbProvider.cs DbProviderCollection.cs DataGrid.cs FileSelectionDialog.cs

all : $(PROJECT)

$(PROJECT) : $(SOURCES)
	$(CSHARPCOMPILER) -o $(PROJECT) $(SOURCES) -lib:$(CLR_LIBS_PATH) $(SQLSHARP_GTK_LIBS)

clean:
	rm *.exe

