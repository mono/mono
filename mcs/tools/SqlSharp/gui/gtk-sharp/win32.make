#
# Makefile for Win32 to build SqlSharpGtk for Mono
#
# Author:
#    Daniel Morgan <danmorg@sc.rr.com>
#
# (c)copyright 2002 Daniel Morgan 
#

# used for debugging
DEBUG = /d:DEBUG
# DEBUG =

PROJECT = sqlsharpgtk.exe

# Environment Variable CSHARPCOMPILER needs to be defined to use for your compiler
# For example:
# export CSHARPCOMPILER="mono f:/cygwin/home/DanielMorgan/mono/install/bin/mcs.exe"

# Environment Variable CLR_LIBS_PATH needs to be defined to find the CLR class libraries
# For example:
# export CLR_LIBS_PATH=f:/cygwin/home/DanielMorgan/mono/install/lib

GTK_SHARP_LIBS = -r $(CLR_LIBS_PATH)/glib-sharp.dll -r $(CLR_LIBS_PATH)/pango-sharp.dll -r $(CLR_LIBS_PATH)/atk-sharp.dll -r $(CLR_LIBS_PATH)/gtk-sharp.dll -r $(CLR_LIBS_PATH)/System.Drawing.dll

SQLSHARP_GTK_LIBS = $(GTK_SHARP_LIBS) -r $(CLR_LIBS_PATH)/System.Data.dll

MODULES = sqlsharpgtk.cs SqlEditorSharp.cs LoginDialog.cs DbProvider.cs DbProviderCollection.cs

all : $(PROJECT)

$(PROJECT) : $(MODULES)
	$(CSHARPCOMPILER) -o $(PROJECT) $(MODULES) $(SQLSHARP_GTK_LIBS) $(DEBUG)

clean:
	rm *.exe

