# makes monodoc-qt.exe

include ../Makefile.defs

CFLAGS  := $(CFLAGS) -lib:../lib -r Qt -r monodoc
SOURCES  = docedit.cs ieditform.cs listview.cs options.cs progress.cs \
           summaryform.cs typeedit.cs global.cs listitem.cs monodoc.cs \
           paramedit.cs remarksform.cs tageditor.cs

all: monodoc-qt.exe

monodoc-qt.exe: $(SOURCES)
	$(MONO) $(MCS) $(CFLAGS) $(SOURCES) -o $@
	chmod 0755 $@

clean:
	-rm monodoc-qt.exe

install: monodoc-qt.exe
	$(MAKE) uninstall
	install -o root -g root -m 0755 monodoc-qt.exe $(PREFIX)/$(BINDIR)/monodoc-qt.exe
	ln -s $(PREFIX)/$(BINDIR)/monodoc-qt.exe $(PREFIX)/$(BINDIR)/monodoc.exe
	ln -s $(PREFIX)/$(BINDIR)/monodoc.exe $(PREFIX)/$(BINDIR)/monodoc
	#install -o root -g root -m 0755 monodoc.sh $(PREFIX)/$(BINDIR)/monodoc

uninstall:
	-rm -rf $(PREFIX)/$(BINDIR)/monodoc-qt.exe
	-rm -rf $(PREFIX)/$(BINDIR)/monodoc.exe
	-rm -rf $(PREFIX)/$(BINDIR)/monodoc
