# Makefile for Win32 to build SqlSharpGtk for Mono

PROJECT = sqlsharpgtk.exe

CSC = mono f:/cygwin/home/DanielMorgan/mono/install/bin/mcs.exe

MCS_LIBS_PATH = f:/cygwin/home/DanielMorgan/mono/install/lib

GTK_SHARP_LIBS = -r $(MCS_LIBS_PATH)/glib-sharp.dll -r $(MCS_LIBS_PATH)/pango-sharp.dll -r $(MCS_LIBS_PATH)/atk-sharp.dll -r $(MCS_LIBS_PATH)/gtk-sharp.dll -r $(MCS_LIBS_PATH)/System.Drawing.dll

SQLSHARP_GTK_LIBS = $(GTK_SHARP_LIBS) -r $(MCS_LIBS_PATH)/System.Drawing.dll -r $(MCS_LIBS_PATH)/System.Data.dll

MODULES = sqlsharpgtk.cs SqlEditor.cs

all : $(PROJECT)

$(PROJECT) :
	$(CSC) -o $(PROJECT) $(MODULES) $(SQLSHARP_GTK_LIBS)

clean:
	rm *.o
	rm *.exe


