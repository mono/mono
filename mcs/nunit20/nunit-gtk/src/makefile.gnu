#
# Makefile for nunit-gtk.exe
#
# Authors:
#   Gonzalo Paniagua Javier (gonzalo@ximian.com)
#
topdir=../../..

INSTALL=/usr/bin/install
RUNTIME=mono
MONO_PATH_PREFIX=
MCS=$(topdir)/mcs/mcs.exe
MCSFLAGS= /debug+ /debug:full /nologo -L $(topdir)/class/lib

ASSEMBLIES= NUnit.Framework \
	    gtk-sharp \
	    glib-sharp  \
	    gdk-sharp \
	    glade-sharp \
	    gnome-sharp \
	    gconf-sharp

RESOURCES= nunit-gtk.glade \
	   ../art/nunit-gui.png \
	   ../art/none.png \
	   ../art/red.png \
	   ../art/yellow.png \
	   ../art/green.png

SOURCES= main.cs \
	 AssemblyStore.cs \
	 CircleRenderer.cs \
	 FileDialog.cs \
	 Settings.cs \
	 AssemblyInfo.cs

SCHEMA=nunit-gtk.schema

# 
REFS= $(addsuffix .dll, $(addprefix /r:, $(ASSEMBLIES)))
RESS= $(foreach res,$(RESOURCES), $(addprefix /resource:,$(res)),$(notdir $(res)))

all: nunit-gtk.exe

install: nunit-gtk.exe installschema
	if test x$$prefix = x; then \
		echo Usage is: make -f makefile.gnu install prefix=YOURPREFIX; \
		exit 1; \
	fi;
	mkdir -p $(prefix)/bin
	$(INSTALL) -m 755 nunit-gtk.exe $(prefix)/bin

nunit-gtk.exe: $(SOURCES) $(RESOURCES)
	MONO_PATH=$(MONO_PATH_PREFIX)$(MONO_PATH) \
	  $(RUNTIME) $(MCS) $(MCSFLAGS) $(REFS) $(RESS) /out:$@ $(SOURCES)

installschema:
	GCONF_CONFIG_SOURCE="" gconftool-2 --makefile-install-rule $(SCHEMA)

clean:
	rm -f *~ *.exe *.bak *.temp

